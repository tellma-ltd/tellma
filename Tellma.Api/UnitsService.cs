﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class UnitsService : CrudServiceBase<UnitForSave, Unit, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        public UnitsService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
        }

        protected override string View => "units";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<Unit>> Search(EntityQuery<Unit> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Unit.Name);
                var name2 = nameof(Unit.Name2);
                var name3 = nameof(Unit.Name3);
                var code = nameof(Unit.Code);
                var desc = nameof(Unit.Description);
                var desc2 = nameof(Unit.Description2);
                var desc3 = nameof(Unit.Description3);

                var filterString = $"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}' or {desc} contains '{search}' or {desc2} contains '{search}' or {desc3} contains '{search}'";
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return Task.FromResult(query);
        }

        protected override Task<List<UnitForSave>> SavePreprocessAsync(List<UnitForSave> entities)
        {
            // Preprocess
            foreach (var entity in entities)
            {
                entity.UnitAmount ??= 1;
                entity.BaseAmount ??= 1;
            }

            return base.SavePreprocessAsync(entities);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<UnitForSave> entities, bool returnIds)
        {
            // Save
            SaveOutput result = await _behavior.Repository.Units__Save(
                entities: entities,
                returnIds: returnIds,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            // Return
            return result.Ids;
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            DeleteOutput result = await _behavior.Repository.Units__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        public Task<EntitiesResult<Unit>> Activate(List<int> ids, ActionArguments args) => SetIsActive(ids, args, isActive: true);

        public Task<EntitiesResult<Unit>> Deactivate(List<int> ids, ActionArguments args) => SetIsActive(ids, args, isActive: false);

        private async Task<EntitiesResult<Unit>> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            await Initialize();

            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = TransactionFactory.ReadCommitted();
            OperationOutput output = await _behavior.Repository.Units__Activate(
                    ids: ids,
                    isActive: isActive,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(output.Errors);

            // Prepare result
            var result = (args.ReturnEntities ?? false) ?
                await GetByIds(ids, args, action, cancellation: default) :
                EntitiesResult<Unit>.Empty();

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, result.Data);

            trx.Complete();
            return result;
        }
    }
}
