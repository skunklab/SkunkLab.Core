using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piraeus.Core.Messaging;
using Piraeus.Core.Metadata;
using Piraeus.Grains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Piraeus.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourceController : ControllerBase
    {   

        [HttpGet("GetPiSystemMetadata")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<EventMetadata>> GetPiSystemMetadata(string resourceUriString)
        {
            try
            {
                EventMetadata metadata = await GraphManager.GetPiSystemMetadataAsync(resourceUriString);
                return StatusCode(200, metadata);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetSigmaAlgebra")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<string>>> GetSigmaAlgebra()
        {
            try
            {
                List<string> list = await GraphManager.GetSigmaAlgebraAsync();
                return StatusCode(200, list.ToArray());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetPiSystemMetrics")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<CommunicationMetrics>> GetPiSystemMetrics(string resourceUriString)
        {
            try
            {
                CommunicationMetrics metrics = await GraphManager.GetPiSystemMetricsAsync(resourceUriString);
                return StatusCode(200, metrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("UpsertPiSystemMetadata")]
        [Authorize]
        public async Task<IActionResult> UpsertPiSystemMetadata(EventMetadata metadata)
        {
            try
            {
                await GraphManager.UpsertPiSystemMetadataAsync(metadata);
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

        [HttpGet("GetPiSystemSubscriptionList")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<string>>> GetPiSystemSubscriptionList(string resourceUriString)
        {
            try
            {
                IEnumerable<string> list = await GraphManager.GetPiSystemSubscriptionListAsync(resourceUriString);
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

        [HttpDelete("DeletePiSystem")]
        [Authorize]
        public async Task<IActionResult> DeletePiSystem(string resourceUriString)
        {
            try
            {
                await GraphManager.ClearPiSystemAsync(resourceUriString);
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        
    }
}