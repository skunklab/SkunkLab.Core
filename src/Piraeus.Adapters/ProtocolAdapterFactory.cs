using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Crypto.Tls;
using Piraeus.Configuration;
using Piraeus.Core.Logging;
using Piraeus.Grains;
using SkunkLab.Channels;
using SkunkLab.Channels.Psk;
using SkunkLab.Channels.WebSocket;
using SkunkLab.Security.Authentication;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;

namespace Piraeus.Adapters
{
    public class ProtocolAdapterFactory
    {

        public static ProtocolAdapter Create(PiraeusConfig config, GraphManager graphManager, HttpContext context, WebSocket socket, ILog logger = null, IAuthenticator authenticator = null, CancellationToken token = default(CancellationToken))
        {
            WebSocketConfig webSocketConfig = GetWebSocketConfig(config);
            IChannel channel = ChannelFactory.Create(webSocketConfig, context, socket, token);
            string subprotocol = context.WebSockets.WebSocketRequestedProtocols[0];
            if (subprotocol == "mqtt")
            {
                return new MqttProtocolAdapter(config, graphManager, authenticator, channel, logger, context);
            }
            else if (subprotocol == "coapV1")
            {
                return new CoapProtocolAdapter(config, graphManager, authenticator, channel, logger);
            }
            else
            {
                throw new InvalidOperationException("invalid web socket subprotocol");
            }

        }

        /// <summary>
        /// Create protocol adapter for rest service or Web socket
        /// </summary>
        /// <param name="config"></param>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <param name="authenticator"></param>
        /// <returns></returns>
        public static ProtocolAdapter Create(PiraeusConfig config, GraphManager graphManager, HttpContext context, ILog logger = null, IAuthenticator authenticator = null, CancellationToken token = default(CancellationToken))
        {
            IChannel channel = null;

            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocketConfig webSocketConfig = new WebSocketConfig(config.MaxBufferSize, config.BlockSize, config.BlockSize);
                channel = ChannelFactory.Create(context, webSocketConfig, token);
                if (context.WebSockets.WebSocketRequestedProtocols.Contains("mqtt"))
                {
                    return new MqttProtocolAdapter(config, graphManager, authenticator, channel, logger);
                }
                else if (context.WebSockets.WebSocketRequestedProtocols.Contains("coapv1"))  //(context.WebSocketRequestedProtocols.Contains("coapv1"))
                {
                    return new CoapProtocolAdapter(config, graphManager, authenticator, channel, logger);
                }
                else if (context.WebSockets.WebSocketRequestedProtocols.Count == 0)
                {
                    return new WsnProtocolAdapter(config, graphManager, channel, context, logger);
                }
                else
                {
                    throw new InvalidOperationException("invalid web socket subprotocol");
                }
            }
            else
            {
                if (context.Request.Method.ToUpperInvariant() != "POST" && context.Request.Method.ToUpperInvariant() != "GET")
                {
                    throw new HttpRequestException("Protocol adapter requires HTTP get or post.");
                }

                channel = ChannelFactory.Create(context);
                return new RestProtocolAdapter(config, graphManager, channel, context, logger);
            }

        }

