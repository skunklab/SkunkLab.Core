using System;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Get, "PiraeusPskSecret")]
    public class GetPskSecretCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Psk Identity", Mandatory = true)]
        public string Identity;

        protected override void ProcessRecord()
        {
            string url = String.Format("{0}/api/psk/GetPskSecret?key={1}", ServiceUrl, Identity);
            RestRequestBuilder builder = new RestRequestBuilder("GET", url, RestConstants.ContentType.Json, true, SecurityToken);
            RestRequest request = new RestRequest(builder);
            string value = request.Get<string>();

            WriteObject(value);
        }
    }


}
