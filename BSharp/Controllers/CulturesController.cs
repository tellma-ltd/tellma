using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
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
    [ApiController]
    public class CulturesController : ReadControllerBase<CultureDefinition, Culture, string>
    {
        private readonly ILogger<CulturesController> _logger;
        private readonly IStringLocalizer<CulturesController> _localizer;
        private readonly IMapper _mapper;

        public CulturesController(ILogger<CulturesController> logger, IStringLocalizer<CulturesController> localizer,
            IMapper mapper) : base(logger, localizer, mapper)
        {
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
        }

        protected override AbstractDataGrid DtosToAbstractGrid(GetResponse<Culture> response, ExportArguments args)
        {
            throw new NotImplementedException();
        }

        protected override IQueryable<CultureDefinition> GetBaseQuery()
        {
            var repo = new CulturesRepository();
            return repo.GetAllCultures().AsQueryable();
        }

        protected override IQueryable<CultureDefinition> IncludeInactive(IQueryable<CultureDefinition> query, bool inactive)
        {
            return query;
        }

        protected override IQueryable<CultureDefinition> Search(IQueryable<CultureDefinition> query, string search)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Id.ToLower().Contains(search.ToLower()) || e.Name.ToLower().Contains(search.ToLower()) || e.EnglishName.ToLower().Contains(search.ToLower()));
            }

            return query;
        }

        protected override IQueryable<CultureDefinition> SingletonQuery(IQueryable<CultureDefinition> query, string id)
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

    public class CulturesRepository
    {
        private IEnumerable<CultureDefinition> _cultures;

        public IEnumerable<CultureDefinition> GetAllCultures()
        {
            if (_cultures == null)
            {
                _cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Select(Map).OrderBy(e => e.EnglishName);
            }

            return _cultures;
        }

        public CultureDefinition GetCulture(string cultureId)
        {
            if(cultureId == null)
            {
                return null;
            }

            try
            {
                var cultureInfo =  CultureInfo.GetCultureInfo(cultureId);
                return Map(cultureInfo);
            }
            catch (CultureNotFoundException)
            {
                return null;
            }
        }

        private CultureDefinition Map(CultureInfo cultureInfo)
        {
            return new CultureDefinition
            {
                Id = cultureInfo.Name,
                Name = cultureInfo.NativeName,
                EnglishName = cultureInfo.EnglishName,
                IsNeutralCulture = cultureInfo.IsNeutralCulture
            };
        }
    }

    public class CultureDefinition : M.ModelBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string EnglishName { get; set; }
        public bool IsNeutralCulture { get; set; }
    }
}
