using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piraeus.Grains;
using System;
using System.Threading.Tasks;

namespace Piraeus.ManagementApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccessControlController : ControllerBase
    {
        [HttpGet("GetAccessControlPolicy")]
        [Authorize]
        [Produces("application/xml")]
        public async Task<ActionResult<Capl.Authorization.AuthorizationPolicy>> GetAccessControlPolicy(string policyUriString)
        {
            try
            {
                Capl.Authorization.AuthorizationPolicy policy = await GraphManager.GetAccessControlPolicyAsync(policyUriString);
                return StatusCode(200, policy);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("UpsertAccessControlPolicy")]
        [Authorize]
        public async Task<IActionResult> UpsertAccessControlPolicy(Capl.Authorization.AuthorizationPolicy policy)
        {
            try
            {
                await GraphManager.UpsertAcessControlPolicyAsync(policy.PolicyId.ToString(), policy);
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("DeleteAccessControlPolicy")]
        [Authorize]
        public async Task<IActionResult> DeleteAccessControlPolicy(string policyUriString)
        {
            try
            {
                await GraphManager.ClearAccessControlPolicyAsync(policyUriString);
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
