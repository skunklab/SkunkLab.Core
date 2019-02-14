using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Piraeus.Adapters;
using Piraeus.Configuration.Settings;
using Piraeus.Grains;
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
        public async Task Get()
        {
            
            source = new CancellationTokenSource();
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                //string subprotocol = HttpContext.WebSockets.WebSocketRequestedProtocols[0];
                socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                //socket = HttpContext.WebSockets.AcceptWebSocketAsync().GetAwaiter().GetResult();


                try
                {
                    //adapter = ProtocolAdapterFactory.Create(config, HttpContext, socket, authn, source.Token);
                    adapter = ProtocolAdapterFactory.Create(config, HttpContext, socket, null, authn, source.Token);
                    adapter.OnClose += Adapter_OnClose;
                    adapter.OnError += Adapter_OnError;
                    adapter.Init();
                    //StatusCode(101);
                    //return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
                }
                catch(Exception ex)
                {
                    StatusCode(500);
                    Console.WriteLine(ex.Message);
                    //return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                //return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
