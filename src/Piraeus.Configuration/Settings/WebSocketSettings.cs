using Newtonsoft.Json;
using System;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class WebSocketSettings
    {
        public WebSocketSettings()
        {
        }       

        [JsonProperty("maxIncomingMessageSize")]
        public int MaxIncomingMessageSize { get; set; } = 0x400000;

        [JsonProperty("receiveLoopBufferSize")]
        public int ReceiveLoopBufferSize { get; set; } = 0x2000;

        [JsonProperty("sendBufferSize")]
        public int SendBufferSize { get; set; } = 0x2000;

        [JsonProperty("closeTimeoutMilliseconds")]
        public double CloseTimeoutMilliseconds { get; set; } = 250.0;

    }
}
