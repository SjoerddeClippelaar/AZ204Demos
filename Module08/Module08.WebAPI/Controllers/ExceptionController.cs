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
    public class ExceptionController : ControllerBase
    {

        [HttpPost]
        public void Post()
        {
            throw new NotImplementedException("This will trigger an internal server error response");
        }
    }
}
