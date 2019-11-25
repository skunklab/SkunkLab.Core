using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using Piraeus.Core.Logging;
using Piraeus.Grains;
using System;
using System.Threading.Tasks;

namespace Piraeus.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessControlController : ControllerBase
    {
        public AccessControlController(IClusterClient clusterClient, Logger logger = null)
        {
            if (!GraphManager.IsInitialized)
            {
                this.graphManager = GraphManager.Create(clusterClient);
            }
            else
            {
                this.graphManager = GraphManager.Instance;
            }

            this.logger = logger;
        }

        private readonly GraphManager graphManager;
        private readonly ILogger logger;


        [HttpGet("GetAccessControlPolicy")]
        [Authorize]
        [Produces("application/xml")]
        public async Task<ActionResult<Capl.Authorization.AuthorizationPolicy>> GetAccessControlPolicy(string policyUriString)
        {
            try
            {
                if (string.IsNullOrEmpty(policyUriString))
                {
                    throw new ArgumentNullException("policyUriString");
                }

                Capl.Authorization.AuthorizationPolicy policy = await graphManager.GetAccessControlPolicyAsync(policyUriString);
                return StatusCode(200, policy);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Error getting CAPL policy.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("UpsertAccessControlPolicy")]
        [Authorize]
        public async Task<IActionResult> UpsertAccessControlPolicy(Capl.Authorization.AuthorizationPolicy policy)
        {
            try
            {
                if (policy == null)
                {
                    throw new ArgumentNullException("policy");
                }

                await graphManager.UpsertAcessControlPolicyAsync(policy.PolicyId.ToString(), policy);
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Error upserting CAPL policy.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("DeleteAccessControlPolicy")]
        [Authorize]
        public async Task<IActionResult> DeleteAccessControlPolicy(string policyUriString)
        {
            try
            {
                if (string.IsNullOrEmpty(policyUriString))
                {
                    throw new ArgumentNullException("policyUriString");
                }
                await graphManager.ClearAccessControlPolicyAsync(policyUriString);
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Error deleting CAPL policy.");
                return StatusCode(500, ex.Message);
            }
        }


    }
}