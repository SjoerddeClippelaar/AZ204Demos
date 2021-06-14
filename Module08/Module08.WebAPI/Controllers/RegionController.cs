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
    public class RegionController : ControllerBase
    {

        public static readonly string[] Regions = new[]
        {
            "Africa",
            "Asia",
            "Europe",
            "North America",
            "Oceania",
            "South America",
        };

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return Regions;
        }

    }
}
