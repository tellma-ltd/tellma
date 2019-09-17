using BSharp.Controllers.Dto;
using BSharp.Controllers.Utilities;
using BSharp.Data;
using BSharp.Services.ApiAuthentication;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    [Route("api/permissions")]
    [ApiController]
    [AuthorizeAccess]
    [ApplicationApi]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class PermissionsController : ControllerBase
    {
        private readonly ApplicationRepository _repo;
        private readonly ILogger _logger;

        public PermissionsController(ApplicationRepository repo, ILogger<PermissionsController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        [HttpGet("client")]
        public virtual async Task<ActionResult<DataWithVersion<PermissionsForClient>>> GetForClient()
        {
            try
            {
                // Retrieve the current version of the permissions
                Guid version = await _repo.GetUserPermissionsVersion();
                if (version == null)
                {
                    // Programmer mistake
                    return BadRequest("No user in the system");
                }

                // Retrieve all the permissions
                IEnumerable<AbstractPermission> allPermissions = await _repo.GetUserPermissions();

                // Arrange the permission in a DTO that is easy for clients to consume
                var permissions = new PermissionsForClient();
                foreach (var gViewIds in allPermissions.GroupBy(e => e.ViewId))
                {
                    string viewId = gViewIds.Key;
                    Dictionary<string, bool> viewActions = gViewIds
                        .GroupBy(e => e.Action)
                        .ToDictionary(g => g.Key, g => true);

                    permissions[viewId] = viewActions;
                }

                // Tag the permissions for client with their current version
                var result = new DataWithVersion<PermissionsForClient>
                {
                    Version = version.ToString(),
                    Data = permissions
                };

                // Return the result
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }
    }
}
