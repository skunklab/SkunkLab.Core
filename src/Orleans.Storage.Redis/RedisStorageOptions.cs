namespace Orleans.Storage.Redis
{
    public class RedisStorageOptions
    {
        public RedisStorageOptions()
        {
        }

        public const int DEFAULT_INIT_STAGE = ServiceLifecycleStage.ApplicationServices;
        public int InitStage { get; set; } = DEFAULT_INIT_STAGE;

        public SerializerType Serializer { get; set; }

        public int? ConnectRetry { get; set; }

        public int? DatabaseNo { get; set; }

        public int? SyncTimeout { get; set; }

        public int? ResponseTimeout { get; set; }

        public string Password { get; set; }

        public string Hostname { get; set; }

        public string ConnectionString { get; set; }

        public bool IsLocalDocker { get; set; } = false;
    }

    public enum SerializerType
    {
        BinaryFormatter,
        Json
    }
}
