using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkunkLab.Storage;
using System.Threading.Tasks;

namespace Piraeus.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PskController : ControllerBase
    {
        public PskController(PskStorageAdapter adapter)
        {
            this.adapter = adapter;
        }

        private PskStorageAdapter adapter;


        [HttpPost("SetSecret")]
        [Authorize]
        [Produces("application/json")]
        public async Task<IActionResult> SetSecret(string key, string value)
        {
            try
            {
                await adapter.SetSecretAsync(key, value);
                return StatusCode(200);
            }
            catch
            {
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
                string secret = await adapter.GetSecretAsync(key);
                return StatusCode(200, secret);
            }
            catch
            {
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
                await adapter.RemoveSecretAsync(key);
                return StatusCode(200);
            }
            catch
            {
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
                return StatusCode(200,keys);
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}