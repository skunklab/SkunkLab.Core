using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Piraeus.Configuration.Settings;

namespace Piraeus.WebGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static PiraeusConfig GetPiraeusConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(Environment.CurrentDirectory + "\\piraeusconfig.json")
                .AddEnvironmentVariables("PI_");

            IConfigurationRoot root = builder.Build();
            PiraeusConfig config = new PiraeusConfig();
            ConfigurationBinder.Bind(root, config);

            return config;
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    PiraeusConfig config = GetPiraeusConfig();
                    options.Limits.MaxConcurrentConnections = config.Channels.Http.MaxConnections;
                    options.Limits.MaxConcurrentUpgradedConnections = config.Channels.Http.MaxUpgradedConnections;
                    options.Limits.MaxRequestBodySize = config.Channels.Http.MaxRequestSize;
                    options.Limits.MinRequestBodyDataRate =
                        new MinDataRate(bytesPerSecond: config.Channels.Http.MinRequestDataRate, gracePeriod: TimeSpan.FromSeconds(10));
                    options.Limits.MinResponseDataRate =
                        new MinDataRate(bytesPerSecond: config.Channels.Http.MinResponseDataRate, gracePeriod: TimeSpan.FromSeconds(10));
                    if (!string.IsNullOrEmpty(config.Channels.Http.X509Filename))
                    {
                        options.ListenAnyIP(config.Channels.Http.ListenPort, (a) => a.UseHttps(config.Channels.Http.X509Filename, config.Channels.Http.X509Password));
                    }
                    else
                    {
                        options.ListenAnyIP(config.Channels.Http.ListenPort, (a) => a.UseHttps(".\\localhost.pfx", "pass@word1"));
                    }
                });
    }
}
