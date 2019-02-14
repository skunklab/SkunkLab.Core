using Orleans;
using Orleans.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using Piraeus.GrainInterfaces;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace Piraeus.Grains
{
    [StorageProvider(ProviderName = "store")]
    [Serializable]
    public class ServiceIdentity : Grain<ServiceIdentityState>, IServiceIdentity
    {
        public override Task OnActivateAsync()
        {
            return Task.CompletedTask;
        }

        public override async Task OnDeactivateAsync()
        {
            await WriteStateAsync();
        }


        public async Task<byte[]> GetCertificateAsync()
        {
            return await Task.FromResult<byte[]>(State.Certificate);
        }

        public async Task<List<KeyValuePair<string, string>>> GetClaimsAsync()
        {
            return await Task.FromResult<List<KeyValuePair<string, string>>>(State.Claims);
        }

        public Task AddCertificateAsync(byte[] certificate)
        {
            State.Certificate = certificate;
            return WriteStateAsync();
        }

        public async Task AddClaimsAsync(List<KeyValuePair<string, string>> claims)
        {
            if(claims == null || claims.Count == 0)
            {
                return;
            }

            State.Claims = new List<KeyValuePair<string, string>>();
            foreach(var claim in claims)
            {
                State.Claims.Add(new KeyValuePair<string, string>(claim.Key, claim.Value));
            }

            await Task.CompletedTask;
        }

        
    }
}
