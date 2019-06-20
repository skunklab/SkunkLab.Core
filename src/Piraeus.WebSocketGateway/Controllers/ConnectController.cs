using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Piraeus.Adapters;
using Piraeus.Configuration;
using Piraeus.Grains;
using SkunkLab.Channels;
using SkunkLab.Security.Authentication;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.WebSocketGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectController : ControllerBase
    {
        private PiraeusConfig config;
        private CancellationTokenSource source;
        private ProtocolAdapter adapter;
        private WebSocket socket;
        private IAuthenticator authn;


        public ConnectController(PiraeusConfig config, IClusterClient client)
        {
            this.config = config;
            BasicAuthenticator basicAuthn = new BasicAuthenticator();
           
            SkunkLab.Security.Tokens.SecurityTokenType tokenType = Enum.Parse<SkunkLab.Security.Tokens.SecurityTokenType>(config.ClientTokenType, true);
            basicAuthn.Add(tokenType, config.ClientSymmetricKey, config.ClientIssuer, config.ClientAudience);
            authn = basicAuthn;


            if (!GraphManager.IsInitialized)
            {
                GraphManager.Initialize(client);
            }
        }


        [HttpGet]
        public async Task<HttpResponseMessage> Get()
        {            
            source = new CancellationTokenSource();
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                try
                {
                    socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                    adapter = ProtocolAdapterFactory.Create(config, HttpContext, socket, null, authn, source.Token);
                    adapter.OnClose += Adapter_OnClose;
                    adapter.OnError += Adapter_OnError;
                    adapter.Init();
                    await adapter.Channel.OpenAsync();
                    return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
                }
                catch(Exception ex)
                {
                    StatusCode(500);
                    Console.WriteLine(ex.Message);
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs e)
        {
            try
            {
                adapter.Channel.CloseAsync().GetAwaiter();
            }
            catch { }
        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs e)
        {
            try
            {
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
