using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using Piraeus.Configuration.Core;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class PiraeusConfig
    {

        private Dictionary<string, byte[]> psks;
        private List<KeyValuePair<string, string>> clientIndexes;

        #region Client Certificate (Optional)

        /// <summary>
        /// Optional path to client ceritficate.  
        /// </summary>
        /// <remarks>Must be omitted if client certificate is specified by store, location, and thumbprint.</remarks>
        [JsonProperty("clientCertificateFilename")]
        public string ClientCertificateFilename { get; set; }

        /// <summary>
        /// Optional certificate store where the client certificate is held. If used, location and thumbprint must also be used.
        /// </summary>
        /// <remarks>Must be omitted if client certificate is specified by ClientCertificateFilename</remarks>
        [JsonProperty("clientCertificateStore")]
        public string ClientCertificateStore { get; set; }

        /// <summary>
        /// Optional certificate location where the client certificate is held. If used, store and thumbprint must also be used.
        /// </summary>
        /// <remarks>Must be omitted if client certificate is specified by ClientCertificateFilename</remarks>
        [JsonProperty("clientCertificateLocation")]
        public string ClientCertificateLocation { get; set; }

        /// <summary>
        /// Optional certificate thumbprint of the client certificate. If used, store and location must also be used.
        /// </summary>
        /// <remarks>Must be omitted if client certificate is specified by ClientCertificateFilename</remarks>
        [JsonProperty("clientCertificateThumbprint")]
        public string ClientCertificateThumbprint { get; set; }
        #endregion

        #region Service Certificate (Optional)

        /// <summary>
        /// Optional path to server ceritficate.  
        /// </summary>
        /// <remarks>Must be omitted if server certificate is specified by store, location, and thumbprint.</remarks>
        [JsonProperty("serverCertificateFilename")]
        public string ServerCertificateFilename { get; set; }

        /// <summary>
        /// Required when ServerCertificateFilename is used to identify a server certificate.
        /// </summary>
        /// <remarks>Must be omitted if server certificate is specified by store, location, and thumbprint.</remarks>
        [JsonProperty("serverCertificatePassword")]
        public string ServerCertificatePassword { get; set; }

        /// <summary>
        /// Optional certificate store where the server certificate is held. If used, location and thumbprint must also be used.
        /// </summary>
        /// <remarks>Must be omitted if server certificate is specified by ServerCertificateFilename</remarks>
        [JsonProperty("serverCertificateStore")]
        public string ServerCertificateStore { get; set; }

        /// <summary>
        /// Optional certificate location where the server certificate is held. If used, store and thumbprint must also be used.
        /// </summary>
        /// <remarks>Must be omitted if server certificate is specified by ServerCertificateFilename</remarks>
        [JsonProperty("serverCertificateLocation")]
        public string ServerCertificateLocation { get; set; }

        /// <summary>
        /// Optional certificate thumbprint of the server certificate. If used, store and location must also be used.
        /// </summary>
        /// <remarks>Must be omitted if server certificate is specified by ServerCertificateFilename</remarks>
        [JsonProperty("serverCertificateThumbprint")]
        public string ServerCertificateThumbprint { get; set; }

        #endregion

        #region Channels

        /// <summary>
        /// Block size used for reading and writing from a channel.  Default is 16334.
        /// </summary>
        [JsonProperty("blockSize")]
        public int BlockSize { get; set; } = 16344;

        /// <summary>
        /// Maxium size of a message.  Default is 4,122,000 bytes.
        /// </summary>
        [JsonProperty("maxBufferSize")]
        public int MaxBufferSize { get; set; } = 4112000;

        /// <summary>
        /// Determines whether a TCP channel should use a 4-byte prefix on each message to communicate the length of a message.  Default is false.
        /// </summary>
        [JsonProperty("usePrefixLength")]
        public bool UsePrefixLength { get; set; } = false;



        #endregion

        #region Management API

        [JsonProperty("managementApiIssuer")]
        public string ManagementApiIssuer { get; set; }

        [JsonProperty("managementApiAudience")]
        public string ManagementApiAudience { get; set; }

        [JsonProperty("managmentApiSymmetricKey")]
        public string ManagmentApiSymmetricKey { get; set; }

        [JsonProperty("managementApiSecurityCodes")]
        public string ManagementApiSecurityCodes { get; set; }

        #endregion

        #region Gateway

        /// <summary>
        /// Required authority used for CoAP protocol in a gateway, i.e, http://authority
        /// </summary>
        /// <remarks>The CoAP authority can is not required to be related to the hostname of the gateway.</remarks>
        [JsonProperty("coapAuthority")]
        public string CoapAuthority { get; set; }        

        /// <summary>
        /// Hostname of a gateway
        /// </summary>
        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        /// <summary>
        /// List of ports semi-colon (;) separated implemented by a gateway.
        /// </summary>
        [JsonProperty("ports")]
        public string Ports { get; set; }

        /// <summary>
        /// Maximum number of connections allowed by a gateway.
        /// </summary>
        [JsonProperty("maxConnections")]
        public int MaxConnections { get; set; } = 10000;

        /// <summary>
        /// Optional enbles a TCP client to authenticate using the public cert of the TCP server when using TCP channel for encryption.  Default is false.
        /// </summary>
        /// <remarks>Authenticating the server TLS connection does not omit the requirement of a security token to authenticate the caller.</remarks>
        [JsonProperty("tlsCertficateAuthentication")]
        public bool TlsCertficateAuthentication = false;

        ///// <summary>
        ///// Optional Azure storage connection string used for audit logs and user login logs.
        ///// </summary>
        //[JsonProperty("azureStorageConnectionString")]
        //public string AzureStorageConnectionString { get; set; }

        /// <summary>
        /// Either a path to a folder or an Azure Storage connection string.
        /// </summary>
        [JsonProperty("auditConnectionString")]
        public string AuditConnectionString { get; set; }

        #endregion

        #region PSKs (Optional)

        /// <summary>
        /// One of KeyVault, Redis, EnvironmentVariables; otherwise if omitted no PSK is used.
        /// </summary>
        [JsonProperty("pskStorageType")]
        public string PskStorageType { get; set; }

        [JsonProperty("pskKeyVaultAuthority")]
        public string PskKeyVaultAuthority { get; set; }

        [JsonProperty("pskKeyVaultClientId")]
        public string PskKeyVaultClientId { get; set; }

        [JsonProperty("pskKeyVaultClientSecret")]
        public string PskKeyVaultClientSecret { get; set; }

        [JsonProperty("pskRedisConnectionString")]
        public string PskRedisConnectionString { get; set; }
        /// <summary>
        /// Optional list of PSK identities semi-colon (;) separated the map to PSK Keys.  Only used when PSKs are used to encrypted a channel.
        /// </summary>
        /// <remarks>If used the number of PSKIdentifies must match exactly the number of PSKKeys.</remarks>
        [JsonProperty("pskIdentities")]
        public string PskIdentities { get; set; }

        /// <summary>
        /// Optional list of PSK keys base64 encoded and semi-colon (;) separated that map to PSK Identities.  Only used when PSKs are used to encrypted a channel.
        /// </summary>
        /// <remarks>If used the number of PSKIdentifies must match exactly the number of PSKKeys.</remarks>
        [JsonProperty("pskKeys")]
        public string PskKeys { get; set; }

        #endregion

        #region Client Identity
        /// <summary>
        /// Unique identifier of the client as a claim type.
        /// </summary>
        [JsonProperty("clientIdentityNameClaimType")]
        public string ClientIdentityNameClaimType { get; set; }

        /// <summary>
        /// A list of claim types semicolon (;) separated that map to ClientIdentityClaimKeys.  Used to create indexes for ephemeral subscriptions.
        /// </summary>
        /// <remarks>If used the number of ClientIdentityClaimsTypes must match exactly the number of ClientIdentityClaimKeys</remarks>
        [JsonProperty("clientIdentityClaimTypes")]
        public string ClientIdentityClaimTypes { get; set; }

        /// <summary>
        /// A list of claim keys semicolon (;) separated that map to ClientIdentityClaimTypes.  Used to create indexes for ephemeral subscriptions.
        /// </summary>
        /// <remarks>If used the number of ClientIdentityClaimsTypes must match exactly the number of ClientIdentityClaimKeys</remarks>
        [JsonProperty("clientIdentityClaimKeys")]
        public string ClientIdentityClaimKeys { get; set; }

        #endregion

        #region Service Identity

        [JsonProperty("serviceIdentityClaimTypes")]
        public string ServiceIdentityClaimTypes { get; set; }
        [JsonProperty("serviceIdentityClaimValues")]
        public string ServiceIdentityClaimValues { get; set; }

        #endregion

        #region Client Security 
        /// <summary>
        /// Client issuer when using a symmetric key for authentication, e.g., JWT security token.
        /// </summary>
        [JsonProperty("clientIssuer")]
        public string  ClientIssuer { get; set; }

        /// <summary>
        /// Client audience when using a symmetric key for authentication, e.g., JWT security token.
        /// </summary>
        [JsonProperty("clientAudience")]
        public string ClientAudience { get; set; }

        /// <summary>
        /// Client authentication token type, e.g., JWT, SWT, or X509.
        /// </summary>
        [JsonProperty("clientTokenType")]
        public string ClientTokenType { get; set; }

        /// <summary>
        /// Client symmetric key authentication is used, e.g., JWT security token.
        /// </summary>
        [JsonProperty("clientSymmetricKey")]
        public string ClientSymmetricKey { get; set; }

        #endregion

        #region Protocols

        /// <summary>
        /// Keep alive interval for a protocol. Default 180.0 seconds.  If keep alives exceeded the channel is closed.
        /// </summary>
        /// <remarks>Not required for HTTP channels and REST.</remarks>
        [JsonProperty("keepAliveSeconds")]
        public double KeepAliveSeconds { get; set; } = 180.0;

        /// <summary>
        /// Timeout for an ACK to be acknowledged. Default 2.0 seconds.
        /// </summary>
        /// <remarks>Required for CoAP and MQTT</remarks>
        [JsonProperty("ackTimeoutSeconds")]
        public double AckTimeoutSeconds { get; set; } = 2.0;

        /// <summary>
        /// Random factor multiplied to the expectation of an ACK response.  Default 1.5.
        /// </summary>
        /// <remarks>Not required for HTTP channels and REST.</remarks>
        [JsonProperty("ackRandomFactor")]
        public double AckRandomFactor { get; set; } = 1.5;

        /// <summary>
        /// Maximum number of retransmissions when an ACK is not returned.  Default 4.
        /// </summary>
        /// <remarks>Not required for HTTP channels and REST.</remarks>
        [JsonProperty("maxRetransmit")]
        public int MaxRetransmit { get; set; } = 4;

        /// <summary>
        /// Maxium latency in seconds required for an ACK.  Default 100.0 seconds.
        /// </summary>
        /// <remarks>Not required for HTTP channels and REST.</remarks>
        [JsonProperty("maxLatencySeconds")]
        public double MaxLatencySeconds { get; set; } = 100.0;

        /// <summary>
        /// CoAP only specifies whether retries should be attempted.
        /// </summary>
        [JsonProperty("autoRetry")]
        public bool AutoRetry { get; set; } = false;

        /// <summary>
        /// CoAP only sets whether the Observe Option should be implemented. Default is TRUE.
        /// </summary>
        [JsonProperty("observeOption")]
        public bool ObserveOption { get; set; } = true;

        /// <summary>
        /// CoAP only sets whether the No Response Option should be implemented.  Default is TRUE.
        /// </summary>
        [JsonProperty("noResponseOption")]
        public bool NoResponseOption { get; set; } = true;

        /// <summary>
        /// CoAP only sets NStart.  Default 1.
        /// </summary>
        [JsonProperty("nstart")]
        public int NStart { get; set; } = 1;

        /// <summary>
        /// CoAP only sets Default Leisure.  Default is 4.0.
        /// </summary>
        [JsonProperty("defaultLeisure")]
        public double DefaultLeisure { get; set; } = 4.0;

        /// <summary>
        /// CoAP only sets Probing Rate.  Default is 1.0.
        /// </summary>
        [JsonProperty("probingRate")]
        public double ProbingRate { get; set; } = 1.0;

        #endregion

        #region Logging

        [JsonProperty("loggerTypes")]
        public string LoggerTypes { get; set; } = "Console;Debug";

        [JsonProperty("logLevel")]
        public string LogLevel { get; set; } = "Warning";


        [JsonProperty("appInsightsKey")]
        public string AppInsightsKey { get; set; }


        #endregion

        public string[] GetSecurityCodes()
        {
            return ManagementApiSecurityCodes.Split(";", StringSplitOptions.RemoveEmptyEntries);
        }

        public LoggerType GetLoggerTypes()
        {
            if (string.IsNullOrEmpty(LoggerTypes))
            {
                return default(LoggerType);
            }

            string loggerTypes = LoggerTypes.Replace(";", ",");
            return Enum.Parse<LoggerType>(loggerTypes, true);
        }

        /// <summary>
        /// Gets a clone of PSK identities and keys
        /// </summary>
        /// <returns></returns>
        //public Dictionary<string, byte[]> GetPskClone()
        //{
        //    if(psks == null)
        //    {
        //        string[] pskIdentities = PskIdentities.Split(";", StringSplitOptions.RemoveEmptyEntries);
        //        string[] pskKeys = PskKeys.Split(";", StringSplitOptions.RemoveEmptyEntries);

        //        if (pskIdentities == null && pskKeys == null)
        //        {
        //            return null;
        //        }
        //        else if (pskIdentities != null && pskKeys != null && pskIdentities.Length == pskKeys.Length)
        //        {
        //            psks = new Dictionary<string, byte[]>();
        //            for (int index = 0; index < pskIdentities.Length; index++)
        //                psks.Add(pskIdentities[index], Convert.FromBase64String(pskKeys[index]));
        //        }
        //        else
        //        {
        //            throw new IndexOutOfRangeException("Service PSK identities and values out of range.");
        //        }
        //    }


        //    return DeepClone<Dictionary<string, byte[]>>(psks);
        //}

        /// <summary>
        /// Gets the client indexes
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string,string>> GetClientIndexes()
        {
            if (clientIndexes == null)
            {
                string[] claimTypes = ClientIdentityClaimTypes.Split(";", StringSplitOptions.RemoveEmptyEntries);
                string[] claimKeys = ClientIdentityClaimKeys.Split(";", StringSplitOptions.RemoveEmptyEntries);

                if (claimTypes == null && claimKeys == null)
                {
                    return null;
                }
                else if (claimTypes != null && claimKeys != null && claimTypes.Length == claimKeys.Length)
                {
                    clientIndexes = new List<KeyValuePair<string, string>>();
                    for (int index = 0; index < claimTypes.Length; index++)
                        clientIndexes.Add(new KeyValuePair<string,string>(claimTypes[index], claimKeys[index]));
                }
                else
                {
                    throw new IndexOutOfRangeException("Client claim types and values for indexing out of range.");
                }
            }

            return clientIndexes;
        }

        /// <summary>
        /// Gets the claim indexes that map to the client and can be used for an ephemeral subscription.
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        public List<KeyValuePair<string, string>>  GetClientIndexs(IEnumerable<Claim> claims)
        {
            List<KeyValuePair<string, string>> container = new List<KeyValuePair<string, string>>();

            List<KeyValuePair<string, string>> clientIndexes = GetClientIndexes();
            if(clientIndexes == null)
            {
                return null;
            }

            foreach(Claim claim in claims)
            {
                var query = clientIndexes.Where((c) => c.Key == claim.Type.ToLowerInvariant());
                foreach(KeyValuePair<string,string> kvp in query)
                {
                    container.Add(new KeyValuePair<string, string>(kvp.Value, claim.Value));
                }
            }

            if(container.Count > 0)
            {
                return container;
            }
            else
            {
                return null;
            }
            
        }


        //Gets the ports used by a gateway
        public int[] GetPorts()
        {
            string[] parts = Ports.Split(";", StringSplitOptions.RemoveEmptyEntries);
            return parts != null ? Array.ConvertAll(parts, s => int.Parse(s)) : null;
        }

        /// <summary>
        /// Get the service claims for the service identity.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Claim> GetServiceClaims()
        {
            string[] claimTypes = ServiceIdentityClaimTypes.Split(";", StringSplitOptions.RemoveEmptyEntries);
            string[] claimValues = ServiceIdentityClaimValues.Split(";", StringSplitOptions.RemoveEmptyEntries);

            if(claimTypes == null && claimValues == null)
            {
                return null;
            }
            else if(claimTypes != null && claimValues != null && claimTypes.Length == claimTypes.Length)
            {
                List<Claim> list = new List<Claim>();
                for (int index = 0; index < claimTypes.Length; index++)
                    list.Add(new Claim(claimTypes[index], claimValues[index]));

                return list;
            }
            else
            {
                throw new IndexOutOfRangeException("Service claim types and values out of range.");
            }
        }
               
        /// <summary>
        /// Gets the cliet certificate.
        /// </summary>
        /// <returns></returns>
        public X509Certificate2 GetClientCertificate()
        {
            string filename = ClientCertificateFilename ?? null;
            if(filename != null)
            {
                return new X509Certificate2(File.ReadAllBytes(filename));
            }

            string store = ClientCertificateStore ?? null;
            string location = ClientCertificateLocation ?? null;
            string thumbprint = ClientCertificateThumbprint ?? null;

            if(store != null && location != null && thumbprint != null)
            {
                return GetCertificateFromStore(store, location, thumbprint);
            }

            return null;
        }

        /// <summary>
        /// Gets the server certificate.
        /// </summary>
        /// <returns></returns>
        public X509Certificate2 GetServerCerticate()
        {
            string filename = ServerCertificateFilename ?? null;
            if (!string.IsNullOrEmpty(filename))
            {
                return new X509Certificate2(filename, ServerCertificatePassword);
            }

            string store = ServerCertificateStore ?? null;
            string location = ServerCertificateLocation ?? null;
            string thumbprint = ServerCertificateThumbprint ?? null;

            if (!string.IsNullOrEmpty(store) && !string.IsNullOrEmpty(location) && !string.IsNullOrEmpty(thumbprint))
            {
                return GetCertificateFromStore(store, location, thumbprint);
            }

            return null;

        }
                
        private X509Certificate2 GetCertificateFromStore(string store, string location, string thumbprint)
        {
            StoreName storeName = Enum.Parse<StoreName>(store, true);
            StoreLocation storeLocation = Enum.Parse<StoreLocation>(location, true);
            string thumb = thumbprint.ToUpperInvariant();

            X509Store x509Store = new X509Store(storeName, storeLocation);
            
            try
            {
                x509Store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection collection = x509Store.Certificates;
                foreach (var item in collection)
                {
                    if (item.Thumbprint == thumbprint)
                    {
                        return item;
                    }
                }

                return null;
            }
            finally
            {
                x509Store.Close();
            }
        }

        private T DeepClone<T>(T obj)
        {
            if (obj == null)
            {
                return default(T);
            }

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

    }
}
