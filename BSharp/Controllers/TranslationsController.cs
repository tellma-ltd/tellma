using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.ImportExport;
using BSharp.Services.SqlLocalization;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    [Route("api/translations")]
    public class TranslationsController : CrudControllerBase<TranslationForSave, Translation, TranslationForQuery, string>
    {
        private readonly AdminContext _db;
        private readonly ISqlStringLocalizerFactory _localizerFactory;
        private readonly ILogger _logger;

        public TranslationsController(AdminContext db, ISqlStringLocalizerFactory localizerFactory, ILogger<TranslationsController> logger,
            IStringLocalizer<TranslationsController> localizer, IServiceProvider serviceProvider) : base(logger, localizer, serviceProvider)
        {
            _db = db;
            _localizerFactory = localizerFactory;
            _logger = logger;
        }

        [HttpGet("client/{cultureName}")]
        [AllowAnonymous]
        public ActionResult<Dictionary<string, string>> GetClientTranslations(string cultureName)
        {
            try
            {
                var result = _localizerFactory.GetTranslations(cultureName, Constants.Shared, Constants.Client);
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

        protected override Task CheckPermissionsForNew(IEnumerable<TranslationForSave> newItems, Expression<Func<TranslationForQuery, bool>> lambda)
        {
            throw new NotImplementedException();
        }

        protected override Task CheckPermissionsForOld(IEnumerable<string> entityIds, Expression<Func<TranslationForQuery, bool>> lambda)
        {
            throw new NotImplementedException();
        }

        protected override Task DeleteAsync(List<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override AbstractDataGrid DtosToAbstractGrid(GetResponse<Translation> response, ExportArguments args)
        {
            throw new NotImplementedException();
        }

        protected override IQueryable<TranslationForQuery> GetBaseQuery()
        {
            throw new NotImplementedException();
        }

        protected override AbstractDataGrid GetImportTemplate()
        {
            throw new NotImplementedException();
        }

        protected override IQueryable<TranslationForQuery> IncludeInactive(IQueryable<TranslationForQuery> query, bool inactive)
        {
            throw new NotImplementedException();
        }

        protected override Task<(List<TranslationForQuery>, IQueryable<TranslationForQuery>)> PersistAsync(List<TranslationForSave> entities, SaveArguments args)
        {
            throw new NotImplementedException();
        }

        protected override IQueryable<TranslationForQuery> Search(IQueryable<TranslationForQuery> query, string search, IEnumerable<AbstractPermission> filteredPermissions)
        {
            throw new NotImplementedException();
        }

        protected override Task<(List<TranslationForSave>, Func<string, int?>)> ToDtosForSave(AbstractDataGrid grid, ParseArguments args)
        {
            throw new NotImplementedException();
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(PermissionLevel level)
        {
            throw new NotImplementedException();
        }

        protected override Task ValidateAsync(List<TranslationForSave> entities)
        {
            throw new NotImplementedException();
        }
    }
}
