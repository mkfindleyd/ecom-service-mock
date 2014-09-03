using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MK.Ordering.Service.Models
{
    public class OrderQueryResult
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public Order[] Orders { get; set; }
    }

    public class UpdateItemsRequest
    {
        public Order.Item[] Items { get; set; }
    }

    public class InventoryCheckRequest
    {
        public Product[] Products { get; set; }

        public class Product
        {
            public string Sku { get; set; }
            public int QuantityRequested { get; set; }
        }
    }

    public class InventoryCheckResponse
    {
        public Result[] Results { get; set; }
        
        public class Result
        {
            public string Sku { get; set; }
            public int QuantityRequested { get; set; }
            public int QuantityAvailable { get; set; }
            public bool IsAvailable { get; set; }
        }
    }

    public class Order
    {
        public Guid OrderID { get; set; }
        public Guid ConsultantKey { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public decimal SubTotal { get; set; }
        public OrderStatus Status { get; set; }
        public Item[] Items { get; set; }

        public class Item
        {
            public string Sku { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public ushort Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Price { get; set; }
        }

        public enum OrderStatus
        {
            New,
            Submitted,
            Shipped
        }
    }
}