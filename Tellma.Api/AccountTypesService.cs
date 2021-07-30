using Microsoft.Extensions.Localization;
using System.Collections.Generic;
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
    public class AccountTypesService : CrudTreeServiceBase<AccountTypeForSave, AccountType, int>
    {
        private static readonly string _documentDetailsSelect = string.Join(',', DocDetails.AccountTypePaths());

        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer _localizer;

        public AccountTypesService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
            _localizer = deps.Localizer;
        }

        protected override string View => "account-types";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<AccountType>> Search(EntityQuery<AccountType> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(AccountType.Name);
                var name2 = nameof(AccountType.Name2);
                var name3 = nameof(AccountType.Name3);

                var filterString = $"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}'";
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return Task.FromResult(query);
        }

        protected override Task<List<AccountTypeForSave>> SavePreprocessAsync(List<AccountTypeForSave> entities)
        {
            // Set defaults
            entities.ForEach(entity =>
            {
                entity.IsMonetary ??= true;
                entity.IsAssignable ??= true;
                entity.StandardAndPure ??= false;
            });

            return base.SavePreprocessAsync(entities);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<AccountTypeForSave> entities, bool returnIds)
        {
            SaveResult result = await _behavior.Repository.AccountTypes__Save(
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
            try
            {
                DeleteResult result = await _behavior.Repository.AccountTypes__Delete(
                    ids: ids,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

                AddErrorsAndThrowIfInvalid(result.Errors);
            }
            catch (ForeignKeyViolationException)
            {
                var meta = await GetMetadata(cancellation: default);
                throw new ServiceException(_localizer["Error_CannotDelete0AlreadyInUse", meta.SingularDisplay()]);
            }
        }

        protected override async Task DeleteWithDescendantsAsync(List<int> ids)
        {
            try
            {
                DeleteResult result = await _behavior.Repository.AccountTypes__DeleteWithDescendants(
                    ids: ids,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

                AddErrorsAndThrowIfInvalid(result.Errors);
            }
            catch (ForeignKeyViolationException)
            {
                var meta = await GetMetadata(cancellation: default);
                throw new ServiceException(_localizer["Error_CannotDelete0AlreadyInUse", meta.SingularDisplay()]);
            }
        }

        public Task<(List<AccountType>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<AccountType>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<AccountType>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            await Initialize();

            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            OperationResult result = await _behavior.Repository.AccountTypes__Activate(
                    ids: ids,
                    isActive: isActive,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            List<AccountType> data = null;
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
    }
}
