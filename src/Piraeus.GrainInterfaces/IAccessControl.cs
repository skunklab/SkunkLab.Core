using Capl.Authorization;
using Orleans;
using Orleans.Concurrency;
using System.Threading.Tasks;

namespace Piraeus.GrainInterfaces
{
    public interface IAccessControl : IGrainWithStringKey
    {
        [AlwaysInterleave]
        Task UpsertPolicyAsync(AuthorizationPolicy policy);

        Task ClearAsync();

        [AlwaysInterleave]
        Task<AuthorizationPolicy> GetPolicyAsync();
    }
}
