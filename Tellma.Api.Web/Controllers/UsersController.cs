using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Application;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [AuthorizeJwtBearer]
    [ApplicationController]
    public class UsersController : CrudControllerBase<UserForSave, User, int>
    {
        public const string BASE_ADDRESS = "users";

        private readonly UsersService _service;

        public UsersController(UsersService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpGet("client")]
        public async Task<ActionResult<Versioned<UserSettingsForClient>>> UserSettingsForClient(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await GetService().UserSettingsForClient(cancellation);
                return Ok(result);
            },
            _logger);
        }

        [HttpPost("client")]
        public async Task<ActionResult<Versioned<UserSettingsForClient>>> SaveUserSetting(SaveUserSettingsArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await GetService().SaveUserSetting(args);
                return Ok(result);
            },
            _logger);
        }

        [HttpPost("client/preferred-language")]
        public async Task<ActionResult<Versioned<UserSettingsForClient>>> SaveUserPreferredLanguage(string preferredLanguage, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await GetService().SaveUserPreferredLanguage(preferredLanguage, cancellation);
                return Ok(result);
            },
            _logger);
        }

        [HttpPost("client/preferred-calendar")]
        public async Task<ActionResult<Versioned<UserSettingsForClient>>> SaveUserPreferredCalendar(string preferredCalendar, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await GetService().SaveUserPreferredCalendar(preferredCalendar, cancellation);
                return Ok(result);
            },
            _logger);
        }

        [HttpPut("invite")]
        public async Task<ActionResult> ResendInvitationEmail(int id)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                await GetService().ResendInvitationEmail(id);
                return Ok();
            },
            _logger);
        }

        [HttpGet("{id}/image")]
        public async Task<ActionResult> GetImage(int id, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var (imageId, imageBytes) = await GetService().GetImage(id, cancellation);
                Response.Headers.Add("x-image-id", imageId);
                return File(imageBytes, "image/jpeg");
            },
            _logger);
        }

        [HttpGet("me")]
        public async Task<ActionResult<GetByIdResponse<User>>> GetMyUser(CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                User user = await GetService().GetMyUser(cancellation);
                GetByIdResponse<User> response = TransformToResponse(user, cancellation);
                return Ok(response);
            },
            _logger);
        }

        [HttpPost("me")]
        public async Task<ActionResult<GetByIdResponse<User>>> SaveMyUser([FromBody] MyUserForSave me)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                User user = await GetService().SaveMyUser(me);
                GetByIdResponse<User> result = TransformToResponse(user, cancellation: default);
                Response.Headers.Set("x-user-settings-version", Constants.Stale);
                return Ok(result);

            }, _logger);
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<User>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await GetService().Activate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            },
            _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<User>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await GetService().Deactivate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            },
            _logger);
        }

        [HttpPut("test-email")]
        public async Task<ActionResult<string>> TestEmail([FromQuery] string email)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                string result = await GetService().TestEmail(email);
                return Ok(new
                {
                    Message = result
                });
            },
            _logger);
        }

        [HttpPut("test-phone")]
        public async Task<ActionResult<string>> TestPhone([FromQuery] string phone)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                string result = await GetService().TestPhone(phone);
                return Ok(new
                {
                    Message = result
                });
            },
            _logger);
        }

        private GetByIdResponse<User> TransformToResponse(User me, CancellationToken cancellation)
        {
            // Apply the permission masks (setting restricted fields to null) and adjust the metadata accordingly
            var relatedEntities = FlattenAndTrim(new List<User> { me }, cancellation);

            // Return
            return new GetByIdResponse<User>
            {
                Result = me,
                CollectionName = ControllerUtilities.GetCollectionName(typeof(User)),
                RelatedEntities = relatedEntities
            };
        }

        protected override CrudServiceBase<UserForSave, User, int> GetCrudService()
        {
            return _service.SetUrlHelper(Url).SetScheme(Request.Scheme);
        }

        private UsersService GetService()
        {
            return _service.SetUrlHelper(Url).SetScheme(Request.Scheme);
        }
    }
}
