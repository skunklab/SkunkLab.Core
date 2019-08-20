//using Microsoft.Azure.DataLake.Store;
//using Microsoft.Azure.Management.DataLake.Store;
//using Microsoft.Rest;
//using Microsoft.Rest.Azure.Authentication;
//using Piraeus.Core.Messaging;
//using Piraeus.Core.Metadata;
//using SkunkLab.Protocols.Coap;
//using SkunkLab.Protocols.Mqtt;
//using System;
//using Piraeus.Auditing;
//using System.Collections.Concurrent;
//using System.Collections.Specialized;
//using System.Diagnostics;
//using System.IO;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Web;


//namespace Piraeus.Grains.Notifications
//{
//    public class DataLakeSink : EventSink
//    {
//        private IAuditor auditor;
//        private string appId;
//        private string secret;
//        private string domain;
//        private string account;
//        private string folder;
//        private string filename;
//        private string subscriptionUriString;
//        private Uri ADL_TOKEN_AUDIENCE = new System.Uri(@"https://datalake.azure.net/");
//        private int clientCount;
//        //private DataLakeStoreFileSystemManagementClient[] clients;
//        private int arrayIndex;
//        private DateTime lastLogin;
//        private AdlsClient[] clients;
//        private Uri uri;
//        private TaskQueue tqueue;
//        //private ConcurrentQueue<EventMessage> queue;
//        private ConcurrentQueueManager cqm;

//        /// <summary>
//        /// Creates Azure Data Lake notification 
//        /// </summary>
//        /// <param name="contentType"></param>
//        /// <param name="messageId"></param>
//        /// <param name="metadata"></param>
//        /// <remarks>adl://host.azuredatalakestore.net?appid=id&tenantid=id&secret=token&folder=name</remarks>
//        public DataLakeSink(SubscriptionMetadata metadata)
//            : base(metadata)
//        {
//            tqueue = new TaskQueue();
//            cqm = new ConcurrentQueueManager();
//            auditor = AuditFactory.CreateSingleton().GetAuditor(AuditType.Message);
//            uri = new Uri(metadata.NotifyAddress);

//            try
//            {
//                subscriptionUriString = metadata.SubscriptionUriString;

//                Uri uri = new Uri(metadata.NotifyAddress);
//                account = uri.Authority.Replace(".azuredatalakestore.net", "");
//                NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
//                appId = nvc["appid"];
//                domain = nvc["domain"];
//                folder = nvc["folder"];
//                filename = nvc["file"];


//                if (!int.TryParse(nvc["clients"], out clientCount))
//                {
//                    clientCount = 1;
//                }

//                secret = metadata.SymmetricKey;


//            }
//            catch (Exception ex)
//            {
//                Trace.TraceWarning("Azure data lake subscription client not created for {0}", subscriptionUriString);
//                Trace.TraceError("Azure data lake subscription {0} ctor error {1}", subscriptionUriString, ex.Message);
//            }
//        }


//        public override async Task SendAsync(EventMessage message)
//        {
//            AuditRecord record = null;
//            byte[] payload = null;
//            string mid = null;
//            int len = 0;

//            await tqueue.Enqueue(() => cqm.EnqueueAsync(message));

//            if (clients == null)
//            {
//                await CreateClientsAsync();
//            }

//            if (DateTime.UtcNow.Subtract(lastLogin).TotalHours > 1.0)
//            {
//                await CreateClientsAsync();
//            }

//            try
//            {
//                while (!cqm.IsEmpty)
//                {
//                    EventMessage msg = await cqm.DequeueAsync();
//                    mid = msg.MessageId;

//                    if (msg == null || msg.Message.Length == 0)
//                    {
//                        break;
//                    }

//                    arrayIndex = arrayIndex.RangeIncrement(0, clientCount - 1);
//                    payload = GetPayload(msg);
//                    len = payload.Length;
//                    string path = GetPath(msg.ContentType);

//                    if (!string.IsNullOrEmpty(filename))
//                    {
//                        byte[] suffix = Encoding.UTF8.GetBytes(Environment.NewLine);
//                        byte[] buffer = new byte[payload.Length + suffix.Length];
//                        Buffer.BlockCopy(payload, 0, buffer, 0, payload.Length);
//                        Buffer.BlockCopy(suffix, 0, buffer, payload.Length, suffix.Length);
//                        payload = buffer;
//                    }

