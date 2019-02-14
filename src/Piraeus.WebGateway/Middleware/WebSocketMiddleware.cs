using Microsoft.AspNetCore.Http;
using Piraeus.Adapters;
using Piraeus.Configuration.Settings;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Security.Authentication;
using System.Threading;

namespace Piraeus.WebGateway.Middleware
{
    public class WebSocketMiddleware
    {
        private ProtocolAdapter adapter;
        private PiraeusConfig config;
        private CancellationTokenSource source;
        private readonly RequestDelegate _next;
        private bool disposed;
        private WebSocketHandler _webSocketHandler { get; set; }

        public WebSocketMiddleware(RequestDelegate next, PiraeusConfig config)
        {
            _next = next;
            this.config = config;
            source = new CancellationTokenSource();
        }

        public void Invoke(HttpContext context)
        {
            
            adapter = ProtocolAdapterFactory.Create(config, context, source.Token, new BasicAuthenticator());
            adapter.OnClose += Adapter_OnClose;
            adapter.OnError += Adapter_OnError;
            adapter.Init();
        }

        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs e)
        {
            if (!disposed)
            {
                disposed = true;
                adapter.Dispose();
            }            
        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs e)
        {
            if(!disposed)
            {
                disposed = true;
                adapter.Dispose();
            }            
        }
    }
    
}
