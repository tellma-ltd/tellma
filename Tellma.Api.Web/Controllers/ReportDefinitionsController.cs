using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Model.Application;
using Tellma.Services.Utilities;

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
                // Makes subsequent code simpler
                entity.Rows ??= new List<ReportDefinitionRowForSave>();
                entity.Rows.ForEach(row => row.Attributes ??= new List<ReportDefinitionDimensionAttributeForSave>());
                entity.Columns ??= new List<ReportDefinitionColumnForSave>();
                entity.Columns.ForEach(col => col.Attributes ??= new List<ReportDefinitionDimensionAttributeForSave>());
                entity.Measures ??= new List<ReportDefinitionMeasureForSave>();
                entity.Select ??= new List<ReportDefinitionSelectForSave>();
                entity.Parameters ??= new List<ReportDefinitionParameterForSave>();

                // Default Values
                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    entity.Code = Guid.NewGuid().ToString("D");
                }

                // Summary reports
                if (entity.Type == "Summary")
                {
                    if (!(entity.IsCustomDrilldown ?? false))
                    {
                        // Those properties aren't needed
                        entity.Select = new List<ReportDefinitionSelectForSave>();
                        entity.Top = null;
                        entity.OrderBy = null;
                    }

                    // Defaults for Show Totals
                    entity.ShowColumnsTotal ??= false;
                    if (entity.Columns.Count == 0)
                    {
                        entity.ShowColumnsTotal = true;
                    }

                    if (!entity.ShowColumnsTotal.Value || entity.Columns.Count == 0)
                    {
                        entity.ColumnsTotalLabel = null;
                        entity.ColumnsTotalLabel2 = null;
                        entity.ColumnsTotalLabel3 = null;
                    }

                    entity.ShowRowsTotal ??= false;
                    if (entity.Rows.Count == 0)
                    {
                        entity.ShowRowsTotal = true;
                    }

                    if (!entity.ShowRowsTotal.Value || entity.Rows.Count == 0)
                    {
                        entity.RowsTotalLabel = null;
                        entity.RowsTotalLabel2 = null;
                        entity.RowsTotalLabel3 = null;
                    }
                }

                // Details Report
                if (entity.Type == "Details")
                {
                    // Those properties aren't needed
                    entity.Rows = new List<ReportDefinitionRowForSave>();
                    entity.Columns = new List<ReportDefinitionColumnForSave>();
                    entity.Measures = new List<ReportDefinitionMeasureForSave>();
                    entity.ShowColumnsTotal = false;
                    entity.ShowRowsTotal = false;
                    entity.IsCustomDrilldown = false;
                    entity.Having = null;
                }

                // Defaults to Chart
                if (string.IsNullOrWhiteSpace(entity.Chart))
                {
                    entity.DefaultsToChart = false;
                    entity.ChartOptions = null;
                }
                else
                {
                    entity.DefaultsToChart ??= true;
                }

                // Main Menu
                entity.ShowInMainMenu ??= false;
                if (!entity.ShowInMainMenu.Value)
                {
                    entity.Roles = new List<ReportDefinitionRoleForSave>();
                    entity.MainMenuIcon = null;
                    entity.MainMenuSection = null;
                    entity.MainMenuSortKey = null;
                }

                // Rows
                entity.Rows.ForEach(row =>
                {
                    if (row.Control != null)
                    {
                        row.ControlOptions = ControllerUtilities.PreprocessControlOptions(row.Control, row.ControlOptions, settings);
                    }
                    else
                    {
                        row.ControlOptions = null;
                    }
                });

                // Columns
                entity.Columns.ForEach(col =>
                {
                    if (col.Control != null)
                    {
                        col.ControlOptions = ControllerUtilities.PreprocessControlOptions(col.Control, col.ControlOptions, settings);
                    }
                    else
                    {
                        col.ControlOptions = null;
                    }
                });

                // Generate Parameters
                entity.Parameters.ForEach(parameter =>
                {
                    if (parameter.Control != null)
                    {
                        parameter.ControlOptions = ControllerUtilities.PreprocessControlOptions(parameter.Control, parameter.ControlOptions, settings);
                    }
                    else
                    {
                        parameter.ControlOptions = null;
                    }
                });

                // Generate Parameters
                entity.Measures.ForEach(measure =>
                {
                    if (measure.Control != null)
                    {
                        measure.ControlOptions = ControllerUtilities.PreprocessControlOptions(measure.Control, measure.ControlOptions, settings);
                    }
                    else
                    {
                        measure.ControlOptions = null;
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
