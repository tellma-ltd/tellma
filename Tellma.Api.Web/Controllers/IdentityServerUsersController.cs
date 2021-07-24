using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Identity;
using Tellma.Services.EmbeddedIdentityServer;

namespace Tellma.Controllers
{
    [Route("api/identity-server-users")]
    [AdminController]
    public class IdentityServerUsersController : FactGetByIdControllerBase<IdentityServerUser, string>
    {
        private readonly IdentityServerUsersService _service;

        public IdentityServerUsersController(IdentityServerUsersService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpPut("reset-password")]
        public async Task<ActionResult<EntitiesResponse<IdentityServerUser>>> ResetPassword(ResetPasswordArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.ResetPassword(args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            }
            , _logger);
        }

        protected override FactGetByIdServiceBase<IdentityServerUser, string> GetFactGetByIdService()
        {
            return _service;
        }
    }
}
