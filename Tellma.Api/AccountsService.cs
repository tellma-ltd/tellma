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
    public class AccountsService : CrudServiceBase<AccountForSave, Account, int>
    {
        private static readonly string _documentDetailsSelect = string.Join(',', DocDetails.AccountPaths());

        private readonly ApplicationFactServiceBehavior _behavior;

        public AccountsService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
        }

        protected override string View => "accounts";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<Account>> Search(EntityQuery<Account> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Account.Name);
                var name2 = nameof(Account.Name2);
                var name3 = nameof(Account.Name3);
                var code = nameof(Account.Code);

                query = query.Filter($"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}'");
            }

            return Task.FromResult(query);
        }

        protected override async Task<List<AccountForSave>> SavePreprocessAsync(List<AccountForSave> entities)
        {
            // Service Preprocess
            entities.ForEach(entity =>
            {
                entity.IsAutoSelected ??= false;

                // Can't have a agent without the agent definition
                if (entity.AgentDefinitionId == null)
                {
                    entity.AgentId = null;
                }

                // Can't have a resource without the resource definition
                if (entity.ResourceDefinitionId == null)
                {
                    entity.ResourceId = null;
                }

                // Can't have a noted agent without the noted agent definition
                if (entity.NotedAgentDefinitionId == null)
                {
                    entity.NotedAgentId = null;
                }

                // Can't have a noted resource without the noted resource definition
                if (entity.NotedResourceDefinitionId == null)
                {
                    entity.NotedResourceId = null;
                }
            });

            // Repo preprocess
            await _behavior.Repository.Accounts__Preprocess(entities);

            // Return
            return entities;
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<AccountForSave> entities, bool returnIds)
        {
            SaveOutput result = await _behavior.Repository.Accounts__Save(
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
            DeleteOutput result = await _behavior.Repository.Accounts__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken _)
        {
            var result = ExpressionOrderBy.Parse(nameof(Account.Code));
            return Task.FromResult(result);
        }

        protected override ExpressionSelect ParseSelect(string select)
        {
            string shorthand = "$DocumentDetails";
            if (select == null)
            {
                return null;
            }
            else
            {
                select = select.Replace(shorthand, _documentDetailsSelect);
                return base.ParseSelect(select);
            }
        }

        public Task<EntitiesResult<Account>> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<EntitiesResult<Account>> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<EntitiesResult<Account>> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            await Initialize();

            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = TransactionFactory.ReadCommitted();
            OperationOutput output = await _behavior.Repository.Accounts__Activate(
                    ids: ids,
                    isActive: isActive,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(output.Errors);

            var result = (args.ReturnEntities ?? false) ?
                await GetByIds(ids, args, action, cancellation: default) :
                EntitiesResult<Account>.Empty();

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, result.Data);

            trx.Complete();
            return result;
        }
    }
}
