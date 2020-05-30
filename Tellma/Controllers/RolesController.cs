using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using Tellma.Controllers.ImportExport;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class RolesController : CrudControllerBase<RoleForSave, Role, int>
    {
        public const string BASE_ADDRESS = "roles";

        private readonly RolesService _service;
        private readonly ILogger _logger;

        public RolesController(RolesService service, ILogger<RolesController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Role>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Activate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            }, 
            _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Role>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Deactivate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            }, 
            _logger);
        }

        protected override CrudServiceBase<RoleForSave, Role, int> GetCrudService()
        {
            return _service;
        }
    }

    public class RolesService : CrudServiceBase<RoleForSave, Role, int>
    {
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;

        private string View => RolesController.BASE_ADDRESS;

        public RolesService(IStringLocalizer<Strings> localizer, ApplicationRepository repo, IServiceProvider sp) : base(sp)
        {
            _localizer = localizer;
            _repo = repo;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return await _repo.UserPermissions(action, View, cancellation);
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

        protected override async Task<List<int>> SaveExecuteAsync(List<RoleForSave> entities, bool returnIds)
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

        public Task<(List<Role>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<Role>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<Role>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            // Check user permissions
            await CheckActionPermissions("IsActive", ids);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.Roles__Activate(ids, isActive);

            if (args.ReturnEntities ?? false)
            {
                var (data, extras) = await GetByIds(ids, args, cancellation: default);

                trx.Complete();
                return (data, extras);
            }
            else
            {
                trx.Complete();
                return (null, null);
            }
        }

        protected override MappingInfo ProcessDefaultMapping(MappingInfo mapping)
        {
            // Remove the RoleId property from the template, it's supposed to be hidden
            var roleMemberships = mapping.CollectionProperty(nameof(RoleMembership));
            var roleProp = roleMemberships.SimpleProperty(nameof(RoleMembership.RoleId));

            roleMemberships.SimpleProperties = roleMemberships.SimpleProperties.Where(p => p != roleProp);

            // Shift the index of all columns after the role property to prevent a gap
            foreach(var propMapping in mapping.AllPropertyMappings().Where(p => p.Index > roleProp.Index))
            {
                propMapping.Index--;
            }

            return base.ProcessDefaultMapping(mapping);
        }
    }
}
