using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Runtime;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Binary;
using System.Linq;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace Orleans.Clustering.Redis
{
    public class RedisMembershipTable : IMembershipTable
    {
        private readonly ConnectionMultiplexer connection;
        private readonly IDatabase database;
        private readonly BinarySerializer serializer;
        private readonly string clusterId;
        public static readonly TableVersion _tableVersion = new TableVersion(0, "0");
        private readonly ILogger<RedisMembershipTable> logger;

        public RedisMembershipTable(ILogger<RedisMembershipTable> logger, IOptions<RedisClusteringOptions> membershipTableOptions, IOptions<ClusterOptions> clusterOptions)
        {
            
            this.logger = logger;
            ConfigurationOptions configOptions = null;

            if (!String.IsNullOrEmpty(membershipTableOptions.Value.ConnectionString))
            {
                configOptions = ConfigurationOptions.Parse(membershipTableOptions.Value.ConnectionString);
            }
            else
            {
                configOptions = new ConfigurationOptions()
                {
                    ConnectRetry = membershipTableOptions.Value.ConnectRetry ?? 4,
                    DefaultDatabase = membershipTableOptions.Value.DatabaseNo ?? 2,
                    SyncTimeout = membershipTableOptions.Value.SyncTimeout ?? 10000,
                    ResponseTimeout = membershipTableOptions.Value.ResponseTimeout ?? 10000,
                    EndPoints =    {
                                            { membershipTableOptions.Value.Hostname, 6380 }
                                       },
                    Password = membershipTableOptions.Value.Password
                };
            }

            if(membershipTableOptions.Value.IsLocalDocker)
            {
                IPAddress address = GetIPAddress(configOptions.EndPoints[0]);
                EndPoint endpoint = configOptions.EndPoints[0];
                configOptions.EndPoints.Remove(endpoint);
                configOptions.EndPoints.Add(new IPEndPoint(address, 6380));
            }

            configOptions.DefaultDatabase = 2;
            connection = ConnectionMultiplexer.ConnectAsync(configOptions).GetAwaiter().GetResult();
            database = connection.GetDatabase();

            clusterId = clusterOptions.Value.ClusterId;
            database.KeyDelete(clusterId);

            serializer = new BinarySerializer();

            logger?.LogInformation("It worked !!! :-)");
        }
        public async Task DeleteMembershipTableEntries(string clusterId)
        {
            try
            {
                await database.KeyDeleteAsync(clusterId);
            }
            catch(Exception ex)
            {
                logger?.LogError(ex, "Redis membership table key '{0}' failed delete table entries.", clusterId);
                throw ex;
            }
        }

        public Task InitializeMembershipTable(bool tryInitTableVersion)
        {
            return Task.CompletedTask;
        }

        public async Task<bool> InsertRow(MembershipEntry entry, TableVersion tableVersion)
        {
            try
            {
                RedisMembershipEntry rentry = RedisMembershipEntry.Create(clusterId, entry, "0");
                RedisMembershipCollection collection = null;

                var val = await database.StringGetAsync(clusterId);

                if (!val.IsNull)
                {
                    collection = serializer.Deserialize<RedisMembershipCollection>(val);
                    collection.Add(rentry);
                }
                else
                {
                    collection = new RedisMembershipCollection();
                    collection.Add(rentry);
                }

                bool ret = await database.StringSetAsync(clusterId, serializer.Serialize(collection));

                return ret;
            }
            catch(Exception ex)
            {
                logger?.LogError(ex, "Redis membership table key '{0}' failed insert row.", clusterId);
                throw ex;
            }
        }

        public async Task<MembershipTableData> ReadAll()
        {
            logger?.LogInformation("It worked !!! :-)");
            try
            {
                MembershipTableData data = null;
                var val = await database.StringGetAsync(clusterId);

                if (!val.IsNull)
                {
                    RedisMembershipCollection collection = serializer.Deserialize<RedisMembershipCollection>(val);
                    data = collection.ToMembershipTableData();
                }
                else
                {
                    data = new MembershipTableData(_tableVersion);
                }
                return data;
            }
            catch(Exception ex)
            {
                logger?.LogError(ex, "Redis membership table key '{0}' failed read all.", clusterId);
                throw ex;
            }
        }

        public async Task<MembershipTableData> ReadRow(SiloAddress key)
        {
            try
            {
                MembershipTableData data = null;
                var val = await database.StringGetAsync(clusterId);

                if (!val.IsNull)
                {
                    RedisMembershipCollection collection = serializer.Deserialize<RedisMembershipCollection>(val);
                    data = collection.ToMembershipTableData();
                }
                else
                {
                    data = new MembershipTableData(_tableVersion);
                }

                return data;
            }
            catch(Exception ex)
            {
                logger?.LogError(ex, "Redis membership table key '{0}' failed read row.", clusterId);
                throw ex;
            }
        }

        public async Task UpdateIAmAlive(MembershipEntry entry)
        {
            try
            {
                var val = await database.StringGetAsync(clusterId);

                if (!val.IsNull)
                {
                    RedisMembershipCollection collection = serializer.Deserialize<RedisMembershipCollection>(val);
                    if (collection.UpdateIAmAlive(clusterId, entry.SiloAddress, entry.IAmAliveTime))
                    {
                        var collVal = serializer.Serialize(collection);
                        await database.StringSetAsync(clusterId, collVal);
                    }
                }
            }
            catch(Exception ex)
            {
                logger?.LogError(ex, "Redis membership table key '{0}' failed update i-am-alive.", clusterId);
                throw ex;
            }

            
        }

        public async Task<bool> UpdateRow(MembershipEntry entry, string etag, TableVersion tableVersion)
        {
            try
            {
                bool ret = false;

                if (string.IsNullOrEmpty(etag))
                {
                    etag = "0";
                }


                var rentry = RedisMembershipEntry.Create(clusterId, entry, etag);
                var val = await database.StringGetAsync(clusterId);

                if (!val.IsNull)
                {
                    RedisMembershipCollection collection = serializer.Deserialize<RedisMembershipCollection>(val);
                    var items = collection.Where((x) => x.DeploymentId == clusterId && x.Address.ToParsableString() == rentry.Address.ToParsableString());
                    if (items != null && items.Count() > 0)
                    {
                        RedisMembershipEntry oldEntry = items.First();
                        rentry.LastIndex = oldEntry.LastIndex++;
                        collection.Remove(oldEntry);
                        collection.Add(rentry);
                        ret = await database.StringSetAsync(clusterId, serializer.Serialize(collection));
                    }
                }
                return ret;
            }
            catch(Exception ex)
            {
                logger?.LogError(ex, "Redis membership table key '{0}' failed update row.", clusterId);
                throw ex;
            }
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
            if(dnsEndpoint != null)
            {
                return GetIPAddress(dnsEndpoint.Host);               
            }

            IPEndPoint ipEndpoint = endpoint as IPEndPoint;
            if(ipEndpoint != null)
            {
                return ipEndpoint.Address;
            }

            return null;
        }

        public async Task CleanupDefunctSiloEntries(DateTimeOffset beforeDate)
        {
            await Task.CompletedTask;
            //throw new NotImplementedException();
        }
    }
}
