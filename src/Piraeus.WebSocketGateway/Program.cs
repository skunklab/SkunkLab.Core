using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Piraeus.Configuration;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Piraeus.WebSocketGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static PiraeusConfig GetPiraeusConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("./piraeusconfig.json")
                .AddEnvironmentVariables("PI_");

            IConfigurationRoot root = builder.Build();
            PiraeusConfig config = new PiraeusConfig();
            ConfigurationBinder.Bind(root, config);

            return config;
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHost((options) =>
            {
                options.UseStartup<Startup>();
                options.UseKestrel();
                options.ConfigureKestrel(options =>
                {
                    PiraeusConfig config = GetPiraeusConfig();
                    options.Limits.MaxConcurrentConnections = config.MaxConnections;
                    options.Limits.MaxConcurrentUpgradedConnections = config.MaxConnections;
                    options.Limits.MaxRequestBodySize = config.MaxBufferSize;
                    options.Limits.MinRequestBodyDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                    options.Limits.MinResponseDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));

                    if (!string.IsNullOrEmpty(config.ServerCertificateFilename))
                    {
                        Console.WriteLine("Port for cert with filename");
                        options.ListenAnyIP(config.GetPorts()[0], (a) => a.UseHttps(config.ServerCertificateFilename, config.ServerCertificatePassword));
                    }
                    else if (!string.IsNullOrEmpty(config.ServerCertificateStore))
                    {
                        Console.WriteLine("Port for cert with store");
                        X509Certificate2 cert = config.GetServerCerticate();
                        options.ListenAnyIP(config.GetPorts()[0], (a) => a.UseHttps(cert));
                    }
                    else
                    {
                        Console.WriteLine("Hard coded port 8081");
                        options.ListenAnyIP(8081);
                    }
                });
            });




        //private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //        .UseStartup<Startup>()
        //        .UseKestrel()
        //        .ConfigureKestrel((context, options) =>
        //        {
        //            PiraeusConfig config = GetPiraeusConfig();
        //            options.Limits.MaxConcurrentConnections = config.MaxConnections;
        //            options.Limits.MaxConcurrentUpgradedConnections = config.MaxConnections;
        //            options.Limits.MaxRequestBodySize = config.MaxBufferSize;
        //            options.Limits.MinRequestBodyDataRate =
        //                new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
        //            options.Limits.MinResponseDataRate =
        //                new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
        //            if (!string.IsNullOrEmpty(config.ServerCertificateFilename))
        //            {
        //                Console.WriteLine("Port for cert with filename");
        //                options.ListenAnyIP(config.GetPorts()[0], (a) => a.UseHttps(config.ServerCertificateFilename, config.ServerCertificatePassword));
        //            }
        //            else if (!string.IsNullOrEmpty(config.ServerCertificateStore))
        //            {
        //                Console.WriteLine("Port for cert with store");
        //                X509Certificate2 cert = config.GetServerCerticate();
        //                options.ListenAnyIP(config.GetPorts()[0], (a) => a.UseHttps(cert));
        //            }
        //            else
        //            {
        //                Console.WriteLine("Hard coded port 8081");
        //                options.ListenAnyIP(8081);
        //            }
        //        });
    }
}
