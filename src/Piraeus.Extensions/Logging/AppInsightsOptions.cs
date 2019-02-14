using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Piraeus.Extensions.Logging
{
    [Serializable]
    public class AppInsightsOptions
    {
        public bool DeveloperMode { get; set; } = false;
        public string InstrumentationKey { get; set; }
        public LogLevel LoggingLevel { get; set; }

    }
}
