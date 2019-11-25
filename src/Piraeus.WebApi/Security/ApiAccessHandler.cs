using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Piraeus.WebApi.Security
{
    public class ApiAccessHandler : AuthorizationHandler<ApiAccessRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiAccessRequirement requirement)
        {

            ClaimsPrincipal prin = context.User;
            ClaimsIdentity identity = new ClaimsIdentity(prin.Identity);

            if (requirement.Policy.Evaluate(identity))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
