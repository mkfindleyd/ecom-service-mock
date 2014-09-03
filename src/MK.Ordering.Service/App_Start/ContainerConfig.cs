using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using MK.Ordering.Service.Models;

namespace MK.Ordering.Service
{
    public static class ContainerConfig
    {
        public static void Configure(HttpConfiguration config)
        {
            // Create the container builder.
            var builder = new ContainerBuilder();

            // Register the Web API controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // Register other dependencies.
            builder
                .Register(c => new OrderRepository("localhost", 9200))
                .As<IOrderRepository>()
                .SingleInstance();

            // Build the container.
            var container = builder.Build();

            // Create the depenedency resolver.
            var resolver = new AutofacWebApiDependencyResolver(container);

            // Configure Web API with the dependency resolver.
            config.DependencyResolver = resolver;
        }
    }
}