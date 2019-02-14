using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Piraeus.Configuration.Core
{

    [Serializable]
    [JsonObject]
    public class OrleansConfig
    {
        public OrleansConfig()
        {
        }

        [JsonProperty("dockerized")]
        public bool Dockerized { get; set; }

        [JsonProperty("clusterId")]
        public string ClusterId { get; set; } //orleans cluster id

        [JsonProperty("serviceId")]
        public string ServiceId { get; set; } //orleans service id

        [JsonProperty("dataConnectionString")]
        public string DataConnectionString { get; set; } 

        [JsonProperty("servicePointFactor")]
        public int ServicePointFactor { get; set; } = 24; //service point factor, e.g., 24 associated with Azure storage

        [JsonProperty("loggerTypes")]
        public string LoggerTypes { get; set; } = "Console;Debug";

        [JsonProperty("logLevel")]
        public string LogLevel { get; set; } = "Warning";

        [JsonProperty("appInsightsKey")]
        public string AppInsightsKey { get; set; }                      

        public LoggerType GetLoggerTypes()
        {
            if(string.IsNullOrEmpty(LoggerTypes))
            {
                return default(LoggerType);
            }

            string loggerTypes = LoggerTypes.Replace(";", ",");
            return Enum.Parse<LoggerType>(loggerTypes, true);
        }
    }

    [Flags]
    public enum LoggerType
    {
        None = 0,
        Console = 1,
        Debug = 2,
        AppInsights = 4,
        File = 8
    }
}
