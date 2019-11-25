using Microsoft.AspNetCore.Authorization;

namespace Piraeus.WebApi.Security
{
    public class ApiAccessRequirement : IAuthorizationRequirement
    {
        public Capl.Authorization.AuthorizationPolicy Policy { get; private set; }

        public ApiAccessRequirement(Capl.Authorization.AuthorizationPolicy policy)
        {
            Policy = policy;
        }
    }
}
