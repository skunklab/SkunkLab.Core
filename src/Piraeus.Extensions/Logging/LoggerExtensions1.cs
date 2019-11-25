//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Orleans;
//using Orleans.Configuration;
//using Orleans.Hosting;
//using Piraeus.Configuration;
//using Piraeus.Extensions.Options;
//using System;
//using LoggerType = Piraeus.Configuration.LoggerType;

//namespace Piraeus.Extensions.Logging
//{
//    public static class LoggerExtensions
//    {
//        public static IClientBuilder AddLoggers(this IClientBuilder builder, Action<PiraeusGatewayOptions> configureOptions)
//        {
//            return builder.ConfigureServices(services => services.AddLoggers(configureOptions));
//        }

//        public static IServiceCollection AddLoggers(this IServiceCollection services,
//            Action<PiraeusGatewayOptions> configureOptions)
//        {
//            return services.AddLoggers(ob => ob.Configure(configureOptions));
//        }

//        public static IServiceCollection AddLoggers(this IServiceCollection services,
//            Action<OptionsBuilder<PiraeusGatewayOptions>> configureOptions)
//        {
//            configureOptions?.Invoke(services.AddOptions<PiraeusGatewayOptions>());
//            return services.AddSingleton<IGatewayListProvider, RedisGatewayListProvider>();
//        }
//        internal static IClientBuilder AddLoggers(this IClientBuilder clientBuilder, PiraeusGatewayOptions options)
//        {
//            clientBuilder.ConfigureLogging(builder =>
//            {
//                if (options.LoggerTypes.HasFlag(LoggerType.Console) && options.LoggingLevel != LogLevel.None)
//                    builder.AddConsole();

//                if (options.LoggerTypes.HasFlag(LoggerType.Debug) && options.LoggingLevel != LogLevel.None)
//                    builder.AddDebug();

//                if (options.LoggingLevel != LogLevel.None)
//                    builder.SetMinimumLevel(options.LoggingLevel);

//            });

//            if (options.LoggingLevel.HasFlag(LoggerType.AppInsights) && options.LoggingLevel != LogLevel.None && !string.IsNullOrEmpty(options.AppInsightKey))
//                clientBuilder.AddApplicationInsightsTelemetryConsumer(options.AppInsightKey);

//            return clientBuilder;
//        }

//        public static ISiloHostBuilder AddLoggers(this ISiloHostBuilder siloBuilder, OrleansConfig config)
//        {
//            LoggerType loggerType = config.GetLoggerTypes();

//            siloBuilder.ConfigureLogging(builder =>
//            {
//                if (loggerType.HasFlag(LoggerType.Console))
//                    builder.AddConsole();

//                if (loggerType.HasFlag(LoggerType.Debug))
//                    builder.AddDebug();

//                builder.SetMinimumLevel(Enum.Parse<LogLevel>(config.LogLevel));
//            });

//            if (loggerType.HasFlag(LoggerType.AppInsights))
//                siloBuilder.AddApplicationInsightsTelemetryConsumer(config.AppInsightsKey);

//            return siloBuilder;
//        }



//    }
//}
