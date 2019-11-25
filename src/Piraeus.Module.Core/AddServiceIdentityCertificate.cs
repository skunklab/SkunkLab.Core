using System;
using System.Management.Automation;

namespace Piraeus.Module
{
    [Cmdlet(VerbsCommon.Add, "PiraeusServiceIdentityCertificate")]
    public class AddServiceIdentityCertificate : Cmdlet
    {
        [Parameter(HelpMessage = "Url of the service.", Mandatory = true)]
        public string ServiceUrl;

        [Parameter(HelpMessage = "Security token used to access the REST service.", Mandatory = true)]
        public string SecurityToken;

        [Parameter(HelpMessage = "Unique name of the service identity.", Mandatory = true)]
        public string Name;

        [Parameter(HelpMessage = "Path to certificate. Use either this OR store, location, and thumbprint.", Mandatory = false)]
        public string Path;

        [Parameter(HelpMessage = "Store name where certificate is located. Used with location and thumbprint, but must omit Path.", Mandatory = false)]
        public string Store;

        [Parameter(HelpMessage = "Location where certificate is located. Used with store and thumbprint, but must omit Path.", Mandatory = false)]
        public string Location;

        [Parameter(HelpMessage = "Thumbprint of certificate. Used with store and location, but must omit Path.", Mandatory = false)]
        public string Thumbprint;

        [Parameter(HelpMessage = "Certificate password.", Mandatory = true)]
        public string Password;

        protected override void ProcessRecord()
        {
            string url = null;
            if (string.IsNullOrEmpty(Password))
            {
                throw new ArgumentNullException("Password");
            }

            if (!string.IsNullOrEmpty(Path) && string.IsNullOrEmpty(Store) && string.IsNullOrEmpty(Location) && string.IsNullOrEmpty(Thumbprint))
            {
                url = String.Format($"{ServiceUrl}/api/serviceidentity/addcertificate?key={Name}&path={Path}&pwd={Password}");
            }
            else if (string.IsNullOrEmpty(Path) && !string.IsNullOrEmpty(Store) && !string.IsNullOrEmpty(Location) && !string.IsNullOrEmpty(Thumbprint))
            {
                url = String.Format($"{ServiceUrl}/api/serviceidentity/addcertificate2?key={Name}&store={Store}&location={Location}&thumbprint={Thumbprint}&pwd={Password}");
            }
            RestRequestBuilder builder = new RestRequestBuilder("POST", url, RestConstants.ContentType.Json, false, SecurityToken);
            RestRequest request = new RestRequest(builder);
            request.Post();
        }
    }
}
