using Microsoft.AspNetCore.Mvc;

namespace BSharp.Controllers
{
    [Route("api/ping")]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class PingController : ControllerBase
    {
        [HttpGet]
        public ActionResult Ping()
        {
            // Used by clients to check if they are online
            return Ok();
        }
    }
}
