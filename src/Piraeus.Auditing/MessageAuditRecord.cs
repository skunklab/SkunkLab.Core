using Newtonsoft.Json;
using System;

namespace Piraeus.Auditing
{
    [Serializable]
    [JsonObject]
    public class MessageAuditRecord : AuditRecord
    {
        public MessageAuditRecord()
        {
        }
        public MessageAuditRecord(string messageId, string identity, string channel, string protocol, int length, MessageDirectionType direction, bool success, DateTime messageTime, string error = null)
        {
            MessageId = messageId;
            Identity = identity;
            Channel = channel;
            Protocol = protocol;
            Length = length;
            Direction = direction.ToString();
            Error = error;
            Success = success;
            MessageTime = messageTime;
            Key = Guid.NewGuid().ToString();
        }


        [JsonProperty("key")]
        public string Key
        {
            get
            {
                return PartitionKey;
            }
            set
            {
                PartitionKey = value;
            }

        }

        [JsonProperty("messageId")]
        public string MessageId
        {
            get { return RowKey; }
            set
            {
                RowKey = value;
            }
        }

        [JsonProperty("identity")]
        public string Identity { get; set; }

        [JsonProperty("direction")]
        public string Direction { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("messageTimestamp")]
        public DateTime MessageTime { get; set; }

        public override string ConvertToCsv()
        {
            return String.Format($"{Key},{MessageId},{Identity},{Direction},{Channel},{Protocol},{Length},{Error},{Success},{Error},{MessageTime}");
        }

        public override string ConvertToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
