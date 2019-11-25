using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Piraeus.Core.Logging;
using SkunkLab.Storage;
using System;
using System.Threading.Tasks;

namespace Piraeus.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PskController : ControllerBase
    {
        public PskController(PskStorageAdapter adapter, Logger logger = null)
        {
            this.adapter = adapter;
            this.logger = logger;
        }

        private readonly PskStorageAdapter adapter;
        private readonly ILogger logger;

        [HttpPost("SetSecret")]
        [Authorize]
        [Produces("application/json")]
        public async Task<IActionResult> SetSecret(string key, string value)
        {
            try
            {
                if (string.IsNullOrEmpty("key"))
                {
                    throw new ArgumentNullException("key");
                }

                if (string.IsNullOrEmpty("value"))
                {
                    throw new ArgumentNullException("value");
                }
                await adapter.SetSecretAsync(key, value);
                logger?.LogInformation("Set PSK secret.");
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error setting PSK secret.");
                return StatusCode(500);
            }
        }

        [HttpGet("GetSecret")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<string>> GetSecretAsync(string key)
        {
            try
            {
                if (string.IsNullOrEmpty("key"))
                {
                    throw new ArgumentNullException("key");
                }

                string secret = await adapter.GetSecretAsync(key);
                if (string.IsNullOrEmpty(secret))
                {
                    logger?.LogWarning("PSK secret not found.");
                }
                else
                {
                    logger?.LogInformation("Return PSK secret.");
                }

                return StatusCode(200, secret);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error getting PSK secret.");
                return StatusCode(500);
            }
        }

        [HttpDelete("RemoveSecret")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult> RemoveSecretAsync(string key)
        {
            try
            {
                if (string.IsNullOrEmpty("key"))
                {
                    throw new ArgumentNullException("key");
                }

                await adapter.RemoveSecretAsync(key);
                logger?.LogInformation("Deleted PSK secret.");
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error deleting PSK secret.");
                return StatusCode(500);
            }
        }


        [HttpGet("GetKeys")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<string[]>> GetKeys()
        {
            try
            {
                string[] keys = await adapter.GetKeys();
                if (keys == null)
                {
                    logger?.LogWarning("PSK keys not found.");
                }
                else
                {
                    logger?.LogInformation("Returned PSK keys.");
                }
                return StatusCode(200, keys);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error getting PSK keys.");
                return StatusCode(500);
            }
        }
    }
}