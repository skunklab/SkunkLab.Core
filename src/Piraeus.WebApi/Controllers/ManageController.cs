using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Piraeus.Configuration;
using Piraeus.Grains;
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
            string[] codes = config.GetSecurityCodes();

            if(codes.Contains(codeString))
            {
                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim("http://www.skunklab.io/name", Guid.NewGuid().ToString()));
                claims.Add(new Claim("http://www.skunklab.io/role", "manage"));
                //build the JWT token
                //JsonWebToken jwt = new JsonWebToken(new Uri(), config.Security.WebApi.SymmetricKey, config.Security.WebApi.Issuer, claims, 120.0);
                JsonWebToken jwt = new JsonWebToken(config.ManagmentApiSymmetricKey, claims, 120.0, config.ManagementApiIssuer, config.ManagementApiAudience);
                return jwt.ToString();
            }
            else
            {
               
                throw new IndexOutOfRangeException("Invalid code");
            }
        }
    }
}