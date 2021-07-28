using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Services.ApiAuthentication;

namespace Tellma.Controllers
{
    [Route("api/definitions")]
    [AuthorizeJwtBearer]
    [ApplicationController]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class DefinitionsController : ControllerBase
    {
        private readonly DefinitionsService _service;

        public DefinitionsController(DefinitionsService service)
        {
            _service = service;
        }

        [HttpGet("client")]
        public async Task<ActionResult<Versioned<DefinitionsForClient>>> DefinitionsForClient(CancellationToken cancellation)
        {
            var result = await _service.DefinitionsForClient(cancellation);
            return Ok(result);
        }
    }
}
