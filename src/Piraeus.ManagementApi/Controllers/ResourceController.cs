using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Piraeus.ManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourceController : ControllerBase
    {

        [HttpGet("GetResourceMetadata")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<ResourceMetadata>> GetResourceMetadata(string resourceUriString)
        {
            try
            {
                ResourceMetadata metadata = await GraphManager.GetResourceMetadataAsync(resourceUriString);
                return StatusCode(200, metadata);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetResourceList")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<string>>> GetResourceList()
        {
            try
            {
                IEnumerable<string> list = await GraphManager.GetResourceListAsync();
                return StatusCode(200, list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetResourceMetrics")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<CommunicationMetrics>> GetResourceMetrics(string resourceUriString)
        {
            try
            {
                CommunicationMetrics metrics = await GraphManager.GetResourceMetricsAsync(resourceUriString);
                return StatusCode(200, metrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("UpsertResourceMetadata")]
        [Authorize]
        public async Task<IActionResult> UpsertResourceMetadata(ResourceMetadata metadata)
        {
            try
            {
                await GraphManager.UpsertResourceMetadataAsync(metadata);
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("Subscribe")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<string>> Subscribe(string resourceUriString, SubscriptionMetadata metadata)
        {
            try
            {
                string subscriptionUriString = await GraphManager.SubscribeAsync(resourceUriString, metadata);
                return StatusCode(200, subscriptionUriString);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);

            }
        }

        [HttpGet("GetResourceSubscriptionList")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<string>>> GetResourceSubscriptionList(string resourceUriString)
        {
            try
            {
                IEnumerable<string> list = await GraphManager.GetResourceSubscriptionListAsync(resourceUriString);
                return StatusCode(200, list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("Unsubscribe")]
        [Authorize]
        public async Task<IActionResult> Unsubscribe(string subscriptionUriString)
        {
            try
            {
                await GraphManager.UnsubscribeAsync(subscriptionUriString.ToLowerInvariant());
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("DeleteResource")]
        [Authorize]
        public async Task<IActionResult> DeleteResource(string resourceUriString)
        {
            try
            {
                await GraphManager.ClearResourceAsync(resourceUriString);
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
