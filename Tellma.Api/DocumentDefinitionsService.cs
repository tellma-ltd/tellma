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
    public class DocumentDefinitionsService : CrudServiceBase<DocumentDefinitionForSave, DocumentDefinition, int>
    {
        protected override string View => "document-definitions";

        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer<Strings> _localizer;

        public DocumentDefinitionsService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps, IStringLocalizer<Strings> localizer) : base(deps)
        {
            _behavior = behavior;
            _localizer = localizer;
        }

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<DocumentDefinition>> Search(EntityQuery<DocumentDefinition> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var titleP = nameof(DocumentDefinition.TitlePlural);
                var titleP2 = nameof(DocumentDefinition.TitlePlural2);
                var titleP3 = nameof(DocumentDefinition.TitlePlural3);

                var titleS = nameof(DocumentDefinition.TitleSingular);
                var titleS2 = nameof(DocumentDefinition.TitleSingular2);
                var titleS3 = nameof(DocumentDefinition.TitleSingular3);
                var code = nameof(DocumentDefinition.Code);

                query = query.Filter($"{titleS} contains '{search}' or {titleS2} contains '{search}' or {titleS3} contains '{search}' or {titleP} contains '{search}' or {titleP2} contains '{search}' or {titleP3} contains '{search}' or {code} contains '{search}'");
            }

            return Task.FromResult(query);
        }

        protected override Task<List<DocumentDefinitionForSave>> SavePreprocessAsync(List<DocumentDefinitionForSave> entities)
        {
            // Defaults
            entities?.ForEach(e =>
            {
                e.IsOriginalDocument ??= true;
                e.DocumentType ??= 2;
                e.HasAttachments ??= true;
                e.HasBookkeeping ??= true;
                e.CodeWidth ??= 4;

                e.MemoVisibility ??= Visibility.None;
                e.PostingDateVisibility ??= Visibility.None;
                e.CenterVisibility ??= Visibility.None;
                e.ClearanceVisibility ??= Visibility.None;
            });

            return base.SavePreprocessAsync(entities);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<DocumentDefinitionForSave> entities, bool returnIds)
        {
            #region Validate

            foreach (var (docDef, docDefIndex) in entities.Select((e, i) => (e, i)))
            {
                if (docDef.LineDefinitions == null || docDef.LineDefinitions.Count == 0)
                {
                    string path = $"[{docDefIndex}].{nameof(DocumentDefinition.LineDefinitions)}";
                    string msg = _localizer["Error_OneLineDefinitionIsRquired"];

                    ModelState.AddError(path, msg);
                }
                else
                {
                    // Line Definitions that are duplicated within the same document
                    var duplicateIndices = docDef.LineDefinitions
                        .Select((entity, index) => (entity.LineDefinitionId, index))
                        .GroupBy(pair => pair.LineDefinitionId)
                        .Where(g => g.Count() > 1)
                        .SelectMany(g => g)
                        .Select((_, index) => index);

                    foreach (var index in duplicateIndices)
                    {
                        string path = $"[{docDefIndex}].{nameof(DocumentDefinition.LineDefinitions)}[{index}].{nameof(DocumentDefinitionLineDefinition.LineDefinitionId)}";
                        string msg = _localizer["Error_DuplicateLineDefinition"];

                        ModelState.AddError(path, msg);
                    }
                }
            }

            #endregion

            #region Save

            SaveResult result = await _behavior.Repository.DocumentDefinitions__Save(
                entities: entities,
                returnIds: returnIds,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            return result.Ids;

            #endregion
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            var defs = await _behavior.Definitions();
            int jvDefinitionId = defs.ManualJournalVouchersDefinitionId;

            foreach (var (id, index) in ids.Select((e, i) => (e, i)))
            {
                if (id == jvDefinitionId)
                {
                    string path = $"[{index}]";
                    string msg = _localizer["Error_CannotModifySystemItem"];

                    ModelState.AddError(path, msg);
                }
            }

            DeleteResult result = await _behavior.Repository.DocumentDefinitions__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        public async Task<(List<DocumentDefinition>, Extras)> UpdateState(List<int> ids, UpdateStateArguments args)
        {
            await Initialize();

            // Check user permissions
            var action = "State";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Validation
            if (string.IsNullOrWhiteSpace(args.State))
            {
                throw new ServiceException(_localizer[ErrorMessages.Error_Field0IsRequired, _localizer["State"]]);
            }

            if (!DefStates.All.Contains(args.State))
            {
                string validStates = string.Join(", ", DefStates.All);
                throw new ServiceException($"'{args.State}' is not a valid definition state, valid states are: {validStates}.");
            }

            var defs = await _behavior.Definitions();
            int jvDefId = defs.ManualJournalVouchersDefinitionId;

            int index = 0;
            ids.ForEach(id =>
            {
                if (id == jvDefId)
                {
                    string path = $"[{index}]";
                    string msg = _localizer["Error_CannotModifySystemItem"];

                    ModelState.AddError(path, msg);
                }

                index++;
            });

            // Execute
            using var trx = Transactions.ReadCommitted();
            OperationResult result = await _behavior.Repository.DocumentDefinitions__UpdateState(
                    ids: ids,
                    state: args.State,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            // Prepare response
            List<DocumentDefinition> data = null;
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
    }
}
