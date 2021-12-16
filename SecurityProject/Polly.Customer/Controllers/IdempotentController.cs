using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polly.Customer.Context;
using Polly.Customer.Entity;
using Polly.Customer.ViewModel;
using System.Collections.Generic;
using System.Linq;

namespace Polly.Customer.Controllers
{
    /// <summary>
    /// Api 幂等操作
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class IdempotentController : ControllerBase
    {
        private DbContexts _dbContext;
        public IdempotentController(DbContexts dbContexts)
        {
            _dbContext = dbContexts;
        }

        [Idempotent]
        [HttpPost]
        public ActionResult Post([FromBody]CustomersRequest req)
        {
            var data = new Customers { Id = req.Id, Name = req.Name, Age = req.Age, Address = req.Address };
            _dbContext.Add(data);
            _dbContext.SaveChanges();

            return Ok(data);
        }

        [HttpGet]
        public IEnumerable<Customers> Get()
        {
            return _dbContext.Customers.Select(p => new Customers
            {
                Id = p.Id,
                Name = p.Name
            })
            .ToArray();
        }
    }
}
