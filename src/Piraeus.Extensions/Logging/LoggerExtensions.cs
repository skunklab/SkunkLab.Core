using Microsoft.Extensions.Logging;
using Piraeus.Configuration;
using System;

namespace Piraeus.Extensions.Logging
{
    public static class LoggerExtensions
    {
        public static ILoggingBuilder AddLogging(this ILoggingBuilder builder, PiraeusConfig config)
        {
            LoggerType loggerTypes = config.GetLoggerTypes();

            //if (loggerTypes.HasFlag(LoggerType.None))
            //{
            //    return builder;
            //}

            LogLevel logLevel = Enum.Parse<LogLevel>(config.LogLevel, true);


            if (loggerTypes.HasFlag(LoggerType.Console))
            {
                builder.AddConsole();
            }

            if (loggerTypes.HasFlag(LoggerType.Debug))
            {
                builder.AddDebug();
            }



            builder.SetMinimumLevel(logLevel);

            return builder;
        }
    }
}
