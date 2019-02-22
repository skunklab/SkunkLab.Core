using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Get, "PiraeusPskKeys")]
    public class GetPskKeysCmdlet : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;


        protected override void ProcessRecord()
        {
            string url = String.Format("{0}/api/psk/RemovePskSecret", ServiceUrl);
            RestRequestBuilder builder = new RestRequestBuilder("GET", url, RestConstants.ContentType.Json, true, SecurityToken);
            RestRequest request = new RestRequest(builder);
            string[] keys = request.Get<string[]>();

            WriteObject(keys);


        }
    }
}
