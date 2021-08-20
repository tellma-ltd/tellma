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
    public class AgentsService : CrudServiceBase<AgentForSave, Agent, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;

        public AgentsService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
        }

        protected override string View => "agents";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<Agent>> Search(EntityQuery<Agent> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Agent.Name);
                var name2 = nameof(Agent.Name2);
                var name3 = nameof(Agent.Name3);

                var filterString = $"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}'";
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return Task.FromResult(query);
        }

        protected override Task<List<AgentForSave>> SavePreprocessAsync(List<AgentForSave> entities)
        {
            entities.ForEach(entity =>
            {
                entity.IsRelated ??= false;
            });

            return base.SavePreprocessAsync(entities);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<AgentForSave> entities, bool returnIds)
        {
            SaveOutput result = await _behavior.Repository.Agents__Save(
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
            DeleteOutput result = await _behavior.Repository.Agents__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        public Task<EntitiesResult<Agent>> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<EntitiesResult<Agent>> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<EntitiesResult<Agent>> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            await Initialize();

            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = TransactionFactory.ReadCommitted();
            OperationOutput output = await _behavior.Repository.Agents__Activate(
                    ids: ids,
                    isActive: isActive,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(output.Errors);

            // Prepare result
            var result = (args.ReturnEntities ?? false) ?
                await GetByIds(ids, args, action, cancellation: default) :
                EntitiesResult<Agent>.Empty();

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, result.Data);

            trx.Complete();
            return result;
        }
    }
}
