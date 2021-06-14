using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Module12.WebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ExceptionController : ControllerBase
    {
        [HttpGet()]
        public void Get()
        {
            throw new InvalidOperationException("[Test]The user request triggered an internal exception");
        }

    }
}
