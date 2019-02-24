using AutoMapper;
using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Services.Identity;
using BSharp.Services.ImportExport;
using BSharp.Services.SqlLocalization;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Authorization;
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
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using M = BSharp.Data.Model;

namespace BSharp.Controllers
{
    [Route("api/translations")]
    public class TranslationsController : CrudControllerBase<M.Translation, Translation, TranslationForSave, string>
    {
        private readonly AdminContext _db;
        private readonly ISqlStringLocalizerFactory _localizerFactory;
        private readonly ILogger _logger;

        public TranslationsController(AdminContext db, ISqlStringLocalizerFactory localizerFactory, ILogger<TranslationsController> logger, 
            IStringLocalizer<TranslationsController> localizer, IMapper mapper) : base(logger, localizer, mapper)
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

        protected override Task DeleteAsync(List<string> ids)
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
            // TODO overrite the translations version and bust the localizer factory cache
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

        protected override Task<IEnumerable<M.AbstractPermission>> UserPermissions(PermissionLevel level)
        {
            throw new NotImplementedException();
        }

        protected override Task CheckPermissionsForNew(IEnumerable<TranslationForSave> newItems, Expression<Func<M.Translation, bool>> lambda)
        {
            throw new NotImplementedException();
        }

        protected override Task CheckPermissionsForOld(IEnumerable<string> entityIds, Expression<Func<M.Translation, bool>> lambda)
        {
            throw new NotImplementedException();
        }
    }
}