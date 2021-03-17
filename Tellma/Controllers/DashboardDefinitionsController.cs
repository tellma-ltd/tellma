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
using Tellma.Entities;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class DashboardDefinitionsController : CrudControllerBase<DashboardDefinitionForSave, DashboardDefinition, int>
    {
        public const string BASE_ADDRESS = "dashboard-definitions";

        private readonly DashboardDefinitionsService _service;

        public DashboardDefinitionsController(DashboardDefinitionsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override CrudServiceBase<DashboardDefinitionForSave, DashboardDefinition, int> GetCrudService()
        {
            return _service;
        }

        protected override Task OnSuccessfulSave(List<DashboardDefinition> data, Extras extras)
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

    public class DashboardDefinitionsService : CrudServiceBase<DashboardDefinitionForSave, DashboardDefinition, int>
    {
        private readonly ApplicationRepository _repo;
        private readonly ISettingsCache _settingsCache;
        private readonly IDefinitionsCache _defCache;

        private string View => DashboardDefinitionsController.BASE_ADDRESS;

        public DashboardDefinitionsService(ApplicationRepository repo, ISettingsCache settingsCache, IDefinitionsCache defCache, IServiceProvider sp) : base(sp)
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

        protected override Query<DashboardDefinition> Search(Query<DashboardDefinition> query, GetArguments args)
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

            return query;
        }

        protected override Task<List<DashboardDefinitionForSave>> SavePreprocessAsync(List<DashboardDefinitionForSave> entities)
        {
            var settings = _settingsCache.GetCurrentSettingsIfCached().Data;

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

            return Task.FromResult(entities);
        }

        protected override async Task SaveValidateAsync(List<DashboardDefinitionForSave> entities)
        {
            const int maxOffset = 1000;
            const int maxSize = 16;

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

                var duplicateReportIds = entity.Widgets
                    .GroupBy(e => e.ReportDefinitionId)
                    .Where(g => g.Count() > 1 )
                    .Select(g => g.Key)
                    .ToHashSet();

                foreach (var (widget, widgetIndex) in entity.Widgets.Select((e, i) => (e, i)))
                {
                    if (widget.OffsetX < 0)
                    {
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.OffsetX)}";
                        string msg = _localizer["Error_TheField0CannotBeNegative", _localizer["DashboardDefinition_OffsetX"]];

                        ModelState.AddModelError(path, msg);
                    }

                    if (widget.OffsetX >= maxOffset)
                    {
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.OffsetX)}";
                        string msg = _localizer["Error_Field0MaximumIs1", _localizer["DashboardDefinition_OffsetX"], maxOffset];

                        ModelState.AddModelError(path, msg);
                    }

                    if (widget.OffsetY < 0)
                    {
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.OffsetY)}";
                        string msg = _localizer["Error_TheField0CannotBeNegative", _localizer["DashboardDefinition_OffsetY"]];

                        ModelState.AddModelError(path, msg);
                    }

                    if (widget.OffsetY >= maxOffset)
                    {
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.OffsetY)}";
                        string msg = _localizer["Error_Field0MaximumIs1", _localizer["DashboardDefinition_OffsetY"], maxOffset];

                        ModelState.AddModelError(path, msg);
                    }

                    if (widget.Width < 0)
                    {
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.Width)}";
                        string msg = _localizer["Error_TheField0CannotBeNegative", _localizer["DashboardDefinition_Width"]];

                        ModelState.AddModelError(path, msg);
                    }

                    if (widget.Width >= maxSize)
                    {
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.Width)}";
                        string msg = _localizer["Error_Field0MaximumIs1", _localizer["DashboardDefinition_Width"], maxSize];

                        ModelState.AddModelError(path, msg);
                    }

                    if (widget.Height < 0)
                    {
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.Height)}";
                        string msg = _localizer["Error_TheField0CannotBeNegative", _localizer["DashboardDefinition_Height"]];

                        ModelState.AddModelError(path, msg);
                    }

                    if (widget.Height >= maxSize)
                    {
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.Height)}";
                        string msg = _localizer["Error_Field0MaximumIs1", _localizer["DashboardDefinition_Height"], maxSize];

                        ModelState.AddModelError(path, msg);
                    }

                    if (duplicateReportIds.Contains(widget.ReportDefinitionId))
                    {
                        defs.Reports.TryGetValue(widget.ReportDefinitionId.Value, out ReportDefinitionForClient reportDef);
                        string reportName = reportDef == null ? null : settings.Localize(reportDef.Title, reportDef.Title2, reportDef.Title3);
                        string path = $"[{index}].{nameof(entity.Widgets)}[{widgetIndex}].{nameof(widget.ReportDefinitionId)}";
                        string msg = _localizer["Error_The01IsDuplicated", _localizer["DashboardDefinition_ReportDefinition"], reportName ?? widget.ReportDefinitionId.ToString()];

                        ModelState.AddModelError(path, msg);
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
            var sqlErrors = await _repo.DashboardDefinitions_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<DashboardDefinitionForSave> entities, bool returnIds)
        {
            return await _repo.DashboardDefinitions__Save(entities, returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.DashboardDefinitions_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.DashboardDefinitions__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["DashboardDefinition"]]);
            }
        }

        protected override ExpressionOrderBy DefaultOrderBy()
        {
            // By default: Order dashboard definitions by name
            var tenantInfo = _repo.GetTenantInfo();
            string orderby = $"{nameof(DashboardDefinition.Title)},{nameof(DashboardDefinition.Id)}";
            if (tenantInfo.SecondaryLanguageId == CultureInfo.CurrentUICulture.Name)
            {
                orderby = $"{nameof(DashboardDefinition.Title2)},{nameof(DashboardDefinition.Title)},{nameof(DashboardDefinition.Id)}";
            }
            else if (tenantInfo.TernaryLanguageId == CultureInfo.CurrentUICulture.Name)
            {
                orderby = $"{nameof(DashboardDefinition.Title3)},{nameof(DashboardDefinition.Title)},{nameof(DashboardDefinition.Id)}";
            }

            return ExpressionOrderBy.Parse(orderby);

        }
    }
}
