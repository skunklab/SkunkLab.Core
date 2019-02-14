using Microsoft.Extensions.Logging;
using System;

namespace Piraeus.Extensions.Logging
{
    public class AppInsightsLogProvider : ILoggerProvider
    {
        public AppInsightsLogProvider(AppInsightsOptions configOptions)
            : this(null, configOptions)
        {

        }

        public AppInsightsLogProvider(Func<string, LogLevel, bool> filter, AppInsightsOptions configOptions)
        {
            this.filter = filter;
            options = configOptions;
        }

        private readonly Func<string, LogLevel, bool> filter;
        private readonly AppInsightsOptions options;
        private AppInsightsLogger logger;
        private bool disposed;

        public ILogger CreateLogger(string categoryName)
        {
            logger = new AppInsightsLogger(categoryName, filter, options);
            return logger;
        }

        protected void Disposing(bool disposing)
        {
            if(!disposed)
            {
                if(logger != null)
                {
                    logger.Flush();
                    logger = null;
                }
            }

            disposed = true;
        }

        public void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);

        }
    }
}
