
using System;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "PiraeusServiceIdentityClaims")]
    public class AddServiceIdentityClaims : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Unique name of the service identity.", Mandatory = true)]
        public string Name;

        [Parameter(HelpMessage = "Semi-colon delimited list of claim types.  Must match number of claim values.", Mandatory = true)]
        public string ClaimTypes;

        [Parameter(HelpMessage = "Semi-colon delimited list of claim values.  Must match number of claim types.", Mandatory = true)]
        public string ClaimValues;


        protected override void ProcessRecord()
        {
            string[] claimTypes = ClaimTypes.Split(";", StringSplitOptions.RemoveEmptyEntries);
            string[] claimValues = ClaimValues.Split(";", StringSplitOptions.RemoveEmptyEntries);

            if(claimTypes.Length != claimValues.Length)
            {
                throw new IndexOutOfRangeException("Claim types and values items do not match same length.");
            }
            string url = String.Format($"{ServiceUrl}/api/serviceidentity/addclaimtypes?key={Name}&claimtypes={ClaimTypes}&claimvalues={ClaimValues}");
            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, false, SecurityToken);
            RestRequest request = new RestRequest(builder);

            request.Post();
        }
    }

}
