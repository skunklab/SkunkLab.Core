using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Orleans;
using Piraeus.Auditing;
using Piraeus.Configuration.Settings;
using Piraeus.Core;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Core.Utilities;
using Piraeus.Grains;
using Piraeus.Grains.Notifications;
using SkunkLab.Channels;
using SkunkLab.Security.Identity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Threading.Tasks;

namespace Piraeus.Adapters
{
    public class RestProtocolAdapter : ProtocolAdapter
    {
        public RestProtocolAdapter(PiraeusConfig config, IChannel channel, HttpContext context, ILogger logger)
        {
            this.config = config;
            Channel = channel;
            this.context = context;

            messageAuditor = AuditFactory.CreateSingleton().GetAuditor(AuditType.Message);
            userAuditor = AuditFactory.CreateSingleton().GetAuditor(AuditType.User);
        }

        public override IChannel Channel { get; set; }

        public override event EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public override event EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public override event EventHandler<ChannelObserverEventArgs> OnObserve;

        private HttpContext context;
        private PiraeusConfig config;
        private OrleansAdapter adapter;
        private bool disposedValue;
        private IAuditor messageAuditor;
        private string identity;
        private IAuditor userAuditor;
        private bool closing;


        public override void Init()
        {
            Channel.OnOpen += Channel_OnOpen;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;

            Channel.OpenAsync().LogExceptions();
        }

        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            if (!Channel.IsAuthenticated)  //requires channel authentication
            {
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, new SecurityException("Not authenticated.")));
                Channel.CloseAsync().Ignore();
                return;
            }

            if (e.Message.Method != HttpMethod.Post && e.Message.Method != HttpMethod.Get)
            {
                Channel.CloseAsync().Ignore();
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, new SecurityException("Rest protocol adapter requires GET or POST only.")));
            }

            MessageUri uri = new MessageUri(e.Message);
            IdentityDecoder decoder = new IdentityDecoder(config.ClientIdentityNameClaimType, context, config.GetClientIndexes());
            identity = decoder.Id;

            adapter = new OrleansAdapter(decoder.Id, "HTTP", "REST");
            adapter.OnObserve += Adapter_OnObserve;
            HttpRequestMessage request = (HttpRequestMessage)e.Message;

            AuditRecord record = new UserAuditRecord(Channel.Id, identity, config.ClientIdentityNameClaimType, Channel.TypeId, String.Format("REST-{0}", request.Method.ToString()), "Granted", DateTime.UtcNow);
            userAuditor?.WriteAuditRecordAsync(record).Ignore();

            if (request.Method == HttpMethod.Get)
            {
                foreach (var item in uri.Subscriptions)
                {
                    Task t = Task.Factory.StartNew(async () =>
                    {
                        await SubscribeAsync(item, decoder.Id, decoder.Indexes);
                    });

                    t.LogExceptions();
                }
            }

            if (request.Method == HttpMethod.Post)
            {
                byte[] buffer = request.Content.ReadAsByteArrayAsync().Result;
                Task t = Task.Factory.StartNew(async () =>
                {
                    ResourceMetadata metadata = await GraphManager.GetResourceMetadataAsync(uri.Resource);
                    EventMessage message = new EventMessage(uri.ContentType, uri.Resource, ProtocolType.REST, buffer, DateTime.UtcNow, metadata.Audit);

                    if (!string.IsNullOrEmpty(uri.CacheKey))
                    {
                        message.CacheKey = uri.CacheKey;
                    }

                    List<KeyValuePair<string, string>> indexList = uri.Indexes == null ? null : new List<KeyValuePair<string, string>>(uri.Indexes);

                    await PublishAsync(decoder.Id, message, indexList);
                    await Channel.CloseAsync();
                });

                t.LogExceptions();
            }


        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {

        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, e.Error));
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            if (!closing)
            {
                closing = true;
                AuditRecord record = new UserAuditRecord(Channel.Id, DateTime.UtcNow);
                userAuditor?.WriteAuditRecordAsync(record).Ignore();

                OnClose?.Invoke(this, new ProtocolAdapterCloseEventArgs(Channel.Id));
            }
        }

        #region Adapter event
        private void Adapter_OnObserve(object sender, ObserveMessageEventArgs e)
        {
            byte[] payload = ProtocolTransition.ConvertToHttp(e.Message);
            OnObserve?.Invoke(this, new ChannelObserverEventArgs(e.Message.ResourceUri, e.Message.ContentType, payload));
        }

        #endregion

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    adapter.Dispose();
                }

                disposedValue = true;
            }
        }

        public override void Dispose()
        {

            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion


        #region private methods

        private async Task SubscribeAsync(string resourceUriString, string identity, List<KeyValuePair<string, string>> indexes)
        {
            if (await adapter.CanSubscribeAsync(resourceUriString, Channel.IsEncrypted))
            {
                SubscriptionMetadata metadata = new SubscriptionMetadata()
                {
                    Identity = identity,
                    Indexes = indexes,
                    IsEphemeral = true
                };

                string subscriptionUriString = await adapter.SubscribeAsync(resourceUriString, metadata);
                //await Log.LogInfoAsync("Identity {0} subscribed to resource {1} with subscription URI {2}", identity, resourceUriString, subscriptionUriString);
            }
            else
            {
                //await Log.LogErrorAsync("REST protocol cannot subscribe identity {0} to resource {1}", identity, resourceUriString);
            }
        }

        private async Task PublishAsync(string identity, EventMessage message, List<KeyValuePair<string, string>> indexes = null)
        {
            ResourceMetadata metadata = await GraphManager.GetResourceMetadataAsync(message.ResourceUri);

            if (await adapter.CanPublishAsync(metadata, Channel.IsEncrypted))
            {
                await adapter.PublishAsync(message, indexes);
            }
            else
            {
                if (metadata.Audit)
                {
                    await messageAuditor?.WriteAuditRecordAsync(new MessageAuditRecord("XXXXXXXXXXXX", identity, this.Channel.TypeId, "REST", message.Message.Length, MessageDirectionType.In, false, DateTime.UtcNow, "Not authorized, missing resource metadata, or channel encryption requirements"));
                }
            }
        }
        #endregion
    }
}
