using Microsoft.Extensions.Logging;
using Piraeus.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Piraeus.Extensions.Options
{
    public class PiraeusGatewayOptions
    {
        public PiraeusGatewayOptions()
        {
        }

        public PiraeusGatewayOptions(OrleansConfig config)
        {
            Dockerized = config.Dockerized;
            ClusterId = config.ClusterId;
            ServiceId = config.ServiceId;
            DataConnectionString = config.DataConnectionString;
            AppInsightKey = config.AppInsightsKey;
            LoggerTypes = config.GetLoggerTypes();
            LoggingLevel = Enum.Parse<LogLevel>(config.LogLevel);
            SetStorageType();
        }

        public bool Dockerized { get; set; } = false;

        public string ClusterId { get; set; }

        public string ServiceId { get; set; }

        public string DataConnectionString { get; set; }

        public string AppInsightKey { get; set; }

        public LoggerType LoggerTypes { get; set; }

        public LogLevel LoggingLevel { get; set; }

        public OrleansStorageType StorageType { get; set; }

        private OrleansStorageType SetStorageType()
        {
            if(string.IsNullOrEmpty(DataConnectionString))
            {
                return default(OrleansStorageType);
            }

            string cs = DataConnectionString.ToLowerInvariant();
            if (cs.Contains(":6380") || cs.Contains(":6379"))
            {
                return OrleansStorageType.Redis;
            }
            else if (cs.Contains("defaultendpointsprotocol=") && cs.Contains("accountname=") && cs.Contains("accountkey="))
            {
                return OrleansStorageType.AzureStorage;
            }
            else
            {
                throw new ArgumentException("Invalid connection string");
            }

        }
    }

    public enum OrleansStorageType
    {
        Memory = 0,
        AzureStorage = 1,
        Redis = 2
    }
}
