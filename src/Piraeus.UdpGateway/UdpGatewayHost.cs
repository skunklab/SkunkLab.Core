using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Piraeus.Configuration;
using Piraeus.Core.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Piraeus.Core;
using Orleans;

namespace Piraeus.UdpGateway
{
    public class UdpGatewayHost : IHostedService
    {
        public UdpGatewayHost(PiraeusConfig config, OrleansConfig orleansConfig, Logger logger)
        {
            this.config = config;
            this.orleansConfig = orleansConfig;
            this.logger = logger;
            sources = new Dictionary<int, CancellationTokenSource>();
            listeners = new Dictionary<int, UdpServerListener>();
        }

        private readonly Logger logger;
        private readonly PiraeusConfig config;
        private readonly OrleansConfig orleansConfig;
        private readonly Dictionary<int, UdpServerListener> listeners;
        private readonly Dictionary<int, CancellationTokenSource> sources;
        private string hostname;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine(" **     ** *******   *******   ");
            Console.WriteLine("/**    /**/**////** /**////** ");
            Console.WriteLine("/**    /**/**    /**/**   /**");
            Console.WriteLine("/**    /**/**    /**/*******");
            Console.WriteLine("/**    /**/**    /**/**//// ");
            Console.WriteLine("/**    /**/**    ** /**  ");
            Console.WriteLine("//******* /*******  /**  ");
            Console.WriteLine(" ///////  ///////   //      ");
            Console.WriteLine("   ********              **     ");
            Console.WriteLine("  **//////**            /**                                 **   **");
            Console.WriteLine(" **      //   ******   ******  *****  ***     **  ******   //** ** ");
            Console.WriteLine("/**          //////** ///**/  **///**//**  * /** //////**   //*** ");
            Console.WriteLine("/**    *****  *******   /**  /******* /** ***/**  *******    /** ");
            Console.WriteLine("//**  ////** **////**   /**  /**////  /****/**** **////**    ** ");
            Console.WriteLine(" //******** //********  //** //****** ***/ ///**//********  **   ");
            Console.WriteLine("  ////////   ////////    //   ////// ///    ///  ////////  // ");
            Console.WriteLine("");

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
                listeners.Add(ports[index], new UdpServerListener(new IPEndPoint(GetIPAddress(hostname), ports[index]), config, orleansConfig, this.logger, sources[ports[index]].Token));
                logger?.LogInformation($"UDP listener added to port {ports[index]}");
                index++;
            }

            KeyValuePair<int, UdpServerListener>[] tcpKvps = listeners.ToArray();

            foreach (var item in tcpKvps)
            {
                item.Value.StartAsync().LogExceptions(logger);
                logger?.LogInformation($"TCP listener started on port {item.Key}");
            }

            logger?.LogInformation("TCP server started.");
            return Task.CompletedTask;

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            UdpServerListener[] servers = listeners.Values.ToArray();
            foreach (var server in servers)
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
    }
}
