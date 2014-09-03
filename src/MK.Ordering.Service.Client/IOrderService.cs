using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MK.Ordering.Service.Models;

namespace MK.Ordering.Service
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(Guid consultantKey);
        Task<Order> GetOrderByIdAsync(Guid orderID);
        Task<OrderQueryResult> QueryOrders(Guid consultantKey, int page = 1, int pageSize = 5);
        Task<Order> UpdateItems(Guid orderID, Order.Item[] changedItems);
        Task<Order> RemoveItem(Guid orderID, string sku);
    }
}
