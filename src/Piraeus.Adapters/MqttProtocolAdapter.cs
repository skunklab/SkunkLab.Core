using Piraeus.Configuration.Settings;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
using Piraeus.Grains.Notifications;
using SkunkLab.Channels;
using SkunkLab.Channels.Udp;
using SkunkLab.Protocols.Mqtt;
using SkunkLab.Protocols.Mqtt.Handlers;
using SkunkLab.Security.Authentication;
using SkunkLab.Security.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;
using System.Threading.Tasks;
using Piraeus.Core;
using Orleans;
using SkunkLab.Channels.WebSocket;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Piraeus.Auditing;

namespace Piraeus.Adapters
{
    public class MqttProtocolAdapter : ProtocolAdapter
    {
        public MqttProtocolAdapter(PiraeusConfig config, IAuthenticator authenticator, IChannel channel, ILogger logger, HttpContext context = null)
        {
            this.config = config;
            this.logger = logger;

            MqttConfig mqttConfig = new MqttConfig(config.KeepAliveSeconds, config.AckTimeoutSeconds, config.AckRandomFactor, config.MaxRetransmit, config.MaxLatencySeconds, authenticator, config.ClientIdentityNameClaimType, config.GetClientIndexes());


            this.context = context;
            session = new MqttSession(mqttConfig);
            userAuditor = AuditFactory.CreateSingleton().GetAuditor(AuditType.User);
            messageAuditor = AuditFactory.CreateSingleton().GetAuditor(AuditType.Message);


            Channel = channel;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
            Channel.OnStateChange += Channel_OnStateChange;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnOpen += Channel_OnOpen;
        }

        public override event System.EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public override event System.EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public override event System.EventHandler<ChannelObserverEventArgs> OnObserve;

        private ILogger logger;
        private HttpContext context;
        private IAuditor messageAuditor;
        private MqttSession session;
        private bool disposed;
        private OrleansAdapter adapter;
        private readonly PiraeusConfig config;
        private bool forcePerReceiveAuthn;
        private IAuditor userAuditor;
        private bool closing;


        public override IChannel Channel { get; set; }

        public override void Init()
        {
            Trace.TraceInformation("{0} - MQTT Protocol Adapter intialization on Channel '{1}'.", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id);

            messageAuditor = AuditFactory.CreateSingleton().GetAuditor(AuditType.Message);

            forcePerReceiveAuthn = Channel as UdpChannel != null;
            session.OnPublish += Session_OnPublish;
            session.OnSubscribe += Session_OnSubscribe;
            session.OnUnsubscribe += Session_OnUnsubscribe;
            session.OnDisconnect += Session_OnDisconnect; ;
            session.OnConnect += Session_OnConnect;
            logger?.LogInformation($"MQTT adpater on channel '{Channel.Id}' is initialized.");
        }

        #region Orleans Adapter Events
        private void Adapter_OnObserve(object sender, ObserveMessageEventArgs e)
        {
            MessageAuditRecord record = null;
            int length = 0;
            DateTime sendTime = DateTime.UtcNow;
            try
            {
                byte[] message = ProtocolTransition.ConvertToMqtt(session, e.Message);
                Send(message).LogExceptions();

                MqttMessage mm = MqttMessage.DecodeMessage(message);

                length = mm.Payload.Length;
                record = new MessageAuditRecord(e.Message.MessageId, session.Identity, this.Channel.TypeId, "MQTT", length, MessageDirectionType.Out, true, sendTime);
            }
            //catch(AggregateException ae)
            //{
            //    string msg = String.Format("{0} - MQTT adapter observe error on channel '{1}' with '{2}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id, ae.Flatten().InnerException.Message);
            //    Trace.TraceError(msg);
            //    record = new AuditRecord(e.Message.MessageId, session.Identity, this.Channel.TypeId, "MQTT", length, MessageDirectionType.Out, true, sendTime, msg);
            //}
            catch(Exception ex)
            {
                string msg = String.Format("{0} - MQTT adapter observe error on channel '{1}' with '{2}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id, ex.Message);
                logger.LogError(ex, $"MQTT adapter observe error on channel '{Channel.Id}'.");
                record = new MessageAuditRecord(e.Message.MessageId, session.Identity, this.Channel.TypeId, "MQTT", length, MessageDirectionType.Out, true, sendTime, msg);
            }
            finally
            {
                if (e.Message.Audit)
                {
                    messageAuditor?.WriteAuditRecordAsync(record).Ignore();
                }
            }
        }

        private async Task Send(byte[] message)
        {
            try
            {
                await Channel.SendAsync(message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"MQTT adapter send error on channel '{Channel.Id}'.");
            }
        }

        #endregion

        #region MQTT Session Events
        private void Session_OnConnect(object sender, MqttConnectionArgs args)
        {
            try
            {
                adapter.LoadDurableSubscriptionsAsync(session.Identity).GetAwaiter();                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"MQTT adapter Session_OnConnect error on channel '{Channel.Id}'.");
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }
        }

