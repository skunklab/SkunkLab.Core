namespace SkunkLab.Storage
{
    public abstract class PskStorageAdapterFactory
    {
        public static PskStorageAdapter Create(string keys, string values)
        {
            return EnvironmentVariablePskStorage.CreateSingleton(keys, values);
        }

        public static PskStorageAdapter Create(string connectionString)
        {
            return RedisPskStorage.CreateSingleton(connectionString);
        }

        public static KeyVaultPskStorage Create(string authority, string clientId, string clientSecret)
        {
            return KeyVaultPskStorage.CreateSingleton(authority, clientId, clientSecret);
        }


    }
}
