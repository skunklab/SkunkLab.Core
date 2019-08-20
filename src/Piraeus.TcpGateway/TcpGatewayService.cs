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

namespace Piraeus.TcpGateway
{
    public class TcpGatewayService
    {
        public TcpGatewayService(PiraeusConfig piraeusConfig, IClusterClient clusterClient, ILogger<TcpGatewayService> logger = null)
        {
            this.config = piraeusConfig;
            this.clusterClient = clusterClient;
            this.logger = logger;

            if(!GraphManager.IsInitialized)
            {
                GraphManager.Initialize(this.clusterClient);
            }
        }

        private PiraeusConfig config;
        private IClusterClient clusterClient;
        private Dictionary<int, TcpServerListener> listeners;
        private Dictionary<int, CancellationTokenSource> sources;
        private ILogger<TcpGatewayService> logger;
        private string hostname;


        public void Init(bool dockerized)
        {
            listeners = new Dictionary<int, TcpServerListener>();
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
                listeners.Add(ports[index], new TcpServerListener(new IPEndPoint(GetIPAddress(hostname), ports[index]), config, this.logger, sources[ports[index]].Token));
                index++;
            }

            KeyValuePair<int, TcpServerListener>[] tcpKvps = listeners.ToArray();

            foreach (var item in tcpKvps)
            {
                item.Value.OnError += Listener_OnError;
                item.Value.StartAsync().LogExceptions();
                logger?.LogInformation($"TCP listener started on port {item.Key}");
            }

            logger?.LogInformation("TCP server started.");
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
                logger?.LogError($"TCP server faulted on channel type '{e.ChannelType}' and port '{e.Port}'.");
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
                    logger?.LogInformation($"Stopping TCP server on channel type '{e.ChannelType}' and port '{e.Port}'.");
                    listeners[e.Port].StopAsync().Ignore();
                    logger?.LogInformation($"Removing TCP server on listener and port on channel type '{e.ChannelType}' and port '{e.Port}'.");
                    listeners.Remove(e.Port);
                    sources.Remove(e.Port);

                    logger?.LogInformation($"Restarting TCP server on listener and port on channel type '{e.ChannelType}' and port '{e.Port}'.");
                    //restart the server
                    sources.Add(e.Port, new CancellationTokenSource());

                    //string hostname = config.Hostname == null ? "localhost" : config.Hostname;
                    listeners.Add(e.Port, new TcpServerListener(new IPEndPoint(GetIPAddress(hostname), e.Port), config, logger, sources[e.Port].Token));
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Faulted handling a TCP server failed event on channel type '{e.ChannelType}' and port '{e.Port}'.");
            }
        }

       


    }
}
