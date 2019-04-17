using Microsoft.Extensions.Logging;
using Piraeus.Adapters;
using Piraeus.Configuration;
using Piraeus.Core;
using SkunkLab.Security.Authentication;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Configuration;

namespace Piraeus.TcpGateway
{
    public class TcpServerListener
    {
        public TcpServerListener(IPEndPoint localEP, PiraeusConfig config, ILogger logger = null, CancellationToken token = default(CancellationToken))
        {
            serverIP = localEP.Address;
            serverPort = localEP.Port;
            listener = new TcpListener(localEP);
            this.token = token;
            dict = new Dictionary<string, ProtocolAdapter>();
            this.config = config;
            this.logger = logger;

            if (config.ClientTokenType != null && config.ClientSymmetricKey != null)
            {
                SecurityTokenType stt = Enum.Parse<SecurityTokenType>(config.ClientTokenType, true);
                BasicAuthenticator bauthn = new BasicAuthenticator();
                bauthn.Add(stt, config.ClientSymmetricKey, config.ClientIssuer, config.ClientAudience);
                this.authn = bauthn;
            }
        }

        public TcpServerListener(IPAddress address, int port, PiraeusConfig config, ILogger logger = null, CancellationToken token = default(CancellationToken))
        {
            serverIP = address;
            serverPort = port;
            listener = new TcpListener(address, port);
            listener.ExclusiveAddressUse = false;
            this.token = token;
            dict = new Dictionary<string, ProtocolAdapter>();
            this.config = config;
            this.logger = logger;


            if (config.ClientTokenType != null && config.ClientSymmetricKey != null)
            {
                SecurityTokenType stt = (SecurityTokenType)System.Enum.Parse(typeof(SecurityTokenType), config.ClientTokenType, true);
                BasicAuthenticator bauthn = new BasicAuthenticator();
                bauthn.Add(stt, config.ClientSymmetricKey, config.ClientIssuer, config.ClientAudience);
                this.authn = bauthn;
            }
        }

        public event EventHandler<ServerFailedEventArgs> OnError;
        private IPAddress serverIP;
        private readonly int serverPort;
        private TcpListener listener;
        private CancellationToken token;
        private Dictionary<string, ProtocolAdapter> dict;
        private readonly PiraeusConfig config;
        private readonly IAuthenticator authn;
        private readonly ILogger logger;

        public async Task StartAsync()
        {   
            Trace.TraceInformation("<----- TCP Listener staring on Address {0} and Port {1} ----->", serverIP.ToString(), serverPort);
            listener.ExclusiveAddressUse = false;
            listener.Start();

            Console.WriteLine("Listener started on IP {0} Port {1}", serverIP.ToString(), serverPort);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    client.LingerState = new LingerOption(false, 0);
                    client.NoDelay = true;
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    client.Client.UseOnlyOverlappedIO = true;
                    ManageConnection(client);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new ServerFailedEventArgs("TCP", serverPort));
                    logger?.LogError("TCP server listener failed to start '{0}'", ex.Message);
                }
            }
        }

        public async Task StopAsync()
        {
            logger?.LogInformation("TCP Listener stopping on Address {0} and Port {1}", serverIP.ToString(), serverPort);
            //dispose all adapters first
            if (dict != null & dict.Count > 0)
            {
                var keys = dict.Keys;
                if (keys != null && keys.Count > 0)
                {
                    try
                    {
                        string[] keysArray = keys.ToArray();
                        foreach (var key in keysArray)
                        {
                            if (dict.ContainsKey(key))
                            {
                                ProtocolAdapter adapter = dict[key];
                                dict.Remove(key);
                                try
                                {
                                    adapter.Dispose();
                                    logger.LogWarning("TCP Listener stopping and dispose Protcol adapter {0}", key);

                                }
                                catch (Exception ex)
                                {
                                    logger.LogError("Fault dispose protcol adaper while Stopping TCP Listener - {0}",ex.Message);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError("{TCP Listener fault during stop '{0}'", ex.Message);
                    }
                }
            }
            else
            {
                logger.LogWarning("No protocol adapters in TCP Listener dictionary to dispose and remove");
            }

            listener.Stop();

            await Task.CompletedTask;
        }

        private void ManageConnection(TcpClient client)
        {
            
            ProtocolAdapter adapter = ProtocolAdapterFactory.Create(config, authn, client, logger, token);
            dict.Add(adapter.Channel.Id, adapter);
            adapter.OnError += Adapter_OnError;
            adapter.OnClose += Adapter_OnClose;
            adapter.Init();
            adapter.Channel.OpenAsync().LogExceptions();
            adapter.Channel.ReceiveAsync().LogExceptions();
        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs args)
        {
            //Trace.TraceWarning("{0} - Protocol adapter on channel {1} closing.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), args.ChannelId);

            try
            {
                if (dict.ContainsKey(args.ChannelId))
                {
                    ProtocolAdapter adapter = dict[args.ChannelId];
                    dict.Remove(args.ChannelId);
                    adapter.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} - Exception disposing adapter Listener_OnClose - {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), ex.Message);
                //Trace.TraceWarning("{0} - TCP Listener exception disposing adapter Listener_OnClose", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"));
                //Trace.TraceError("{0} - Adapter dispose exception Listener_OnClose - '{1}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"),ex.Message);
                //Trace.TraceError("{0} - Adapter dispose stack trace Listener_OnClose - {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), ex.StackTrace);
            }
        }

        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs args)
        {
            //Trace.TraceError("{0} - Protocol Adapter on channel {1} threw error {2}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), args.ChannelId, args.Error.Message);
            Console.WriteLine("{0} - Adpater exception - {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), args.Error.Message);

            Exception inner = args.Error.InnerException;
            while (inner != null)
            {
                Console.WriteLine("{0} - Adapter Exception Inner - {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), inner.Message);
                inner = inner.InnerException;
            }

            //Trace.WriteLine("------ Stack Trace -------");
            //Trace.TraceError(args.Error.StackTrace);
            //Trace.WriteLine("------ End Stack Trace -------");

            try
            {
                if (dict.ContainsKey(args.ChannelId))
                {
                    ProtocolAdapter adapter = dict[args.ChannelId];
                    dict.Remove(args.ChannelId);
                    adapter.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} - Exception disposing adapter Listener_OnError - {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), ex.Message);
                //Trace.TraceWarning("{0} - TCP Listener exception disposing adapter Listener_OnError", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"));
                //Trace.TraceError("{0} - Adapter dispose exception Listener_OnError - '{1}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"),ex.Message);
                //Trace.TraceError("{0} - Adapter dispose stack trace Listener_OnError - {1}", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), ex.StackTrace);
            }
        }
    }
}
