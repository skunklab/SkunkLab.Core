using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Piraeus.Configuration.Core;
using Piraeus.Configuration.Settings;
using Piraeus.GrainInterfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Orleans.Clustering.Redis;
using Piraeus.Grains;
using Microsoft.Extensions.DependencyInjection;

namespace Piraeus.SiloHost.Core
{
    class Program
    {
        private static OrleansConfig orleansConfig;
        private static PiraeusConfig piraeusConfig;
        private static readonly string hostname;
        private static readonly IPAddress address;
        private static ISiloHost host;
        private static ManualResetEventSlim done;

        static void Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();
            Startup startup = new Startup(null);
            startup.ConfigureServices(services);


            //orleansConfig = GetOrleansConfiguration();

            //CreateSiloHost();

            //Task task = host.StartAsync();
            //Task.WhenAll(task);

            done = new ManualResetEventSlim(false);


            Console.CancelKeyPress += (sender, eventArgs) =>
            {

                done.Set();
                eventArgs.Cancel = true;
            };

            Console.WriteLine("Orleans silo is running...");
            done.Wait();

        }

        //private static void CreateSiloHost()
        //{
        //    if(orleansConfig.Dockerized)
        //    {
        //        CreateClusteredSiloHost();
        //    }
        //    else
        //    {
        //        CreateLocalSiloHost();
        //    }
        //}

        //private static void CreateClusteredSiloHost()
        //{
        //    var silo = new SiloHostBuilder()
        //        .Configure<ClusterOptions>(options =>
        //        {
        //            options.ClusterId = orleansConfig.ClusterId;
        //            options.ServiceId = orleansConfig.ServiceId;
        //        })              
        //        .EnableDirectClient();

            
            

        //    string storageType = GetStorageType(orleansConfig.DataConnectionString);
        //    if (storageType.ToLowerInvariant() == "redis")
        //    {
        //        ILogger<RedisMembershipTable> logger = new Logger<RedisMembershipTable>(new LoggerFactory());

               
        //        //silo.UseRedisMembership(options => options.ConnectionString = orleansConfig.DataConnectionString);
        //        //silo.AddRedisGrainStorage("store", logger, op => op.ConnectionString = "piraeus.redis.cache.windows.net:6380,password=y4fGNTZkH+NI2Msz0yiH8Q+WFICgE1yO1FKysaL97oA=,ssl=True,abortConnect=False");
        //    }
        //    else if (storageType.ToLowerInvariant() == "azurestorage")
        //    {
        //        silo.UseAzureStorageClustering(options => options.ConnectionString = orleansConfig.DataConnectionString);
        //        silo.AddAzureBlobGrainStorage("store", options => options.ConnectionString = orleansConfig.DataConnectionString);
        //    }
        //    else
        //    {
        //        //throw
        //    }

        //    silo.ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000);            
        //    silo.ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning).AddConsole());
            
        //    host = silo.Build();

        //    var clusterClient = (IClusterClient)host.Services.GetService(typeof(IClusterClient));
        //    GraphManager.Initialize(clusterClient);
        //}

        //private static void CreateLocalSiloHost()
        //{
        //    var builder = new SiloHostBuilder()
        //    // Use localhost clustering for a single local silo
        //    .UseLocalhostClustering()
        //    // Configure ClusterId and ServiceId
        //    .Configure<ClusterOptions>(options =>
        //    {
        //        options.ClusterId = orleansConfig.ClusterId;
        //        options.ServiceId = orleansConfig.ServiceId;
        //    })
        //    .AddMemoryGrainStorage("store")
        //    // Configure connectivity
        //    .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
            
        //    // Configure logging with any logging framework that supports Microsoft.Extensions.Logging.
        //    // In this particular case it logs using the Microsoft.Extensions.Logging.Console package.
        //    .ConfigureLogging(logging => logging.AddConsole());
            
        //    host = builder.Build();
        //}

        //private static OrleansConfig GetOrleansConfiguration()
        //{
        //    var builder = new ConfigurationBuilder()
        //        .AddJsonFile("orleansconfig.json")
        //        .AddEnvironmentVariables("OR_");
        //    IConfigurationRoot root = builder.Build();
        //    OrleansConfig config = new OrleansConfig();
        //    root.Bind(config);
        //    return config;
        //}

        //private static PiraeusConfig GetPiraeusConfiguration()
        //{
        //    var builder = new ConfigurationBuilder()
        //        .AddJsonFile("piraeusconfig.json")
        //        .AddEnvironmentVariables("PI_");
        //    IConfigurationRoot root = builder.Build();
        //    PiraeusConfig config = new PiraeusConfig();
        //    root.Bind(config);
        //    return config;
        //}

        //private static string GetStorageType(string connectionString)
        //{
        //    string cs = connectionString.ToLowerInvariant();
        //    if (cs.Contains(":6380") || cs.Contains(":6379"))
        //    {
        //        return "Redis";
        //    }
        //    else if (cs.Contains("defaultendpointsprotocol="))
        //    {
        //        return "AzureStorage";
        //    }
        //    else
        //    {
        //        throw new ArgumentException("Invalid connection string");
        //    }

        //}


        //static string GetLocalHostName()
        //{
        //    return orleansConfig.Dockerized ? piraeusConfig.Hostname : "localhost";
        //}

        //static IPAddress GetIPAddress(string hostname)
        //{
        //    IPHostEntry hostInfo = Dns.GetHostEntry(hostname);
        //    for (int index = 0; index < hostInfo.AddressList.Length; index++)
        //    {
        //        if (hostInfo.AddressList[index].AddressFamily == AddressFamily.InterNetwork)
        //        {
        //            return hostInfo.AddressList[index];
        //        }
        //    }

        //    return null;
        //}
    }
}
