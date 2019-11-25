using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Piraeus.Configuration;
using Piraeus.Core.Logging;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Piraeus.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageController : ControllerBase
    {
        public ManageController(PiraeusConfig config, Logger logger = null)
        {
            this.config = config;
            this.logger = logger;
        }

        private readonly ILogger logger;
        private readonly PiraeusConfig config;

        [HttpGet]
        [Produces("application/json")]
        [AllowAnonymous]
        public ActionResult<string> Get(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    throw new ArgumentNullException("code");
                }

                string codeString = HttpUtility.UrlDecode(code);
                string[] codes = config.GetSecurityCodes();

                if (codes.Contains(codeString))
                {
                    List<Claim> claims = new List<Claim>();
                    claims.Add(new Claim($"{config.ManagementApiIssuer}/name", Guid.NewGuid().ToString()));
                    claims.Add(new Claim($"{config.ManagementApiIssuer}/role", "manage"));
                    JsonWebToken jwt = new JsonWebToken(config.ManagmentApiSymmetricKey, claims, 120.0, config.ManagementApiIssuer, config.ManagementApiAudience);
                    logger?.LogInformation("Returning security token.");

                    return StatusCode(200, jwt.ToString());
                }
                else
                {
                    logger?.LogWarning("Security code mismatch attempting to acquire security token.");
                    throw new IndexOutOfRangeException("Invalid code");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error obtaining security token.");
                return StatusCode(500, ex.Message);
            }
        }
    }
}