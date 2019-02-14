using Orleans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Piraeus.GrainInterfaces
{

    public interface IResourceList : IGrainWithStringKey
    {
        Task AddAsync(string resourceUriString);

        Task RemoveAsync(string resourceUriString);

        Task ClearAsync();

        Task<List<string>> GetListAsync();

        Task<bool> Contains(string resourceUriString);
    }
}
