using System;
using System.Net.Http;
using System.Threading.Tasks;
using MK.Ordering.Service.Models;

namespace MK.Ordering.Service
{
    public class OrderService : IOrderService
    {
        Uri _baseAddress;

        public OrderService(string baseAddress)
        {
            _baseAddress = new Uri(baseAddress);
        }

        public async Task<Order> CreateOrderAsync(Guid consultantKey)
        {
            var pathAndQuery = string.Format("./v1/orders?consultantKey={0:D}", consultantKey);
            using (var req = new HttpRequestMessage(HttpMethod.Put, new Uri(_baseAddress, pathAndQuery)))
            using (var client = new HttpClient())
            using (var resp = await client.SendAsync(req).ConfigureAwait(false))
            {
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadAsAsync<Order>().ConfigureAwait(false);
            }
        }

        public async Task<Order> GetOrderByIdAsync(Guid orderID)
        {
            var pathAndQuery = string.Format("./v1/orders/{0:D}", orderID);
            using (var req = new HttpRequestMessage(HttpMethod.Get, new Uri(_baseAddress, pathAndQuery)))
            using (var client = new HttpClient())
            using (var resp = await client.SendAsync(req).ConfigureAwait(false))
            {
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadAsAsync<Order>().ConfigureAwait(false);
            }
        }

        public async Task<OrderQueryResult> QueryOrders(Guid consultantKey, int page = 1, int pageSize = 5)
        {
            var pathAndQuery = string.Format("./v1/orders?consultantKey={0:D}&page={1}&pageSize={2}", consultantKey, page, pageSize);
            using (var req = new HttpRequestMessage(HttpMethod.Get, new Uri(_baseAddress, pathAndQuery)))
            using (var client = new HttpClient())
            using (var resp = await client.SendAsync(req).ConfigureAwait(false))
            {
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadAsAsync<OrderQueryResult>().ConfigureAwait(false);
            }
        }

        public async Task<Order> UpdateItems(Guid orderID, Order.Item[] changedItems)
        {
            var req = new UpdateItemsRequest
            {
                Items = changedItems
            };

            var pathAndQuery = string.Format("./v1/orders/{0:D}/items", orderID);
            using (var client = new HttpClient())
            using (var resp = await client.PostAsJsonAsync(new Uri(_baseAddress, pathAndQuery), req))
            {
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadAsAsync<Order>().ConfigureAwait(false);
            }
        }

        public async Task<Order> RemoveItem(Guid orderID, string sku)
        {
            var pathAndQuery = string.Format("./v1/orders/{0:D}/items/{1}", orderID, sku);
            using (var client = new HttpClient())
            using (var resp = await client.DeleteAsync(new Uri(_baseAddress, pathAndQuery)))
            {
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadAsAsync<Order>().ConfigureAwait(false);
            }
        }
    }
}
