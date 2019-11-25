using Microsoft.AspNetCore.Http;
using Orleans;
using Piraeus.Auditing;
using Piraeus.Configuration;
using Piraeus.Core.Logging;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Core.Utilities;
using Piraeus.Grains;
using SkunkLab.Channels;
using SkunkLab.Security.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Piraeus.Adapters
{
    public class RestProtocolAdapter : ProtocolAdapter
    {

        public RestProtocolAdapter(PiraeusConfig config, GraphManager graphManager, IChannel channel, HttpContext context, ILog logger = null)
        {
            this.config = config;
            this.channel = channel;
            this.logger = logger;
            method = context.Request.Method.ToUpperInvariant();
            messageUri = new MessageUri(context.Request);

            //this.graphManager = graphManager;
            IdentityDecoder decoder = new IdentityDecoder(config.ClientIdentityNameClaimType, context, config.GetClientIndexes());
            identity = decoder.Id;
            indexes = decoder.Indexes;
            adapter = new OrleansAdapter(identity, channel.TypeId, "REST", graphManager, logger);
            if (method == "GET")
            {
                adapter.OnObserve += Adapter_OnObserve;
            }
            protocolType = ProtocolType.REST;
            contentType = messageUri.ContentType;
            resource = messageUri.Resource;
            subscriptions = messageUri.Subscriptions;

            auditFactory = AuditFactory.CreateSingleton();
            if (config.AuditConnectionString != null && config.AuditConnectionString.Contains("DefaultEndpointsProtocol"))
            {
                auditFactory.Add(new AzureTableAuditor(config.AuditConnectionString, "messageaudit"), AuditType.Message);
                auditFactory.Add(new AzureTableAuditor(config.AuditConnectionString, "useraudit"), AuditType.User);
            }
            else if (config.AuditConnectionString != null)
            {
                auditFactory.Add(new FileAuditor(config.AuditConnectionString), AuditType.Message);
                auditFactory.Add(new FileAuditor(config.AuditConnectionString), AuditType.User);
            }

            messageAuditor = auditFactory.GetAuditor(AuditType.Message);
            userAuditor = auditFactory.GetAuditor(AuditType.User);
        }

        private readonly PiraeusConfig config;
        private readonly IAuditFactory auditFactory;
        private readonly IAuditor userAuditor;
        private readonly IAuditor messageAuditor;
        private readonly IEnumerable<string> subscriptions;
        private readonly List<KeyValuePair<string, string>> indexes;
        private readonly string contentType;
        private readonly string resource;
        private readonly ProtocolType protocolType;
        private readonly string identity;
        private readonly MessageUri messageUri;
        private readonly string method;
        private IChannel channel;
        private readonly ILog logger;
        //private readonly GraphManager graphManager;
        private readonly OrleansAdapter adapter;
        private bool disposed;

        public override IChannel Channel
        {
            get { return channel; }
            set { channel = value; }
        }

        public override event EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public override event EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public override event EventHandler<ChannelObserverEventArgs> OnObserve;

        public override void Init()
        {
            channel.OnOpen += Channel_OnOpen;
            channel.OnReceive += Channel_OnReceive;
            channel.ReceiveAsync().GetAwaiter();

        }
        private void Adapter_OnObserve(object sender, ObserveMessageEventArgs e)
        {
            logger?.LogDebugAsync("REST adapter received observed message");
            OnObserve?.Invoke(this, new ChannelObserverEventArgs(channel.Id, e.Message.ResourceUri, e.Message.ContentType, e.Message.Message));
            AuditRecord record = new UserAuditRecord(channel.Id, identity, DateTime.UtcNow);
            userAuditor?.UpdateAuditRecordAsync(record).Ignore();
            AuditRecord messageRecord = new MessageAuditRecord(e.Message.MessageId, identity, channel.TypeId, protocolType.ToString(), e.Message.Message.Length, MessageDirectionType.Out, true, DateTime.UtcNow);
            messageAuditor?.WriteAuditRecordAsync(messageRecord);
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            Exception error = null;

            if (method == "POST" && string.IsNullOrEmpty(resource))
            {
                error = new Exception("REST adapter cannot send message without resource.");
            }

            if (method == "POST" && string.IsNullOrEmpty(contentType))
            {
                error = new Exception("REST adapter cannot send message without content-type.");
            }

            if (method == "POST" && (e.Message == null || e.Message.Length == 0))
            {
                error = new Exception("REST adapter cannot send empty message.");
            }

            if (method == "GET" && (subscriptions == null || subscriptions.Count() == 0))
            {
                error = new Exception("REST adapter cannot subscribe to '0' subscriptions.");
            }

            if (error != null)
            {
                logger?.LogWarningAsync(error.Message).GetAwaiter();
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(channel.Id, error));
                return;
            }

            try
            {
                if (method == "POST")
                {
                    EventMessage message = new EventMessage(contentType, resource, protocolType, e.Message);
                    if (!string.IsNullOrEmpty(messageUri.CacheKey))
                    {
                        message.CacheKey = messageUri.CacheKey;
                    }

                    adapter.PublishAsync(message, indexes).GetAwaiter();
                    logger?.LogDebugAsync("REST adapter published message");
                    MessageAuditRecord record = new MessageAuditRecord(message.MessageId, identity, channel.TypeId, protocolType.ToString(), e.Message.Length, MessageDirectionType.In, true, DateTime.UtcNow);
                    messageAuditor?.WriteAuditRecordAsync(record).Ignore();
                    OnClose?.Invoke(this, new ProtocolAdapterCloseEventArgs(Channel.Id));
                }

                if (method == "GET")
                {
                    foreach (var subscription in subscriptions)
                    {
                        SubscriptionMetadata metadata = new SubscriptionMetadata()
                        {
                            Identity = identity,
                            Indexes = indexes,
                            IsEphemeral = true
                        };

                        adapter.SubscribeAsync(subscription, metadata).GetAwaiter();
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogErrorAsync($"REST adapter processing error on receive - {ex.Message}");
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(channel.Id, ex));
            }


        }

        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            AuditRecord record = new UserAuditRecord(Channel.Id, identity, config.ClientIdentityNameClaimType, Channel.TypeId, $"REST-{method}", "Granted", DateTime.UtcNow);
            userAuditor?.WriteAuditRecordAsync(record).Ignore();

            logger?.LogDebugAsync("REST adapter channel is open.").GetAwaiter();
        }


        protected void Disposing(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    try
                    {
                        if (adapter != null)
                        {
                            adapter.Dispose();
                            logger?.LogDebugAsync($"HTTP orleans adapter disposed on channel {Channel.Id}").GetAwaiter();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogErrorAsync(ex, $"REST adapter disposing orleans adapter error on channel '{Channel.Id}'.").GetAwaiter();
                    }

                    try
                    {
                        if (Channel != null)
                        {
                            string channelId = Channel.Id;
                            Channel.Dispose();
                            logger?.LogDebugAsync($"REST adapter channel {channelId} disposed").GetAwaiter();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogErrorAsync(ex, $"REST adapter Disposing channel on channel '{Channel.Id}'.").GetAwaiter();
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


    }
}
