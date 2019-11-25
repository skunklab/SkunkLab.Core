using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Piraeus.Configuration;
using Piraeus.Extensions.Configuration;
using System;
using System.Security.Cryptography.X509Certificates;


namespace Piraeus.HttpGateway
{
    public class Program
    {
        private static PiraeusConfig config;
        public static void Main(string[] args)
        {
            //CreateWebHostBuilder(args).Build().Run();
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddPiraeusConfiguration(out config);
            })
            .ConfigureWebHost((options) =>
            {
                options.UseStartup<Startup>();
                options.UseKestrel();
                options.ConfigureKestrel(options =>
                {
                    options.Limits.MaxConcurrentConnections = config.MaxConnections;
                    options.Limits.MaxConcurrentUpgradedConnections = config.MaxConnections;
                    options.Limits.MaxRequestBodySize = config.MaxBufferSize;
                    options.Limits.MinRequestBodyDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                    options.Limits.MinResponseDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));

                    X509Certificate2 cert = config.GetServerCerticate();
                    int[] ports = config.GetPorts();
                    
                    foreach (int port in ports)
                    {
                        if (cert != null)
                        {
                            Console.WriteLine($"Listening on {port} using certificate.");
                            options.ListenAnyIP(port, (a) => a.UseHttps(cert));
                        }
                        else
                        {
                            Console.WriteLine($"Listening on {port}.");
                            options.ListenAnyIP(port);
                        }
                    }                    
                });
            });

        
    }
}
