using Org.BouncyCastle.Crypto.Tls;

namespace SkunkLab.Channels.Psk
{
    public abstract class TlsPskIdentityManagerFactory
    {
        public static TlsPskIdentityManager Create(string keys, string values)
        {
            return new EnvironmentVariableTlsPskIdentityManager(keys, values);
        }

        public static TlsPskIdentityManager Create(string connectionString)
        {
            return new RedisTlsPskIdentityManager(connectionString);
        }

        public static TlsPskIdentityManager Create(string authority, string clientId, string clientSecret)
        {
            return new KeyVaultTlsPskIdentityManager(authority, clientId, clientSecret);
        }
    }
}
