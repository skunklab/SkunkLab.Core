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
        private ProtocolAdapter adapter;
        private CancellationTokenSource source;
        private WebSocket socket;
        private readonly WebSocketOptions _options;

        

        public PiraeusWebSocketMiddleware(RequestDelegate next, PiraeusConfig config, IClusterClient client, IOptions<WebSocketOptions> options)
        {            
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

            socket = await context.WebSockets.AcceptWebSocketAsync();

            source = new CancellationTokenSource();
            //adapter = ProtocolAdapterFactory.Create(config, context, socket, authn, source.Token);
            adapter = ProtocolAdapterFactory.Create(config, context, socket, null, authn, source.Token);
            adapter.OnClose += Adapter_OnClose;
            adapter.OnError += Adapter_OnError;
            adapter.Init();

            await adapter.Channel.OpenAsync();  //blocking until closed
        }

        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs e)
        {
            //write the error
            adapter.Channel.CloseAsync().GetAwaiter();
        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs e)
        {
            adapter.Dispose();
        }




    }
}
