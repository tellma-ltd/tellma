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
        public virtual async Task<ActionResult<DataWithVersion<PermissionsForClient>>> PermissionsForClient()
        {
            try
            {
                // Retrieve the user permissions and their current version
                var (version, permissions)  = await _repo.Permissions__Load();

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
