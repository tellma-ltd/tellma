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
    public class RelationDefinitionsService : CrudServiceBase<RelationDefinitionForSave, RelationDefinition, int>
    {
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationFactServiceBehavior _behavior;

        public RelationDefinitionsService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps) : base(deps)
        {
            _localizer = deps.Localizer;
            _behavior = behavior;
        }

        protected override string View => "relation-definitions";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        public async Task<(List<RelationDefinition>, Extras)> UpdateState(List<int> ids, UpdateStateArguments args)
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
            using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            OperationResult result = await _behavior.Repository.RelationDefinitions__UpdateState(
                    ids: ids,
                    state: args.State,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            // Prepare response
            List<RelationDefinition> data = null;
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

        protected override Task<EntityQuery<RelationDefinition>> Search(EntityQuery<RelationDefinition> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var titleP = nameof(RelationDefinition.TitlePlural);
                var titleP2 = nameof(RelationDefinition.TitlePlural2);
                var titleP3 = nameof(RelationDefinition.TitlePlural3);

                var titleS = nameof(RelationDefinition.TitleSingular);
                var titleS2 = nameof(RelationDefinition.TitleSingular2);
                var titleS3 = nameof(RelationDefinition.TitleSingular3);
                var code = nameof(RelationDefinition.Code);

                query = query.Filter($"{titleS} contains '{search}' or {titleS2} contains '{search}' or {titleS3} contains '{search}' or {titleP} contains '{search}' or {titleP2} contains '{search}' or {titleP3} contains '{search}' or {code} contains '{search}'");
            }

            return Task.FromResult(query);
        }

        protected override Task<List<RelationDefinitionForSave>> SavePreprocessAsync(List<RelationDefinitionForSave> entities)
        {
            entities.ForEach(e =>
            {
                e.ReportDefinitions ??= new List<RelationDefinitionReportDefinitionForSave>();

                e.AgentVisibility ??= Visibility.None;
                e.BankAccountNumberVisibility ??= Visibility.None;
                e.CenterVisibility ??= Visibility.None;
                e.ContactAddressVisibility ??= Visibility.None;
                e.ContactEmailVisibility ??= Visibility.None;
                e.ContactMobileVisibility ??= Visibility.None;
                e.CurrencyVisibility ??= Visibility.None;
                e.Date1Visibility ??= Visibility.None;
                e.Date2Visibility ??= Visibility.None;
                e.Date3Visibility ??= Visibility.None;
                e.Date4Visibility ??= Visibility.None;
                e.DateOfBirthVisibility ??= Visibility.None;
                e.Decimal1Visibility ??= Visibility.None;
                e.Decimal2Visibility ??= Visibility.None;
                e.DescriptionVisibility ??= Visibility.None;
                e.ExternalReferenceVisibility ??= Visibility.None;
                e.FromDateVisibility ??= Visibility.None;
                e.ImageVisibility ??= Visibility.None;
                e.Int1Visibility ??= Visibility.None;
                e.Int2Visibility ??= Visibility.None;
                e.LocationVisibility ??= Visibility.None;
                e.Lookup1Visibility ??= Visibility.None;
                e.Lookup2Visibility ??= Visibility.None;
                e.Lookup3Visibility ??= Visibility.None;
                e.Lookup4Visibility ??= Visibility.None;
                e.Lookup5Visibility ??= Visibility.None;
                e.Lookup6Visibility ??= Visibility.None;
                e.Lookup7Visibility ??= Visibility.None;
                e.Lookup8Visibility ??= Visibility.None;
                e.Relation1Visibility ??= Visibility.None;
                e.TaxIdentificationNumberVisibility ??= Visibility.None;
                e.Text1Visibility ??= Visibility.None;
                e.Text2Visibility ??= Visibility.None;
                e.Text3Visibility ??= Visibility.None;
                e.Text4Visibility ??= Visibility.None;
                e.ToDateVisibility ??= Visibility.None;
                e.UserCardinality ??= Cardinality.None;
                e.HasAttachments ??= false;

                if (!e.HasAttachments.Value)
                {
                    e.AttachmentsCategoryDefinitionId = null;
                }
            });

            return base.SavePreprocessAsync(entities);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<RelationDefinitionForSave> entities, bool returnIds)
        {
            SaveResult result = await _behavior.Repository.RelationDefinitions__Save(
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
            try
            {
                DeleteResult result = await _behavior.Repository.RelationDefinitions__Delete(
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
    }
}
