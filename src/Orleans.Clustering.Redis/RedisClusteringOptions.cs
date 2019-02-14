using System;
using System.Collections.Generic;
using System.Text;

namespace Orleans.Clustering.Redis
{
    [Serializable]
    public class RedisClusteringOptions
    {
        public RedisClusteringOptions()
        {

        }

        public string ConnectionString { get; set; }

        public int? ConnectRetry { get; set; }

        public int? DatabaseNo { get; set; }

        public int? SyncTimeout { get; set; }

        public int? ResponseTimeout { get; set; }

        public string Password { get; set; }

        public string Hostname { get; set; }

        public bool IsLocalDocker { get; set; } = false;
    }
}
