using System;
using System.Net.Http;
using System.Web.Http.SelfHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MK.Ordering.Service.Models;

namespace MK.Ordering.Service
{
    [TestClass]
    public static class Setup
    {
        static HttpSelfHostServer _server;

        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            var config = new HttpSelfHostConfiguration("http://localhost:8888/");
            
            WebApiConfig.Register(config);
            config.EnsureInitialized();

            ContainerConfig.Configure(config);

            _server = new HttpSelfHostServer(config);
            _server.OpenAsync().Wait();
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            _server.CloseAsync().Wait();
        }
    }

    [TestClass]
    public class OrderTests
    {
        [TestMethod]
        public void CanCreateAndRetrieveOrder()
        {
            var consultantKey = Guid.NewGuid();
            var orderID = Guid.Empty;

            var svc = new OrderService("http://localhost:8888");
            var order = svc.CreateOrderAsync(consultantKey).Result;

            Assert.IsNotNull(order);
            Assert.AreEqual(consultantKey, order.ConsultantKey);

            orderID = order.OrderID;
            order = svc.GetOrderByIdAsync(order.OrderID).Result;
            Assert.IsNotNull(order);
            Assert.AreEqual(consultantKey, order.ConsultantKey);
            Assert.AreEqual(orderID, order.OrderID);
        }

        [TestMethod]
        public void CanAddItemsToOrder()
        {
            var consultantKey = Guid.NewGuid();
            var orderID = Guid.Empty;

            var svc = new OrderService("http://localhost:8888");
            var order = svc.CreateOrderAsync(consultantKey).Result;

            Assert.IsNotNull(order);
            Assert.AreEqual(consultantKey, order.ConsultantKey);

            var items = new[]
            {
                new Order.Item
                {
                    Sku = "12345",
                    Name = "Test Product",
                    Description = "Test Product Description",
                    Quantity = 2,
                    UnitPrice = 3
                }
            };

            orderID = order.OrderID;
            order = svc.UpdateItems(order.OrderID, items).Result;
            Assert.IsNotNull(order);
            Assert.AreEqual(consultantKey, order.ConsultantKey);
            Assert.AreEqual(orderID, order.OrderID);
            Assert.IsNotNull(order.Items);
            Assert.AreEqual(1, order.Items.Length);
            Assert.AreEqual(items[0].Sku, order.Items[0].Sku);
            Assert.AreEqual(items[0].Name, order.Items[0].Name);
            Assert.AreEqual(items[0].Description, order.Items[0].Description);
            Assert.AreEqual(items[0].Quantity, order.Items[0].Quantity);
            Assert.AreEqual(items[0].UnitPrice, order.Items[0].UnitPrice);
            Assert.AreEqual(6, order.Items[0].Price);
        }
    }
}
