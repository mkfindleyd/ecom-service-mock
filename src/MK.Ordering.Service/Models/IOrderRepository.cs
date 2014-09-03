using System;
using System.Threading.Tasks;

namespace MK.Ordering.Service.Models
{
    public interface IOrderRepository
    {
        Task<bool> ExistsAsync(Guid orderID);
        Task<Order> GetByIdAsync(Guid orderID);
        Task<OrderQueryResult> GetConsultantOrdersAsync(Guid consultantKey, int page, int pageSize);
        Task SaveAsync(Order order);
    }
}
