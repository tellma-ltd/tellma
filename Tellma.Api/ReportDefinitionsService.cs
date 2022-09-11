using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Repository.Common;
using Tellma.Utilities.Common;

namespace Tellma.Api
{
    public class ReportDefinitionsService : CrudServiceBase<ReportDefinitionForSave, ReportDefinition, int>
    {
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationFactServiceBehavior _behavior;

        public ReportDefinitionsService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps) : base(deps)
        {
            _localizer = deps.Localizer;
            _behavior = behavior;
        }

        protected override string View => "report-definitions";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<ReportDefinition>> Search(EntityQuery<ReportDefinition> query, GetArguments args, CancellationToken _)
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

            return Task.FromResult(query);
        }

        protected override async Task<List<ReportDefinitionForSave>> SavePreprocessAsync(List<ReportDefinitionForSave> entities)
        {
            var settings = await _behavior.Settings();

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
                entity.Roles ??= new List<ReportDefinitionRoleForSave>();

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
                if (entity.Roles.Count == 0)
                {
                    entity.MainMenuIcon = null;
                    entity.MainMenuSection = null;
                    entity.MainMenuSortKey = null;
                }

                // Rows
                entity.Rows.ForEach(row =>
                {
                    if (row.Control != null)
                    {
                        row.ControlOptions = ControlOptionsUtil.PreprocessControlOptions(row.Control, row.ControlOptions, settings);
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
                        col.ControlOptions = ControlOptionsUtil.PreprocessControlOptions(col.Control, col.ControlOptions, settings);
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
                        parameter.ControlOptions = ControlOptionsUtil.PreprocessControlOptions(parameter.Control, parameter.ControlOptions, settings);
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
                        measure.ControlOptions = ControlOptionsUtil.PreprocessControlOptions(measure.Control, measure.ControlOptions, settings);
                    }
                    else
                    {
                        measure.ControlOptions = null;
                    }
                });
            });

            return entities;
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<ReportDefinitionForSave> entities, bool returnIds)
        {
            var defs = await _behavior.Definitions();
            var settings = await _behavior.Settings();

            foreach (var (entity, index) in entities.Indexed())
            {
                if (entity.Roles.Any())
                {
                    if (string.IsNullOrWhiteSpace(entity.Title))
                    {
                        string path = $"[{index}].{nameof(entity.Title)}";
                        string msg = _localizer["Error_TitleIsRequiredWhenShowInMainMenu"];

                        ModelState.AddError(path, msg);
                    }
                }

                foreach (var (parameter, paramIndex) in entity.Parameters.Indexed())
                {
                    // TODO: Need to figure out how to retrieve the default control
                    var errors = ControlOptionsUtil.ValidateControlOptions(parameter.Control, parameter.ControlOptions, _localizer, settings, defs);
                    foreach (var msg in errors)
                    {
                        ModelState.AddError($"[{index}].{nameof(entity.Parameters)}[{paramIndex}].{nameof(parameter.ControlOptions)}", msg);
                    }
                }
            }

            SaveOutput result = await _behavior.Repository.ReportDefinitions__Save(
                entities: entities,
                returnIds: returnIds,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            return result.Ids;
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            DeleteOutput result = await _behavior.Repository.ReportDefinitions__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        protected override async Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken cancellation)
        {
            // By default: Order report definitions by name
            var settings = await _behavior.Settings(cancellation);
            string orderby = $"{nameof(ReportDefinition.Title)},{nameof(ReportDefinition.Id)}";
            if (settings.SecondaryLanguageId == CultureInfo.CurrentUICulture.Name)
            {
                orderby = $"{nameof(ReportDefinition.Title2)},{nameof(ReportDefinition.Title)},{nameof(ReportDefinition.Id)}";
            }
            else if (settings.TernaryLanguageId == CultureInfo.CurrentUICulture.Name)
            {
                orderby = $"{nameof(ReportDefinition.Title3)},{nameof(ReportDefinition.Title)},{nameof(ReportDefinition.Id)}";
            }

            return ExpressionOrderBy.Parse(orderby);
        }
    }
}
