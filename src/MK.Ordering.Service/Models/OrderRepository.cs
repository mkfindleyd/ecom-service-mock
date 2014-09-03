using System;
using System.Linq;
using System.Threading.Tasks;
using MK.Ordering.Service.Infrastructure;
using PlainElastic.Net;
using PlainElastic.Net.Queries;
using PlainElastic.Net.Serialization;

namespace MK.Ordering.Service.Models
{
    public class OrderRepository : IOrderRepository
    {
        const string INDEX_NAME = "ecom-poc";
        const string TYPE_ORDER = "order";

        //static JilSerializer _serializer = new JilSerializer();
        static JsonNetSerializer _serializer = new JsonNetSerializer();

        ElasticConnection _connection;

        public OrderRepository(string host, int port)
        {
            _connection = new ElasticConnection(host, port);
        }

        public async Task<bool> ExistsAsync(Guid orderID)
        {
            try
            {
                var command = Commands.Get(INDEX_NAME, TYPE_ORDER, orderID.ToString("D"));
                var result  = await _connection.HeadAsync(command).ConfigureAwait(false);
                return true;
            }
            catch (OperationException ex)
            {
                if (ex.HttpStatusCode != 404)
                    throw;
            }

            return false;
        }

        public async Task SaveAsync(Order order)
        {
            order.LastModifiedDate = DateTime.Now;

            var json    = _serializer.Serialize(order);
            var command = Commands.Index(INDEX_NAME, TYPE_ORDER, order.OrderID.ToString("D"));
            var result  = await _connection.PutAsync(command, json).ConfigureAwait(false);
        }

        public async Task<Order> GetByIdAsync(Guid orderID)
        {
            var command     = Commands.Get(INDEX_NAME, TYPE_ORDER, orderID.ToString("D"));
            var result      = await _connection.GetAsync(command).ConfigureAwait(false);
            var orderResult = _serializer.ToGetResult<Order>(result.Result);
            return orderResult.Document;
        }

        public async Task<OrderQueryResult> GetConsultantOrdersAsync(Guid consultantKey, int page, int pageSize)
        {
            var query = new QueryBuilder<Order>()
                .Query
                (
                    q => q.Match
                    (
                        m => m.Field(o => o.ConsultantKey)
                            .Query(consultantKey.ToString("D"))
                    )
                )
                .Sort(s => s.Field(o => o.CreatedDate, SortDirection.desc))
                .From(page - 1)
                .Size(pageSize)
                .Build();

            var command = Commands.Search(INDEX_NAME, TYPE_ORDER);

            var jsonResult = await _connection.PostAsync(command, query).ConfigureAwait(false);
            var result = _serializer.ToSearchResult<Order>(jsonResult.Result);

            var queryResult = new OrderQueryResult
            {
                Page       = page,
                PageSize   = pageSize,
                TotalCount = result.hits.total,
                Orders     = result.hits.hits.Select(h => h._source).ToArray()
            };

            return queryResult;
        }
    }
}