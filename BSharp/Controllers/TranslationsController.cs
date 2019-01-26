using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.Identity;
using BSharp.Services.ImportExport;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using M = BSharp.Data.Model;

namespace BSharp.Controllers
{
    [Route("api/translations")]
    public class TranslationsController : CrudControllerBase<M.Translation, Translation, TranslationForSave, string>
    {
        private readonly AdminContext _db;
        private readonly ILogger _logger;

        public TranslationsController(AdminContext db, ILogger<TranslationsController> logger, 
            IStringLocalizer<TranslationsController> localizer, IMapper mapper, IUserService userService) : base(logger, localizer, mapper, userService)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet("client-translations/{culture}")]
        public async Task<ActionResult<Dictionary<string, string>>> GetClientTranslations(string culture)
        {
            try
            {
                var query = from e in _db.Translations
                            where (e.Tier == Constants.Shared || e.Tier == Constants.Client) && e.CultureId == culture
                            select e;

                var result = await query.AsNoTracking().ToDictionaryAsync(e => e.Name, e => e.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        protected override Task<IDbContextTransaction> BeginSaveTransaction()
        {
            throw new NotImplementedException();
        }

        protected override Task DeleteImplAsync(List<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override IQueryable<M.Translation> GetBaseQuery()
        {
            throw new NotImplementedException();
        }

        protected override AbstractDataGrid GetImportTemplate()
        {
            throw new NotImplementedException();
        }

        protected override IQueryable<M.Translation> IncludeInactive(IQueryable<M.Translation> query, bool inactive)
        {
            throw new NotImplementedException();
        }

        protected override Task<List<M.Translation>> PersistAsync(List<TranslationForSave> entities, SaveArguments args)
        {
            throw new NotImplementedException();
        }

        protected override IQueryable<M.Translation> Search(IQueryable<M.Translation> query, string search)
        {
            throw new NotImplementedException();
        }

        protected override IQueryable<M.Translation> SingletonQuery(IQueryable<M.Translation> query, string id)
        {
            throw new NotImplementedException();
        }

        protected override AbstractDataGrid DtosToAbstractGrid(GetResponse<Translation> response, ExportArguments args)
        {
            throw new NotImplementedException();
        }

        protected override Task<(List<TranslationForSave>, Func<string, int?>)> ToDtosForSave(AbstractDataGrid grid, ParseArguments args)
        {
            throw new NotImplementedException();
        }

        protected override Task ValidateAsync(List<TranslationForSave> entities)
        {
            throw new NotImplementedException();
        }
    }
}