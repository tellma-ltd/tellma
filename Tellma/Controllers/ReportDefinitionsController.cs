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
    public class ReportDefinitionsController : CrudControllerBase<ReportDefinitionForSave, ReportDefinition, string>
    {
        public const string BASE_ADDRESS = "report-definitions";

        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;

        private string View => BASE_ADDRESS;

        public ReportDefinitionsController(
            ILogger<ReportDefinitionsController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
        }

        protected override Query<ReportDefinition> GetAsQuery(List<ReportDefinitionForSave> entities)
        {
            throw new NotImplementedException();
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return await _repo.UserPermissions(action, View, cancellation);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<ReportDefinition> Search(Query<ReportDefinition> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
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

                var filterString = $"{title} {Ops.contains} '{search}' or {title2} {Ops.contains} '{search}' or {title3} {Ops.contains} '{search}' or {desc} {Ops.contains} '{search}' or {desc2} {Ops.contains} '{search}' or {desc3} {Ops.contains} '{search}'";
                query = query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }

        protected override Task<List<ReportDefinitionForSave>> SavePreprocessAsync(List<ReportDefinitionForSave> entities)
        {
            entities.ForEach(entity =>
            {
                // Default Id
                if (string.IsNullOrWhiteSpace(entity.Id))
                {
                    entity.Id = Guid.NewGuid().ToString("D");
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
            });

            return Task.FromResult(entities);
        }

        protected override async Task SaveValidateAsync(List<ReportDefinitionForSave> entities)
        {
            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                // Ensure that Id adhers to maximum size
                if (entity.Id.Length > 50)
                {
                    string path = $"[{index}].{nameof(entity.Id)}";
                    string msg = _localizer[nameof(StringLengthAttribute), _localizer["Id"], 3];

                    ModelState.AddModelError(path, msg);
                }

                if (entity.ShowInMainMenu ?? false)
                {
                    if (string.IsNullOrWhiteSpace(entity.Title))
                    {
                        string path = $"[{index}].{nameof(entity.Title)}";
                        string msg = _localizer["Error_TitleIsRequiredWhenShowInMainMenu"];

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
            var sqlErrors = await _repo.ReportDefinitions_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<string>> SaveExecuteAsync(List<ReportDefinitionForSave> entities, bool returnIds)
        {
            await _repo.ReportDefinitions__Save(entities);
            Response.Headers.Set("x-definitions-version", Constants.Stale);
            return entities.Select(e => e.Id).ToList();
        }

        protected override async Task DeleteValidateAsync(List<string> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.ReportDefinitions_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<string> ids)
        {
            try
            {
                await _repo.ReportDefinitions__Delete(ids);
                Response.Headers.Set("x-definitions-version", Constants.Stale);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["ReportDefinition"]]);
            }
        }

        protected override OrderByExpression DefaultOrderBy()
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

            return OrderByExpression.Parse(orderby);

        }
    }
}
