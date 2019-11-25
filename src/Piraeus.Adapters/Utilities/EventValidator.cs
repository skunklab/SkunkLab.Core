using Capl.Authorization;
using Microsoft.AspNetCore.Http;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
using SkunkLab.Channels;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;

namespace Piraeus.Adapters.Utilities
{
    public abstract class EventValidator
    {
        private delegate ValidatorResult MetadataHandler(EventMetadata metadata, bool? encryptedChannel = null);
        private delegate ValidatorResult PolicyHandler(AuthorizationPolicy policy, ClaimsIdentity identity = null);
        private static List<MetadataHandler> metadataHandlers;
        private static List<PolicyHandler> policyHandlers;
        private static bool initialized;
        

        public static ValidatorResult Validate(bool publish, string resourceUriString, IChannel channel, GraphManager graphManager, HttpContext context = null)
        {
            EventMetadata metadata = graphManager.GetPiSystemMetadataAsync(resourceUriString).GetAwaiter().GetResult();
            return Validate(publish, metadata, channel, graphManager, context);
        }

        public static ValidatorResult Validate(bool publish, EventMetadata metadata, IChannel channel, GraphManager graphManager, HttpContext context = null)
        {
            metadataHandlers = metadataHandlers ?? new List<MetadataHandler>();
            policyHandlers = policyHandlers ?? new List<PolicyHandler>();
            Init();

            int index = 0;
            bool result = true;
            ValidatorResult vr = null;

            while (result && index < metadataHandlers.Count)
            {
                vr = metadataHandlers[index].Invoke(metadata, channel.IsEncrypted);
                result = vr.Validated;
                index++;
            }

            AuthorizationPolicy policy = graphManager.GetAccessControlPolicyAsync(publish ? metadata.PublishPolicyUriString : metadata.SubscribePolicyUriString).GetAwaiter().GetResult();
            ClaimsIdentity identity = context == null ? Thread.CurrentPrincipal.Identity as ClaimsIdentity : new ClaimsIdentity(context.User.Claims);

            index = 0;
            while (result && index < policyHandlers.Count)
            {
                vr = policyHandlers[index].Invoke(policy, identity);
                index++;
            }

            return vr;
        }

        private static void Init()
        {
            if (initialized)
                return;

            metadataHandlers.Add(ValidateNotNullMetadata);
            metadataHandlers.Add(ValidateEncryptedChannel);
            metadataHandlers.Add(ValidateEnabled);
            metadataHandlers.Add(ValidateExpired);

            policyHandlers.Add(ValidateNotNullPolicy);
            policyHandlers.Add(ValidateAuthorizationPolicy);

            initialized = true;
        }
        
        private static ValidatorResult ValidateNotNullPolicy(AuthorizationPolicy policy, ClaimsIdentity identity = null)
        {
            return new ValidatorResult(policy != null, "Access control policy is null.");
        }

        private static ValidatorResult ValidateAuthorizationPolicy(AuthorizationPolicy policy, ClaimsIdentity identity = null)
        {
            return new ValidatorResult(policy.Evaluate(identity), $"Access control check failed for {policy.PolicyId.ToString()}");
        }

        private static ValidatorResult ValidateNotNullMetadata(EventMetadata metadata, bool? encryptedChannel = null)
        {
            return new ValidatorResult(metadata != null, "Metadata is null");
        }

        private static ValidatorResult ValidateEncryptedChannel(EventMetadata metadata, bool? encryptedChannel = null)
        {
            return new ValidatorResult(!metadata.RequireEncryptedChannel || (metadata.RequireEncryptedChannel && encryptedChannel.Value), "Requires encrypted channel");
        }

        private static ValidatorResult ValidateEnabled(EventMetadata metadata, bool? encryptedChannel = null)
        {
            return new ValidatorResult(metadata.Enabled, "Metadata disabled.");
        }

        private static ValidatorResult ValidateExpired(EventMetadata metadata, bool? encryptedChannel = null)
        {
            return new ValidatorResult(!(metadata.Expires.HasValue && metadata.Expires.Value < DateTime.UtcNow), "Metadata has expired.");
        }
    }
}
