using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
