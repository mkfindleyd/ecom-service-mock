using System;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Routing;
using CacheCow.Server;
using CacheCow.Server.CacheRefreshPolicy;
using MK.Ordering.Service.Infrastructure;
using NLog;

namespace MK.Ordering.Service
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            // Configure JSON formatter and remove all others
            //var jsonFormatter = new JilFormatter();
            //GlobalConfiguration.Configuration.Formatters.Clear();
            //GlobalConfiguration.Configuration.Formatters.Add(jsonFormatter);

            var cacheHandler = new CachingHandler(config)
            {
                CacheRefreshPolicyProvider = new AttributeBasedCacheRefreshPolicy(TimeSpan.FromMinutes(5)).GetCacheRefreshPolicy
            };
            config.MessageHandlers.Add(cacheHandler);

            config.Filters.Add(new GlobalExceptionFilter());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{*path}",
                defaults: new { controller = "NotFound" }
            );
        }
    }

    public class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();

        public override void OnException(HttpActionExecutedContext context)
        {
            _logger.Error(context.Exception);
            base.OnException(context);
        }
    }
}
