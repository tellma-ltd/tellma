using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.ApiAuthentication;
using BSharp.Services.Identity;
using BSharp.Services.ImportExport;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
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
    public class CulturesController : ReadControllerBase<M.Culture, Culture, string>
    {
        private readonly AdminContext _db;
        private readonly ILogger<CulturesController> _logger;
        private readonly IStringLocalizer<CulturesController> _localizer;
        private readonly IMapper _mapper;

        public CulturesController(AdminContext db, ILogger<CulturesController> logger, IStringLocalizer<CulturesController> localizer,
            IMapper mapper) : base(logger, localizer, mapper)
        {
            _db = db;
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
        }

        protected override AbstractDataGrid DtosToAbstractGrid(GetResponse<Culture> response, ExportArguments args)
        {
            throw new NotImplementedException();
        }

        protected override IQueryable<M.Culture> GetBaseQuery()
        {
            return _db.Cultures;
        }

        protected override IQueryable<M.Culture> IncludeInactive(IQueryable<M.Culture> query, bool inactive)
        {
            if(!inactive)
            {
                query = query.Where(e => e.IsActive);
            }

            return query;
        }

        protected override IQueryable<M.Culture> Search(IQueryable<M.Culture> query, string search)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Id.Contains(search) || e.Name.Contains(search) || e.EnglishName.Contains(search));
            }

            return query;
        }

        protected override IQueryable<M.Culture> SingletonQuery(IQueryable<M.Culture> query, string id)
        {
            return query.Where(e => e.Id == id);
        }

        protected override Task<IEnumerable<M.AbstractPermission>> UserPermissions(PermissionLevel level)
        {
            // Cultures are always readable for all
            IEnumerable<M.AbstractPermission> result = new List<M.AbstractPermission>
            {
                new M.AbstractPermission { ViewId = "cultures", Level = Constants.Update }
            };

            return Task.FromResult(result);
        }
    }
}
