using Piraeus.Adapters;
using Piraeus.Configuration;
using Piraeus.Core.Logging;
using Piraeus.Grains;
using SkunkLab.Channels;
using SkunkLab.Security.Authentication;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Piraeus.UdpGateway
{
    public class UdpServerListener
    {
        public UdpServerListener(IPEndPoint localEP, PiraeusConfig config, OrleansConfig orleansConfig, ILog logger = null, CancellationToken token = default)
        {
            this.localEP = localEP;
            listener = new UdpClient();
            this.token = token;
            dict = new Dictionary<string, ProtocolAdapter>();
            this.config = config;
            //this.orleansConfig = orleansConfig;
            this.logger = logger;
            graphManager = new GraphManager(orleansConfig);

            if (config.ClientTokenType != null && config.ClientSymmetricKey != null)
            {
                SecurityTokenType stt = Enum.Parse<SecurityTokenType>(config.ClientTokenType, true);
                BasicAuthenticator bauthn = new BasicAuthenticator();
                bauthn.Add(stt, config.ClientSymmetricKey, config.ClientIssuer, config.ClientAudience);
                this.authn = bauthn;
            }

            cache = MemoryCache.Default;
        }

        public UdpServerListener(IPAddress address, int port, PiraeusConfig config, OrleansConfig orleansConfig, ILog logger = null, CancellationToken token = default)
        {            
            localEP = new IPEndPoint(address, port);
            listener = new UdpClient();
            this.token = token;
            dict = new Dictionary<string, ProtocolAdapter>();
            this.config = config;
            //this.orleansConfig = orleansConfig;
            this.logger = logger;
            graphManager = new GraphManager(orleansConfig);


            if (config.ClientTokenType != null && config.ClientSymmetricKey != null)
            {
                SecurityTokenType stt = (SecurityTokenType)System.Enum.Parse(typeof(SecurityTokenType), config.ClientTokenType, true);
                BasicAuthenticator bauthn = new BasicAuthenticator();
                bauthn.Add(stt, config.ClientSymmetricKey, config.ClientIssuer, config.ClientAudience);
                this.authn = bauthn;
            }

            cache = MemoryCache.Default;            
        }

        
        private readonly CancellationToken token;
        private readonly Dictionary<string, ProtocolAdapter> dict;
        private readonly PiraeusConfig config;
        private readonly IAuthenticator authn;
        private readonly ILog logger;
        //private readonly OrleansConfig orleansConfig;
        private readonly UdpClient listener;
        private readonly IPEndPoint localEP;
        private readonly MemoryCache cache;
        private readonly GraphManager graphManager;


        public async Task StartAsync()
        {
            listener.ExclusiveAddressUse = false;
            listener.DontFragment = true;            
            listener.Client.Bind(localEP);

            await logger.LogDebugAsync($"UDP Gateway started on {localEP.Address.ToString()} and port {localEP.Port}");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    UdpReceiveResult result = await listener.ReceiveAsync();
                    if (result.Buffer.Length > 0)
                    {   
                        string key = CreateNamedKey($"{result.RemoteEndPoint.Address.ToString()}:{result.RemoteEndPoint.Port}");
                        if (cache.Contains(key))
                        {
                            Tuple<ProtocolAdapter, CancellationTokenSource> tuple = (Tuple<ProtocolAdapter, CancellationTokenSource>)cache.Get(key); 
                            if(tuple != null && tuple.Item1 != null)
                            {
                                cache.Get(CreateNamedKey(tuple.Item1.Channel.Id)); //ensure do not expire sliding
                                if (tuple.Item1.Channel.State == ChannelState.Open)
                                {
                                    await tuple.Item1.Channel.AddMessageAsync(result.Buffer);
                                }
                            }                            
                        }
                        else
                        {
                            CancellationTokenSource cts = new CancellationTokenSource();                            
                            ProtocolAdapter adapter = ProtocolAdapterFactory.Create(config, graphManager, authn, listener, result.RemoteEndPoint, logger, cts.Token);
                            string namedKey = CreateNamedKey(adapter.Channel.Id);
                            cache.Add(namedKey, key, GetCachePolicy(5.0 * 60.0));
                            cache.Add(key, new Tuple<ProtocolAdapter, CancellationTokenSource>(adapter, cts), GetCachePolicy(5.0 * 60.0));

                            adapter.OnError += Adapter_OnError;
                            adapter.OnClose += Adapter_OnClose;
                            adapter.OnObserve += Adapter_OnObserve;
                            await adapter.Channel.OpenAsync();                            
                            adapter.Init();
                            await adapter.Channel.AddMessageAsync(result.Buffer);
                        }
                    }
                }
                catch(Exception ex)
                {
                    logger?.LogErrorAsync(ex, "Fault UDP listener.");
                    throw ex;
                }
            }

        }

        public async Task StopAsync()
        {
            await logger?.LogInformationAsync($"UDP Listener stopping on Address {localEP.Address.ToString()} and Port {localEP.Port}");

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
                                    await logger.LogWarningAsync($"UDP Listener stopping and dispose Protcol adapter {key}");

                                }
                                catch (Exception ex)
                                {
                                    await logger.LogErrorAsync(ex, "Fault dispose protcol adaper while Stopping UDP Listener - {ex.Message}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await logger.LogErrorAsync(ex, $"UDP Listener fault during stop.'");
                    }
                }
            }
            else
            {
                await logger.LogWarningAsync($"No protocol adapters in TCP Listener dictionary to dispose and remove");
            }

            listener.Close();
        }

        private void Adapter_OnObserve(object sender, ChannelObserverEventArgs e)
        {
            string key = CreateNamedKey(e.ChannelId);

            if (cache.Contains(key))
            {
                string namedKey = (string)cache.Get(key);
                if (cache.Contains(namedKey))
                {
                    Tuple<ProtocolAdapter, CancellationTokenSource> tuple = (Tuple<ProtocolAdapter, CancellationTokenSource>)cache.Get(namedKey);
                    if (tuple != null && tuple.Item1 != null)
                    {
                        tuple.Item1.Channel.SendAsync(e.Message).GetAwaiter();
                    }                    
                }
            }
        }

        private void Adapter_OnClose(object sender, ProtocolAdapterCloseEventArgs e)
        {

            Cleanup(e.ChannelId);
        }

        private void Adapter_OnError(object sender, ProtocolAdapterErrorEventArgs e)
        {
            Cleanup(e.ChannelId);
        }

        private void Cleanup(string id)
        {
            string channelKey = CreateNamedKey(id);

            if (cache.Contains(channelKey))
            {
                string tupleKey = (string)cache.Get(channelKey);
                if (cache.Contains(tupleKey))
                {
                    try
                    {
                        Tuple<ProtocolAdapter, CancellationTokenSource> tuple = (Tuple<ProtocolAdapter, CancellationTokenSource>)cache.Get(tupleKey);
                        if (tuple != null && tuple.Item1 != null)
                        {
                            tuple.Item1.Dispose();
                        }
                        cache.Remove(tupleKey);
                        cache.Remove(channelKey);
                    }
                    catch(Exception ex)
                    {
                        logger?.LogErrorAsync(ex, "Fault UDP cleanup.").GetAwaiter();
                    }
                }
            }
        }

        private string CreateNamedKey(string key)
        {
            return $"{localEP.Port}:key={key}";
        }

        private CacheItemPolicy GetCachePolicy(double expirySeconds)
        {
            return new CacheItemPolicy()
            {
                SlidingExpiration = TimeSpan.FromSeconds(expirySeconds),
                RemovedCallback = CacheItemRemovedCallback
            };            
        }

        private void CacheItemRemovedCallback(CacheEntryRemovedArguments args)
        {
            if(args.RemovedReason == CacheEntryRemovedReason.Expired)
            {
                logger?.LogInformationAsync($"Expired {args.CacheItem.Key} removed.");
                try
                {
                    string[] parts = args.CacheItem.Key.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2) //channel id
                    {
                        if (cache.Contains((string)args.CacheItem.Value))
                        {
                            Tuple<ProtocolAdapter, CancellationTokenSource> tuple = (Tuple<ProtocolAdapter, CancellationTokenSource>)cache.Get((string)args.CacheItem.Value);
                            if (tuple != null && tuple.Item1 != null)
                            {
                                tuple.Item1.Dispose();
                            }

                            cache.Remove((string)args.CacheItem.Value);
                        }
                    }

                    if (parts.Length == 3)
                    {
                        Tuple<ProtocolAdapter, CancellationTokenSource> tuple = (Tuple<ProtocolAdapter, CancellationTokenSource>)args.CacheItem.Value;
                        if (tuple != null && tuple.Item1 != null)
                        {
                            tuple.Item1.Dispose();
                        }
                    }
                }
                catch(Exception ex)
                {
                    logger?.LogErrorAsync(ex, "Fault UDP cache expiry.").GetAwaiter();
                }
               

            }
        }

    

    }
}
