using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Data;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.ClientInfo;
using Tellma.Services.Identity;
using Tellma.Services.MultiTenancy;
using Tellma.Services.Sharding;

namespace Tellma.Controllers
{
    [Route("api/companies")]
    [ApiController]
    [AuthorizeAccess]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class CompaniesController : ControllerBase
    {
        private readonly CompaniesService _service;
        private readonly ILogger<CompaniesController> _logger;

        public CompaniesController(CompaniesService service, ILogger<CompaniesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("client")]
        public async Task<ActionResult<CompaniesForClient>> CompaniesForClient(CancellationToken cancellation)
        {
            try
            {
                var result = await _service.GetForClient(cancellation);
                return Ok(result);
            }
            catch (TaskCanceledException)
            {
                return Ok();
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
    }

    public class CompaniesService : ServiceBase
    {
        private readonly AdminRepository _repo;
        private readonly ILogger _logger;
        private readonly IShardResolver _shardResolver;
        private readonly IExternalUserAccessor _externalUserAccessor;
        private readonly IClientInfoAccessor _clientInfoAccessor;
        private readonly ITenantIdAccessor _tenantIdAccessor;

        public CompaniesService(AdminRepository db, ILogger<CompaniesController> logger, IShardResolver shardResolver,
            IExternalUserAccessor externalUserAccessor, IClientInfoAccessor clientInfoAccessor,
            ITenantIdAccessor tenantIdAccessor)
        {
            _repo = db;
            _logger = logger;
            _shardResolver = shardResolver;
            _externalUserAccessor = externalUserAccessor;
            _clientInfoAccessor = clientInfoAccessor;
            _tenantIdAccessor = tenantIdAccessor;
        }

        public async Task<CompaniesForClient> GetForClient(CancellationToken cancellation)
        {
            var companies = new List<UserCompany>();

            var externalId = _externalUserAccessor.GetUserId();
            var externalEmail = _externalUserAccessor.GetUserEmail();
            var (databaseIds, isAdmin) = await _repo.GetAccessibleDatabaseIds(externalId, externalEmail, cancellation);

            // Confirm each database Id by checking the respective DB
            foreach (var databaseId in databaseIds)
            {
                try
                {
                    using var appRepo = new ApplicationRepository(_shardResolver, _externalUserAccessor, _clientInfoAccessor, null, _tenantIdAccessor);

                    await appRepo.InitConnectionAsync(databaseId, setLastActive: false, cancellation);
                    var userInfo = await appRepo.GetUserInfoAsync(cancellation);
                    if (userInfo.UserId != null)
                    {
                        var tenantInfo = await appRepo.GetTenantInfoAsync(cancellation);
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
                var adminUserInfo = await _repo.GetAdminUserInfoAsync(cancellation);
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
