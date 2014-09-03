using System.Web;
using System.Web.Http;

namespace MK.Ordering.Service
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            ContainerConfig.Configure(GlobalConfiguration.Configuration);
        }
    }
}
