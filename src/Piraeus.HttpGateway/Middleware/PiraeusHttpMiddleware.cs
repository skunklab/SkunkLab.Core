using Microsoft.AspNetCore.Http;
using Orleans;
using Piraeus.Adapters;
using Piraeus.Configuration;
using Piraeus.Grains;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.HttpGateway.Middleware
{
    public class PiraeusHttpMiddleware
    {
        private HttpContext context;
        private readonly PiraeusConfig config;
        private ProtocolAdapter adapter;
        private CancellationTokenSource source;
        private delegate void HttpResponseObserverHandler(object sender, SkunkLab.Channels.ChannelObserverEventArgs args);
        private event HttpResponseObserverHandler OnMessage;

        private readonly WaitHandle[] waitHandles = new WaitHandle[]
        {
            new AutoResetEvent(false)
        };

        private readonly GraphManager graphManager;

        public PiraeusHttpMiddleware(RequestDelegate next, PiraeusConfig config, IClusterClient client)
        {
            this.config = config;
            graphManager = new GraphManager(client);
        }

        public async Task Invoke(HttpContext context)
        {
            source = new CancellationTokenSource();
            if (context.Request.Method.ToUpperInvariant() == "POST")
            {
                //sending a message
                adapter = ProtocolAdapterFactory.Create(config, graphManager, context, null, null, source.Token);
                adapter.Init();
            }
            else if (context.Request.Method.ToUpperInvariant() == "GET")
            {
                //long polling               
                adapter = ProtocolAdapterFactory.Create(config, graphManager, context, null, null, source.Token);
                adapter.OnObserve += Adapter_OnObserve;
                adapter.Init();
                this.context = context;
                ThreadPool.QueueUserWorkItem(new WaitCallback(Listen), waitHandles[0]);
                WaitHandle.WaitAll(waitHandles);
                adapter.Dispose();
            }

            await Task.CompletedTask;
        }

        private void Listen(object state)
        {
            AutoResetEvent are = (AutoResetEvent)state;
            OnMessage += (o, a) =>
            {
                
                context.Response.ContentType = a.ContentType;
                context.Response.ContentLength = a.Message.Length;
                context.Response.Headers.Add("x-sl-resource", a.ResourceUriString);
                context.Response.StatusCode = 200;
                context.Response.BodyWriter.WriteAsync(a.Message);                
                context.Response.CompleteAsync();
                are.Set();
            };
        }

        private void Adapter_OnObserve(object sender, SkunkLab.Channels.ChannelObserverEventArgs e)
        {
            OnMessage?.Invoke(this, e);
        }
    }
}
