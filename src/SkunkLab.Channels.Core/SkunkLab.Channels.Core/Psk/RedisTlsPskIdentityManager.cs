using Org.BouncyCastle.Crypto.Tls;
using SkunkLab.Storage;
using System;
using System.Text;

namespace SkunkLab.Channels.Psk
{
    public class RedisTlsPskIdentityManager : TlsPskIdentityManager
    {
        public RedisTlsPskIdentityManager(string connectionString)
        {
            storage = RedisPskStorage.CreateSingleton(connectionString);
        }

        private RedisPskStorage storage;

        public byte[] GetHint()
        {
            return null;
        }

        public byte[] GetPsk(byte[] identity)
        {
            string key = Encoding.UTF8.GetString(identity);
            string value = storage.GetSecretAsync(key).GetAwaiter().GetResult();
            byte[] psk = Convert.FromBase64String(value);

            return psk;
        }
    }
}
