//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging.ApplicationInsights;
//using System;

//namespace Piraeus.Extensions.Logging
//{
//    public static class AppInsightsExtensions
//    {

//        public static ILoggerFactory AddAppInsights(
//        this ILoggerFactory factory,
//        Func<string, LogLevel, bool> filter,
//        AppInsightsOptions options)
//        {
//            factory.AddProvider(new AppInsightsLogProvider(filter, options));
//            return factory;
//        }

//        public static ILoggerFactory AddAppInsights(
//            this ILoggerFactory factory,
//            AppInsightsOptions options)
//        {
//            factory.AddProvider(new AppInsightsLogProvider(null, options));

//            return factory;
//        }
//    }
//}
