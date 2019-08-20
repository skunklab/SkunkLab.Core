using Microsoft.Extensions.Logging;
using Orleans;
using Piraeus.Configuration;
using Piraeus.Core;
using Piraeus.Grains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace Piraeus.UdpGateway
{
    public class UdpGatewayService
    {
        public UdpGatewayService(PiraeusConfig piraeusConfig, IClusterClient clusterClient, ILogger<UdpGatewayService> logger = null)
        {
            this.config = piraeusConfig;
            this.clusterClient = clusterClient;
            this.logger = logger;
            this.ports = config.GetPorts();
            if (!GraphManager.IsInitialized)
            {
                GraphManager.Initialize(this.clusterClient);
            }
        }

        private PiraeusConfig config;
        private IClusterClient clusterClient;
        private Dictionary<int, UdpServerListener> listeners;
        private Dictionary<int, CancellationTokenSource> sources;
        private ILogger<UdpGatewayService> logger;
        private string hostname;
        private int[] ports;


        public void Init(bool dockerized)
        {
            listeners = new Dictionary<int, UdpServerListener>();
            sources = new Dictionary<int, CancellationTokenSource>();

            int[] ports = config.GetPorts();

            foreach (var port in ports)
            {
                sources.Add(port, new CancellationTokenSource());
            }

            hostname = !dockerized ? "localhost" : Dns.GetHostName();
            //string hostname = config.Hostname == null ? "localhost" : config.Hostname;
            
            int index = 0;
            while (index < ports.Length)
            {
                listeners.Add(ports[index], new UdpServerListener(config, new IPEndPoint(GetIPAddress(hostname), ports[index]), this.logger, sources[ports[index]].Token));
                index++;
            }

            KeyValuePair<int, UdpServerListener>[] tcpKvps = listeners.ToArray();

            foreach (var item in tcpKvps)
            {
                item.Value.OnError += Listener_OnError;
                item.Value.StartAsync().LogExceptions();
                logger?.LogInformation($"UDP listener started on port {item.Key}");
            }

            logger?.LogInformation("UDP server started.");
        }

        private IPAddress GetIPAddress(string hostname)
        {
            IPHostEntry hostInfo = Dns.GetHostEntry(hostname);
            for (int index = 0; index < hostInfo.AddressList.Length; index++)
            {
                if (hostInfo.AddressList[index].AddressFamily == AddressFamily.InterNetwork)
                {
                    logger?.LogInformation($"IP address '{hostInfo.AddressList[index]}'.");
                    return hostInfo.AddressList[index];
                }
            }

            logger?.LogInformation("IP address is null");
            return null;
        }

        private void Listener_OnError(object sender, ServerFailedEventArgs e)
        {
            try
            {
                logger?.LogError($"UDP server faulted on channel type '{e.ChannelType}' and port '{e.Port}'.");
                if (sources.ContainsKey(e.Port))
                {
                    logger?.LogInformation($"Sending cancellation on on channel type '{e.ChannelType}' and port '{e.Port}'.");
                    //cancel the server
                    sources[e.Port].Cancel();
                }
                else
                {
                    logger?.LogInformation($"Cancellation unavailable on channel type '{e.ChannelType}' and port '{e.Port}'.");
                    return;
                }

                if (listeners.ContainsKey(e.Port))
                {
                    logger?.LogInformation($"Stopping UDP server on channel type '{e.ChannelType}' and port '{e.Port}'.");
                    listeners[e.Port].StopAsync().Ignore();
                    logger?.LogInformation($"Removing UDP server on listener and port on channel type '{e.ChannelType}' and port '{e.Port}'.");
                    listeners.Remove(e.Port);
                    sources.Remove(e.Port);

                    logger?.LogInformation($"Restarting UDP server on listener and port on channel type '{e.ChannelType}' and port '{e.Port}'.");
                    //restart the server
                    sources.Add(e.Port, new CancellationTokenSource());

                    //string hostname = config.Hostname == null ? "localhost" : config.Hostname;
                    listeners.Add(e.Port, new UdpServerListener(config, new IPEndPoint(GetIPAddress(hostname), e.Port), logger, sources[e.Port].Token));
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Faulted handling a TCP server failed event on channel type '{e.ChannelType}' and port '{e.Port}'.");
            }
        }

        //private OrleansConfig GetOrleansConfig()
        //{
        //    var builder = new ConfigurationBuilder()
        //        .AddJsonFile(Environment.CurrentDirectory + "\\orleans.json")
        //        .AddEnvironmentVariables("OR_");

        //    IConfigurationRoot root = builder.Build();
        //    OrleansConfig config = new OrleansConfig();
        //    ConfigurationBinder.Bind(root, config);

        //    return config;
        //}

        //private PiraeusConfig GetPiraeusConfig()
        //{
        //    var builder = new ConfigurationBuilder()
        //        .AddJsonFile(Environment.CurrentDirectory + "\\piraeusconfig.json")
        //        .AddEnvironmentVariables("PI_");

        //    IConfigurationRoot root = builder.Build();
        //    PiraeusConfig config = new PiraeusConfig();
        //    ConfigurationBinder.Bind(root, config);

        //    return config;
        //}

        //private static async Task<bool> RetryFilter(Exception exception)
        //{
        //    if (exception.GetType() != typeof(SiloUnavailableException))
        //    {
        //        Console.WriteLine($"Cluster client failed to connect to cluster with unexpected error.  Exception: {exception}");
        //        return false;
        //    }
        //    attempt++;
        //    Console.WriteLine($"Cluster client attempt {attempt} of {initializeAttemptsBeforeFailing} failed to connect to cluster.  Exception: {exception}");
        //    if (attempt > initializeAttemptsBeforeFailing)
        //    {
        //        return false;
        //    }
        //    await Task.Delay(TimeSpan.FromSeconds(10));
        //    return true;
        //}

        //private static IClusterClient GetClient(OrleansConfig config)
        //{
        //    if (!config.Dockerized)
        //    {
        //        var client = new ClientBuilder()
        //            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IResource).Assembly))
        //            .UseLocalhostClustering()
        //            .Build();

        //        return client;
        //    }
        //    else if (config.OrleansClusterId != null && config.OrleansDataProviderType == "AzureStore")
        //    {
        //        var client = new ClientBuilder()
        //            .Configure<ClusterOptions>(options =>
        //            {
        //                options.ClusterId = config.OrleansClusterId;
        //                options.ServiceId = config.OrleansServiceId;
        //            })
        //            .UseAzureStorageClustering(options => options.ConnectionString = config.OrleansDataConnectionString)
        //            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IResource).Assembly))
        //            .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning))
        //            .Build();

        //        return client;
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException("Invalid configuration for Orleans client");
        //    }

        //}


    }
}
