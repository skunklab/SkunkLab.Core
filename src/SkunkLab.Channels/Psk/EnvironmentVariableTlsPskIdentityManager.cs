using Org.BouncyCastle.Crypto.Tls;
using SkunkLab.Storage;
using System;
using System.Text;

namespace SkunkLab.Channels.Psk
{
    public class EnvironmentVariableTlsPskIdentityManager : TlsPskIdentityManager
    {
        public EnvironmentVariableTlsPskIdentityManager(string keys, string values)
        {
            storage = EnvironmentVariablePskStorage.CreateSingleton(keys, values);
        }

        private EnvironmentVariablePskStorage storage;

        public byte[] GetHint()
        {
            return null;
        }

        public byte[] GetPsk(byte[] identity)
        {
            string key = Encoding.UTF8.GetString(identity);
            string value = storage.GetSecretAsync(key).GetAwaiter().GetResult();

            return Convert.FromBase64String(value);
        }
    }
}
