using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
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
        private readonly IStringLocalizer _localizer;

        public AccountsService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
            _localizer = deps.Localizer;
        }

        protected override string View => "accounts";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override EntityQuery<Account> Search(EntityQuery<Account> query, GetArguments args)
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

            return query;
        }

        protected override async Task<List<AccountForSave>> SavePreprocessAsync(List<AccountForSave> entities)
        {
            // Service Preprocess
            entities.ForEach(entity =>
            {
                // Can't have a relation without the relation definition
                if (entity.RelationDefinitionId == null)
                {
                    entity.RelationId = null;
                }

                // Can't have a resource without the resource definition
                if (entity.ResourceDefinitionId == null)
                {
                    entity.ResourceId = null;
                }

                // Can't have a noted relation without the noted relation definition
                if (entity.NotedRelationDefinitionId == null)
                {
                    entity.NotedRelationId = null;
                }
            });

            // Repo preprocess
            await _behavior.Repository.Accounts__Preprocess(entities);

            // Return
            return entities;
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<AccountForSave> entities, bool returnIds)
        {
            SaveResult result = await _behavior.Repository.Accounts__Save(entities, returnIds: returnIds, UserId);
            AddLocalizedErrors(result.Errors);

            // Return
            return result.Ids;
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                DeleteResult result = await _behavior.Repository.Accounts__Delete(ids, UserId);
                AddLocalizedErrors(result.Errors);
            }
            catch (ForeignKeyViolationException)
            {
                var meta = await GetMetadata(cancellation: default);
                throw new ServiceException(_localizer["Error_CannotDelete0AlreadyInUse", meta.SingularDisplay()]);
            }
        }

        protected override ExpressionOrderBy DefaultOrderBy()
        {
            return ExpressionOrderBy.Parse(nameof(Account.Code));
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

        public Task<(List<Account>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<Account>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<Account>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            await Initialize();

            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            OperationResult result = await _behavior.Repository.Accounts__Activate(ids, isActive, userId: UserId);
            AddLocalizedErrors(result.Errors);
            ModelState.ThrowIfInvalid();

            List<Account> data = null;
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
