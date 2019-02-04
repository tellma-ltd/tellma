using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    [Route("api/permissions")]
    [ApiController]
    [LoadTenantInfo]
    public class PermissionsController : ControllerBase
    {
        // Private fields

        private readonly ApplicationContext _db;
        private readonly ILogger<PermissionsController> _logger;
        private readonly ITenantUserInfoAccessor _tenantInfo;


        // Constructor

        public PermissionsController(ApplicationContext db, ILogger<PermissionsController> logger,
            IStringLocalizer<PermissionsController> localizer, IMapper mapper, ITenantUserInfoAccessor tenantInfo)
        {
            _db = db;
            _logger = logger;
            _tenantInfo = tenantInfo;
        }


        // API

        [HttpGet("client")]
        public virtual async Task<ActionResult<DataWithVersion<PermissionsForClient>>> GetForClient()
        {
            try
            {
                // Retrieve the current version of the permissions
                int userId = _tenantInfo.UserId();
                Guid version = await _db.LocalUsers.Where(e => e.Id == userId).Select(e => e.PermissionsVersion).FirstOrDefaultAsync();
                if (version == null)
                {
                    // This should never happen
                    return BadRequest("No user in the system");
                }

                // Retrieve all the permissions
                var allPermissions = await _db.AbstractPermissions.FromSql($@"
    DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

    SELECT ViewId, Criteria, Level 
    FROM [dbo].[Permissions] P
    JOIN [dbo].[Roles] R ON P.RoleId = R.Id
    JOIN [dbo].[RoleMemberships] RM ON R.Id = RM.RoleId
    WHERE R.IsActive = 1 
    AND RM.UserId = @UserId
    UNION
    SELECT ViewId, Criteria, Level 
    FROM [dbo].[Permissions] P
    JOIN [dbo].[Roles] R ON P.RoleId = R.Id
    WHERE R.IsPublic = 1 
    AND R.IsActive = 1
").ToListAsync();

                // Arrange the permission in a DTO that is easy for clients to consume
                var permissions = new PermissionsForClient();
                foreach (var gViewIds in allPermissions.GroupBy(e => e.ViewId))
                {
                    string viewId = gViewIds.Key;
                    var gLevels = gViewIds.GroupBy(e => e.Level);
                    var viewPermissions = new ViewPermissionsForClient();
                    foreach (var gLevel in gLevels)
                    {
                        var level = gLevel.Key;
                        if (level == Constants.Read)
                        {
                            viewPermissions.Read = true;
                        }

                        if (level == Constants.Create)
                        {
                            viewPermissions.Create = true;
                        }

                        if (level == Constants.ReadCreate)
                        {
                            viewPermissions.Read = true;
                            viewPermissions.Create = true;
                        }

                        if (level == Constants.Update)
                        {
                            viewPermissions.Update = true;
                        }

                        if (level == Constants.Sign)
                        {
                            viewPermissions.Sign = true;
                        }
                    }

                    permissions[viewId] = viewPermissions;
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