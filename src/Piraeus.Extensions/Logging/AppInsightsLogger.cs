//using System;
//using System.Collections.Generic;
//using System.Text;
//using Microsoft.ApplicationInsights;
//using Microsoft.ApplicationInsights.DataContracts;
//using Microsoft.ApplicationInsights.Extensibility;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.DependencyInjection.Extensions;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;

//namespace Piraeus.Extensions.Logging
//{
//    public class AppInsightsLogger : ILogger
//    {
//        private readonly TelemetryClient client;
//        private readonly string name;
//        private readonly Func<string, LogLevel, bool> filter;
//        private readonly LogLevel _logLevel;

//        public AppInsightsLogger(AppInsightsOptions configOptions)
//            : this(null, null, configOptions)
//        {

//        }

//        public AppInsightsLogger(string name, AppInsightsOptions configOptions)
//            : this(name, null, configOptions)
//        {

//        }

//        public AppInsightsLogger(Func<string, LogLevel, bool> filter, AppInsightsOptions configOptions)
//            : this(null, filter, configOptions)
//        {

//        }

//        public AppInsightsLogger(string name, Func<string, LogLevel, bool> filter, AppInsightsOptions configOptions)
//        {
//            this.name = string.IsNullOrEmpty(name) ? "AppInsightsLogger" : name;
//            this.filter = filter == null ? null : filter;
//            _logLevel = configOptions.LoggingLevel;

//            TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = configOptions.DeveloperMode;
//            TelemetryConfiguration.Active.InstrumentationKey = configOptions.InstrumentationKey;

//            client = new TelemetryClient();            
//            client.InstrumentationKey = configOptions.InstrumentationKey;           
//        }

//        public IDisposable BeginScope<TState>(TState state)
//        {
//            return FooDisposable.Instance;
//        }

//        public bool IsEnabled(LogLevel logLevel)
//        {
//            if(filter == null)
//            {
//                return (int)logLevel >= (int)_logLevel;
//            }

//            return filter(name, logLevel);
//        }

//        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
//        {
//            if (exception != null)
//            {
//                client.TrackException(new ExceptionTelemetry(exception));
//                return;
//            }

//            var message = string.Empty;
//            if (formatter != null)
//            {
//                message = formatter(state, exception);
//            }
//            else
//            {
//                if (state != null)
//                {
//                    message += state;
//                }
//            }
//            if (!string.IsNullOrEmpty(message))
//            {
//                client.TrackTrace(message, GetSeverityLevel(logLevel));
//            }
//        }

//        public void Flush()
//        {
//            if(client != null)
//            {
//                client.Flush();
//            }
//        }


//        private static SeverityLevel GetSeverityLevel(LogLevel logLevel)
//        {
//            switch (logLevel)
//            {
//                case LogLevel.Critical: return SeverityLevel.Critical;
//                case LogLevel.Error: return SeverityLevel.Error;
//                case LogLevel.Warning: return SeverityLevel.Warning;
//                case LogLevel.Information: return SeverityLevel.Information;
//                case LogLevel.Trace:
//                default: return SeverityLevel.Verbose;
//            }
//        }

//        private class FooDisposable : IDisposable
//        {
//            public static FooDisposable Instance = new FooDisposable();

//            public void Dispose()
//            {
//                GC.SuppressFinalize(this);
//            }
//        }
//    }
//}
