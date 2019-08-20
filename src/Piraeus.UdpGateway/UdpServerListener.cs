using Microsoft.Extensions.Logging;
using Piraeus.Adapters;
using Piraeus.Configuration;
using SkunkLab.Channels;
using SkunkLab.Security.Authentication;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.UdpGateway
{
    public class UdpServerListener
    {
        public UdpServerListener(PiraeusConfig config, IPEndPoint localEP, ILogger logger = null, CancellationToken token = default(CancellationToken))
        {
            this.config = config;
            this.localEP = localEP;
            this.logger = logger;
            this.token = token;
            cache = new Dictionary<string, Tuple<ProtocolAdapter, CancellationTokenSource>>();
            container = new Dictionary<string, string>();

            SecurityTokenType stt = System.Enum.Parse<SecurityTokenType>(config.ClientTokenType, true);
            BasicAuthenticator bauthn = new BasicAuthenticator();
            bauthn.Add(stt, config.ClientSymmetricKey, config.ClientIssuer, config.ClientAudience);
            authn = bauthn;

        }

        public event EventHandler<ServerFailedEventArgs> OnError;

        private ILogger logger;
        private readonly PiraeusConfig config;
        private readonly IAuthenticator authn;

        private IPEndPoint localEP;
        private CancellationToken token;
        private Dictionary<string, Tuple<ProtocolAdapter, CancellationTokenSource>> cache;
        private Dictionary<string, string> container;

        public async Task StartAsync()
        {
            UdpClient listener = new UdpClient();
            listener.ExclusiveAddressUse = false;
            listener.DontFragment = true;
            listener.Client.Bind(localEP);

            Console.WriteLine("UDP listener initialized on port {0}", localEP.Port);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    UdpReceiveResult result = await listener.ReceiveAsync();
                    if (result.Buffer.Length > 0)
                    {
                        string key = String.Format("{0}:{1}", result.RemoteEndPoint.Address.ToString(), result.RemoteEndPoint.Port);

                        if (!cache.ContainsKey(key))
                        {
                            CancellationTokenSource cts = new CancellationTokenSource();
                            ProtocolAdapter adapter = ProtocolAdapterFactory.Create(config, authn, listener, result.RemoteEndPoint, null, cts.Token);
                            adapter.OnError += Adapter_OnError;
                            adapter.OnClose += Adapter_OnClose;
                            adapter.OnObserve += Adapter_OnObserve;
                            await adapter.Channel.OpenAsync();
                            container.Add(adapter.Channel.Id, key);
                            cache.Add(key, new Tuple<ProtocolAdapter, CancellationTokenSource>(adapter, cts));
                            adapter.Init();
                            await adapter.Channel.AddMessageAsync(result.Buffer);
                        }
                        else
                        {
                            Tuple<ProtocolAdapter, CancellationTokenSource> tuple = cache[key];
                            if (tuple.Item1.Channel.State == ChannelState.Open)
                            {
                                await tuple.Item1.Channel.AddMessageAsync(result.Buffer);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {                   
                    OnError?.Invoke(this, new ServerFailedEventArgs(ex, "UDP", localEP.Port));
                }
            }
        }

        private void Adapter_OnObserve(object sender, ChannelObserverEventArgs e)
        {
            if(container.ContainsKey(e.ChannelId))
            {
                string key = container[e.ChannelId];
               if(cache.ContainsKey(key))
                {
                    IChannel channel = cache[key].Item1.Channel;
                    channel.SendAsync(e.Message).GetAwaiter();
                }
               else
                {
                    logger?.Log(LogLevel.Warning, "UDP cache does not contain channel.");
                }
            }
            else
            {
                logger?.Log(LogLevel.Warning, "Channel not available to UDP");
            }
        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs e)
        {

            if (container.ContainsKey(e.ChannelId))
            {
                if (cache.ContainsKey(container[e.ChannelId]))
                {
                    string cacheKey = container[e.ChannelId];
                    Tuple<ProtocolAdapter, CancellationTokenSource> tuple = cache[cacheKey];
                    //tuple.Item2.Cancel();
                    tuple.Item1.Dispose();
                    cache.Remove(cacheKey);
                    container.Remove(e.ChannelId);
                }
            }
        }

        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs e)
        {
            if(container.ContainsKey(e.ChannelId))
            {
                string key = container[e.ChannelId];
                if (cache.ContainsKey(key))
                {
                    Tuple<ProtocolAdapter, CancellationTokenSource> tuple = cache[key];
                    tuple.Item1.Dispose();
                    cache.Remove(key);
                }

                container.Remove(e.ChannelId);
            }
        }

        public async Task StopAsync()
        {

            //dispose all adapters
            KeyValuePair<string, Tuple<ProtocolAdapter, CancellationTokenSource>>[] kvps = cache.ToArray();

            foreach (var kvp in kvps)
            {
                kvp.Value.Item2.Cancel();
                kvp.Value.Item1.Dispose();
            }

            cache.Clear();
            container.Clear();

            await Task.CompletedTask;
        }
    }
}