        /// <summary>
        /// Creates a protocol adapter for TCP server channel
        /// </summary>
        /// <param name="client">TCP client initialized by TCP Listener on server.</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        public static ProtocolAdapter Create(PiraeusConfig config, GraphManager graphManager, IAuthenticator authenticator, TcpClient client, ILog logger = null, CancellationToken token = default(CancellationToken))
        {
            IChannel channel = null;
            TlsPskIdentityManager pskManager = null;

            if (!string.IsNullOrEmpty(config.PskStorageType))
            {
                if (config.PskStorageType.ToLowerInvariant() == "redis")
                {
                    pskManager = TlsPskIdentityManagerFactory.Create(config.PskRedisConnectionString);
                }

                if (config.PskStorageType.ToLowerInvariant() == "keyvault")
                {
                    pskManager = TlsPskIdentityManagerFactory.Create(config.PskKeyVaultAuthority, config.PskKeyVaultClientId, config.PskKeyVaultClientSecret);
                }

                if (config.PskStorageType.ToLowerInvariant() == "environmentvariable")
                {
                    pskManager = TlsPskIdentityManagerFactory.Create(config.PskIdentities, config.PskKeys);
                }

            }

            if (pskManager != null)
            {
                channel = ChannelFactory.Create(config.UsePrefixLength, client, pskManager, config.BlockSize, config.MaxBufferSize, token);
            }
            else
            {
                channel = ChannelFactory.Create(config.UsePrefixLength, client, config.BlockSize, config.MaxBufferSize, token);
            }

            IPEndPoint localEP = (IPEndPoint)client.Client.LocalEndPoint;
            int port = localEP.Port;

            if (port == 5684) //CoAP over TCP
            {
                return new CoapProtocolAdapter(config, graphManager, authenticator, channel, logger);
            }
            else if (port == 1883 || port == 8883) //MQTT over TCP
            {
                //MQTT
                return new MqttProtocolAdapter(config, graphManager, authenticator, channel, logger);
            }
            else
            {
                throw new ProtocolAdapterPortException("TcpClient port does not map to a supported protocol.");
            }

        }

        public static ProtocolAdapter Create(PiraeusConfig config, GraphManager graphManager, IAuthenticator authenticator, UdpClient client, IPEndPoint remoteEP, ILog logger = null, CancellationToken token = default(CancellationToken))
        {
            IPEndPoint endpoint = client.Client.LocalEndPoint as IPEndPoint;

            IChannel channel = ChannelFactory.Create(client, remoteEP, token);
            if (endpoint.Port == 5683)
            {
                return new CoapProtocolAdapter(config, graphManager, authenticator, channel, logger);
            }
            else if (endpoint.Port == 5883)
            {
                return new MqttProtocolAdapter(config, graphManager, authenticator, channel, logger);
            }
            else
            {
                throw new ProtocolAdapterPortException("UDP port does not map to a supported protocol.");
            }

        }

        #region configurations
        private static WebSocketConfig GetWebSocketConfig(PiraeusConfig config)
        {
            return new WebSocketConfig(config.MaxBufferSize,
                config.BlockSize,
                config.BlockSize, 250.0);
        }

        //private static CoapConfig GetCoapConfig(PiraeusConfig config, IAuthenticator authenticator)
        //{
        //    CoapConfigOptions options = config.Protocols.Coap.ObserveOption && config.Protocols.Coap.NoResponseOption ? CoapConfigOptions.Observe | CoapConfigOptions.NoResponse : config.Protocols.Coap.ObserveOption ? CoapConfigOptions.Observe : config.Protocols.Coap.NoResponseOption ? CoapConfigOptions.NoResponse : CoapConfigOptions.None;
        //    return new CoapConfig(authenticator, config.Protocols.Coap.HostName, options, config.Protocols.Coap.AutoRetry,
        //        config.Protocols.Coap.KeepAliveSeconds, config.Protocols.Coap.AckTimeoutSeconds, config.Protocols.Coap.AckRandomFactor,
        //        config.Protocols.Coap.MaxRetransmit, config.Protocols.Coap.NStart, config.Protocols.Coap.DefaultLeisure, config.Protocols.Coap.ProbingRate, config.Protocols.Coap.MaxLatencySeconds);
        //}

        //private static MqttConfig GetMqttConfig(PiraeusConfig config, IAuthenticator authenticator)
        //{
        //    MqttConfig mqttConfig = new MqttConfig(authenticator, config.Protocols.Mqtt.KeepAliveSeconds,
        //           config.Protocols.Mqtt.AckTimeoutSeconds, config.Protocols.Mqtt.AckRandomFactor, config.Protocols.Mqtt.MaxRetransmit, config.Protocols.Mqtt.MaxLatencySeconds);
        //    mqttConfig.IdentityClaimType = config.Identity.Client.IdentityClaimType;
        //    mqttConfig.Indexes = config.Identity.Client.Indexes;

        //    return mqttConfig;
        //}

        #endregion
    }
}
