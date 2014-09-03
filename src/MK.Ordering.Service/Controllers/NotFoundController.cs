using System.Web.Http;

namespace MK.Ordering.Service.Controllers
{
    public class NotFoundController : ApiController
    {
        public IHttpActionResult Get()
        {
            return NotFound();
        }
    }
}
