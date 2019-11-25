using Microsoft.AspNetCore.Http;
using Orleans;
using Piraeus.Auditing;
using Piraeus.Configuration;
using Piraeus.Core;
using Piraeus.Core.Logging;
using Piraeus.Grains;
using SkunkLab.Channels;
using SkunkLab.Channels.Udp;
using SkunkLab.Protocols.Coap;
using SkunkLab.Protocols.Coap.Handlers;
using SkunkLab.Security.Authentication;
using SkunkLab.Security.Identity;
using System;
using System.Threading.Tasks;

namespace Piraeus.Adapters
{
    public class CoapProtocolAdapter : ProtocolAdapter
    {
        public CoapProtocolAdapter(PiraeusConfig config, GraphManager graphManager, IAuthenticator authenticator, IChannel channel, ILog logger, HttpContext context = null)
        {
            this.context = context;
            this.graphManager = graphManager;
            this.logger = logger;
            this.config = config;

            CoapConfigOptions options = config.ObserveOption && config.NoResponseOption ? CoapConfigOptions.Observe | CoapConfigOptions.NoResponse : config.ObserveOption ? CoapConfigOptions.Observe : config.NoResponseOption ? CoapConfigOptions.NoResponse : CoapConfigOptions.None;
            CoapConfig coapConfig = new CoapConfig(authenticator, config.CoapAuthority, options, config.AutoRetry,
                config.KeepAliveSeconds, config.AckTimeoutSeconds, config.AckRandomFactor,
                config.MaxRetransmit, config.NStart, config.DefaultLeisure, config.ProbingRate, config.MaxLatencySeconds);
            coapConfig.IdentityClaimType = config.ClientIdentityNameClaimType;
            coapConfig.Indexes = config.GetClientIndexes();

            InitializeAuditor(config);
            logger?.LogDebugAsync("CoAP protocol auditor initialized.").GetAwaiter();

            Channel = channel;
            Channel.OnClose += Channel_OnClose;
            Channel.OnError += Channel_OnError;
            Channel.OnOpen += Channel_OnOpen;
            Channel.OnReceive += Channel_OnReceive;
            Channel.OnStateChange += Channel_OnStateChange;
            session = new CoapSession(coapConfig, context);
            logger?.LogDebugAsync("CoAP protocol session open.").GetAwaiter();

            if (Channel.State != ChannelState.Open)
            {
                Channel.OpenAsync().GetAwaiter();
                Channel.ReceiveAsync();
                logger?.LogDebugAsync("CoAP protocol channel opened and receiving.").GetAwaiter();
            }
        }


        #region public members
        public override IChannel Channel { get; set; }

        public override event System.EventHandler<ProtocolAdapterErrorEventArgs> OnError;
        public override event System.EventHandler<ProtocolAdapterCloseEventArgs> OnClose;
        public override event System.EventHandler<ChannelObserverEventArgs> OnObserve;
        #endregion



        private readonly GraphManager graphManager;
        private readonly ILog logger;
        private readonly HttpContext context;
        private readonly CoapSession session;
        private ICoapRequestDispatch dispatcher;
        private bool disposed;
        private bool forcePerReceiveAuthn;
        private IAuditor userAuditor;
        private bool closing;
        private IAuditFactory auditFactory;
        private readonly PiraeusConfig config;




        #region init
        public override void Init()
        {
            forcePerReceiveAuthn = Channel as UdpChannel != null;
            Channel.OpenAsync().GetAwaiter();
            logger?.LogDebugAsync($"CoAP adapter on channel '{Channel.Id}' is initialized.").GetAwaiter();
        }

        #endregion

        #region events

        private void Channel_OnOpen(object sender, ChannelOpenEventArgs e)
        {
            session.IsAuthenticated = Channel.IsAuthenticated;
            logger?.LogDebugAsync($"CoAP protocol channel opening with session authenticated '{session.IsAuthenticated}'.").GetAwaiter();

            try
            {
                if (!Channel.IsAuthenticated && e.Message != null)
                {
                    CoapMessage msg = CoapMessage.DecodeMessage(e.Message);
                    CoapUri coapUri = new CoapUri(msg.ResourceUri.ToString());
                    session.IsAuthenticated = session.Authenticate(coapUri.TokenType, coapUri.SecurityToken);
                    logger?.LogDebugAsync($"CoAP protocol channel opening session authenticated '{session.IsAuthenticated}' by authenticator.").GetAwaiter();
                }

                if (session.IsAuthenticated)
                {
                    IdentityDecoder decoder = new IdentityDecoder(session.Config.IdentityClaimType, context, session.Config.Indexes);
                    session.Identity = decoder.Id;
                    session.Indexes = decoder.Indexes;
                    logger?.LogDebugAsync($"CoAP protocol channel opening with session identity '{session.Identity}'.").GetAwaiter();

                    UserAuditRecord record = new UserAuditRecord(Channel.Id, session.Identity, session.Config.IdentityClaimType, Channel.TypeId, "COAP", "Granted", DateTime.UtcNow);
                    userAuditor?.WriteAuditRecordAsync(record).Ignore();
                }
            }
            catch (Exception ex)
            {
                logger?.LogErrorAsync(ex, $"CoAP adapter opening channel '{Channel.Id}'.").GetAwaiter();
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
            }

            if (!session.IsAuthenticated && e.Message != null)
            {
                //close the channel
                logger?.LogWarningAsync("CoAP adpater closing due to unauthenticated user.");
                Channel.CloseAsync().Ignore();
            }
            else
            {
                dispatcher = new CoapRequestDispatcher(session, Channel, config, graphManager, this.logger);
            }
        }

