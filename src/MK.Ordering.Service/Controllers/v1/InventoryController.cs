using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using MK.Ordering.Service.Models;

namespace MK.Ordering.Service.Controllers.v1
{
    [RoutePrefix("v1")]
    public class InventoryController : ApiController
    {
        static Random _random = new Random();

        [HttpPost]
        [Route("inventory")]
        public IHttpActionResult Check(InventoryCheckRequest request)
        {
            Thread.Sleep(_random.Next(0, 250));

            var response = new InventoryCheckResponse
            {
                Results = request.Products.Select
                    (
                        p => new InventoryCheckResponse.Result 
                        {
                            Sku = p.Sku,
                            QuantityRequested = p.QuantityRequested,
                            QuantityAvailable = _random.Next(0, p.QuantityRequested * 4),
                        }
                    )
                    .ToArray()
            };

            foreach (var result in response.Results)
                result.IsAvailable = result.QuantityRequested <= result.QuantityAvailable;

            return Ok(response);
        }
    }
}
