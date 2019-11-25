using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using Piraeus.Core.Logging;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Piraeus.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        public SubscriptionController(IClusterClient clusterClient, Logger logger = null)
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

        [HttpGet("GetSubscriptionMetadata")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<SubscriptionMetadata>> GetSubscriptionMetadata(string subscriptionUriString)
        {
            try
            {
                if (string.IsNullOrEmpty("subscriptionUriString"))
                {
                    throw new ArgumentNullException("subscriptionUriString");
                }

                SubscriptionMetadata metadata = await graphManager.GetSubscriptionMetadataAsync(subscriptionUriString);
                if (metadata == null)
                {
                    logger?.LogWarning("Subscription metadata is null.");
                }
                else
                {
                    logger?.LogInformation($"Subscription metadata '{metadata.SubscriptionUriString}' returned.");
                }

                return StatusCode(200, metadata);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Error getting subscription metadata.");
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
                if (string.IsNullOrEmpty("subscriptionUriString"))
                {
                    throw new ArgumentNullException("subscriptionUriString");
                }

                CommunicationMetrics metrics = await graphManager.GetSubscriptionMetricsAsync(subscriptionUriString);

                if (metrics == null)
                {
                    logger?.LogWarning("Subscription metrics are null.");
                }
                else
                {
                    logger?.LogInformation("Returned subscription metrics.");
                }

                return StatusCode(200, metrics);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Error getting subscription metrics.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("UpsertSubscriptionMetadata")]
        [Authorize]
        public async Task<IActionResult> UpsertSubscriptionMetadata(SubscriptionMetadata metadata)
        {
            try
            {
                if (metadata == null)
                {
                    throw new ArgumentNullException("metadata");
                }

                await graphManager.UpsertSubscriptionMetadataAsync(metadata);
                logger?.LogInformation($"Upserted subscription metadata '{metadata.SubscriptionUriString}'");
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Error upserting subscription metadata.");
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
                if (string.IsNullOrEmpty(identity))
                {
                    throw new ArgumentNullException("identity");
                }

                IEnumerable<string> list = await graphManager.GetSubscriberSubscriptionsListAsync(identity);
                if (list == null || list.Count() == 0)
                {
                    logger?.LogWarning($"No subscriber subscriptions found for '{identity}'");
                }
                else
                {
                    logger?.LogWarning($"Subscriber subscriptions '{list.Count()}' returned for '{identity}'");
                }
                return StatusCode(200, list);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error getting subscriber subscriptions.");
                return StatusCode(500, ex.Message);
            }
        }

    }

}