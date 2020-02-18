using Tellma.Controllers.Dto;
using Tellma.Data;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.ClientInfo;
using Tellma.Services.Identity;
using Tellma.Services.Sharding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tellma.Controllers
{
    [Route("api/companies")]
    [ApiController]
    [AuthorizeAccess]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class CompaniesController : ControllerBase
    {
        // Private fields

        private readonly AdminRepository _repo;
        private readonly ILogger _logger;
        private readonly IShardResolver _shardResolver;
        private readonly IExternalUserAccessor _externalUserAccessor;
        private readonly IClientInfoAccessor _clientInfoAccessor;


        // Constructor

        public CompaniesController(AdminRepository db, ILogger<CompaniesController> logger,
            IShardResolver shardResolver, IExternalUserAccessor externalUserAccessor, IClientInfoAccessor clientInfoAccessor)
        {
            _repo = db;
            _logger = logger;
            _shardResolver = shardResolver;
            _externalUserAccessor = externalUserAccessor;
            _clientInfoAccessor = clientInfoAccessor;
        }

        [HttpGet("client")]
        public async Task<ActionResult<CompaniesForClient>> CompaniesForClient()
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

        private async Task<CompaniesForClient> GetForClientImpl()
        {
            var companies = new List<UserCompany>();

            var externalId = _externalUserAccessor.GetUserId();
            var externalEmail = _externalUserAccessor.GetUserEmail();
            var (databaseIds, isAdmin) = await _repo.GetAccessibleDatabaseIds(externalId, externalEmail);

            // Confirm each database Id by checking the respective DB
            foreach (var databaseId in databaseIds)
            {
                try
                {
                    var connString = _shardResolver.GetConnectionString(databaseId);
                    using var appRepo = new ApplicationRepository(null, _externalUserAccessor, _clientInfoAccessor, null);

                    await appRepo.InitConnectionAsync(connString, setLastActive: false);
                    var userInfo = await appRepo.GetUserInfoAsync();
                    if (userInfo.UserId != null)
                    {
                        var tenantInfo = await appRepo.GetTenantInfoAsync();
                        companies.Add(new UserCompany
                        {
                            Id = databaseId,
                            Name = tenantInfo.ShortCompanyName,
                            Name2 = string.IsNullOrWhiteSpace(tenantInfo.SecondaryLanguageId) ? null : tenantInfo.ShortCompanyName2,
                            Name3 = string.IsNullOrWhiteSpace(tenantInfo.TernaryLanguageId) ? null : tenantInfo.ShortCompanyName3
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception while loading user companies: DatabaseId: {databaseId}, User email: {_externalUserAccessor.GetUserEmail()}, {ex.GetType().Name}: {ex.Message}");
                }
            }
            
            // Confirm isAdmin by checking with the admin DB
            if (isAdmin)
            {
                var adminUserInfo = await _repo.GetAdminUserInfoAsync();
                isAdmin = adminUserInfo?.UserId != null;
            }

            return new CompaniesForClient
            {
                IsAdmin = isAdmin,
                Companies = companies,
            };
        }
    }
}
