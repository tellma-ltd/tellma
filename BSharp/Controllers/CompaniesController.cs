using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BSharp.Controllers.Dto;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.ApiAuthentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using BSharp.Services.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BSharp.Controllers
{
    [Route("api/companies")]
    [ApiController]
    [AuthorizeAccess]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class CompaniesController : ControllerBase
    {
        // Private fields

        private readonly AdminContext _db;
        private readonly ILogger<CompaniesController> _logger;
        private readonly IStringLocalizer<CompaniesController> _localizer;
        private readonly IMapper _mapper;


        // Constructor

        public CompaniesController(AdminContext db, ILogger<CompaniesController> logger,
            IStringLocalizer<CompaniesController> localizer, IMapper mapper)
        {
            _db = db;
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
        }


        [HttpGet("client")]
        public async Task<ActionResult<IEnumerable<TenantForClient>>> GetForClient()
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

        private async Task<IEnumerable<TenantForClient>> GetForClientImpl()
        {
            var externalId = User.ExternalUserId();
            var dbUser = await _db.GlobalUsers
                .Where(e => e.ExternalId == externalId)
                .Include(user => user.Memberships)
                .ThenInclude(membership => membership.Tenant)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if(dbUser != null)
            {
                var dbCompanies = dbUser.Memberships.Select(e => e.Tenant);
                var companies = _mapper.Map<IEnumerable<TenantForClient>>(dbCompanies);

                // Prepare the settings for client
                return companies;
            }
            else
            {
                return new List<TenantForClient>();
            }
        }
    }
}