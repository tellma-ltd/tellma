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
    [Route("api/users")]
    [AuthorizeJwtBearer]
    [ApplicationController]
    public class UsersController : CrudControllerBase<UserForSave, User, int>
    {
        private readonly UsersService _service;

        public UsersController(UsersService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpGet("client")]
        public async Task<ActionResult<Versioned<UserSettingsForClient>>> UserSettingsForClient(CancellationToken cancellation)
        {
            var result = await _service.UserSettingsForClient(cancellation);
            return Ok(result);
        }

        [HttpPost("client")]
        public async Task<ActionResult<Versioned<UserSettingsForClient>>> SaveUserSetting(SaveUserSettingsArguments args)
        {
            var result = await _service.SaveUserSetting(args);
            return Ok(result);
        }

        [HttpPost("client/preferred-language")]
        public async Task<ActionResult<Versioned<UserSettingsForClient>>> SaveUserPreferredLanguage(string preferredLanguage, CancellationToken cancellation)
        {
            var result = await _service.SaveUserPreferredLanguage(preferredLanguage, cancellation);
            return Ok(result);
        }

        [HttpPost("client/preferred-calendar")]
        public async Task<ActionResult<Versioned<UserSettingsForClient>>> SaveUserPreferredCalendar(string preferredCalendar, CancellationToken cancellation)
        {
            var result = await _service.SaveUserPreferredCalendar(preferredCalendar, cancellation);
            return Ok(result);
        }

        [HttpPut("invite")]
        public async Task<ActionResult> SendInvitation([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await _service.SendInvitation(ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            return Ok(response);
        }

        [HttpGet("{id}/image")]
        public async Task<ActionResult> GetImage(int id, CancellationToken cancellation)
        {
            var (imageId, imageBytes) = await _service.GetImage(id, cancellation);
            Response.Headers.Add("x-image-id", imageId);

            return File(imageBytes, MimeTypes.Jpeg);
        }

        [HttpGet("me")]
        public async Task<ActionResult<GetByIdResponse<User>>> GetMyUser(CancellationToken cancellation)
        {
            User user = await _service.GetMyUser(cancellation);
            GetByIdResponse<User> response = TransformToResponse(user, cancellation);

            return Ok(response);
        }

        [HttpPost("me")]
        public async Task<ActionResult<GetByIdResponse<User>>> SaveMyUser([FromBody] MyUserForSave me)
        {
            User user = await _service.SaveMyUser(me);
            GetByIdResponse<User> result = TransformToResponse(user, cancellation: default);
            Response.Headers.Set("x-user-settings-version", Constants.Stale);

            return Ok(result);
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<User>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await _service.Activate(ids: ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            return Ok(response);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<User>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await _service.Deactivate(ids: ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            return Ok(response);
        }

        [HttpPut("test-email")]
        public async Task<ActionResult<string>> TestEmail([FromQuery] string email)
        {
            string result = await _service.TestEmail(email);
            return Ok(new
            {
                Message = result
            });
        }

        [HttpPut("test-phone")]
        public async Task<ActionResult<string>> TestPhone([FromQuery] string phone)
        {
            string result = await _service.TestPhone(phone);
            return Ok(new
            {
                Message = result
            });
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
            return _service;
        }
    }
}