//                    if (string.IsNullOrEmpty(filename))
//                    {
//                        if (clients[arrayIndex].CheckExists(path))
//                        {
//                            path = GetPath(msg.ContentType, true);
//                        }

//                        Task task = WriteDiscreteAsync(arrayIndex, path, payload);
//                        Task innerTask = task.ContinueWith(async (a) => { await FaultTask(msg.MessageId, payload, msg.ContentType, auditor.CanAudit && msg.Audit); }, TaskContinuationOptions.OnlyOnFaulted);
//                        await Task.WhenAll(task);
//                    }
//                    else
//                    {
//                        Task task = clients[arrayIndex].ConcurrentAppendAsync(path, true, payload, 0, payload.Length);
//                        Task innerTask = task.ContinueWith(async (a) => { await FaultTask(msg.MessageId, payload, msg.ContentType, auditor.CanAudit && msg.Audit); }, TaskContinuationOptions.OnlyOnFaulted);
//                        await Task.WhenAll(task);
//                    }

//                    if (message.Audit)
//                    {                        
//                        record = new MessageAuditRecord(message.MessageId, String.Format($"{uri.Scheme}://{uri.Authority}/{domain}/{folder}"), "AzureDataLake", "AzureDataLake", payload.Length, MessageDirectionType.Out, true, DateTime.UtcNow);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                record = new MessageAuditRecord(message.MessageId, String.Format($"{uri.Scheme}://{uri.Authority}/{domain}/{folder}"), "AzureDataLake", "AzureDataLake", payload.Length, MessageDirectionType.Out, false, DateTime.UtcNow, ex.Message);
//                throw;
//            }
//            finally
//            {
//                if (message.Audit && record != null)
//                {
//                    await auditor?.WriteAuditRecordAsync(record);
//                }
//            }
//        }


//        private async Task WriteDiscreteAsync(int index, string path, byte[] payload)
//        {
//            try
//            {
//                using (var stream = await clients[index].CreateFileAsync(path, IfExists.Fail))
//                {
//                    await stream.WriteAsync(payload, 0, payload.Length);
//                    await stream.FlushAsync();
//                    stream.Close();
//                }
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceError(ex.Message);
//                string[] parts = path.Split(new char[] { '.' });
//                string path2 = parts.Length == 2 ? String.Format("{0}-1.{1}", parts[0], parts[1]) : String.Format("{0}-1", path);

//                using (var stream = await clients[arrayIndex].CreateFileAsync(path2, IfExists.Fail))
//                {
//                    await stream.WriteAsync(payload, 0, payload.Length);
//                    await stream.FlushAsync();
//                    stream.Close();
//                }
//            }
//        }

//        private async Task FaultTask(string id, byte[] payload, string contentType, bool canAudit)
//        {
//            Trace.TraceWarning("Entering fault data lake write with {0} file", string.IsNullOrEmpty(filename) ? "discrete" : "apppend");
//            AuditRecord record = null;
//            string path = GetPath(contentType, true);
//            string[] parts = path.Split(new char[] { '.' });
//            string path2 = parts.Length == 2 ? String.Format("{0}-R.{1}", parts[0], parts[1]) : String.Format("{0}-R", path);
//            try
//            {
//                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

//                var serviceSettings = ActiveDirectoryServiceSettings.Azure;
//                serviceSettings.TokenAudience = ADL_TOKEN_AUDIENCE;

//                var creds = await ApplicationTokenProvider.LoginSilentAsync(
//                 domain,
//                 appId,
//                 secret,
//                 serviceSettings);

//                lastLogin = DateTime.UtcNow;
//                AdlsClient client = AdlsClient.CreateClient(String.Format("{0}.azuredatalakestore.net", account), creds);

//                if (!string.IsNullOrEmpty(filename))
//                {
//                    byte[] suffix = Encoding.UTF8.GetBytes(Environment.NewLine);
//                    byte[] buffer = new byte[payload.Length + suffix.Length];
//                    Buffer.BlockCopy(payload, 0, buffer, 0, payload.Length);
//                    Buffer.BlockCopy(suffix, 0, buffer, payload.Length, suffix.Length);
//                    payload = buffer;
//                }

