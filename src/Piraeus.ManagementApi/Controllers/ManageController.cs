using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Piraeus.Configuration.Settings;
using Piraeus.Grains;
using SkunkLab.Security.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Piraeus.ManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageController : ControllerBase
    {
        public ManageController(PiraeusConfig config, IClusterClient client)
        {
            this.config = config;
            if (!GraphManager.IsInitialized)
            {
                GraphManager.Initialize(client);
            }
        }

        private PiraeusConfig config;

        [HttpGet]
        [Produces("application/json")]
        [AllowAnonymous]
        public ActionResult<string> Get(string code)
        {
            string codeString = HttpUtility.UrlDecode(code);
            if (config.Security.WebApi.SecurityCodes.Contains(codeString))
            {
                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim(config.Security.WebApi.NameClaimType, Guid.NewGuid().ToString()));
                claims.Add(new Claim(config.Security.WebApi.RoleClaimType, "manage"));
                //build the JWT token
                JsonWebToken jwt = new JsonWebToken(new Uri(config.Security.WebApi.Audience), config.Security.WebApi.SymmetricKey, config.Security.WebApi.Issuer, claims, 120.0);
                return jwt.ToString();
            }
            else
            {

                throw new IndexOutOfRangeException("Invalid code");
            }
        }
    }
}
