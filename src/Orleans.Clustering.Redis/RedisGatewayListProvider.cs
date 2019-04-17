using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Messaging;
using Orleans.Runtime;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Orleans.Clustering.Redis
{
    public class RedisGatewayListProvider : IGatewayListProvider
    {

        //public RedisGatewayListProvider(ILogger<RedisGatewayListProvider> logger, IOptions<RedisClusteringOptions> membershipTableOptions, IOptions<ClusterOptions> clusterOptions)
        //{
        //    this.logger = logger;
        //    this.options = membershipTableOptions.Value;
        //    ConfigurationOptions configOptions = GetRedisConfiguration();



        //    clusterId = clusterOptions.Value.ClusterId;
        //    connection = ConnectionMultiplexer.Connect(configOptions);
        //    database = connection.GetDatabase();
        //    serializer = new BinarySerializer();
        //}

        public RedisGatewayListProvider(ILogger<RedisGatewayListProvider> logger, IOptions<RedisClusteringOptions> membershipTableOptions, IOptions<ClusterOptions> clusterOptions)
        {
            this.logger = logger;
            this.options = membershipTableOptions.Value;
            ConfigurationOptions configOptions = GetRedisConfiguration();

           

            clusterId = clusterOptions.Value.ClusterId;
            connection = ConnectionMultiplexer.Connect(configOptions);
            database = connection.GetDatabase();
            serializer = new BinarySerializer();
        }


        private readonly ILogger<RedisGatewayListProvider> logger;
        private readonly TimeSpan maxStaleness = TimeSpan.FromMinutes(1.0);
        private readonly ConnectionMultiplexer connection;
        private readonly IDatabase database;
        private readonly BinarySerializer serializer;
        private readonly string clusterId;
        private readonly RedisClusteringOptions options;

        public TimeSpan MaxStaleness
        {
            get { return this.maxStaleness; }
        }

        public Boolean IsUpdatable
        {
            get { return true; }
        }

        public Task<IList<Uri>> GetGateways()
        {
            if(database.KeyExists(clusterId))
            {
                var val = database.StringGet(clusterId);
                RedisMembershipCollection collection = serializer.Deserialize<RedisMembershipCollection>(val);
                try
                {
                    return Task.FromResult<IList<Uri>>(collection.Where(x => x.Status == SiloStatus.Active && x.ProxyPort != 0)
                        .Select(y =>
                        {
                            var endpoint = new IPEndPoint(y.Address.Endpoint.Address, y.ProxyPort);
                            var gatewayAddress = SiloAddress.New(endpoint, y.Address.Generation);
                            return gatewayAddress.ToGatewayUri();
                        }).ToList());
                }
                catch(Exception ex)
                {
                    
                    return Task.FromResult<IList<Uri>>(null);
                }
            }
            else
            {
                return Task.FromResult<IList<Uri>>(null);
            }
        }

        public Task InitializeGatewayListProvider()
        {
            return Task.CompletedTask;
        }

        private ConfigurationOptions GetRedisConfiguration()
        {
            ConfigurationOptions configOptions = null;

            if (!String.IsNullOrEmpty(options.ConnectionString))
            {
                configOptions = ConfigurationOptions.Parse(options.ConnectionString);
                if(options.DatabaseNo == null)
                {
                    configOptions.DefaultDatabase = 2;
                }
            }
            else
            {
                configOptions = new ConfigurationOptions()
                {
                    ConnectRetry = options.ConnectRetry ?? 4,
                    DefaultDatabase = options.DatabaseNo ?? 2,
                    SyncTimeout = options.SyncTimeout ?? 10000,
                    ResponseTimeout = options.ResponseTimeout ?? 10000,
                    EndPoints =    {
                                    { options.Hostname, 6380 }
                               },
                    Password = options.Password
                };
            }

            if (options.IsLocalDocker)  //redis instance on same host as clients
            {
                IPAddress address = GetIPAddress(configOptions.EndPoints[0]);
                EndPoint endpoint = configOptions.EndPoints[0];
                configOptions.EndPoints.Remove(endpoint);
                configOptions.EndPoints.Add(new IPEndPoint(address, 6379));
            }

            return configOptions;
        }


        private IPAddress GetIPAddress(string hostname)
        {
            IPHostEntry hostInfo = Dns.GetHostEntry(hostname);
            for (int index = 0; index < hostInfo.AddressList.Length; index++)
            {
                if (hostInfo.AddressList[index].AddressFamily == AddressFamily.InterNetwork)
                {
                    return hostInfo.AddressList[index];
                }
            }

            return null;
        }


        private IPAddress GetIPAddress(EndPoint endpoint)
        {
            DnsEndPoint dnsEndpoint = endpoint as DnsEndPoint;
            if (dnsEndpoint != null)
            {
                return GetIPAddress(dnsEndpoint.Host);
            }

            IPEndPoint ipEndpoint = endpoint as IPEndPoint;
            if (ipEndpoint != null)
            {
                return ipEndpoint.Address;
            }

            return null;
        }
    }
}
