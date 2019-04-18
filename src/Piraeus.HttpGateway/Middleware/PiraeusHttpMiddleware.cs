using Microsoft.AspNetCore.Http;
using Orleans;
using Piraeus.Adapters;
using Piraeus.Configuration;
using Piraeus.Grains;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.HttpGateway.Middleware
{
    public class PiraeusHttpMiddleware
    {
        private readonly RequestDelegate _next;
        private HttpContext context;
        private PiraeusConfig config;
        private ProtocolAdapter adapter;
        private CancellationTokenSource source;
        private delegate void HttpResponseObserverHandler(object sender, SkunkLab.Channels.ChannelObserverEventArgs args);
        private event HttpResponseObserverHandler OnMessage;
        
        private WaitHandle[] waitHandles = new WaitHandle[]
        {
            new AutoResetEvent(false)
        };

        public PiraeusHttpMiddleware(RequestDelegate next, PiraeusConfig config, IClusterClient client)
        {
            _next = next;
            this.config = config;

            if (!GraphManager.IsInitialized)
            {
                GraphManager.Initialize(client);
            }
        }

        public async Task Invoke(HttpContext context)
        {
            source = new CancellationTokenSource();
            if(context.Request.Method.ToUpperInvariant() == "POST")
            {
                //sending a message
                //adapter = ProtocolAdapterFactory.Create(config, context, source.Token);
                adapter = ProtocolAdapterFactory.Create(config, context, null, null, source.Token);
                adapter.Init();
            }
            else if (context.Request.Method.ToUpperInvariant() == "GET")
            {
                //long polling
                //adapter = ProtocolAdapterFactory.Create(config, context, source.Token);
                adapter = ProtocolAdapterFactory.Create(config, context, null, null, source.Token);
                adapter.OnObserve += Adapter_OnObserve;
                adapter.Init();
                this.context = context;
                ThreadPool.QueueUserWorkItem(new WaitCallback(Listen), waitHandles[0]);
                WaitHandle.WaitAll(waitHandles);
                await adapter.Channel.CloseAsync();

                await _next(context);
            }
            else
            {
                
            }
        }

        private void Listen(object state)
        {
            AutoResetEvent are = (AutoResetEvent)state;
            OnMessage += (o, a) => {

                //MediaTypeFormatter formatter = null;
                //if (a.ContentType == "application/octet-stream")
                //{
                //    formatter = new BinaryMediaTypeFormatter();
                //}
                //else if (a.ContentType == "text/plain")
                //{
                //    formatter = new TextMediaTypeFormatter();
                //}
                //else if (a.ContentType == "application/xml" || a.ContentType == "text/xml")
                //{
                //    formatter = new XmlMediaTypeFormatter();
                //}
                //else if (a.ContentType == "application/json" || a.ContentType == "text/json")
                //{
                //    formatter = new JsonMediaTypeFormatter();
                //}
                //else
                //{
                //    throw new SkunkLab.Protocols.Coap.UnsupportedMediaTypeException("Media type formatter not available.");
                //}

                //if (a.ContentType != "application/octet-stream")
                //{
                    

                //    response = Request.CreateResponse<string>(HttpStatusCode.OK, Encoding.UTF8.GetString(a.Message), formatter);
                //}
                //else
                //{
                //    response = Request.CreateResponse<byte[]>(HttpStatusCode.OK, a.Message, formatter);
                //}

                //response.Headers.Add("x-sl-resource", a.ResourceUriString);
                context.Response.ContentType = a.ContentType;
                context.Response.ContentLength = a.Message.Length;
                context.Response.Headers.Add("x-sl-resource", a.ResourceUriString);
                context.Response.StatusCode = 200;
                context.Response.Body = new MemoryStream(a.Message);
                are.Set();
            };
        }

        private void Adapter_OnObserve(object sender, SkunkLab.Channels.ChannelObserverEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
