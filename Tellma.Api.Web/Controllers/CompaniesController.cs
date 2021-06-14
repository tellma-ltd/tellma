using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Data;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.ClientInfo;
using Tellma.Services.Identity;
using Tellma.Services.MultiTenancy;
using Tellma.Services.Sharding;
using Tellma.Services.Instrumentation;
using Microsoft.Extensions.DependencyInjection;

namespace Tellma.Controllers
{
    [Route("api/companies")]
    [ApiController]
    [AuthorizeJwtBearer]
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
                _logger.LogError(ex, $"Error caught in {nameof(CompaniesController)}.{nameof(CompaniesForClient)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }
    }

    public class CompaniesService : ServiceBase
    {
        private readonly AdminRepository _repo;
        private readonly ILogger _logger;
        private readonly IExternalUserAccessor _externalUserAccessor;
        private readonly IServiceProvider _serviceProvider;

        //private readonly IShardResolver _shardResolver;
        //private readonly IClientInfoAccessor _clientInfoAccessor;
        //private readonly ITenantIdAccessor _tenantIdAccessor;

        public CompaniesService(AdminRepository db,
            ILogger<CompaniesController> logger,
            IExternalUserAccessor externalUserAccessor,
            IServiceProvider serviceProvider)
        {
            _repo = db;
            _logger = logger;
            _externalUserAccessor = externalUserAccessor;
            _serviceProvider = serviceProvider;
        }

        public async Task<CompaniesForClient> GetForClient(CancellationToken cancellation)
        {
            var companies = new ConcurrentBag<UserCompany>();

            var externalId = _externalUserAccessor.GetUserId();
            var externalEmail = _externalUserAccessor.GetUserEmail();
            var (databaseIds, isAdmin) = await _repo.GetAccessibleDatabaseIds(externalId, externalEmail, cancellation);

            // Connect all the databases in parallel, ensure the user cann access them all
            var tasks = databaseIds.Select(databaseId => GetCompanyInfoAsync(databaseId, companies, cancellation));
            await Task.WhenAll(tasks);

            // Confirm isAdmin by checking with the admin DB
            if (isAdmin)
            {
                var adminUserInfo = await _repo.GetAdminUserInfoAsync(cancellation);
                isAdmin = adminUserInfo?.UserId != null;
            }

            return new CompaniesForClient
            {
                IsAdmin = isAdmin,
                Companies = companies.OrderBy(e => e.Id).ToList(),
            };
        }

        /// <summary>
        /// Connects to the database with the given Id, if the user is indeed a member of
        /// it and adds the database info into the companies concurrent bag parameter
        /// </summary>
        private async Task GetCompanyInfoAsync(int databaseId, ConcurrentBag<UserCompany> companies, CancellationToken cancellation)
        {
            try
            {
                using var appRepo = new ApplicationRepository(_serviceProvider); // new ApplicationRepository(_shardResolver, _externalUserAccessor, _clientInfoAccessor, null, _tenantIdAccessor, new DoNothingService());
                
                await appRepo.InitConnectionAsync(databaseId, setLastActive: false, cancellation);
                UserInfo userInfo = await appRepo.GetUserInfoAsync(cancellation);
                if (userInfo.UserId != null)
                {
                    TenantInfo tenantInfo = await appRepo.GetTenantInfoAsync(cancellation);
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
                _logger.LogError(ex, $"Exception while loading user companies: DatabaseId: {databaseId}, User email: {_externalUserAccessor.GetUserEmail()}, {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
