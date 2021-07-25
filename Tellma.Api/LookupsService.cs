using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class LookupsService : CrudServiceBase<LookupForSave, Lookup, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer _localizer;

        public LookupsService(
            ApplicationFactServiceBehavior behavior,
            CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
        }

        protected override string View => $"lookups/{DefinitionId}";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        /// <summary>
        /// The current <see cref="DefinitionId"/>, if null throws an exception.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private new int DefinitionId => base.DefinitionId ??
            throw new InvalidOperationException($"DefinitionId was not set in {nameof(LookupsService)}.");

        /// <summary>
        /// The current TenantId.
        /// </summary>
        private new int TenantId => _behavior.TenantId;

        /// <summary>
        /// Helper method for retrieving the <see cref="LookupDefinitionForClient"/> 
        /// that corresponds to the current <see cref="DefinitionId"/>.
        /// </summary>
        /// <param name="cancellation">The cancellation instruction.</param>
        private async Task<LookupDefinitionForClient> Definition(CancellationToken cancellation = default)
        {
            var defs = await _behavior.Definitions(cancellation);
            var docDef = defs.Lookups.GetValueOrDefault(DefinitionId) ??
                throw new InvalidOperationException($"Lookup definition with Id = {DefinitionId} could not be found.");

            return docDef;
        }

        protected override Task<EntityQuery<Lookup>> Search(EntityQuery<Lookup> query, GetArguments args, CancellationToken cancellation)
        {
            return LookupServiceUtil.SearchImpl(query, args, cancellation);
        }

        protected override async Task<List<LookupForSave>> SavePreprocessAsync(List<LookupForSave> entities)
        {
            var def = await Definition();

            // Creating new entities forbidden if the definition is archived
            if (entities.Any(e => e?.Id == 0) && def.State == DefStates.Archived) // Insert
            {
                var msg = _localizer["Error_DefinitionIsArchived"];
                throw new ServiceException(msg);
            }

            return entities;
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<LookupForSave> entities, bool returnIds)
        {
            SaveResult result = await _behavior.Repository.Lookups__Save(DefinitionId, entities, returnIds: returnIds, UserId);
            AddLocalizedErrors(result.Errors);

            return result.Ids;
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                DeleteResult result = await _behavior.Repository.Lookups__Delete(DefinitionId, ids, userId: UserId);
                AddLocalizedErrors(result.Errors);
            }
            catch (ForeignKeyViolationException)
            {
                var meta = await GetMetadata(cancellation: default);
                throw new ServiceException(_localizer["Error_CannotDelete0AlreadyInUse", meta.SingularDisplay()]);
            }
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken _)
        {
            var result = ExpressionOrderBy.Parse("SortKey,Id desc");
            return Task.FromResult(result);
        }

        public Task<(List<Lookup>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<Lookup>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<Lookup>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            await Initialize();

            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            OperationResult result = await _behavior.Repository.Lookups__Activate(DefinitionId, ids, isActive, userId: UserId);
            AddLocalizedErrors(result.Errors);
            ModelState.ThrowIfInvalid();

            List<Lookup> data = null;
            Extras extras = null;

            if (args.ReturnEntities ?? false)
            {
                (data, extras) = await GetByIds(ids, args, action, cancellation: default);
            }

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, data);

            trx.Complete();
            return (data, extras);
        }
    }

    public class LookupsGenericService : FactWithIdServiceBase<Lookup, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IPermissionsCache _permissionsCache;

        public LookupsGenericService(ApplicationFactServiceBehavior behavior,
            FactServiceDependencies deps,
            IPermissionsCache permissionsCache) : base(deps)
        {
            _behavior = behavior;
            _permissionsCache = permissionsCache;
        }

        protected override string View => throw new NotImplementedException(); // We override user permissions

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            // Get all permissions pertaining to Lookups
            string prefix = "lookups/";
            var permissions = (await _permissionsCache
                .GenericPermissionsFromCache(
                    tenantId: _behavior.TenantId,
                    userId: UserId,
                    version: _behavior.PermissionsVersion,
                    viewPrefix: prefix,
                    action: action,
                    cancellation: cancellation)).ToList();

            // Massage the permissions by adding definitionId = definitionId as an extra clause 
            // (since the controller will not filter the results per any specific definition Id)
            foreach (var permission in permissions.Where(e => e.View != "all"))
            {
                string definitionIdString = permission.View.Remove(0, prefix.Length);
                if (!int.TryParse(definitionIdString, out int definitionId))
                {
                    throw new ServiceException($"Could not parse definition Id '{definitionIdString}' to a valid integer.");
                }

                string definitionPredicate = $"{nameof(Lookup.DefinitionId)} eq {definitionId}";
                if (!string.IsNullOrWhiteSpace(permission.Criteria))
                {
                    permission.Criteria = $"{definitionPredicate} and ({permission.Criteria})";
                }
                else
                {
                    permission.Criteria = definitionPredicate;
                }
            }

            // Return the massaged permissions
            return permissions;
        }

        protected override Task<EntityQuery<Lookup>> Search(EntityQuery<Lookup> query, GetArguments args, CancellationToken cancellation)
        {
            return LookupServiceUtil.SearchImpl(query, args, cancellation);
        }
    }

    internal class LookupServiceUtil
    {
        /// <summary>
        /// This is needed in both the generic and specific controllers, so we move it out here
        /// </summary>
        public static Task<EntityQuery<Lookup>> SearchImpl(EntityQuery<Lookup> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Lookup.Name);
                var name2 = nameof(Lookup.Name2);
                var name3 = nameof(Lookup.Name3);
                var code = nameof(Lookup.Code);

                var filterString = $"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}'";
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return Task.FromResult(query);
        }
    }
}
