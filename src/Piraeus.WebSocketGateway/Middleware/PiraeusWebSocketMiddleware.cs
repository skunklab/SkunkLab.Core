using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Orleans;
using Piraeus.Adapters;
using Piraeus.Configuration;
using Piraeus.Core.Logging;
using Piraeus.Grains;
using SkunkLab.Channels;
using SkunkLab.Security.Authentication;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.WebSocketGateway.Middleware
{
    public class PiraeusWebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly PiraeusConfig config;
        private CancellationTokenSource source;
        private readonly WebSocketOptions _options;
        private readonly Dictionary<string, ProtocolAdapter> container;
        private readonly GraphManager graphManager;
        private readonly ILog logger;


        public PiraeusWebSocketMiddleware(RequestDelegate next, PiraeusConfig config, IClusterClient client, Logger logger, IOptions<WebSocketOptions> options)
        {
            container = new Dictionary<string, ProtocolAdapter>();
            _next = next;
            _options = options.Value;
            this.config = config;

            this.graphManager = new GraphManager(client);
            this.logger = logger;
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
            ProtocolAdapter adapter = ProtocolAdapterFactory.Create(config, graphManager, context, socket, logger, authn, source.Token);
            container.Add(adapter.Channel.Id, adapter);
            adapter.OnClose += Adapter_OnClose;
            adapter.OnError += Adapter_OnError;
            adapter.Init();


            await adapter.Channel.OpenAsync();
            await _next(context);
            Console.WriteLine("Exiting WS Invoke");

        }

        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs e)
        {
            Console.WriteLine($"Adapter OnError - {e.Error.Message}");
            if (container.ContainsKey(e.ChannelId))
            {
                ProtocolAdapter adapter = container[e.ChannelId];
                adapter.Channel.CloseAsync().GetAwaiter();
                Console.WriteLine("Adapter channel closed due to error.");
            }

        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs e)
        {
            Console.WriteLine("Adapter closing");
            ProtocolAdapter adapter = null;

            try
            {
                if (container.ContainsKey(e.ChannelId))
                {
                    adapter = container[e.ChannelId];
                    Console.WriteLine("Adapter on close channel id found adapter to dispose.");
                }
                else
                {
                    Console.WriteLine("Adapter on close did not find a channel id available for the adapter.");
                }

                if ((adapter != null && adapter.Channel != null) && (adapter.Channel.State == ChannelState.Closed || adapter.Channel.State == ChannelState.Aborted || adapter.Channel.State == ChannelState.ClosedReceived || adapter.Channel.State == ChannelState.CloseSent))
                {
                    adapter.Dispose();
                    Console.WriteLine("Adapter disposed");
                }
                else
                {
                    try
                    {
                        Console.WriteLine("Adpater trying to close channel.");
                        adapter.Channel.CloseAsync().GetAwaiter();
                        Console.WriteLine("Adapter has closed the channel");

                    }
                    catch { }
                    adapter.Dispose();
                    Console.WriteLine("Adapter disposed by default");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Adapter on close fault - {ex.Message}");
            }

        }




    }
}
