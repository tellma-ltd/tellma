using Tellma.Controllers.Dto;
using Tellma.Data;
using Tellma.Services.ApiAuthentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Tellma.Controllers.Utilities;

namespace Tellma.Controllers
{
    [Route("api/permissions")]
    [ApiController]
    [AuthorizeAccess]
    [ApplicationController(allowUnobtrusive: true)]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class PermissionsController : ControllerBase
    {
        private readonly ApplicationRepository _repo;
        private readonly PermissionsService _service;
        private readonly ILogger _logger;

        public PermissionsController(PermissionsService service, ILogger<PermissionsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("client")]
        public virtual async Task<ActionResult<DataWithVersion<PermissionsForClient>>> PermissionsForClient(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await _service.PermissionsForClient(cancellation);
                return Ok(result);
            }, 
            _logger);
        }
    }

    public class PermissionsService : ServiceBase
    {
        private readonly ApplicationRepository _repo;

        public PermissionsService(ApplicationRepository repo)
        {
            _repo = repo;
        }

        public async Task<DataWithVersion<PermissionsForClient>> PermissionsForClient(CancellationToken cancellation)
        {                
            // Retrieve the user permissions and their current version
            var (version, permissions) = await _repo.Permissions__Load(cancellation);

            // Arrange the permission in a DTO that is easy for clients to consume
            var permissionsForClient = new PermissionsForClient();
            foreach (var gView in permissions.GroupBy(e => e.View))
            {
                string view = gView.Key;
                Dictionary<string, bool> viewActions = gView
                    .GroupBy(e => e.Action)
                    .ToDictionary(g => g.Key, g => true);

                permissionsForClient[view] = viewActions;
            }

            // Tag the permissions for client with their current version
            var result = new DataWithVersion<PermissionsForClient>
            {
                Version = version.ToString(),
                Data = permissionsForClient
            };

            // Return the result
            return result;
        }
    }
}