        private void Channel_OnReceive(object sender, ChannelReceivedEventArgs e)
        {
            try
            {
                CoapMessage message = CoapMessage.DecodeMessage(e.Message);

                if (!session.IsAuthenticated || forcePerReceiveAuthn)
                {
                    session.EnsureAuthentication(message, forcePerReceiveAuthn);
                    dispatcher.Identity = session.Identity;

                    UserAuditRecord record = new UserAuditRecord(Channel.Id, session.Identity, session.Config.IdentityClaimType, Channel.TypeId, "COAP", "Granted", DateTime.UtcNow);
                    userAuditor?.WriteAuditRecordAsync(record).Ignore();

                }

                OnObserve?.Invoke(this, new ChannelObserverEventArgs(this.Channel.Id, message.ResourceUri.ToString(), MediaTypeConverter.ConvertFromMediaType(message.ContentType), message.Payload));

                Task task = Task.Factory.StartNew(async () =>
                {

                    CoapMessageHandler handler = CoapMessageHandler.Create(session, message, dispatcher);
                    CoapMessage msg = await handler.ProcessAsync();

                    if (msg != null)
                    {
                        byte[] payload = msg.Encode();
                        await Channel.SendAsync(payload);
                    }

                });

                task.LogExceptions();
            }
            catch (Exception ex)
            {
                logger?.LogErrorAsync(ex, $"CoAP adapter receiveing on channel '{Channel.Id}'.").GetAwaiter();
                OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, ex));
                Channel.CloseAsync().Ignore();
            }

        }

        private void Channel_OnError(object sender, ChannelErrorEventArgs e)
        {
            logger?.LogErrorAsync(e.Error, "CoAP adapter error on channel.");
            OnError?.Invoke(this, new ProtocolAdapterErrorEventArgs(Channel.Id, e.Error));
        }

        private void Channel_OnClose(object sender, ChannelCloseEventArgs e)
        {
            if (!closing)
            {
                closing = true;

                logger?.LogWarningAsync("CoAP adapter closing channel.");

                UserAuditRecord record = new UserAuditRecord(Channel.Id, session.Identity, DateTime.UtcNow);
                userAuditor?.UpdateAuditRecordAsync(record).Ignore();

                OnClose?.Invoke(this, new ProtocolAdapterCloseEventArgs(Channel.Id));
            }
        }

        private void Channel_OnStateChange(object sender, ChannelStateEventArgs e)
        {
            logger?.LogDebugAsync($"CoAP adapter channel state changed to {e.State.ToString()}");
        }


        #endregion

        #region dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {

                    try
                    {
                        if (dispatcher != null)
                        {
                            dispatcher.Dispose();
                            logger?.LogDebugAsync($"CoAP adapter disposed dispatcher on channel {Channel.Id}").GetAwaiter();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogErrorAsync(ex, $"CoAP adapter error on channel '{Channel.Id}'.").GetAwaiter();
                    }

                    try
                    {
                        if (Channel != null)
                        {
                            string channelId = Channel.Id;
                            Channel.Dispose();
                            logger?.LogDebugAsync($"CoAP adapter channel {channelId} disposed.");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogErrorAsync(ex, $"CoAP adapter disposing error on channel '{Channel.Id}'.").GetAwaiter();
                    }

                    try
                    {
                        if (session != null)
                        {
                            session.Dispose();
                            logger?.LogDebugAsync($"CoAP adapter disposed session.");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogErrorAsync(ex, $"CoAP adapter Disposing session on channel '{Channel.Id}'.").GetAwaiter();
                    }
                }
                disposed = true;
            }
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private void InitializeAuditor(PiraeusConfig config)
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

            userAuditor = auditFactory.GetAuditor(AuditType.User);
        }
    }
}
