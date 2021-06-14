using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Module08.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private static readonly List<Product> Products = new List<Product>
        {
            new Product() { Id = 1, Name = "Model 1" },
            new Product() { Id = 2, Name = "Model 2" },
            new Product() { Id = 3, Name = "Model 3" },
        };



        // GET: api/<ProductController>
        [HttpGet]
        public IEnumerable<Product> Get()
        {
            return Products;
        }

        // GET api/<ProductController>/5
        [HttpGet("{id}")]
        public Product Get(int id)
        {
            return Products.SingleOrDefault(x => x.Id == id);
        }

        // POST api/<ProductController>
        [HttpPost]
        public IActionResult Post([FromBody] Product value)
        {
            if (value.Id != 0)
                return BadRequest("Cannot add product with non-zero Id, as it is auto-assigned.");

            value.Id = Products.Count + 1;
            Products.Add(value);

            return Ok(value);
        }

    }

    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
