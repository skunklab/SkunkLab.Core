using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class TcpSettings
    {
        public TcpSettings()
        {

        }
        public TcpSettings(bool useLengthPrefix, int blockSize, int maxBufferSize, int[] ports, string hostname, bool authenticate = false, 
                            string certificateFilename = null, string certificatePassword = null, Dictionary<string, byte[]> presharedKeys = null)
        {
            
            UseLengthPrefix = useLengthPrefix;
            BlockSize = blockSize;
            MaxBufferSize = maxBufferSize;
            Ports = ports;
            Hostname = hostname;
            Authenticate = authenticate;
            Ports = ports;
            X509Filename = certificateFilename;
            X509Password = certificatePassword;
            psks =  presharedKeys;
            
        }

        private Dictionary<string, byte[]> psks;
        private Dictionary<string, string> propertyPsks;

        [JsonProperty("useLengthPrefix")]
        public bool UseLengthPrefix { get; set; } = false;

        /// <summary>
        /// Authenticates a certificate used.
        /// </summary>
        [JsonProperty("authenticate")]
        public bool Authenticate { get; set; } = false;

        [JsonProperty("blockSize")]
        public int BlockSize { get; set; } = 16384;

        [JsonProperty("maxBufferSize")]
        public int MaxBufferSize { get; set; } = 1000 * 16384;

        [JsonProperty("ports")]
        public int[] Ports { get; set; } = new int[] { 1883, 8883, 5684 };

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        /// <summary>
        /// Certificate filename, i.e., folder/subfolder/filename.pfx
        /// </summary>
        [JsonProperty("x509Filename")]
        public string X509Filename { get; set; }

        /// <summary>
        /// Certificate password
        /// </summary>
        [JsonProperty("x509Password")]
        public string X509Password { get; set; }

        [JsonProperty("x509Store")]
        public string X509Store { get; set; }

        [JsonProperty("x509Location")]
        public string X509Location { get; set; }

        [JsonProperty("x509Thumbprint")]
        public string X509Thumbprint { get; set; }

        [JsonProperty("presharedKeys")]
        public Dictionary<string, string> PresharedKeys 
        {
            get { return propertyPsks; }
            set
            {
                if(value != null)
                {
                    psks = new Dictionary<string, byte[]>();
                    Dictionary<string, string>.Enumerator en = value.GetEnumerator();
                    while(en.MoveNext())
                    {
                        psks.Add(en.Current.Key, Convert.FromBase64String(en.Current.Value));
                    }
                }

                propertyPsks = value;
            }
        }

        public Dictionary<string, byte[]> GetPskClone()
        {            
            return DeepClone<Dictionary<string, byte[]>>(psks);
        }

        public X509Certificate2 GetCertificate()
        {
            if (HasCertificate())
            {
                if (string.IsNullOrEmpty(X509Password))
                {
                    return new X509Certificate2(X509Filename);
                }
                else
                {
                    return new X509Certificate2(X509Filename, X509Password);
                }
            }
            else
            {
                return null;
            }
        }

        public bool HasCertificate()
        {
            return !string.IsNullOrEmpty(X509Filename);
        }

        private T DeepClone<T>(T obj)
        {
            if(obj == null)
            {
                return default(T);
            }

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

    }
}
