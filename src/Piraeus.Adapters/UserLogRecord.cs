//using Microsoft.WindowsAzure.Storage.Table;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Piraeus.Adapters
//{
//    [Serializable]
//    [JsonObject]
//    public class UserLogRecord : TableEntity
//    {
//        public UserLogRecord()
//        {
//        }

//        public UserLogRecord(string channelId, string identity, string claimType, string channel, string protocol, string status, DateTime loginTime)
//        {
//            ChannelId = channelId;
//            Identity = identity;
//            ClaimType = claimType;
//            Channel = channel;
//            Protocol = protocol;
//            Status = status;
//            LoginTime = loginTime;
//        }

//        public UserLogRecord(DateTime logoutTime)
//        {
//            LogoutTime = logoutTime;
//        }

//        [JsonProperty("channelId")]
//        public string ChannelId
//        {
//            get { return PartitionKey; }
//            set { PartitionKey = value; }
//        }

//        [JsonProperty("identity")]
//        public string Identity
//        {
//            get { return RowKey; }
//            set { RowKey = value; }
//        }

//        [JsonProperty("channel")]
//        public string Channel { get; set; }

//        [JsonProperty("protocol")]
//        public string Protocol { get; set; }

//        [JsonProperty("claimType")]
//        public string ClaimType { get; set; }

//        [JsonProperty("status")]
//        public string Status { get; set; }

//        [JsonProperty("loginTime")]
//        public DateTime? LoginTime { get; set; }

//        [JsonProperty("logoutTime")]
//        public DateTime? LogoutTime { get; set; }

//        public string ConvertToCsv()
//        {
//            return String.Format($"{ChannelId},{Identity},{Channel},{Protocol},{ClaimType},{Status},{LoginTime},{LogoutTime}");
//        }
//        public string ConvertToJson()
//        {
//            return JsonConvert.SerializeObject(this);
//        }
//    }
//}
