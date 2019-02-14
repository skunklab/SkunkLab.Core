using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;

namespace SkunkLab.Storage
{
    public class KeyVaultPskStorage : PskStorageAdapter
    {
        public static KeyVaultPskStorage CreateSingleton(string authority, string clientId, string clientSecret)
        {
            if(instance == null)
            {
                Authority = authority;
                ClientId = clientId;
                ClientSecret = clientSecret;
                instance = new KeyVaultPskStorage();
                instance.client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetAccessToken));
            }

            return instance;
        }



        protected KeyVaultPskStorage()
        {
        }

        private static KeyVaultPskStorage instance;
        internal KeyVaultClient client;
        private static string Authority;
        private static string ClientId;
        private static string ClientSecret;
        private static DateTime expiry;

        public override async Task<string> GetSecretAsync(string secretIdentifier)
        {            
            if(DateTime.Now > expiry)
            {
                client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetAccessToken));
            }

            SecretBundle sec = await client.GetSecretAsync(secretIdentifier);
            return sec.Value;
        }

        public override async Task SetSecretAsync(string secretName, string value)
        {
            if (DateTime.Now > expiry)
            {
                client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetAccessToken));
            }


            SecretBundle bundle = await client.SetSecretAsync(String.Format("https://{0}.vault.azure.net:443/", Authority), secretName, value);            
        }

        public override async Task RemoveSecretAsync(string key)
        {
            if (DateTime.Now > expiry)
            {
                client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetAccessToken));
            }

            await client.DeleteKeyAsync(String.Format("https://{0}.vault.azure.net:443/", Authority), key);
        }


        internal static async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(ClientId,ClientSecret);
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            expiry = result.ExpiresOn.DateTime;
            return result.AccessToken;
        }


    }
}
