using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.ApiAuthentication;
using BSharp.Services.ImportExport;
using BSharp.Services.MultiTenancy;
using BSharp.Services.OData;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace BSharp.Controllers
{
    [Route("api/[controller]")]
    [AuthorizeAccess]
    [ApiController]
    public class CulturesController : ReadControllerBase<Culture, CultureForQuery, string>
    {
        private readonly AdminContext _db;
        private readonly ILogger<CulturesController> _logger;
        private readonly IStringLocalizer<CulturesController> _localizer;
        private readonly ITenantUserInfoAccessor _tenantInfo;
        private readonly IMapper _mapper;

        public CulturesController(ILogger<CulturesController> logger, IStringLocalizer<CulturesController> localizer,
            IServiceProvider serviceProvider) : base(logger, localizer, serviceProvider)
        {
            _db = serviceProvider.GetRequiredService<AdminContext>();
            _logger = logger;
            _localizer = localizer;

            _tenantInfo = serviceProvider.GetRequiredService<ITenantUserInfoAccessor>();
        }

        protected override AbstractDataGrid DtosToAbstractGrid(GetResponse<Culture> response, ExportArguments args)
        {
            throw new NotImplementedException();
        }

        protected override DbContext GetDbContext()
        {
            return _db;
        }

        protected override Func<Type, string> GetSources()
        {
            var info = _tenantInfo.GetCurrentInfo();
            return ControllerUtilities.GetApplicationSources(_localizer, info.PrimaryLanguageId, info.SecondaryLanguageId, info.TernaryLanguageId);
        }

        protected override ODataQuery<CultureForQuery, string> IncludeInactive(ODataQuery<CultureForQuery, string> query, bool inactive)
        {
            if (!inactive)
            {
                query.Filter("IsActive eq true");
            }

            return query;
        }

        protected override ODataQuery<CultureForQuery, string> Search(ODataQuery<CultureForQuery, string> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(CultureForQuery.Name);
                var englishName = nameof(CultureForQuery.EnglishName);

                query.Filter($"{name} contains '{search}' or {englishName} contains '{search}'");
            }

            return query;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(PermissionLevel level)
        {
            // Cultures are always readable for all
            IEnumerable<AbstractPermission> result = new List<AbstractPermission>
            {
                new AbstractPermission { ViewId = "cultures", Level = Constants.Update }
            };

            return Task.FromResult(result);
        }
    }
}
