using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Piraeus.Configuration;
using Piraeus.Core;
using Piraeus.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Piraeus.TcpGateway
{
    public class TcpGatewayHost : IHostedService
    {
        public TcpGatewayHost(PiraeusConfig config, OrleansConfig orleansConfig, Logger logger)
        {
            this.config = config;
            this.orleansConfig = orleansConfig;
            this.logger = logger;
            listeners = new Dictionary<int, TcpServerListener>();
            sources = new Dictionary<int, CancellationTokenSource>();
        }


        private readonly Logger logger;
        private readonly PiraeusConfig config;
        private readonly OrleansConfig orleansConfig;
        private readonly Dictionary<int, TcpServerListener> listeners;
        private readonly Dictionary<int, CancellationTokenSource> sources;
        private string hostname;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("   /$$$$$$$$/$$$$$$ /$$$$$$$         /$$$$$$            /$$");
            Console.WriteLine("  |__  $$__/$$__  $| $$__  $$       /$$__  $$          | $$ ");
            Console.WriteLine("     | $$ | $$  \\__| $$  \\ $$      | $$  \\__/ /$$$$$$ /$$$$$$   /$$$$$$ /$$  /$$  /$$ /$$$$$$ /$$   /$$");
            Console.WriteLine("     | $$ | $$     | $$$$$$$/      | $$ /$$$$|____  $|_  $$_/  /$$__  $| $$ | $$ | $$|____  $| $$  | $$");
            Console.WriteLine("     | $$ | $$     | $$____/       | $$|_  $$ /$$$$$$$ | $$   | $$$$$$$| $$ | $$ | $$ /$$$$$$| $$  | $$");
            Console.WriteLine("     | $$ | $$    $| $$            | $$  \\ $$/$$__  $$ | $$ /$| $$_____| $$ | $$ | $$/$$__  $| $$  | $$");
            Console.WriteLine("     | $$ |  $$$$$$| $$            |  $$$$$$|  $$$$$$$ |  $$$$|  $$$$$$|  $$$$$/$$$$|  $$$$$$|  $$$$$$$");
            Console.WriteLine("     |__/  \\______/|__/             \\______/ \\_______/  \\___/  \\_______/\\_____/\\___/ \\_______/\\____  $$");
            Console.WriteLine("                                                                                              /$$  | $$");
            Console.WriteLine("                                                                                             |  $$$$$$/");
            Console.WriteLine("                                                                                              \\______/ ");

            int[] ports = config.GetPorts();

            foreach (var port in ports)
            {
                sources.Add(port, new CancellationTokenSource());
            }


#if DEBUG
            hostname = "localhost";
#else
            hostname = Dns.GetHostName();
#endif

            int index = 0;
            while (index < ports.Length)
            {
                listeners.Add(ports[index], new TcpServerListener(new IPEndPoint(GetIPAddress(hostname), ports[index]), config, orleansConfig, this.logger, sources[ports[index]].Token));
                logger?.LogInformation($"TCP listener added to port {ports[index]}");
                index++;
            }

            KeyValuePair<int, TcpServerListener>[] tcpKvps = listeners.ToArray();

            foreach (var item in tcpKvps)
            {
                item.Value.OnError += Listener_OnError;
                item.Value.StartAsync().LogExceptions(logger);
                logger?.LogInformation($"TCP listener started on port {item.Key}");
            }

            logger?.LogInformation("TCP server started.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            TcpServerListener[] servers = listeners.Values.ToArray();
            foreach(var server in servers)
            {
                server.StopAsync().Ignore();
            }

            return Task.CompletedTask;

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
                    listeners.Add(e.Port, new TcpServerListener(new IPEndPoint(GetIPAddress(hostname), e.Port), config, orleansConfig, logger, sources[e.Port].Token));
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Faulted handling a TCP server failed event on channel type '{e.ChannelType}' and port '{e.Port}'.");
            }

            throw new Exception("TCP Server");
        }
    }
}
