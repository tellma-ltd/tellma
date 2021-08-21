using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class EntryTypesService : CrudTreeServiceBase<EntryTypeForSave, EntryType, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;

        public EntryTypesService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
        }

        protected override string View => "entry-types";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<EntryType>> Search(EntityQuery<EntryType> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(EntryType.Name);
                var name2 = nameof(EntryType.Name2);
                var name3 = nameof(EntryType.Name3);
                var code = nameof(EntryType.Code);

                query = query.Filter($"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}'");
            }

            return Task.FromResult(query);
        }

        protected override Task<List<EntryTypeForSave>> SavePreprocessAsync(List<EntryTypeForSave> entities)
        {
            entities.ForEach(entity =>
            {
                entity.IsAssignable ??= true;
            });

            return Task.FromResult(entities);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<EntryTypeForSave> entities, bool returnIds)
        {
            SaveOutput result = await _behavior.Repository.EntryTypes__Save(
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
            DeleteOutput result = await _behavior.Repository.EntryTypes__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        protected override async Task DeleteWithDescendantsAsync(List<int> ids)
        {
            DeleteOutput result = await _behavior.Repository.EntryTypes__DeleteWithDescendants(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        public Task<EntitiesResult<EntryType>> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<EntitiesResult<EntryType>> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<EntitiesResult<EntryType>> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            await Initialize();

            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = TransactionFactory.ReadCommitted();
            OperationOutput output = await _behavior.Repository.EntryTypes__Activate(
                    ids: ids,
                    isActive: isActive,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(output.Errors);

            var result = (args.ReturnEntities ?? false) ?
                await GetByIds(ids, args, action, cancellation: default) :
                EntitiesResult<EntryType>.Empty();

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, result.Data);

            trx.Complete();
            return result;
        }
    }
}
