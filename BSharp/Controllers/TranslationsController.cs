using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.ImportExport;
using BSharp.Services.OData;
using BSharp.Services.SqlLocalization;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        protected override Task DeleteAsync(List<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override AbstractDataGrid DtosToAbstractGrid(GetResponse<Translation> response, ExportArguments args)
        {
            throw new NotImplementedException();
        }

        protected override (string PreambleSql, string ComposableSql, List<SqlParameter> Parameters) GetAsSql(IEnumerable<TranslationForSave> entities)
        {
            throw new NotImplementedException();
        }

        protected override DbContext GetDbContext()
        {
            throw new NotImplementedException();
        }

        protected override AbstractDataGrid GetImportTemplate()
        {
            throw new NotImplementedException();
        }

        protected override Func<Type, string> GetSources()
        {
            throw new NotImplementedException();
        }

        protected override ODataQuery<TranslationForQuery, string> IncludeInactive(ODataQuery<TranslationForQuery, string> query, bool inactive)
        {
            throw new NotImplementedException();
        }

        protected override Task<List<string>> PersistAsync(List<TranslationForSave> entitiesAndMasks, SaveArguments args)
        {
            throw new NotImplementedException();
        }

        protected override ODataQuery<TranslationForQuery, string> Search(ODataQuery<TranslationForQuery, string> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
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
