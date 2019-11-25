using Piraeus.Core.Metadata;
using System;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "PiraeusEventMetadata")]
    public class UpsertPiSystemMetadataCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Unique URI identifier of resource.", Mandatory = true)]
        public string ResourceUriString;

        [Parameter(HelpMessage = "Text description of resource.", Mandatory = false)]
        public string Description { get; set; }

        [Parameter(HelpMessage = "Link to additional data about resource.", Mandatory = false)]
        public string DiscoveryUrl { get; set; }

        [Parameter(HelpMessage = "Enable the resource to receive messages.", Mandatory = true)]
        public bool Enabled { get; set; }

        [Parameter(HelpMessage = "Expiration of the resource.", Mandatory = false)]
        public DateTime? Expires { get; set; }

        [Parameter(HelpMessage = "Maximum duration of a subscription", Mandatory = false)]
        public TimeSpan? MaxSubscriptionDuration { get; set; }

        [Parameter(HelpMessage = "Enable audit", Mandatory = false)]
        public bool Audit { get; set; }

        [Parameter(HelpMessage = "Require all messages over an encrypted channel.", Mandatory = false)]
        public bool RequireEncryptedChannel { get; set; }

        [Parameter(HelpMessage = "CAPL policy URI ID of the access control policy for publishing messages.", Mandatory = true)]
        public string PublishPolicyUriString { get; set; }

        [Parameter(HelpMessage = "CAPL policy URI ID of the access control policy for subscribing to messages.", Mandatory = true)]
        public string SubscribePolicyUriString { get; set; }

        protected override void ProcessRecord()
        {
            string url = String.Format("{0}/api/resource/UpsertPiSystemMetadata", this.ServiceUrl);
            RestRequestBuilder builder = new RestRequestBuilder("PUT", url, RestConstants.ContentType.Json, false, this.SecurityToken);
            RestRequest request = new RestRequest(builder);

            EventMetadata metadata = new EventMetadata()
            {
                Audit = this.Audit,
                Description = this.Description,
                DiscoveryUrl = this.DiscoveryUrl,
                Enabled = this.Enabled,
                Expires = this.Expires,
                MaxSubscriptionDuration = this.MaxSubscriptionDuration,
                ResourceUriString = this.ResourceUriString,
                RequireEncryptedChannel = this.RequireEncryptedChannel,
                PublishPolicyUriString = this.PublishPolicyUriString,
                SubscribePolicyUriString = this.SubscribePolicyUriString
            };

            request.Put<EventMetadata>(metadata);
        }
    }
}
