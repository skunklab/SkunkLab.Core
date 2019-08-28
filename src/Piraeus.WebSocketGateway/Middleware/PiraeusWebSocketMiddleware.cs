using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Orleans;
using Piraeus.Adapters;
using Piraeus.Configuration;
using Piraeus.Grains;
using SkunkLab.Security.Authentication;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;


using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.AspNetCore.Builder;

using Microsoft.AspNetCore.WebSockets.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using SkunkLab.Channels;

namespace Piraeus.WebSocketGateway.Middleware
{
    public class PiraeusWebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private PiraeusConfig config;
        private CancellationTokenSource source;
        private readonly WebSocketOptions _options;
        private Dictionary<string, ProtocolAdapter> container;

        

        public PiraeusWebSocketMiddleware(RequestDelegate next, PiraeusConfig config, IClusterClient client, IOptions<WebSocketOptions> options)
        {
            container = new Dictionary<string, ProtocolAdapter>();
            _next = next;
            _options = options.Value;
            this.config = config;

            if (!GraphManager.IsInitialized)
            {
                GraphManager.Initialize(client);
            }            
        }

        public async Task Invoke(HttpContext context)
        {            
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            BasicAuthenticator basicAuthn = new BasicAuthenticator();
            SkunkLab.Security.Tokens.SecurityTokenType tokenType = Enum.Parse<SkunkLab.Security.Tokens.SecurityTokenType>(config.ClientTokenType, true);
            basicAuthn.Add(tokenType, config.ClientSymmetricKey, config.ClientIssuer, config.ClientAudience, context);
            IAuthenticator authn = basicAuthn;

            WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
            
            source = new CancellationTokenSource();
            ProtocolAdapter adapter = ProtocolAdapterFactory.Create(config, context, socket, null, authn, source.Token);
            container.Add(adapter.Channel.Id, adapter);
            adapter.OnClose += Adapter_OnClose;
            adapter.OnError += Adapter_OnError;
            adapter.Init();


            await adapter.Channel.OpenAsync();
            await _next(context);
            Console.WriteLine("Exiting");
            
        }

        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs e)
        {
            if(container.ContainsKey(e.ChannelId))
            {
                ProtocolAdapter adapter = container[e.ChannelId];
                adapter.Channel.CloseAsync().GetAwaiter();
            }            
        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs e)
        {
            ProtocolAdapter adapter = null;

            try
            {
                if(container.ContainsKey(e.ChannelId))
                {
                    adapter = container[e.ChannelId];
                }

                if ((adapter != null && adapter.Channel != null) && (adapter.Channel.State == ChannelState.Closed || adapter.Channel.State == ChannelState.Aborted || adapter.Channel.State == ChannelState.ClosedReceived || adapter.Channel.State == ChannelState.CloseSent))
                {
                    adapter.Dispose();
                }
                else
                {
                    try
                    {
                        adapter.Channel.CloseAsync().GetAwaiter();
                    }
                    catch { }
                    adapter.Dispose();
                }
            }
            catch { }

        }




    }
}