//                if (string.IsNullOrEmpty(filename))
//                {
//                    Trace.TraceInformation("Writing last retry for Data Lake discrete file {0}", path2);
//                    using (var stream = await client.CreateFileAsync(path2, IfExists.Fail))
//                    {
//                        await stream.WriteAsync(payload, 0, payload.Length);
//                        await stream.FlushAsync();
//                        stream.Close();
//                    }
//                }
//                else
//                {
//                    Trace.TraceInformation("Writing last retry for Data Lake append file {0}", path2);
//                    await client.ConcurrentAppendAsync(path, true, payload, 0, payload.Length);
//                }

//                record = new AuditRecord(id, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "AzureDataLake", "AzureDataLake", payload.Length, MessageDirectionType.Out, true, DateTime.UtcNow);

//            }
//            catch (Exception ex)
//            {
//                Trace.TraceWarning("Retry Data Lake failed.");
//                Trace.TraceError(ex.Message);
//                record = new AuditRecord(id, uri.Query.Length > 0 ? uri.ToString().Replace(uri.Query, "") : uri.ToString(), "AzureDataLake", "AzureDataLake", payload.Length, MessageDirectionType.Out, false, DateTime.UtcNow, ex.Message);
//            }
//            finally
//            {
//                if (canAudit)
//                {
//                    await auditor.WriteAuditRecordAsync(record);
//                }
//            }


//        }

//        private string GetFilename(string contentType, bool microseconds = false)
//        {
//            string suffix = null;
//            if (contentType.Contains("text"))
//            {
//                suffix = "txt";
//            }
//            else if (contentType.Contains("json"))
//            {
//                suffix = "json";
//            }
//            else if (contentType.Contains("xml"))
//            {
//                suffix = "xml";
//            }

//            string name = null;

//            if (!microseconds)
//            {
//                name = string.IsNullOrEmpty(filename) ? DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss-fffff") : filename;
//            }
//            else
//            {
//                name = string.IsNullOrEmpty(filename) ? DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss-ffffff") : filename;
//            }

//            //string name = string.IsNullOrEmpty(filename) ? DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss-fffff") : filename;
//            return suffix == null ? filename : String.Format("{0}.{1}", name, suffix);
//        }


//        private byte[] GetPayload(EventMessage message)
//        {
//            switch (message.Protocol)
//            {
//                case ProtocolType.COAP:
//                    CoapMessage coap = CoapMessage.DecodeMessage(message.Message);
//                    return coap.Payload;
//                case ProtocolType.MQTT:
//                    MqttMessage mqtt = MqttMessage.DecodeMessage(message.Message);
//                    return mqtt.Payload;
//                case ProtocolType.REST:
//                    return message.Message;
//                case ProtocolType.WSN:
//                    return message.Message;
//                default:
//                    return null;
//            }
//        }



//        private string GetPath(string contentType, bool microseconds = false)
//        {
//            if (filename != null)
//            {
//                return String.Format("/{0}/{1}", folder, filename);
//            }
//            else
//            {
//                return String.Format("/{0}/{1}", folder, GetFilename(contentType, microseconds));
//            }
//        }


//        private async Task CreateClientsAsync()
//        {
//            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

//            var serviceSettings = ActiveDirectoryServiceSettings.Azure;
//            serviceSettings.TokenAudience = ADL_TOKEN_AUDIENCE;

//            var creds = await ApplicationTokenProvider.LoginSilentAsync(
//             domain,
//             appId,
//             secret,
//             serviceSettings);

//            lastLogin = DateTime.UtcNow;
//            clients = new AdlsClient[clientCount];

//            for (int i = 0; i < clientCount; i++)
//            {
//                clients[i] = AdlsClient.CreateClient(String.Format("{0}.azuredatalakestore.net", account), creds);
//            }


//        }


//        private async Task<ServiceClientCredentials> GetCreds_SPI_SecretKeyAsync(string tenant, Uri tokenAudience, string clientId, string secretKey)
//        {
//            try
//            {
//                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

//                var serviceSettings = ActiveDirectoryServiceSettings.Azure;
//                serviceSettings.TokenAudience = tokenAudience;

//                var creds = await ApplicationTokenProvider.LoginSilentAsync(tenant, clientId, secretKey, serviceSettings);
//                return creds;
//            }
//            catch (AggregateException ex)
//            {
//                throw ex;
//            }
//        }




//    }
//}

