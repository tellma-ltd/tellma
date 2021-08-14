using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.Metadata;
using Tellma.Model.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class ResourceDefinitionsService : CrudServiceBase<ResourceDefinitionForSave, ResourceDefinition, int>
    {
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationFactServiceBehavior _behavior;

        public ResourceDefinitionsService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps) : base(deps)
        {
            _localizer = deps.Localizer;
            _behavior = behavior;
        }

        protected override string View => "resource-definitions";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        public async Task<(List<ResourceDefinition>, Extras)> UpdateState(List<int> ids, UpdateStateArguments args)
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
            OperationResult result = await _behavior.Repository.ResourceDefinitions__UpdateState(
                    ids: ids,
                    state: args.State,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            // Prepare response
            List<ResourceDefinition> data = null;
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

        protected override Task<EntityQuery<ResourceDefinition>> Search(EntityQuery<ResourceDefinition> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var titleP = nameof(ResourceDefinition.TitlePlural);
                var titleP2 = nameof(ResourceDefinition.TitlePlural2);
                var titleP3 = nameof(ResourceDefinition.TitlePlural3);

                var titleS = nameof(ResourceDefinition.TitleSingular);
                var titleS2 = nameof(ResourceDefinition.TitleSingular2);
                var titleS3 = nameof(ResourceDefinition.TitleSingular3);
                var code = nameof(ResourceDefinition.Code);

                query = query.Filter($"{titleS} contains '{search}' or {titleS2} contains '{search}' or {titleS3} contains '{search}' or {titleP} contains '{search}' or {titleP2} contains '{search}' or {titleP3} contains '{search}' or {code} contains '{search}'");
            }

            return Task.FromResult(query);
        }

        protected override Task<List<ResourceDefinitionForSave>> SavePreprocessAsync(List<ResourceDefinitionForSave> entities)
        {
            entities.ForEach(e =>
            {
                e.ReportDefinitions ??= new List<ResourceDefinitionReportDefinitionForSave>();

                e.CenterVisibility ??= Visibility.None;
                e.CurrencyVisibility ??= Visibility.None;
                e.Decimal1Visibility ??= Visibility.None;
                e.Decimal2Visibility ??= Visibility.None;
                e.DescriptionVisibility ??= Visibility.None;
                e.EconomicOrderQuantityVisibility ??= Visibility.None;
                e.FromDateVisibility ??= Visibility.None;
                e.IdentifierVisibility ??= Visibility.None;
                e.ImageVisibility ??= Visibility.None;
                e.Int1Visibility ??= Visibility.None;
                e.Int2Visibility ??= Visibility.None;
                e.LocationVisibility ??= Visibility.None;
                e.Lookup1Visibility ??= Visibility.None;
                e.Lookup2Visibility ??= Visibility.None;
                e.Lookup3Visibility ??= Visibility.None;
                e.Lookup4Visibility ??= Visibility.None;
                e.MonetaryValueVisibility ??= Visibility.None;
                e.ParticipantVisibility ??= Visibility.None;
                e.ReorderLevelVisibility ??= Visibility.None;
                e.Resource1Visibility ??= Visibility.None;
                e.Text1Visibility ??= Visibility.None;
                e.Text2Visibility ??= Visibility.None;
                e.ToDateVisibility ??= Visibility.None;
                e.UnitMassVisibility ??= Visibility.None;
                e.VatRateVisibility ??= Visibility.None;

                e.UnitCardinality ??= Cardinality.None;
            });

            return base.SavePreprocessAsync(entities);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<ResourceDefinitionForSave> entities, bool returnIds)
        {
            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                if (entity.DefaultVatRate < 0m || entity.DefaultVatRate > 1m)
                {
                    var path = $"[{index}].{nameof(ResourceDefinition.DefaultVatRate)}";
                    var msg = _localizer["Error_VatRateMustBeBetweenZeroAndOne"];

                    ModelState.AddError(path, msg);
                }
            }

            SaveResult result = await _behavior.Repository.ResourceDefinitions__Save(
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
            DeleteResult result = await _behavior.Repository.ResourceDefinitions__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }
    }
}
