using Newtonsoft.Json;
using System;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class CoapSettings : MqttSettings
    {
        public CoapSettings()
        {
        }

        public CoapSettings(string hostname)
        {
            HostName = hostname;
        }

        [JsonProperty("hostname")]
        public string HostName { get; set; }

        [JsonProperty("autoRetry")]
        public bool AutoRetry { get; set; } = false;

        [JsonProperty("observeOption")]
        public bool ObserveOption { get; set; } = true;

        [JsonProperty("noResponseOption")]
        public bool NoResponseOption { get; set; } = true;

        [JsonProperty("nstart")]
        public int NStart { get; set; } = 1;

        [JsonProperty("defaultLeisure")]
        public double DefaultLeisure { get; set; } = 4.0;

        [JsonProperty("probingRate")]
        public double ProbingRate { get; set; } = 1.0;
    }
}
