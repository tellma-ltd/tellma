using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Model.Admin;
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
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await _service.ResetPassword(args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            return Ok(response);
        }

        protected override FactGetByIdServiceBase<IdentityServerUser, string> GetFactGetByIdService()
        {
            return _service;
        }
    }
}
