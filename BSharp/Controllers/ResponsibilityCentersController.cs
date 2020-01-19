using BSharp.Controllers.Dto;
using BSharp.Controllers.Utilities;
using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.Entities;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationApi]
    public class ResponsibilityCentersController : CrudTreeControllerBase<ResponsibilityCenterForSave, ResponsibilityCenter, int>
    {
        public const string BASE_ADDRESS = "responsibility-centers";

        private readonly ApplicationRepository _repo;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;

        private string View => BASE_ADDRESS;

        public ResponsibilityCentersController(
            ILogger<ResponsibilityCentersController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<ResponsibilityCenter>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    isActive: true)
            , _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<ResponsibilityCenter>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    isActive: false)
            , _logger);
        }

        private async Task<ActionResult<EntitiesResponse<ResponsibilityCenter>>> Activate([FromBody] List<int> ids, bool returnEntities, string expand, bool isActive)
        {
            // Parse parameters
            var expandExp = ExpandExpression.Parse(expand);
            var idsArray = ids.ToArray();

            // Check user permissions
            await CheckActionPermissions("IsActive", idsArray);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.ResponsibilityCenters__Activate(ids, isActive);

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

        protected override Query<ResponsibilityCenter> Search(Query<ResponsibilityCenter> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(ResponsibilityCenter.Name);
                var name2 = nameof(ResponsibilityCenter.Name2);
                var name3 = nameof(ResponsibilityCenter.Name3);
                var code = nameof(ResponsibilityCenter.Code);

                query = query.Filter($"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'");
            }

            return query;
        }

        protected override async Task SaveValidateAsync(List<ResponsibilityCenterForSave> entities)
        {
            // Check that codes are not duplicated within the arriving collection
            var duplicateCodes = entities.Where(e => e.Code != null).GroupBy(e => e.Code).Where(g => g.Count() > 1);
            if (duplicateCodes.Any())
            {
                // Hash the entities' indices for performance
                Dictionary<ResponsibilityCenterForSave, int> indices = entities.ToIndexDictionary();

                foreach (var groupWithDuplicateCodes in duplicateCodes)
                {
                    foreach (var entity in groupWithDuplicateCodes)
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        ModelState.AddModelError($"[{index}].Code", _localizer["Error_TheCode0IsDuplicated", entity.Code]);
                    }
                }
            }

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.ResponsibilityCenters_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<ResponsibilityCenterForSave> entities, ExpandExpression expand, bool returnIds)
        {
            return await _repo.ResponsibilityCenters__Save(entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.ResponsibilityCenters_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.ResponsibilityCenters__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {               
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["ResponsibilityCenter"]]);
            }
        }

        protected override async Task ValidateDeleteWithDescendantsAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.ResponsibilityCenters_Validate__DeleteWithDescendants(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteWithDescendantsAsync(List<int> ids)
        {
            try
            {
                await _repo.ResponsibilityCenters__DeleteWithDescendants(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["ResponsibilityCenter"]]);
            }
        }

        protected override Query<ResponsibilityCenter> GetAsQuery(List<ResponsibilityCenterForSave> entities)
        {
            return _repo.ResponsibilityCenters__AsQuery(entities);
        }
    }
}