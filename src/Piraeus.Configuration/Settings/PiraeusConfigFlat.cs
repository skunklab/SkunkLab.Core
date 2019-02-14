using Newtonsoft.Json;
using System;

namespace Piraeus.Configuration.Settings
{
    public class PiraeusConfigFlat
    {
        #region Channels

        #region HTTP Channnel
        [JsonProperty("httpMaxConnections")]
        public int HttpMaxConnections { get; set; } = 5000;

        [JsonProperty("httpMaxUpgradedConnections")]
        public int HttpMaxUpgradedConnections { get; set; } = 5000;
        
        [JsonProperty("httpMaxRequestSize")]
        public int HttpMaxRequestSize { get; set; } = 30000000;

        [JsonProperty("httpMinRequestDataRate")]
        public int HttpMinRequestDataRate { get; set; } = 240;

        [JsonProperty("httpMinResponseDataRate")]
        public int HttpMinResponseDataRate { get; set; } = 240;

        [JsonProperty("httpMaxRequestBufferSize")]
        public int HttpMaxRequestBufferSize { get; set; } = 1048576;

        [JsonProperty("httpMaxResponseBufferSize")]
        public int HttpMaxResponseBufferSize { get; set; } = 1048576;

        [JsonProperty("httpPort")]
        public int HttpPort { get; set; } = 80;

        [JsonProperty("HttpUseEncryptedChannel")]
        public bool HttpUseEncryptedChannel { get; set; } = false;

        [JsonProperty("httpX509Filename")]
        public string HttpX509Filename { get; set; }

        [JsonProperty("httpX509Password")]
        public string HttpX509Password { get; set; }

        [JsonProperty("httpX509Store")]
        public string HttpX509Store { get; set; }

        [JsonProperty("httpX509Location")]
        public string HttpX509Location { get; set; }

        [JsonProperty("httpX509Thumbprint")]
        public string HttpX509Thumbprint { get; set; }

        #endregion

        #region Web Socket Channel

        [JsonProperty("webSocketMaxIncomingMessageSize")]
        public int WebSocketMaxIncomingMessageSize { get; set; } = 0x400000;

        [JsonProperty("webSocketReceiveLoopBufferSize")]
        public int WebSocketReceiveLoopBufferSize { get; set; } = 0x2000;

        [JsonProperty("webSocketSendBufferSize")]
        public int WebSocketSendBufferSize { get; set; } = 0x2000;

        [JsonProperty("webSocketCloseTimeoutMilliseconds")]
        public double WebSocketCloseTimeoutMilliseconds { get; set; } = 250.0;
        #endregion

        #region TCP Channel
        [JsonProperty("tcpUseLengthPrefix")]
        public bool TcpUseLengthPrefix { get; set; } = false;

        /// <summary>
        /// Authenticates a certificate used.
        /// </summary>
        [JsonProperty("tcpAuthenticate")]
        public bool TcpAuthenticate { get; set; } = false;

        [JsonProperty("tcpBlockSize")]
        public int TcpBlockSize { get; set; } = 16384;

        [JsonProperty("tcpMaxBufferSize")]
        public int TcpMaxBufferSize { get; set; } = 1000 * 16384;

        [JsonProperty("tcpPorts")]
        public string TcpPorts { get; set; } = "1883;8883;5684"; 

        [JsonProperty("tcpHostname")]
        public string TcpHostname { get; set; }

        /// <summary>
        /// Certificate filename, i.e., folder/subfolder/filename.pfx
        /// </summary>
        [JsonProperty("tcpX509Filename")]
        public string  TcpX509Filename { get; set; }

        /// <summary>
        /// Certificate password
        /// </summary>
        [JsonProperty("tcpX509Password")]
        public string TcpX509Password { get; set; }        

        [JsonProperty("tcpX509Store")]
        public string TcpX509Store { get; set; }

        [JsonProperty("tcpX509Location")]
        public string TcpX509Location { get; set; }

        [JsonProperty("tcpX509Thumbprint")]
        public string TcpX509Thumbprint { get; set; }

        [JsonProperty("tcpPresharedKeys")]
        public string TcpPresharedKeys { get; set; }

        [JsonProperty("tcpPresharedKeyIdentities")]
        public string TcpPresharedKeyIdentities { get; set; }

        #endregion

        #region UDP Channel

        [JsonProperty("udpHostname")]
        public string UdpHostname { get; set; }


        [JsonProperty("udpPorts")]
        public string UdpPorts { get; set; }


        #endregion

        #endregion

        #region Protocols

        #region MQTT Protocol

        [JsonProperty("mqttKeepAliveSeconds")]
        public double MqttKeepAliveSeconds { get; set; } = 180.0;

        [JsonProperty("mqttAckTimeoutSeconds")]
        public double MqttAckTimeoutSeconds { get; set; } = 2.0;

        [JsonProperty("mqttAckRandomFactor")]
        public double MqttAckRandomFactor { get; set; } = 1.5;

        [JsonProperty("mqttMaxRetransmit")]
        public int MqttMaxRetransmit { get; set; } = 4;

        [JsonProperty("mqttMaxLatencySeconds")]
        public double MqttMaxLatencySeconds { get; set; } = 100.0;

        #endregion

        #region CoAP Protocol

        [JsonProperty("coapHostname")]
        public string CoapHostName { get; set; }

        [JsonProperty("coapAutoRetry")]
        public bool CoapAutoRetry { get; set; } = false;

        [JsonProperty("coapObserveOption")]
        public bool CoapObserveOption { get; set; } = true;

        [JsonProperty("coapNoResponseOption")]
        public bool CoapNoResponseOption { get; set; } = true;

        [JsonProperty("coapNStart")]
        public int CoapNStart { get; set; } = 1;

