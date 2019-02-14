namespace SkunkLab.Storage
{
    public abstract class PskStorageAdapterFactory
    {
        public PskStorageAdapter Create(string keys, string values)
        {
            return EnvironmentVariablePskStorage.CreateSingleton(keys, values);
        }

        public PskStorageAdapter Create(string connectionString)
        {
            return RedisPskStorage.CreateSingleton(connectionString);
        }

        public KeyVaultPskStorage Create(string authority, string clientId, string clientSecret)
        {
            return KeyVaultPskStorage.CreateSingleton(authority, clientId, clientSecret);
        }


    }
}
