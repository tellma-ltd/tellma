using Tellma.Controllers.Dto;
using Tellma.Data;
using Tellma.Services.ApiAuthentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Tellma.Controllers.Utilities;

namespace Tellma.Controllers
{
    [Route("api/admin-permissions")]
    [ApiController]
    [AuthorizeJwtBearer]
    [AdminController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AdminPermissionsController : ControllerBase
    {
        private readonly AdminPermissionsService _service;
        private readonly ILogger _logger;

        public AdminPermissionsController(AdminPermissionsService service, ILogger<PermissionsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("client")]
        public virtual async Task<ActionResult<Versioned<PermissionsForClient>>> PermissionsForClient(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Retrieve the user permissions and their current version
                var result = await _service.PermissionsForClient(cancellation);
                return Ok(result);
            },
            _logger);
        }
    }

    public class AdminPermissionsService : ServiceBase
    {
        private readonly AdminRepository _repo;

        public AdminPermissionsService(AdminRepository repo)
        {
            _repo = repo;
        }

        public virtual async Task<Versioned<PermissionsForClient>> PermissionsForClient(CancellationToken cancellation)
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
            var result = new Versioned<PermissionsForClient>
            (
                version: version.ToString(),
                data: permissionsForClient
            );

            // Return the result
            return result;
        }
    }
}
