﻿using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.ImportExport;
using Tellma.Model.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class RolesService : CrudServiceBase<RoleForSave, Role, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer _localizer;

        public RolesService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
            _localizer = deps.Localizer;
        }

        protected override string View => "roles";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<Role>> Search(EntityQuery<Role> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Role.Name);
                var name2 = nameof(Role.Name2);
                var name3 = nameof(Role.Name3);
                var code = nameof(Role.Code);

                query = query.Filter($"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}'");
            }

            return Task.FromResult(query);
        }

        protected override Task<List<RoleForSave>> SavePreprocessAsync(List<RoleForSave> entities)
        {
            entities.ForEach(e =>
            {
                e.IsPublic ??= false;
            });

            return base.SavePreprocessAsync(entities);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<RoleForSave> entities, bool returnIds)
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
                        ModelState.AddError($"[{index}].{nameof(entity.Permissions)}[{lineIndex}].{nameof(entity.Id)}",
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
                        ModelState.AddError($"[{index}].{nameof(entity.Members)}[{lineIndex}].{nameof(entity.Id)}",
                            _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                    }
                }
            }

            // TODO Validate Criteria

            // Save
            SaveOutput result = await _behavior.Repository.Roles__Save(
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
            DeleteOutput result = await _behavior.Repository.Roles__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        public Task<EntitiesResult<Role>> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<EntitiesResult<Role>> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<EntitiesResult<Role>> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            await Initialize();

            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = TransactionFactory.ReadCommitted();
            OperationOutput output = await _behavior.Repository.Roles__Activate(
                    ids: ids,
                    isActive: isActive,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(output.Errors);

            // Prepare result
            var result = (args.ReturnEntities ?? false) ?
                await GetByIds(ids, args, action, cancellation: default) :
                EntitiesResult<Role>.Empty();

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, result.Data);

            trx.Complete();
            return result;
        }

        protected override MappingInfo ProcessDefaultMapping(MappingInfo mapping)
        {
            // Remove the RoleId property from the template, it's supposed to be hidden
            var roleMemberships = mapping.CollectionPropertyByName(nameof(Role.Members));
            var roleProp = roleMemberships.SimplePropertyByName(nameof(RoleMembership.RoleId));

            roleMemberships.SimpleProperties = roleMemberships.SimpleProperties.Where(p => p != roleProp);
            mapping.NormalizeIndices(); // Fix the gap we created in the previous line

            return base.ProcessDefaultMapping(mapping);
        }
    }
}
