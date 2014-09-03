using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using MK.Ordering.Service.Models;

namespace MK.Ordering.Service.Controllers.v1
{
    [RoutePrefix("v1")]
    public class OrdersController : ApiController
    {
        IOrderRepository _repository;

        public OrdersController(IOrderRepository repository)
        {
            _repository = repository;
        }

        [HttpPut]
        [Route("orders")]
        public async Task<IHttpActionResult> CreateOrder(Guid consultantKey)
        {
            var order = new Order
            {
                OrderID          = Guid.NewGuid(),
                ConsultantKey    = consultantKey,
                CreatedDate      = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                Items            = new Order.Item[] {}
            };

            await _repository.SaveAsync(order);

            return Ok(order);
        }

        [HttpGet]
        [Route("orders")]
        public async Task<IHttpActionResult> QueryOrders(Guid consultantKey, int page = 1, int pageSize = 5)
        {
            if (page < 1)
                page = 1;

            if (pageSize > 10)
                pageSize = 10;

            var result = await _repository.GetConsultantOrdersAsync(consultantKey, page, pageSize);

            return Ok(result);
        }

        [HttpGet]
        [Route("orders/{orderID}")]
        public async Task<IHttpActionResult> GetOrderById(Guid orderID)
        {
            var model = await _repository.GetByIdAsync(orderID);
            return Ok(model);
        }

        [HttpPost]
        [Route("orders/{orderID}/items")]
        public async Task<IHttpActionResult> UpdateItems([FromUri] Guid orderID, [FromBody] UpdateItemsRequest request)
        {
            var order = await _repository.GetByIdAsync(orderID);

            var items = request.Items.ToDictionary(i => i.Sku);
            foreach (var item in order.Items)
                if (!items.ContainsKey(item.Sku))
                    items.Add(item.Sku, item);

            order.Items = items.Values.ToArray();
            foreach (var item in order.Items)
                item.Price = item.UnitPrice * item.Quantity;

            await _repository.SaveAsync(order);

            return Ok(order);
        }

        [HttpDelete]
        [Route("orders/{orderID}/items/{sku}")]
        public async Task<IHttpActionResult> RemoveItem([FromUri] Guid orderID, string sku)
        {
            var order = await _repository.GetByIdAsync(orderID);

            order.Items = order.Items.Where(i => i.Sku != sku).ToArray();

            await _repository.SaveAsync(order);

            return Ok(order);
        }
    }
}
