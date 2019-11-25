using Piraeus.Adapters;
using Piraeus.Configuration;
using Piraeus.Core;
using Piraeus.Core.Logging;
using Piraeus.Grains;
using SkunkLab.Security.Authentication;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.TcpGateway
{
    public class TcpServerListener
    {
        public TcpServerListener(IPEndPoint localEP, PiraeusConfig config, OrleansConfig orleansConfig, ILog logger = null, CancellationToken token = default)
        {
            serverIP = localEP.Address;
            serverPort = localEP.Port;
            listener = new TcpListener(localEP);
            this.token = token;
            dict = new Dictionary<string, ProtocolAdapter>();
            this.config = config;
            this.orleansConfig = orleansConfig;
            this.logger = logger;

            if (config.ClientTokenType != null && config.ClientSymmetricKey != null)
            {
                SecurityTokenType stt = Enum.Parse<SecurityTokenType>(config.ClientTokenType, true);
                BasicAuthenticator bauthn = new BasicAuthenticator();
                bauthn.Add(stt, config.ClientSymmetricKey, config.ClientIssuer, config.ClientAudience);
                this.authn = bauthn;
            }
        }

        public TcpServerListener(IPAddress address, int port, PiraeusConfig config, OrleansConfig orleansConfig, ILog logger = null, CancellationToken token = default)
        {
            serverIP = address;
            serverPort = port;
            listener = new TcpListener(address, port)
            {
                ExclusiveAddressUse = false
            };
            this.token = token;
            dict = new Dictionary<string, ProtocolAdapter>();
            this.config = config;
            this.orleansConfig = orleansConfig;
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
        private readonly IPAddress serverIP;
        private readonly int serverPort;
        private readonly TcpListener listener;
        private readonly CancellationToken token;
        private readonly Dictionary<string, ProtocolAdapter> dict;
        private readonly PiraeusConfig config;
        private readonly IAuthenticator authn;
        private readonly ILog logger;
        private readonly OrleansConfig orleansConfig;

        public async Task StartAsync()
        {
            listener.ExclusiveAddressUse = false;
            listener.Start();

            await logger?.LogInformationAsync($"<----- TCP Listener started on Address {serverIP.ToString()} and Port {serverPort} ----->");

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
                    logger?.LogErrorAsync(ex, "TCP server listener failed to start '{ex.Message}'");
                }
            }
        }

        public async Task StopAsync()
        {
            await logger?.LogInformationAsync($"TCP Listener stopping on Address {serverIP.ToString()} and Port {serverPort}");

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
                                    await logger.LogWarningAsync($"TCP Listener stopping and dispose Protcol adapter {key}");

                                }
                                catch (Exception ex)
                                {
                                    await logger.LogErrorAsync(ex, "Fault dispose protcol adaper while Stopping TCP Listener");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await logger.LogErrorAsync(ex, $"TCP Listener fault during stop.");
                    }
                }
            }
            else
            {
                await logger.LogWarningAsync($"No protocol adapters in TCP Listener dictionary to dispose and remove");
            }

            listener.Stop();
        }

        private void ManageConnection(TcpClient client)
        {
            GraphManager graphManager = new GraphManager(orleansConfig);
            ProtocolAdapter adapter = ProtocolAdapterFactory.Create(config, graphManager, authn, client, logger, token);
            dict.Add(adapter.Channel.Id, adapter);
            adapter.OnError += Adapter_OnError;
            adapter.OnClose += Adapter_OnClose;
            adapter.Init();
            adapter.Channel.OpenAsync().LogExceptions(logger);
            adapter.Channel.ReceiveAsync().LogExceptions(logger);
        }

        private async void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs args)
        {
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
                await logger.LogErrorAsync(ex, "Disposing adapter.");
            }
        }

        private async void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs args)
        {
            await logger.LogErrorAsync(args.Error, "Adapter exception.");

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
                await logger.LogErrorAsync(ex, "Adapter disposing");
            }
        }
    }
}
