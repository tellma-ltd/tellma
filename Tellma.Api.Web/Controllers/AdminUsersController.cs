using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Admin;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/admin-users")]
    [AuthorizeJwtBearer]
    [AdminController]
    [ApiVersion("1.0")]
    public class AdminUsersController : CrudControllerBase<AdminUserForSave, AdminUser, int>
    {
        private readonly AdminUsersService _service;

        public AdminUsersController(AdminUsersService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpGet("client")]
        public async Task<ActionResult<Versioned<AdminUserSettingsForClient>>> UserSettingsForClient(CancellationToken cancellation)
        {
            var result = await _service.UserSettingsForClient(cancellation);
            return Ok(result);
        }

        [HttpPost("client")]
        public async Task<ActionResult<Versioned<AdminUserSettingsForClient>>> SaveUserSetting(SaveUserSettingsArguments args)
        {
            var result = await _service.SaveUserSetting(args);
            return Ok(result);
        }

        [HttpGet("me")]
        public async Task<ActionResult<GetByIdResponse<AdminUser>>> GetMyUser(CancellationToken cancellation)
        {
            var user = await _service.GetMyUser(cancellation);
            GetByIdResponse<AdminUser> result = TransformToResponse(user, cancellation);

            return Ok(result);
        }

        [HttpPost("me")]
        public async Task<ActionResult<GetByIdResponse<AdminUser>>> SaveMyUser([FromBody] MyAdminUserForSave me)
        {
            var user = await _service.SaveMyUser(me);
            Response.Headers.Set("x-admin-user-settings-version", Constants.Stale);
            GetByIdResponse<AdminUser> result = TransformToResponse(user, cancellation: default);

            return Ok(result);
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<AdminUser>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await _service.Activate(ids: ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            return Ok(response);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<AdminUser>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await _service.Deactivate(ids: ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            return Ok(response);
        }

        [HttpPut("invite")]
        public async Task<ActionResult> SendInvitation([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await _service.SendInvitation(ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            return Ok(response);
        }

        private GetByIdResponse<AdminUser> TransformToResponse(AdminUser me, CancellationToken cancellation)
        {
            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            var relatedEntities = FlattenAndTrim(new List<AdminUser> { me }, cancellation);

            // Return
            return new GetByIdResponse<AdminUser>
            {
                Result = me,
                CollectionName = ControllerUtilities.GetCollectionName(typeof(AdminUser)),
                RelatedEntities = relatedEntities
            };
        }

        protected override CrudServiceBase<AdminUserForSave, AdminUser, int> GetCrudService()
        {
            return _service;
        }

        protected override async Task OnSuccessfulSave(List<AdminUser> data, Extras extras)
        {
            var meId = _service.UserId;

            if (data.Any(e => e.Id == meId))
            {
                Response.Headers.Set("x-admin-user-settings-version", Constants.Stale);
                Response.Headers.Set("x-admin-permissions-version", Constants.Stale);
            }

            await base.OnSuccessfulSave(data, extras);
        }
    }
}
