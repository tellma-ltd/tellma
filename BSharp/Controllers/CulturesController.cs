using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.ApiAuthentication;
using BSharp.Services.Identity;
using BSharp.Services.ImportExport;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using M = BSharp.Data.Model;


namespace BSharp.Controllers
{
    [Route("api/[controller]")]
    [AuthorizeAccess]
    [ApiController]
    public class CulturesController : ReadControllerBaseOld<Culture, CultureForQuery, string>
    {
        private readonly AdminContext _db;
        private readonly ILogger<CulturesController> _logger;
        private readonly IStringLocalizer<CulturesController> _localizer;
        private readonly IMapper _mapper;

        public CulturesController(ILogger<CulturesController> logger, IStringLocalizer<CulturesController> localizer,
            IServiceProvider serviceProvider) : base(logger, localizer, serviceProvider)
        {
            _db = serviceProvider.GetRequiredService<AdminContext>();
            _logger = logger;
            _localizer = localizer;
        }

        protected override AbstractDataGrid DtosToAbstractGrid(GetResponse<Culture> response, ExportArguments args)
        {
            throw new NotImplementedException();
        }

        protected override IQueryable<CultureForQuery> GetBaseQuery()
        {
            return _db.VW_Cultures;
        }

        protected override IQueryable<CultureForQuery> IncludeInactive(IQueryable<CultureForQuery> query, bool inactive)
        {
            if(!inactive)
            {
                query = query.Where(e => e.IsActive == true);
            }

            return query;
        }

        protected override IQueryable<CultureForQuery> Search(IQueryable<CultureForQuery> query, string search, IEnumerable<AbstractPermission> permissions)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Id.Contains(search) || e.Name.Contains(search) || e.EnglishName.Contains(search));
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
