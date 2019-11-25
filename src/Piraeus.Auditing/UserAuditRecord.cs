using Newtonsoft.Json;
using System;

namespace Piraeus.Auditing
{
    [Serializable]
    [JsonObject]
    public class UserAuditRecord : AuditRecord
    {
        public UserAuditRecord()
        {
        }

        public UserAuditRecord(string channelId, string identity, string claimType, string channel, string protocol, string status, DateTime loginTime)
        {
            ChannelId = channelId;
            Identity = identity;
            ClaimType = claimType;
            Channel = channel;
            Protocol = protocol;
            Status = status;
            LoginTime = loginTime;
        }

        public UserAuditRecord(string channelId, string identity, DateTime logoutTime)
        {
            ChannelId = channelId;
            Identity = identity;
            LogoutTime = logoutTime;
        }

        [JsonProperty("channelId")]
        public string ChannelId
        {
            get { return PartitionKey; }
            set { PartitionKey = value; }
        }

        [JsonProperty("identity")]
        public string Identity
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("claimType")]
        public string ClaimType { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("loginTime")]
        public DateTime? LoginTime { get; set; }

        [JsonProperty("logoutTime")]
        public DateTime? LogoutTime { get; set; }


        public override string ConvertToCsv()
        {
            return String.Format($"{ChannelId},{Identity},{Channel},{Protocol},{ClaimType},{Status},{LoginTime},{LogoutTime}");
        }

        public override string ConvertToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
