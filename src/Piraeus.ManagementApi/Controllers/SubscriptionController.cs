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
    [Route("[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        [HttpGet("GetSubscriptionMetadata")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<SubscriptionMetadata>> GetSubscriptionMetadata(string subscriptionUriString)
        {
            try
            {
                SubscriptionMetadata metadata = await GraphManager.GetSubscriptionMetadataAsync(subscriptionUriString);
                return StatusCode(200, metadata);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);

            }
        }

        [HttpGet("GetSubscriptionMetrics")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<CommunicationMetrics>> GetSubscriptionMetrics(string subscriptionUriString)
        {
            try
            {
                CommunicationMetrics metrics = await GraphManager.GetSubscriptionMetricsAsync(subscriptionUriString);
                return StatusCode(200, metrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("UpsertSubscriptionMetadata")]
        [Authorize]
        public async Task<IActionResult> UpsertSubscriptionMetadata(SubscriptionMetadata metadata)
        {
            try
            {
                await GraphManager.UpsertSubscriptionMetadataAsync(metadata);
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetSubscriberSubscriptions")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<string>>> GetSubscriberSubscriptions(string identity)
        {
            try
            {
                IEnumerable<string> list = await GraphManager.GetSubscriberSubscriptionsListAsync(identity);
                return StatusCode(200, list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
