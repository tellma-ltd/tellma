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
using Tellma.Api.Metadata;
using Tellma.Model.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class LookupDefinitionsService : CrudServiceBase<LookupDefinitionForSave, LookupDefinition, int>
    {
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationFactServiceBehavior _behavior;

        public LookupDefinitionsService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps) : base(deps)
        {
            _localizer = deps.Localizer;
            _behavior = behavior;
        }

        protected override string View => "lookup-definitions";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        public async Task<(List<LookupDefinition>, Extras)> UpdateState(List<int> ids, UpdateStateArguments args)
        {
            await Initialize();

            // Check user permissions
            var action = "State";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // C# Validation 
            if (string.IsNullOrWhiteSpace(args.State))
            {
                throw new ServiceException(_localizer[ErrorMessages.Error_Field0IsRequired, _localizer["State"]]);
            }

            if (!DefStates.All.Contains(args.State))
            {
                string validStates = string.Join(", ", DefStates.All);
                throw new ServiceException($"'{args.State}' is not a valid definition state, valid states are: {validStates}");
            }

            // Execute and return
            using var trx = TransactionFactory.ReadCommitted();
            OperationResult result = await _behavior.Repository.LookupDefinitions__UpdateState(
                    ids: ids,
                    state: args.State,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            // Prepare response
            List<LookupDefinition> data = null;
            Extras extras = null;

            if (args.ReturnEntities ?? false)
            {
                (data, extras) = await GetByIds(ids, args, action, cancellation: default);
            }

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, data);

            // Commit and return
            trx.Complete();
            return (data, extras);
        }

        protected override Task<EntityQuery<LookupDefinition>> Search(EntityQuery<LookupDefinition> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var titleP = nameof(LookupDefinition.TitlePlural);
                var titleP2 = nameof(LookupDefinition.TitlePlural2);
                var titleP3 = nameof(LookupDefinition.TitlePlural3);

                var titleS = nameof(LookupDefinition.TitleSingular);
                var titleS2 = nameof(LookupDefinition.TitleSingular2);
                var titleS3 = nameof(LookupDefinition.TitleSingular3);
                var code = nameof(LookupDefinition.Code);

                query = query.Filter($"{titleS} contains '{search}' or {titleS2} contains '{search}' or {titleS3} contains '{search}' or {titleP} contains '{search}' or {titleP2} contains '{search}' or {titleP3} contains '{search}' or {code} contains '{search}'");
            }

            return Task.FromResult(query);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<LookupDefinitionForSave> entities, bool returnIds)
        {
            SaveResult result = await _behavior.Repository.LookupDefinitions__Save(
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
            DeleteResult result = await _behavior.Repository.LookupDefinitions__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }
    }
}