        [JsonProperty("coapDefaultLeisure")]
        public double CoapDefaultLeisure { get; set; } = 4.0;

        [JsonProperty("coapProbingRate")]
        public double CoapProbingRate { get; set; } = 1.0;

        [JsonProperty("coapKeepAliveSeconds")]
        public double CoapKeepAliveSeconds { get; set; } = 180.0;

        [JsonProperty("coapAckTimeoutSeconds")]
        public double CoapAckTimeoutSeconds { get; set; } = 2.0;

        [JsonProperty("coapAckRandomFactor")]
        public double CoapAckRandomFactor { get; set; } = 1.5;

        [JsonProperty("coapMaxRetransmit")]
        public int CoapMaxRetransmit { get; set; } = 4;

        [JsonProperty("coapMaxLatencySeconds")]
        public double CoapMaxLatencySeconds { get; set; } = 100.0;




        #endregion

        #endregion

        #region Identity

        #region Client Identity

        [JsonProperty("clientIdentityClaimType")]
        public string ClientIdentityClaimType { get; set; }

        [JsonProperty("clientIdentityIndexClaimTypes")]
        public string ClientIdentityIndexClaimTypes { get; set; }

        [JsonProperty("clientIdentityIndexClaimTypeKeys")]
        public string ClientIdentityIndexClaimTypeKeys { get; set; }

        [JsonProperty("clientIdentityIndexValues")]
        public string ClientIdentityIndexValues { get; set; }

        #endregion


        #region Service Identity

        [JsonProperty("serviceIdentityClaimTypes")]
        public string ServiceIdentityClaimTypes { get; set; }

        [JsonProperty("serviceIdentityClaimValues")]
        public string ServiceIdentityClaimValues { get; set; }

        [JsonProperty("serviceIdentityX509Filename")]
        public string ServiceIdentityX509Filename { get; set; }

        [JsonProperty("serviceIdentityX509Password")]
        public string ServiceIdentityX509Password { get; set; }

        [JsonProperty("serviceIdentityX509Store")]
        public string ServiceIdentityX509Store { get; set; }

        [JsonProperty("serviceIdentityX509Location")]
        public string ServiceIdentityX509Location { get; set; }

        [JsonProperty("serviceIdentityX509Thumbprint")]
        public string ServiceIdentityX509Thumbprint { get; set; }

        #endregion

        #endregion

        #region Security

        #region Management API Security
        [JsonProperty("managmentSecurityIssuer")]
        public string ManagmentSecurityIssuer { get; set; }

        [JsonProperty("managmentSecurityAudience")]
        public string ManagmentSecurityAudience { get; set; }

        [JsonProperty("managmentSecurityTokenType")]
        public string ManagmentSecurityTokenType { get; set; }

        [JsonProperty("managmentSecuritySymmetricKey")]
        public string ManagmentSecuritySymmetricKey { get; set; }

        [JsonProperty("managmentSecurityNameClaimType")]
        public string ManagmentSecurityNameClaimType { get; set; }

        [JsonProperty("managmentSecurityRoleClaimType")]
        public string ManagmentSecurityRoleClaimType { get; set; }

        [JsonProperty("managmentApiRoleClaimValue")]
        public string ManagmentApiRoleClaimValue { get; set; }

        [JsonProperty("securityCodes")]
        public string ManagmentApiSecurityCodes { get; set; }

        #endregion

        #region Client Security
        [JsonProperty("ClientSecurityIssuer")]
        public string ClientSecurityIssuer { get; set; }

        [JsonProperty("clientSecurityAudience")]
        public string ClientSecurityAudience { get; set; }

        [JsonProperty("clientSecurityTokenType")]
        public string ClientSecurityTokenType { get; set; }

        [JsonProperty("clientSecuritySymmetricKey")]
        public string ClientSecuritySymmetricKey { get; set; }

        [JsonProperty("clientSecurityX509Filename")]
        public string ClientSecurityX509Filename { get; set; }               

        [JsonProperty("clientSecurityX509Store")]
        public string ClientSecurityX509Store { get; set; }

        [JsonProperty("clientSecurityX509Location")]
        public string ClientSecurityX509Location { get; set; }

        [JsonProperty("clientSecurityX509Thumbprint")]
        public string ClientSecurityX509Thumbprint { get; set; }

        #endregion

        #region Serivce Security

        [JsonProperty("serviceSecuritySymmetricKey")]
        public string ServiceSecuritySymmetricKey { get; set; }

        [JsonProperty("serviceSecurityX509Filename")]
        public string ServiceSecurityX509Filename { get; set; }

        [JsonProperty("serviceSecurityX509Store")]
        public string ServiceSecurityX509Store { get; set; }

        [JsonProperty("serviceSecurityX509Location")]
        public string ServiceSecurityX509Location { get; set; }

        [JsonProperty("serviceSecurityX509Thumbprint")]
        public string ServiceSecurityX509Thumbprint { get; set; }


        #endregion


        #endregion


        #region Utilities

        public string[] ConvertToStringArray(string item)
        {
            Func<string, string[]> func = new Func<string, string[]>((obj) =>
            {
                return obj != null ? obj.Split(";", StringSplitOptions.RemoveEmptyEntries) : null;                
            });

            return func(item);
        }

        public int? ConvertToInt(string item)
        {
            int result;

            if(Int32.TryParse(item, out result))
            {
                return new int?(result);
            }
            else
            {
                return null;
            }
        }

        public int[] ConvertToIntArray(string item)
        {
            string[] parts = ConvertToStringArray(item);
            return parts != null ? Array.ConvertAll(parts, s => int.Parse(s)) : null;
        }


       

        #endregion
    }
}
