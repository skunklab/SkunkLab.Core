using System;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "PiraeusPskSecret")]
    public class SetPskSecretCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Psk Identity", Mandatory = true)]
        public string Identity;

        [Parameter(HelpMessage = "Psk secret as base64 encoded byte arrray.", Mandatory = true)]
        public string Secret;

        protected override void ProcessRecord()
        {
            string url = String.Format("{0}/api/psk/SetPskSecret?key={1}&value={2}", ServiceUrl, Identity, Secret);
            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, true, SecurityToken);
            RestRequest request = new RestRequest(builder);
            request.Post();
        }
    }
}
