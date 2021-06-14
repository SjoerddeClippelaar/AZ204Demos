using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Module08.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenOnlyController : ControllerBase
    {
        private const string Token = "password";


        [HttpGet]
        public IActionResult Get(string tokenParam = null)
        {
            Request.Headers.TryGetValue("Token", out var tokenHeader);
            string token = tokenParam ?? tokenHeader;
            
            if (string.IsNullOrEmpty(token))
                return Unauthorized("Missing token");

            if (token != Token)
                return Unauthorized("Invalid token");

            return Ok("[data placeholder]");
        }
    }
}
