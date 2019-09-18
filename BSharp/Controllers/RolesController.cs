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
    [Route("api/roles")]
    [ApplicationApi]
    public class RolesController : CrudControllerBase<RoleForSave, Role, int>
    {
        private readonly Microsoft.AspNetCore.Mvc.ModelBinding.IModelMetadataProvider _metadataProvider;
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;

        private string VIEW => "roles";

        public RolesController(
            ILogger<RolesController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo,
            Microsoft.AspNetCore.Mvc.ModelBinding.IModelMetadataProvider metadataProvider) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
            _metadataProvider = metadataProvider;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Role>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
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
        public async Task<ActionResult<EntitiesResponse<Role>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    isActive: false)
            , _logger);
        }

        private async Task<ActionResult<EntitiesResponse<Role>>> Activate([FromBody] List<int> ids, bool returnEntities, string expand, bool isActive)
        {
            // Parse parameters
            var expandExp = ExpandExpression.Parse(expand);
            var idsArray = ids.ToArray();

            // Check user permissions
            await CheckActionPermissions("IsActive", idsArray);

            // Execute and return
            using (var trx = ControllerUtilities.CreateTransaction())
            {
                await _repo.Roles__Activate(ids, isActive);

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

        protected override Query<Role> Search(Query<Role> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Role.Name);
                var name2 = nameof(Role.Name2);
                var name3 = nameof(Role.Name3); 
                var code = nameof(Role.Code);

                query = query.Filter($"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'");
            }

            return query;
        }

        protected override async Task SaveValidateAsync(List<RoleForSave> entities)
        {
            // Hash the indices for performance
            var indices = entities.ToIndexDictionary();

            // Check that line ids are unique
            entities.ForEach(e => { if (e.Permissions == null) { e.Permissions = new List<PermissionForSave>(); } });
            var duplicatePermissionId = entities.SelectMany(e => e.Permissions) // All lines
                .Where(e => e.Id != 0).GroupBy(e => e.Id).Where(g => g.Count() > 1) // Duplicate Ids
                .SelectMany(g => g).ToDictionary(e => e, e => e.Id); // to dictionary

            // Check that line ids are unique
            entities.ForEach(e => { if (e.Members == null) { e.Members = new List<RoleMembershipForSave>(); } });
            var duplicateMembershipId = entities.SelectMany(e => e.Members) // All lines
                .Where(e => e.Id != 0).GroupBy(e => e.Id).Where(g => g.Count() > 1) // Duplicate Ids
                .SelectMany(g => g).ToDictionary(e => e, e => e.Id); // to dictionary

            foreach (var entity in entities)
            {
                var permissionIndices = entity.Permissions.ToIndexDictionary();
                foreach (var line in entity.Permissions)
                {
                    if (duplicatePermissionId.ContainsKey(line))
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        var lineIndex = permissionIndices[line];
                        var id = duplicatePermissionId[line];
                        ModelState.AddModelError($"[{index}].{nameof(entity.Permissions)}[{lineIndex}].{nameof(entity.Id)}",
                            _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                    }

                }

                var membersIndices = entity.Members.ToIndexDictionary();
                foreach (var line in entity.Members)
                {
                    if (duplicateMembershipId.ContainsKey(line))
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        var lineIndex = membersIndices[line];
                        var id = duplicateMembershipId[line];
                        ModelState.AddModelError($"[{index}].{nameof(entity.Members)}[{lineIndex}].{nameof(entity.Id)}",
                            _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                    }
                }
            }

            var duplicateCodes = entities.Where(e => e.Code != null).GroupBy(e => e.Code).Where(g => g.Count() > 1);
            if (duplicateCodes.Any())
            {
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

            // TODO Validate Criteria

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Roles_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<RoleForSave> entities, ExpandExpression expand, bool returnIds)
        {
            return await _repo.Roles__Save(entities, returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Roles_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.Roles__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["Role"]]);
            }
        }

        protected override Query<Role> GetAsQuery(List<RoleForSave> entities)
        {
            return _repo.Roles__AsQuery(entities);
        }
    }
}
