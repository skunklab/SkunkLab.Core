using StackExchange.Redis;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SkunkLab.Storage
{
    public class RedisPskStorage : PskStorageAdapter
    {

        public static RedisPskStorage CreateSingleton(string connectionString)
        {
            if (instance == null)
            {
                instance = new RedisPskStorage(connectionString);
            }

            return instance;
        }

        private static RedisPskStorage instance;
        private ConnectionMultiplexer connection;
        private IDatabase database;
        private int? id;

        protected RedisPskStorage(string connectionString)
        {
            ConfigurationOptions configOptions = ConfigurationOptions.Parse(connectionString);
            id = configOptions.DefaultDatabase;
            connection = ConnectionMultiplexer.ConnectAsync(configOptions).GetAwaiter().GetResult();
            database = connection.GetDatabase();
        }

        public override async Task SetSecretAsync(string key, string value)
        {
            await database.StringSetAsync(key, value);
        }

        public override async Task<string> GetSecretAsync(string key)
        {
            return await database.StringGetAsync(key);
        }

        public override async Task RemoveSecretAsync(string key)
        {
            await database.KeyDeleteAsync(key);
        }

        public override async Task<string[]> GetKeys()
        {
            EndPoint[] endpoints = connection.GetEndPoints();
            if (endpoints != null && endpoints.Length > 0)
            {
                var server = connection.GetServer(endpoints[0]);
                int dbNum = id.HasValue ? id.Value : 0;
                var keys = server.Keys(dbNum);
                List<string> list = new List<string>();
                foreach (var key in keys)
                {
                    list.Add(key.ToString());
                }

                return await Task.FromResult<string[]>(list.ToArray());
            }

            return null;
        }




    }
}
