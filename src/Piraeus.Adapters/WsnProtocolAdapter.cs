using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Orleans;
using Piraeus.Auditing;
using Piraeus.Configuration;
using Piraeus.Core;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Core.Utilities;
using Piraeus.Grains;
using SkunkLab.Channels;
using SkunkLab.Security.Identity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Piraeus.Adapters
{
    public class WsnProtocolAdapter : ProtocolAdapter
    {
        public WsnProtocolAdapter(PiraeusConfig config, IChannel channel, HttpContext context, ILogger logger)
        {
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
        private IAuditFactory auditFactory;
        private ILogger logger;
        private string resource;
        private string contentType;
        private string cacheKey;
        private List<KeyValuePair<string, string>> indexes;

        public override void Init()
        {
            Channel.OnOpen += Channel_OnOpen;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;

            Channel.OpenAsync().LogExceptions();
        }

        #region channel events

        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            if (!Channel.IsAuthenticated)
            {
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, new SecurityException("Not authenticated on WSN channel")));
                Channel.CloseAsync().Ignore(); //shutdown channel immediately
                return;
            }
           
            string uriString = UriHelper.GetDisplayUrl(context.Request);

            MessageUri uri = new MessageUri(uriString);
            IdentityDecoder decoder = new IdentityDecoder(config.ClientIdentityNameClaimType, context, config.GetClientIndexes());
            identity = decoder.Id;
            resource = uri.Resource;
            indexes = uri.Indexes == null ? null : new List<KeyValuePair<string, string>>(uri.Indexes);
            var localIndexes = decoder.Indexes;

            adapter = new OrleansAdapter(decoder.Id, "WebSocket", "WSN");
            adapter.OnObserve += Adapter_OnObserve;
            foreach(var sub in uri.Subscriptions)
            {
                //subscribe
                SubscriptionMetadata metadata = new SubscriptionMetadata()
                {
                    Identity = identity,
                    Indexes = localIndexes,
                    IsEphemeral = true
                };

                SubscribeAsync(uri.Resource, metadata).GetAwaiter();
                
            }

        }

        private async Task SubscribeAsync(string resource, SubscriptionMetadata metadata)
        {
            await adapter.SubscribeAsync(resource, metadata);
        }

        private void Adapter_OnObserve(object sender, ObserveMessageEventArgs e)
        {  
            MessageAuditRecord record = null;
            int length = 0;
            DateTime sendTime = DateTime.UtcNow;
            try
            {
                byte[] message = ProtocolTransition.ConvertToHttp(e.Message);
                Send(message).LogExceptions();

                length = message.Length;
                record = new MessageAuditRecord(e.Message.MessageId, identity, this.Channel.TypeId, "WSN", length, MessageDirectionType.Out, true, sendTime);
            }
            catch (Exception ex)
            {
                string msg = String.Format("{0} - WSN adapter observe error on channel '{1}' with '{2}'", DateTime.UtcNow.ToString("yyyy-MM-ddTHH-MM-ss.fffff"), Channel.Id, ex.Message);
                logger?.LogError(ex, $"WSN adapter observe error on channel '{Channel.Id}'.");
                record = new MessageAuditRecord(e.Message.MessageId, identity, this.Channel.TypeId, "WSN", length, MessageDirectionType.Out, true, sendTime, msg);
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
                logger?.LogError(ex, $"WSN adapter send error on channel '{Channel.Id}'.");
            }
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            var metadata = GraphManager.GetPiSystemMetadataAsync(resource).GetAwaiter().GetResult();

            EventMessage msg = new EventMessage(contentType, resource, ProtocolType.WSN, e.Message, DateTime.UtcNow, metadata.Audit);
            msg.CacheKey = cacheKey;

            adapter.PublishAsync(msg, indexes).GetAwaiter();
        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            logger?.LogError(e.Error, $"WSN adapter Channel_OnError error on channel '{Channel.Id}'.");
            OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, e.Error));
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            try
            {
                if (!closing)
                {
                    closing = true;
                    UserAuditRecord record = new UserAuditRecord(Channel.Id, identity, DateTime.UtcNow);
                    userAuditor?.UpdateAuditRecordAsync(record).IgnoreException();
                }

                OnClose?.Invoke(this, new ProtocolAdapterCloseEventArgs(e.ChannelId));
            }
            catch
            {

            }
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
    }
}
