using Newtonsoft.Json;
using System;

namespace Piraeus.Configuration.Settings
{

    [Serializable]
    [JsonObject]
    public class SecuritySettings
    {
        public SecuritySettings()
        {
            WebApi = new ManagementApi();
        }

        //public SecuritySettings(ClientSecurity client, ServiceSecurity service = null, ManagementApi api = null)
        //{
        //    Client = client;
        //    Service = service;
        //    WebApi = api;
        //}
        [JsonProperty("webApi")]
        public ManagementApi WebApi { get; set; }

        [JsonProperty("client")]
        public ClientSecurity Client { get; set; }

        [JsonProperty("service")]
        public ServiceSecurity Service { get; set; }

    }
}
