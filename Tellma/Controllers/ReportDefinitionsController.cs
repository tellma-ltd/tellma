using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class ReportDefinitionsController : CrudControllerBase<ReportDefinitionForSave, ReportDefinition, int>
    {
        public const string BASE_ADDRESS = "report-definitions";

        private readonly ReportDefinitionsService _service;

        public ReportDefinitionsController(ReportDefinitionsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override CrudServiceBase<ReportDefinitionForSave, ReportDefinition, int> GetCrudService()
        {
            return _service;
        }

        protected override Task OnSuccessfulSave(List<ReportDefinition> data, Extras extras)
        {
            Response.Headers.Set("x-definitions-version", Constants.Stale);
            return base.OnSuccessfulSave(data, extras);
        }

        protected override Task OnSuccessfulDelete(List<int> ids)
        {
            Response.Headers.Set("x-definitions-version", Constants.Stale);
            return base.OnSuccessfulDelete(ids);
        }
    }

    public class ReportDefinitionsService : CrudServiceBase<ReportDefinitionForSave, ReportDefinition, int>
    {

        private readonly ApplicationRepository _repo;
        private readonly ISettingsCache _settingsCache;
        private readonly IDefinitionsCache _defCache;

        private string View => ReportDefinitionsController.BASE_ADDRESS;

        public ReportDefinitionsService(ApplicationRepository repo, ISettingsCache settingsCache, IDefinitionsCache defCache, IServiceProvider sp) : base(sp)
        {
            _repo = repo;
            _settingsCache = settingsCache;
            _defCache = defCache;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.PermissionsFromCache(View, action, cancellation);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<ReportDefinition> Search(Query<ReportDefinition> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var title = nameof(ReportDefinition.Title);
                var title2 = nameof(ReportDefinition.Title2);
                var title3 = nameof(ReportDefinition.Title3);
                var desc = nameof(ReportDefinition.Description);
                var desc2 = nameof(ReportDefinition.Description2);
                var desc3 = nameof(ReportDefinition.Description3);

                var filterString = $"{title} contains '{search}' or {title2} contains '{search}' or {title3} contains '{search}' or {desc} contains '{search}' or {desc2} contains '{search}' or {desc3} contains '{search}'";
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return query;
        }

        protected override Task<List<ReportDefinitionForSave>> SavePreprocessAsync(List<ReportDefinitionForSave> entities)
        {
            var settings = _settingsCache.GetCurrentSettingsIfCached().Data;

            entities.ForEach(entity =>
            {
                // Default Id
                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    entity.Code = Guid.NewGuid().ToString("D");
                }

                // Summary reports
                if (entity.Type == "Summary")
                {
                    // Those properties aren't needed
                    entity.Select = new List<ReportSelectDefinitionForSave>();
                    entity.Top = null;
                    entity.OrderBy = null;

                    // Defaults for Show Totals
                    entity.ShowColumnsTotal ??= false;
                    if (entity.Columns == null || entity.Columns.Count == 0)
                    {
                        entity.ShowColumnsTotal = false;
                    }

                    entity.ShowRowsTotal ??= false;
                    if (entity.Rows == null || entity.Rows.Count == 0)
                    {
                        entity.ShowRowsTotal = false;
                    }
                }

                // Details Report
                if (entity.Type == "Details")
                {
                    entity.Rows = new List<ReportRowDefinitionForSave>();
                    entity.Columns = new List<ReportColumnDefinitionForSave>();
                    entity.Measures = new List<ReportMeasureDefinitionForSave>();
                    entity.ShowColumnsTotal = null;
                    entity.ShowRowsTotal = null;
                }

                // Defaults to Chart
                if (string.IsNullOrWhiteSpace(entity.Chart))
                {
                    entity.DefaultsToChart = null;
                }
                else
                {
                    entity.DefaultsToChart ??= true;
                }

                // Main Menu
                entity.ShowInMainMenu ??= false;
                if (!entity.ShowInMainMenu.Value)
                {
                    entity.MainMenuIcon = null;
                    entity.MainMenuSection = null;
                    entity.MainMenuSortKey = null;
                }

                // Generate Parameters
                entity.Parameters.ForEach(parameter =>
                {
                    if (parameter.Control != null) // TODO: Need to figure out how to retrieve the default control
                    {
                        parameter.ControlOptions = ControllerUtilities.PreprocessControlOptions(parameter.Control, parameter.ControlOptions, settings);
                    }
                });
            });

            return Task.FromResult(entities);
        }

        protected override async Task SaveValidateAsync(List<ReportDefinitionForSave> entities)
        {
            var defs = _defCache.GetCurrentDefinitionsIfCached().Data;
            var settings = _settingsCache.GetCurrentSettingsIfCached().Data;

            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                if (entity.ShowInMainMenu ?? false)
                {
                    if (string.IsNullOrWhiteSpace(entity.Title))
                    {
                        string path = $"[{index}].{nameof(entity.Title)}";
                        string msg = _localizer["Error_TitleIsRequiredWhenShowInMainMenu"];

                        ModelState.AddModelError(path, msg);
                    }
                }

                foreach (var (parameter, paramIndex) in entity.Parameters.Select((e, i) => (e, i)))
                {
                    // TODO: Need to figure out how to retrieve the default control
                    var errors = ControllerUtilities.ValidateControlOptions(parameter.Control, parameter.ControlOptions, _localizer, settings, defs);
                    foreach (var msg in errors)
                    {
                        ModelState.AddModelError($"[{index}].{nameof(entity.Parameters)}[{paramIndex}].{nameof(parameter.ControlOptions)}", msg);
                    }
                }
            }

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                // null Ids will cause an error when calling the SQL validation
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.ReportDefinitions_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<ReportDefinitionForSave> entities, bool returnIds)
        {
            return await _repo.ReportDefinitions__Save(entities, returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.ReportDefinitions_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.ReportDefinitions__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["ReportDefinition"]]);
            }
        }

        protected override ExpressionOrderBy DefaultOrderBy()
        {
            // By default: Order report definitions by name
            var tenantInfo = _repo.GetTenantInfo();
            string orderby = $"{nameof(ReportDefinition.Title)},{nameof(ReportDefinition.Id)}";
            if (tenantInfo.SecondaryLanguageId == CultureInfo.CurrentUICulture.Name)
            {
                orderby = $"{nameof(ReportDefinition.Title2)},{nameof(ReportDefinition.Title)},{nameof(ReportDefinition.Id)}";
            }
            else if (tenantInfo.TernaryLanguageId == CultureInfo.CurrentUICulture.Name)
            {
                orderby = $"{nameof(ReportDefinition.Title3)},{nameof(ReportDefinition.Title)},{nameof(ReportDefinition.Id)}";
            }

            return ExpressionOrderBy.Parse(orderby);

        }
    }
}
