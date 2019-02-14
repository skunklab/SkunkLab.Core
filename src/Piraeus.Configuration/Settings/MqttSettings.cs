using Newtonsoft.Json;
using System;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class MqttSettings
    {
        public MqttSettings()
        {

        }

        [JsonProperty("keepAliveSeconds")]
        public double KeepAliveSeconds { get; set; } = 180.0;

        [JsonProperty("ackTimeoutSeconds")]
        public double AckTimeoutSeconds { get; set; } = 2.0;

        [JsonProperty("ackRandomFactor")]
        public double AckRandomFactor { get; set; } = 1.5;

        [JsonProperty("maxRetransmit")]
        public int MaxRetransmit { get; set; } = 4;

        [JsonProperty("maxLatencySeconds")]
        public double MaxLatencySeconds { get; set; } = 100.0;
    }
}
