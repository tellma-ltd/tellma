using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationApi]
    public class LegacyClassificationsController : CrudTreeControllerBase<LegacyClassificationForSave, LegacyClassification, int>
    {
        public const string BASE_ADDRESS = "legacy-classifications";

        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;

        private string View => BASE_ADDRESS;

        public LegacyClassificationsController(
            ILogger<LegacyClassificationsController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<LegacyClassification>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    isDeprecated: false)
            , _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<LegacyClassification>>> Deprecate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    isDeprecated: true)
            , _logger);
        }

        private async Task<ActionResult<EntitiesResponse<LegacyClassification>>> Activate([FromBody] List<int> ids, bool returnEntities, string expand, bool isDeprecated)
        {
            // Parse parameters
            var expandExp = ExpandExpression.Parse(expand);
            var idsArray = ids.ToArray();

            // Check user permissions
            await CheckActionPermissions("IsDeprecated", idsArray);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.LegacyClassifications__Deprecate(ids, isDeprecated);

            if (returnEntities)
            {
                var response = await GetByIdListAsync(idsArray, expandExp);

                trx.Complete();
                return Ok(response);
            }
            else
            {
                trx.Complete();
                return Ok();
            }
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return await _repo.UserPermissions(action, View);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<LegacyClassification> Search(Query<LegacyClassification> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(LegacyClassification.Name);
                var name2 = nameof(LegacyClassification.Name2);
                var name3 = nameof(LegacyClassification.Name3);
                var code = nameof(LegacyClassification.Code);

                var filterString = $"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'";
                query = query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }

        protected override async Task SaveValidateAsync(List<LegacyClassificationForSave> entities)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.LegacyClassifications_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<LegacyClassificationForSave> entities, ExpandExpression expand, bool returnIds)
        {
            return await _repo.LegacyClassifications__Save(entities, returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.LegacyClassifications_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.LegacyClassifications__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["LegacyClassification"]]);
            }
        }

        protected override async Task ValidateDeleteWithDescendantsAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.LegacyClassifications_Validate__DeleteWithDescendants(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteWithDescendantsAsync(List<int> ids)
        {
            try
            {
                await _repo.LegacyClassifications__DeleteWithDescendants(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["LegacyClassification"]]);
            }
        }

        protected override Query<LegacyClassification> GetAsQuery(List<LegacyClassificationForSave> entities)
        {
            return _repo.LegacyClassifications__AsQuery(entities);
        }
    }
}
