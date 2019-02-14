using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class HttpSettings
    {
        public HttpSettings()
        {
        }

        [JsonProperty("maxConnections")]
        public int MaxConnections { get; set; } = 5000;

        [JsonProperty("maxUpgradedConnections")]
        public int MaxUpgradedConnections { get; set; } = 5000;


        [JsonProperty("maxRequestSize")]
        public int MaxRequestSize { get; set; } = 30000000;

        [JsonProperty("minRequestDataRate")]
        public int MinRequestDataRate { get; set; } = 240;

        [JsonProperty("minResponseDataRate")]
        public int MinResponseDataRate { get; set; } = 240;

        [JsonProperty("maxRequestBufferSize")]
        public int MaxRequestBufferSize { get; set; } = 1048576;

        [JsonProperty("maxResponseBufferSize")]
        public int MaxResponseBufferSize { get; set; } = 1048576;

        [JsonProperty("listenPort")]
        public int ListenPort { get; set; } = 80;

        [JsonProperty("useEncryptedChannel")]
        public bool UseEncryptedChannel { get; set; } = false;

        [JsonProperty("x509Filename")]
        public string X509Filename { get; set; }

        [JsonProperty("x509Password")]
        public string X509Password { get; set; }

        [JsonProperty("x509Store")]
        public string X509Store { get; set; }

        [JsonProperty("x509Location")]
        public string X509Location { get; set; }

        [JsonProperty("x509Thumbprint")]
        public string X509Thumbprint { get; set; }


    }
}
