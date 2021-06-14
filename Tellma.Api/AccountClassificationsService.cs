using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Api;
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
        private readonly IStringLocalizer _localizer;

        public AccountClassificationsService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
            _localizer = deps.Localizer;
        }

        protected override string View => "account-classifications";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override EntityQuery<AccountClassification> Search(EntityQuery<AccountClassification> query, GetArguments args)
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

            return query;
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<AccountClassificationForSave> entities, bool returnIds)
        {
            SaveResult result = await _behavior.Repository.AccountClassifications__Save(entities, returnIds, UserId);
            AddLocalizedErrors(result.Errors);

            // Return
            return result.Ids;
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                var result = await _behavior.Repository.AccountClassifications__Delete(ids, UserId);
                AddLocalizedErrors(result.Errors);
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
                DeleteResult result = await _behavior.Repository.AccountClassifications__DeleteWithDescendants(ids, UserId);
                AddLocalizedErrors(result.Errors);
            }
            catch (ForeignKeyViolationException)
            {
                var meta = await GetMetadata(cancellation: default);
                throw new ServiceException(_localizer["Error_CannotDelete0AlreadyInUse", meta.SingularDisplay()]);
            }
        }

        public Task<(List<AccountClassification>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<AccountClassification>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<AccountClassification>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            await Initialize();

            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            OperationResult result = await _behavior.Repository.AccountClassifications__Activate(ids, isActive, UserId);
            AddLocalizedErrors(result.Errors);
            ModelState.ThrowIfInvalid();

            List<AccountClassification> data = null;
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
}
