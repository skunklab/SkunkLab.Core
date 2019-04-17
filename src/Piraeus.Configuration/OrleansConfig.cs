using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Piraeus.Configuration
{

    [Serializable]
    [JsonObject]
    public class OrleansConfig
    {
        public OrleansConfig()
        {
        }

        [JsonProperty("dockerized")]
        public bool Dockerized { get; set; }  //true for docker deployments; otherwise local deployment

        [JsonProperty("clusterId")]
        public string ClusterId { get; set; } //orleans cluster id

        [JsonProperty("serviceId")]
        public string ServiceId { get; set; } //orleans service id

        [JsonProperty("dataConnectionString")]
        public string DataConnectionString { get; set; } //either azure storage connection string or redis connection string

        [JsonProperty("servicePointFactor")]
        public int ServicePointFactor { get; set; } = 24; //service point factor, e.g., 24 associated with Azure storage

        [JsonProperty("loggerTypes")]
        public string LoggerTypes { get; set; } = "Console;Debug"; //any of console, debug, file, appinsights, or none.

        [JsonProperty("logLevel")]
        public string LogLevel { get; set; } = "Warning";  //one of warning, error, information, critical, verbose

        [JsonProperty("appInsightsKey")]
        public string AppInsightsKey { get; set; }         //required when loggertypes as appinsights; otherwise omit             

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
