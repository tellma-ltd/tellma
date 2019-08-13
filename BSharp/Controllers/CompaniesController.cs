using BSharp.Controllers.Dto;
using BSharp.Data;
using BSharp.Services.ApiAuthentication;
using BSharp.Services.ClientInfo;
using BSharp.Services.Identity;
using BSharp.Services.Sharding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    [Route("api/companies")]
    [ApiController]
    [AuthorizeAccess]
    [AdminApi]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class CompaniesController : ControllerBase
    {
        // Private fields

        private readonly AdminRepository _repo;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly IShardResolver _shardResolver;
        private readonly IExternalUserAccessor _externalUserAccessor;
        private readonly IClientInfoAccessor _clientInfoAccessor;


        // Constructor

        public CompaniesController(AdminRepository db, ILogger<CompaniesController> logger,
            IStringLocalizer<Strings> localizer, IShardResolver shardResolver,
            IExternalUserAccessor externalUserAccessor, IClientInfoAccessor clientInfoAccessor)
        {
            _repo = db;
            _logger = logger;
            _localizer = localizer;
            _shardResolver = shardResolver;
            _externalUserAccessor = externalUserAccessor;
            _clientInfoAccessor = clientInfoAccessor;
        }


        [HttpGet("client")]
        public async Task<ActionResult<IEnumerable<UserCompany>>> GetForClient()
        {
            try
            {
                var result = await GetForClientImpl();
                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        private async Task<IEnumerable<UserCompany>> GetForClientImpl()
        {
            var result = new List<UserCompany>();

            var databaseIds = await _repo.GetAccessibleDatabaseIds();
            foreach (var databaseId in databaseIds)
            {
                var connString = _shardResolver.GetConnectionString(databaseId);
                using (var appRepo = new ApplicationRepository(null, _externalUserAccessor, _clientInfoAccessor, null))
                {
                    await appRepo.InitConnectionAsync(connString);
                    var userInfo = await appRepo.GetUserInfoAsync();
                    if (userInfo.UserId != null)
                    {
                        var tenantInfo = await appRepo.GetTenantInfoAsync();
                        result.Add(new UserCompany
                        {
                            Id = databaseId,
                            Name = tenantInfo.ShortCompanyName,
                            Name2 = tenantInfo.ShortCompanyName2,
                            Name3 = tenantInfo.ShortCompanyName3
                        });
                    }
                }
            }

            return result;
        }
    }
}