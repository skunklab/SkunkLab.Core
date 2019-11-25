using Newtonsoft.Json;
using System;

namespace Piraeus.Configuration
{
    [Serializable]
    [JsonObject]
    public class ManagementApiConfig
    {
        public ManagementApiConfig()
        {
        }

        #region Management API 


        [JsonProperty("issuer", Order = 0)]
        public string Issuer { get; set; }

        [JsonProperty("audience", Order = 1)]
        public string Audience { get; set; }

        [JsonProperty("tokenType", Order = 2)]
        public string TokenType { get; set; }

        [JsonProperty("symmetricKey", Order = 3)]
        public string SymmetricKey { get; set; }

        [JsonProperty("nameClaimType", Order = 4)]
        public string NameClaimType { get; set; }

        [JsonProperty("roleClaimType", Order = 5)]
        public string RoleClaimType { get; set; }

        [JsonProperty("roleClaimValue", Order = 6)]
        public string RoleClaimValue { get; set; }

        [JsonProperty("securityCodes", Order = 7)]
        public string[] SecurityCodes { get; set; }


        #endregion
    }
}