        private void Session_OnDisconnect(object sender, MqttMessageEventArgs args)
        {
            logger?.LogInformation($"MQTT adapter Session_OnDisconnect on channel '{Channel.Id}'.");
            OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, new DisconnectException(String.Format("MQTT adapter on channel {0} has been disconnected.", Channel.Id))));            
        }

        private void Session_OnUnsubscribe(object sender, MqttMessageEventArgs args)
        {
            try
            {
                UnsubscribeMessage msg = (UnsubscribeMessage)args.Message;
                foreach (var item in msg.Topics)
                {
                    MqttUri uri = new MqttUri(item.ToLowerInvariant());
                    if (adapter.CanSubscribeAsync(uri.Resource, Channel.IsEncrypted).GetAwaiter().GetResult())
                    {
                        adapter.UnsubscribeAsync(uri.Resource).GetAwaiter();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"MQTT adapter Session_OnUnsubscribe error on channel '{Channel.Id}'.");
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }
        }

        private List<string> Session_OnSubscribe(object sender, MqttMessageEventArgs args)
        {
            List<string> list = new List<string>();

            try
            {
                SubscribeMessage message = args.Message as SubscribeMessage;

                SubscriptionMetadata metadata = new SubscriptionMetadata()
                {
                    Identity = session.Identity,
                    Indexes = session.Indexes,
                    IsEphemeral = true
                };

                foreach (var item in message.Topics)
                {
                    MqttUri uri = new MqttUri(item.Key);
                    string resourceUriString = uri.Resource;

                    Task<bool> t = CanSubscribe(resourceUriString);
                    bool subscribe = t.Result;

                    if (subscribe)
                    {
                        Task<string> subTask = Subscribe(resourceUriString, metadata);
                        string subscriptionUriString = subTask.Result;
                        list.Add(resourceUriString);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"MQTT adapter Session_OnSubscribe error on channel '{Channel.Id}'.");
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }

            return list;
        }

        private Task<string> Subscribe(string resourceUriString, SubscriptionMetadata metadata)
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            Task t = Task.Factory.StartNew(async () =>
            {
                try
                {
                    string id = await adapter.SubscribeAsync(resourceUriString, metadata);
                    tcs.SetResult(id);
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, $"MQTT adapter Subscribe error on channel '{Channel.Id}'.");
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        private Task<bool> CanSubscribe(string resourceUriString)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            Task t = Task.Factory.StartNew(async () =>
            {
                try
                {
                    bool r = await adapter.CanSubscribeAsync(resourceUriString, Channel.IsEncrypted);
                    tcs.SetResult(r);
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, $"MQTT adapter CanSubscribe error on channel '{Channel.Id}'.");
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        private void Session_OnPublish(object sender, MqttMessageEventArgs args)
        {
            try
            {
                PublishMessage message = args.Message as PublishMessage;
                PublishAsync(message).GetAwaiter();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"MQTT adapter Session_OnPublish  error on channel '{Channel.Id}'.");
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }
        }

        private async Task PublishAsync(PublishMessage message)
        {
            MessageAuditRecord record = null;
            ResourceMetadata metadata = null;

            try
            {
                MqttUri mqttUri = new MqttUri(message.Topic);
                metadata = await GraphManager.GetResourceMetadataAsync(mqttUri.Resource);
                if (await adapter.CanPublishAsync(metadata, Channel.IsEncrypted))
                {
                    EventMessage msg = new EventMessage(mqttUri.ContentType, mqttUri.Resource, ProtocolType.MQTT, message.Encode(), DateTime.UtcNow, metadata.Audit);
                    if (!string.IsNullOrEmpty(mqttUri.CacheKey))
                    {
                        msg.CacheKey = mqttUri.CacheKey;
                    }

                    await adapter.PublishAsync(msg, null);
                }
                else
                {
                    if (metadata.Audit)
                    {
                        record = new MessageAuditRecord("XXXXXXXXXXXX", session.Identity, this.Channel.TypeId, "MQTT", message.Payload.Length, MessageDirectionType.In, false, DateTime.UtcNow, "Not authorized, missing resource metadata, or channel encryption requirements");
                    }

                    throw new SecurityException(String.Format("'{0}' not authorized to publish to '{1}'", session.Identity, metadata.ResourceUriString));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"MQTT adapter PublishAsync error on channel '{Channel.Id}'.");
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }
            finally
            {
                if (metadata != null && metadata.Audit && record != null)
                {
                    await messageAuditor?.WriteAuditRecordAsync(record);
                }
            }

        }

        #endregion

        #region Channel Events
        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            try
            {
                session.IsAuthenticated = Channel.IsAuthenticated;
                if(session.IsAuthenticated)                
                {
                    IdentityDecoder decoder = new IdentityDecoder(session.Config.IdentityClaimType, context, session.Config.Indexes);
                    session.Identity = decoder.Id;
                    session.Indexes = decoder.Indexes;

                    UserAuditRecord record = new UserAuditRecord(Channel.Id, session.Identity, session.Config.IdentityClaimType, Channel.TypeId, "MQTT", "Granted", DateTime.UtcNow);
                    userAuditor?.WriteAuditRecordAsync(record).Ignore();
                }

                adapter = new OrleansAdapter(session.Identity, Channel.TypeId, "MQTT", context);
                adapter.OnObserve += Adapter_OnObserve;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"MQTT adapter Channel_OnOpen error on channel '{Channel.Id}'.");
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            try
            {
                MqttMessage msg = MqttMessage.DecodeMessage(e.Message);
                OnObserve?.Invoke(this, new ChannelObserverEventArgs(null, null, e.Message));

                if (!session.IsAuthenticated)
                {
                    ConnectMessage message = msg as ConnectMessage;
                    if (message == null)
                    {
                        throw new SecurityException("Connect message not first message");
                    }

                    if (session.Authenticate(message.Username, message.Password))
                    {
                        IdentityDecoder decoder = new IdentityDecoder(session.Config.IdentityClaimType, context, session.Config.Indexes);
                        session.Identity = decoder.Id;
                        session.Indexes = decoder.Indexes;
                        adapter.Identity = decoder.Id;

                        UserAuditRecord record = new UserAuditRecord(Channel.Id, session.Identity, session.Config.IdentityClaimType, Channel.TypeId, "MQTT", "Granted", DateTime.UtcNow);
                        userAuditor?.WriteAuditRecordAsync(record).Ignore();
                    }
                    else
                    {
                        throw new SecurityException("Session could not be authenticated.");
                    }
                }
                else if (forcePerReceiveAuthn)
                {
                    if (!session.Authenticate())
                    {
                        throw new SecurityException("Per receive authentication failed.");
                    }
                }

                ProcessMessageAsync(msg).GetAwaiter();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"MQTT adapter Channel_OnReceive error on channel '{Channel.Id}'.");
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }
        }

        private async Task ProcessMessageAsync(MqttMessage message)
        {
            try
            {
                MqttMessageHandler handler = MqttMessageHandler.Create(session, message);
                MqttMessage msg = await handler.ProcessAsync();

                if (msg != null)
                {
                    await Channel.SendAsync(msg.Encode());
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"MQTT adapter ProcessMessageAsync error on channel '{Channel.Id}'.");
                OnError.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }
        }

        private void Channel_OnStateChange(object sender, ChannelStateEventArgs e)
        {
            logger?.LogInformation($"MQTT adapter Channel_OnStateChange to '{e.State}' on channel '{Channel.Id}'.");
        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            logger.LogError(e.Error, $"MQTT adapter Channel_OnError error on channel '{Channel.Id}'.");
            OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, e.Error));
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            try
            {                
                if (!closing)
                {
                    closing = true;
                    UserAuditRecord record = new UserAuditRecord(Channel.Id, DateTime.UtcNow);
                    userAuditor?.WriteAuditRecordAsync(record).Ignore();
                }

                OnClose?.Invoke(this, new ProtocolAdapterCloseEventArgs(e.ChannelId));
            }
            catch
            {

            }
        }

        #endregion

        #region Dispose 
        protected void Disposing(bool disposing)
        {
            if (!disposed)
            {
                Trace.TraceInformation("MQTT Protocol Adapter disposing on Channel '{0}'.", Channel.Id);
                if (disposing)
                {

                    try
                    {
                        if (adapter != null)
                        {
                            adapter.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"MQTT adapter Disposing orleans adapter error on channel '{Channel.Id}'.");
                    }

                    try
                    {
                        if (Channel != null)
                        {                          
                            Channel.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"MQTT adapter Disposing channel on channel '{Channel.Id}'.");
                    }

                    try
                    {
                        if (session != null)
                        {
                            session.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"MQTT adapter Disposing session on channel '{Channel.Id}'.");
                    }
                }
                disposed = true;
            }
        }

        public override void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
