using Orleans;
using Orleans.Providers;
using Piraeus.GrainInterfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Piraeus.Grains
{
    [StorageProvider(ProviderName = "store")]
    [Serializable]
    public class SigmaAlgebra : Grain<SigmaAlgebraState>, ISigmaAlgebra
    {
        public override Task OnActivateAsync()
        {
            if (State.Container == null)
            {
                List<string> list = new List<string>();
                State.Container = list;
            }

            return Task.CompletedTask;
        }

        public async override Task OnDeactivateAsync()
        {
            await WriteStateAsync();
        }

        public async Task AddAsync(string resourceUriString)
        {
            if (!State.Container.Contains(resourceUriString))
            {
                State.Container.Add(resourceUriString);
            }

            await WriteStateAsync();
        }

        public async Task ClearAsync()
        {
            await ClearStateAsync();
        }

        public async Task<bool> Contains(string resourceUriString)
        {
            return await Task.FromResult<bool>(State.Container.Contains(resourceUriString));
        }

        public async Task<List<string>> GetListAsync()
        {
            //return await Task.FromResult<IEnumerable<string>>(State.Container.ToArray());
            return await Task.FromResult<List<string>>(State.Container);
        }

        public async Task RemoveAsync(string resourceUriString)
        {
            State.Container.Remove(resourceUriString);
            await Task.CompletedTask;
        }
    }
}
