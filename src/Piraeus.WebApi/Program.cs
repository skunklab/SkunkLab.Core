using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Piraeus.Configuration;
using Piraeus.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Piraeus.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices(services => services.AddPiraeusConfiguration())
            .ConfigureWebHostDefaults(webBuilder =>
            {
                PiraeusConfig config = WebApiHelpers.GetPiraeusConfig();
                webBuilder.ConfigureKestrel((options) =>
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
                            options.ListenAnyIP(port, (a) => a.UseHttps(cert));
                        }
                        else
                        {
                            IPAddress address = GetIPAddress(Dns.GetHostName());
                            options.Listen(address, port);
                        }
                    }

                    if (!string.IsNullOrEmpty(config.ServerCertificateFilename))
                    {
                        string[] portStrings = config.Ports.Split(";", StringSplitOptions.RemoveEmptyEntries);

                        foreach (var portString in portStrings)
                        {
                            options.ListenAnyIP(Convert.ToInt32(portString), (a) => a.UseHttps(config.ServerCertificateFilename, config.ServerCertificatePassword));
                        }
                    }
                });
                webBuilder.UseStartup<Startup>();
            });



        private static IPAddress GetIPAddress(string hostname)
        {
            IPHostEntry hostInfo = Dns.GetHostEntry(hostname);
            for (int index = 0; index < hostInfo.AddressList.Length; index++)
            {
                if (hostInfo.AddressList[index].AddressFamily == AddressFamily.InterNetwork)
                {
                    return hostInfo.AddressList[index];
                }
            }

            return null;
        }


    }
}
