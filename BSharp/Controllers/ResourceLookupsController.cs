using BSharp.Controllers.Dto;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.Entities;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    [Route("api/resource-lookups/{definitionId}")]
    [ApplicationApi]
    public class ResourceLookupsController : CrudControllerBase<ResourceLookupForSave, ResourceLookup, int>
    {
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;

        private string VIEW => RouteData.Values["definitionId"]?.ToString() ?? 
            throw new BadRequestException("URI must be of the form 'api/resource-lookups/{definitionId}'");

        public ResourceLookupsController(
            ILogger<ResourceLookupsController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<ResourceLookup>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
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
        public async Task<ActionResult<EntitiesResponse<ResourceLookup>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    isActive: false)
            , _logger);
        }

        private async Task<ActionResult<EntitiesResponse<ResourceLookup>>> Activate([FromBody] List<int> ids, bool returnEntities, string expand, bool isActive)
        {
            // Parse parameters
            var expandExp = ExpandExpression.Parse(expand);
            var idsArray = ids.ToArray();

            // Check user permissions
            await CheckActionPermissions("IsActive", idsArray);

            // Execute and return
            using (var trx = ControllerUtilities.CreateTransaction())
            {
                await _repo.ResourceLookups__Activate(ids, isActive);

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
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return await _repo.UserPermissions(action, VIEW);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<ResourceLookup> Search(Query<ResourceLookup> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(ResourceLookup.Name);
                var name2 = nameof(ResourceLookup.Name2);
                var name3 = nameof(ResourceLookup.Name3);
                var code = nameof(ResourceLookup.Code);

                var filterString = $"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'";
                query = query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }

        protected override async Task SaveValidateAsync(List<ResourceLookupForSave> entities)
        {
            // Check that codes are not duplicated within the arriving collection
            var duplicateCodes = entities.Where(e => e.Code != null).GroupBy(e => e.Code).Where(g => g.Count() > 1);
            if (duplicateCodes.Any())
            {
                // Hash the entities' indices for performance
                Dictionary<ResourceLookupForSave, int> indices = entities.ToIndexDictionary();

                foreach (var groupWithDuplicateCodes in duplicateCodes)
                {
                    foreach (var entity in groupWithDuplicateCodes)
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        ModelState.AddModelError($"[{index}].{nameof(entity.Code)}", _localizer["Error_TheCode0IsDuplicated", entity.Code]);
                    }
                }
            }

            // Check that Names are not duplicated within the arriving collection
            var duplicateNames = entities.Where(e => e.Name != null).GroupBy(e => e.Name).Where(g => g.Count() > 1);
            if (duplicateNames.Any())
            {
                // Hash the entities' indices for performance
                Dictionary<ResourceLookupForSave, int> indices = entities.ToIndexDictionary();

                foreach (var groupWithDuplicateNames in duplicateNames)
                {
                    foreach (var entity in groupWithDuplicateNames)
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        ModelState.AddModelError($"[{index}].{nameof(entity.Name)}", _localizer["Error_TheName0IsDuplicated", entity.Name]);
                    }
                }
            }

            // Check that Name2s are not duplicated within the arriving collection
            var duplicateName2s = entities.Where(e => e.Name2 != null).GroupBy(e => e.Name2).Where(g => g.Count() > 1);
            if (duplicateName2s.Any())
            {
                // Hash the entities' indices for performance
                Dictionary<ResourceLookupForSave, int> indices = entities.ToIndexDictionary();

                foreach (var groupWithDuplicateName2s in duplicateName2s)
                {
                    foreach (var entity in groupWithDuplicateName2s)
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        ModelState.AddModelError($"[{index}].{nameof(entity.Name2)}", _localizer["Error_TheName0IsDuplicated", entity.Name2]);
                    }
                }
            }

            // Check that Name3s are not duplicated within the arriving collection
            var duplicateName3s = entities.Where(e => e.Name3 != null).GroupBy(e => e.Name3).Where(g => g.Count() > 1);
            if (duplicateName3s.Any())
            {
                // Hash the entities' indices for performance
                Dictionary<ResourceLookupForSave, int> indices = entities.ToIndexDictionary();

                foreach (var groupWithDuplicateName3s in duplicateName3s)
                {
                    foreach (var entity in groupWithDuplicateName3s)
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        ModelState.AddModelError($"[{index}].{nameof(entity.Name3)}", _localizer["Error_TheName0IsDuplicated", entity.Name3]);
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
            var sqlErrors = await _repo.ResourceLookups_Validate__Save(VIEW, entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<ResourceLookupForSave> entities, ExpandExpression expand, bool returnIds)
        {
            return await _repo.ResourceLookups__Save(VIEW, entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.ResourceLookups_Validate__Delete(VIEW, ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.ResourceLookups__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["ResourceLookup"]]);
            }
        }

        protected override Query<ResourceLookup> GetAsQuery(List<ResourceLookupForSave> entities)
        {
            return _repo.ResourceLookups__AsQuery(VIEW, entities);
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse("SortKey,Id desc");
        }
    }
}
