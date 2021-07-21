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
    public class CentersService : CrudTreeServiceBase<CenterForSave, Center, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer _localizer;

        public CentersService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
            _localizer = deps.Localizer;
        }

        protected override string View => "centers";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<Center>> Search(EntityQuery<Center> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Center.Name);
                var name2 = nameof(Center.Name2);
                var name3 = nameof(Center.Name3);
                var code = nameof(Center.Code);

                query = query.Filter($"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}'");
            }

            return Task.FromResult(query);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<CenterForSave> entities, bool returnIds)
        {
            // Save
            SaveResult result = await _behavior.Repository.Centers__Save(entities, returnIds: returnIds, userId: UserId);
            AddLocalizedErrors(result.Errors);

            // Return
            return result.Ids;
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                DeleteResult result = await _behavior.Repository.Centers__Delete(ids, userId: UserId);
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
                DeleteResult result = await _behavior.Repository.Centers__DeleteWithDescendants(ids, UserId);
                AddLocalizedErrors(result.Errors);
            }
            catch (ForeignKeyViolationException)
            {
                var meta = await GetMetadata(cancellation: default);
                throw new ServiceException(_localizer["Error_CannotDelete0AlreadyInUse", meta.SingularDisplay()]);
            }
        }

        public Task<(List<Center>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<Center>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<Center>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            await Initialize();

            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            OperationResult result = await _behavior.Repository.Centers__Activate(ids, isActive, UserId);
            AddLocalizedErrors(result.Errors);
            ModelState.ThrowIfInvalid();

            List<Center> data = null;
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
