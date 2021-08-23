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
    public class AccountClassificationsService : CrudTreeServiceBase<AccountClassificationForSave, AccountClassification, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;

        public AccountClassificationsService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
        }

        protected override string View => "account-classifications";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<AccountClassification>> Search(EntityQuery<AccountClassification> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(AccountClassification.Name);
                var name2 = nameof(AccountClassification.Name2);
                var name3 = nameof(AccountClassification.Name3);
                var code = nameof(AccountClassification.Code);

                var filterString = $"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}'";
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return Task.FromResult(query);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<AccountClassificationForSave> entities, bool returnIds)
        {
            SaveOutput result = await _behavior.Repository.AccountClassifications__Save(
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
            DeleteOutput result = await _behavior.Repository.AccountClassifications__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        protected override async Task DeleteWithDescendantsAsync(List<int> ids)
        {
            DeleteOutput result = await _behavior.Repository.AccountClassifications__DeleteWithDescendants(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        public Task<EntitiesResult<AccountClassification>> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<EntitiesResult<AccountClassification>> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<EntitiesResult<AccountClassification>> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            await Initialize();

            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = TransactionFactory.ReadCommitted();
            OperationOutput output = await _behavior.Repository.AccountClassifications__Activate(
                    ids: ids,
                    isActive: isActive,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(output.Errors);

            var result = (args.ReturnEntities ?? false) ?
                await GetByIds(ids, args, action, cancellation: default) :
                EntitiesResult<AccountClassification>.Empty();

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, result.Data);

            trx.Complete();
            return result;
        }
    }
}
