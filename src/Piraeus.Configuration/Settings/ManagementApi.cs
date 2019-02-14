using Newtonsoft.Json;
using System;

namespace Piraeus.Configuration.Settings
{
    [Serializable]
    [JsonObject]
    public class ManagementApi 
    {
        public ManagementApi()
        {
        }

        private string securityCodeValues;

        [JsonProperty("issuer")]
        public string Issuer { get; set; }

        [JsonProperty("audience")]
        public string Audience { get; set; }

        [JsonProperty("tokenType")]
        public string TokenType { get; set; }

        [JsonProperty("symmetricKey")]
        public string SymmetricKey { get; set; }

        [JsonProperty("nameClaimType")]
        public string NameClaimType { get; set; }

        [JsonProperty("roleClaimType")]
        public string RoleClaimType { get; set; }

        [JsonProperty("roleClaimValue")]
        public string RoleClaimValue { get; set; }

        [JsonIgnore]
        public string[] SecurityCodes { get; set; }

        [JsonProperty("securityCodes")]
        public string SecurityCodeValues
        {
            get
            {
                return securityCodeValues;
            }
            set
            {
                if(value.Contains(";"))
                {
                    string[] parts = value.Split(";", StringSplitOptions.RemoveEmptyEntries);
                    SecurityCodes = parts;
                }
                else
                {
                    SecurityCodes = new string[] { value };
                }
            }
        }
        

    }
}
