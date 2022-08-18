using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.Metadata;
using Tellma.Model.Application;
using Tellma.Repository.Common;
using Tellma.Utilities.Common;

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

        public async Task<EntitiesResult<ResourceDefinition>> UpdateState(List<int> ids, UpdateStateArguments args)
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
            OperationOutput output = await _behavior.Repository.ResourceDefinitions__UpdateState(
                    ids: ids,
                    state: args.State,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(output.Errors);

            // Prepare result
            var result = (args.ReturnEntities ?? false) ?
                await GetByIds(ids, args, action, cancellation: default) :
                EntitiesResult<ResourceDefinition>.Empty();

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, result.Data);

            trx.Complete();
            return result;
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
                e.ToDateVisibility ??= Visibility.None;
                e.Date1Visibility ??= Visibility.None;
                e.Date2Visibility ??= Visibility.None;
                e.Date3Visibility ??= Visibility.None;
                e.Date4Visibility ??= Visibility.None;
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
                e.Agent1Visibility ??= Visibility.None;
                e.ReorderLevelVisibility ??= Visibility.None;
                e.Resource1Visibility ??= Visibility.None;
                e.Text1Visibility ??= Visibility.None;
                e.Text2Visibility ??= Visibility.None;
                e.UnitMassVisibility ??= Visibility.None;
                e.VatRateVisibility ??= Visibility.None;

                e.UnitCardinality ??= Cardinality.None;
            });

            return base.SavePreprocessAsync(entities);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<ResourceDefinitionForSave> entities, bool returnIds)
        {
            foreach (var (entity, index) in entities.Indexed())
            {
                if (entity.DefaultVatRate < 0m || entity.DefaultVatRate > 1m)
                {
                    var path = $"[{index}].{nameof(ResourceDefinition.DefaultVatRate)}";
                    var msg = _localizer["Error_VatRateMustBeBetweenZeroAndOne"];

                    ModelState.AddError(path, msg);
                }
            }

            SaveOutput result = await _behavior.Repository.ResourceDefinitions__Save(
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
            DeleteOutput result = await _behavior.Repository.ResourceDefinitions__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        protected override async Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken cancellation)
        {
            // By default: Order report definitions by name
            var settings = await _behavior.Settings(cancellation);
            string orderby = $"{nameof(ResourceDefinition.TitleSingular)},{nameof(ResourceDefinition.Id)}";
            if (settings.SecondaryLanguageId == CultureInfo.CurrentUICulture.Name)
            {
                orderby = $"{nameof(ResourceDefinition.TitleSingular2)},{nameof(ResourceDefinition.TitleSingular)},{nameof(ResourceDefinition.Id)}";
            }
            else if (settings.TernaryLanguageId == CultureInfo.CurrentUICulture.Name)
            {
                orderby = $"{nameof(ResourceDefinition.TitleSingular3)},{nameof(ResourceDefinition.TitleSingular)},{nameof(ResourceDefinition.Id)}";
            }

            return ExpressionOrderBy.Parse(orderby);
        }
    }
}
