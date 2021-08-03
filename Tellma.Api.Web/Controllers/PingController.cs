using Microsoft.AspNetCore.Mvc;

namespace Tellma.Controllers
{
    [Route("api/ping")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class PingController : ControllerBase
    {
        [HttpGet]
        public ActionResult Ping() => Ok(); // Used by clients to check if they are online
    }
}
