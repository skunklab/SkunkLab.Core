using Microsoft.AspNetCore.Authentication;

namespace SkunkLab.Security.Authentication
{
    public class X509AuthenticationOptions : AuthenticationSchemeOptions
    {
        public X509AuthenticationOptions()
        {

        }

        public string Scheme
        {
            get { return "SkunkLabX509"; }
        }


        public X509AuthenticationOptions(string storeName, string location, string thumbprint)
        {
            StoreName = storeName;
            Location = location;
            Thumbprint = thumbprint;
        }

        public string Thumbprint { get; set; }

        public string StoreName { get; set; }

        public string Location { get; set; }

    }
}
