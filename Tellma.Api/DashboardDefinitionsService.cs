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

namespace Tellma.Api
{
    public class DashboardDefinitionsService : CrudServiceBase<DashboardDefinitionForSave, DashboardDefinition, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer<Strings> _localizer;

        protected override string View => "dashboard-definitions";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        public DashboardDefinitionsService(
            ApplicationFactServiceBehavior behavior,
            CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
            _localizer = deps.Localizer;
        }

        protected override Task<EntityQuery<DashboardDefinition>> Search(EntityQuery<DashboardDefinition> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var title = nameof(DashboardDefinition.Title);
                var title2 = nameof(DashboardDefinition.Title2);
                var title3 = nameof(DashboardDefinition.Title3);
                var code = nameof(DashboardDefinition.Code);

                var filterString = $"{title} contains '{search}' or {title2} contains '{search}' or {title3} contains '{search}' or {code} contains '{search}'";
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return Task.FromResult(query);
        }

        protected override async Task<List<DashboardDefinitionForSave>> SavePreprocessAsync(List<DashboardDefinitionForSave> entities)
        {
            var settings = await _behavior.Settings();

            entities.ForEach(entity =>
            {
                // Makes subsequent code simpler
                entity.Widgets ??= new List<DashboardDefinitionWidgetForSave>();
                entity.Roles ??= new List<DashboardDefinitionRoleForSave>();

                entity.AutoRefreshPeriodInMinutes ??= 1;

                entity.Widgets.ForEach(e =>
                {
                    e.OffsetX ??= 0;
                    e.OffsetY ??= 0;
                    e.Width ??= 0;
                    e.Height ??= 0;
                });


                // Default Values
                if (string.IsNullOrWhiteSpace(entity.Code))
                {
                    entity.Code = Guid.NewGuid().ToString("D");
                }

                // Main Menu
                entity.ShowInMainMenu ??= false;
                if (!entity.ShowInMainMenu.Value)
                {
                    entity.Roles = new List<DashboardDefinitionRoleForSave>();
                    entity.MainMenuIcon = null;
                    entity.MainMenuSection = null;
                    entity.MainMenuSortKey = null;
                }
            });

            return entities;
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<DashboardDefinitionForSave> entities, bool returnIds)
        {
            #region Validate

            const int maxOffset = 1000;
            const int maxSize = 16;

            var defs = await _behavior.Definitions();
            var settings = await _behavior.Settings();

            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                if (entity.ShowInMainMenu ?? false)
                {
                    if (string.IsNullOrWhiteSpace(entity.Title))
                    {
                        string path = $"[{index}].{nameof(entity.Title)}";
                        string msg = _localizer["Error_TitleIsRequiredWhenShowInMainMenu"];

                        ModelState.AddError(path, msg);
                    }
                }

                var duplicateReportIds = entity.Widgets
                    .GroupBy(e => e.ReportDefinitionId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToHashSet();

                foreach (var (widget, widgetIndex) in entity.Widgets.Select((e, i) => (e, i)))
                {
                    if (widget.OffsetX < 0)
                    {
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.OffsetX)}";
                        string msg = _localizer["Error_TheField0CannotBeNegative", _localizer["DashboardDefinition_OffsetX"]];

                        ModelState.AddError(path, msg);
                    }

                    if (widget.OffsetX >= maxOffset)
                    {
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.OffsetX)}";
                        string msg = _localizer["Error_Field0MaximumIs1", _localizer["DashboardDefinition_OffsetX"], maxOffset];

                        ModelState.AddError(path, msg);
                    }

                    if (widget.OffsetY < 0)
                    {
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.OffsetY)}";
                        string msg = _localizer["Error_TheField0CannotBeNegative", _localizer["DashboardDefinition_OffsetY"]];

                        ModelState.AddError(path, msg);
                    }

                    if (widget.OffsetY >= maxOffset)
                    {
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.OffsetY)}";
                        string msg = _localizer["Error_Field0MaximumIs1", _localizer["DashboardDefinition_OffsetY"], maxOffset];

                        ModelState.AddError(path, msg);
                    }

                    if (widget.Width < 0)
                    {
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.Width)}";
                        string msg = _localizer["Error_TheField0CannotBeNegative", _localizer["DashboardDefinition_Width"]];

                        ModelState.AddError(path, msg);
                    }

                    if (widget.Width >= maxSize)
                    {
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.Width)}";
                        string msg = _localizer["Error_Field0MaximumIs1", _localizer["DashboardDefinition_Width"], maxSize];

                        ModelState.AddError(path, msg);
                    }

                    if (widget.Height < 0)
                    {
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.Height)}";
                        string msg = _localizer["Error_TheField0CannotBeNegative", _localizer["DashboardDefinition_Height"]];

                        ModelState.AddError(path, msg);
                    }

                    if (widget.Height >= maxSize)
                    {
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.Height)}";
                        string msg = _localizer["Error_Field0MaximumIs1", _localizer["DashboardDefinition_Height"], maxSize];

                        ModelState.AddError(path, msg);
                    }

                    if (duplicateReportIds.Contains(widget.ReportDefinitionId))
                    {
                        defs.Reports.TryGetValue(widget.ReportDefinitionId.Value, out ReportDefinitionForClient reportDef);
                        string reportName = reportDef == null ? null : settings.Localize(reportDef.Title, reportDef.Title2, reportDef.Title3);
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.ReportDefinitionId)}";
                        string msg = _localizer["Error_The01IsDuplicated", _localizer["DashboardDefinition_ReportDefinition"], reportName ?? widget.ReportDefinitionId.ToString()];

                        ModelState.AddError(path, msg);
                    }
                }
            }

            #endregion

            #region Save

            // Save
            SaveResult result = await _behavior.Repository.DashboardDefinitions__Save(
                entities: entities,
                returnIds: returnIds,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            // Return
            return result.Ids;

            #endregion
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            DeleteResult result = await _behavior.Repository.DashboardDefinitions__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        protected override async Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken cancellation)
        {
            // By default: Order dashboard definitions by title
            var settings = await _behavior.Settings(cancellation);
            string orderby = $"{nameof(DashboardDefinition.Title)},{nameof(DashboardDefinition.Id)}";
            if (settings.SecondaryLanguageId == CultureInfo.CurrentUICulture.Name)
            {
                orderby = $"{nameof(DashboardDefinition.Title2)},{nameof(DashboardDefinition.Title)},{nameof(DashboardDefinition.Id)}";
            }
            else if (settings.TernaryLanguageId == CultureInfo.CurrentUICulture.Name)
            {
                orderby = $"{nameof(DashboardDefinition.Title3)},{nameof(DashboardDefinition.Title)},{nameof(DashboardDefinition.Id)}";
            }

            return ExpressionOrderBy.Parse(orderby);
        }
    }
}
