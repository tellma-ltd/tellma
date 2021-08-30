using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.ImportExport;
using Tellma.Api.Metadata;
using Tellma.Model.Application;
using Tellma.Model.Common;
using Tellma.Repository.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class ReconciliationService : ServiceBase
    {
        public const string View = "reconciliation";

        private readonly ApplicationServiceBehavior _behavior;
        private readonly MetadataProvider _metadata;
        private readonly IPermissionsCache _permissionsCache;
        private readonly IStringLocalizer _localizer;

        public ReconciliationService(
            ApplicationServiceBehavior behavior,
            IServiceContextAccessor accessor,
            MetadataProvider metadata,
            IPermissionsCache permissionsCache, 
            IStringLocalizer<Strings> localizer): base(accessor)
        {
            _behavior = behavior;
            _metadata = metadata;
            _permissionsCache = permissionsCache;
            _localizer = localizer;
        }

        protected override IServiceBehavior Behavior => _behavior;

        public async Task<UnreconciledResult> GetUnreconciled(ReconciliationGetUnreconciledArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Authorized access (Criteria are not supported here)
            var permissions = await UserPermissions(PermissionActions.Read, cancellation);
            if (!permissions.Any())
            {
                throw new ForbiddenException();
            }

            UnreconciledOutput output = await _behavior.Repository.Reconciliation__Load_Unreconciled(
                accountId: args.AccountId,
                agentId: args.AgentId,
                asOfDate: args.AsOfDate,
                top: args.EntriesTop,
                skip: args.EntriesSkip,
                topExternal: args.ExternalEntriesTop,
                skipExternal: args.ExternalEntriesSkip, cancellation);

            return MapFromOutput(output);
        }

        public async Task<ReconciledResult> GetReconciled(ReconciliationGetReconciledArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Authorized access (Criteria are not supported here)
            var permissions = await UserPermissions(PermissionActions.Read, cancellation);
            if (!permissions.Any())
            {
                throw new ForbiddenException();
            }

            ReconciledOutput output = await _behavior.Repository.Reconciliation__Load_Reconciled(
                accountId: args.AccountId,
                agentId: args.AgentId,
                fromDate: args.FromDate,
                toDate: args.ToDate,
                fromAmount: args.FromAmount,
                toAmount: args.ToAmount,
                externalReferenceContains: args.ExternalReferenceContains,
                top: args.Top,
                skip: args.Skip,
                cancellation);

            return MapFromOutput(output);
        }

        public async Task<UnreconciledResult> SaveAndGetUnreconciled(ReconciliationSavePayload payload, ReconciliationGetUnreconciledArguments args)
        {
            await Initialize();

            // Start transaction
            using var trx = TransactionFactory.ReadCommitted();

            // Preprocess and Validate
            await PermissionsPreprocessAndValidate(args.AccountId, args.AgentId, payload);

            // Save
            UnreconciledOutput output = await _behavior.Repository.Reconciliations__SaveAndLoad_Unreconciled(
                accountId: args.AccountId,
                agentId: args.AgentId,
                externalEntriesForSave: payload.ExternalEntries,
                reconciliations: payload.Reconciliations,
                deletedExternalEntryIds: payload.DeletedExternalEntryIds,
                deletedReconciliationIds: payload.DeletedReconciliationIds,
                asOfDate: args.AsOfDate,
                top: args.EntriesTop,
                skip: args.EntriesSkip,
                topExternal: args.ExternalEntriesTop,
                skipExternal: args.ExternalEntriesSkip,
                userId: UserId);

            trx.Complete();

            return MapFromOutput(output);
        }

        public async Task<ReconciledResult> SaveAndGetReconciled(ReconciliationSavePayload payload, ReconciliationGetReconciledArguments args)
        {
            await Initialize();

            // Start transaction
            using var trx = TransactionFactory.ReadCommitted();

            // Preprocess and Validate
            await PermissionsPreprocessAndValidate(args.AccountId, args.AgentId, payload);

            // Save
            ReconciledOutput output = await _behavior.Repository.Reconciliations__SaveAndLoad_Reconciled(
                accountId: args.AccountId,
                agentId: args.AgentId,
                externalEntriesForSave: payload.ExternalEntries,
                reconciliations: payload.Reconciliations,
                deletedExternalEntryIds: payload.DeletedExternalEntryIds,
                deletedReconciliationIds: payload.DeletedReconciliationIds,
                fromDate: args.FromDate,
                toDate: args.ToDate,
                fromAmount: args.FromAmount,
                toAmount: args.ToAmount,
                externalReferenceContains: args.ExternalReferenceContains,
                top: args.Top,
                skip: args.Skip,
                userId: UserId);

            trx.Complete();

            return MapFromOutput(output);
        }

        private async Task PermissionsPreprocessAndValidate(int accountId, int agentId, ReconciliationSavePayload payload)
        {
            // Authorized access (Criteria are not supported here)
            var permissions = await UserPermissions(PermissionActions.Update);
            if (!permissions.Any())
            {
                throw new ForbiddenException();
            }

            // Makes the subsequent logic simpler
            payload.ExternalEntries ??= new List<ExternalEntryForSave>();
            payload.Reconciliations ??= new List<ReconciliationForSave>();
            foreach (var reconciliation in payload.Reconciliations)
            {
                reconciliation.Entries ??= new List<ReconciliationEntryForSave>();
                reconciliation.ExternalEntries ??= new List<ReconciliationExternalEntryForSave>();
            }

            // Trim the only string property
            payload.ExternalEntries.ForEach(e =>
            {
                if (e != null && e.ExternalReference != null)
                {
                    e.ExternalReference = e.ExternalReference.Trim();
                }
            });

            // Structural Validation
            int tenantId = _behavior.TenantId;
            var exEntryMeta = _metadata.GetMetadata(tenantId, typeof(ExternalEntryForSave));
            ValidateList(payload.ExternalEntries, exEntryMeta, nameof(payload.ExternalEntries));

            var reconciliationMeta = _metadata.GetMetadata(tenantId, typeof(ReconciliationForSave));
            ValidateList(payload.Reconciliations, reconciliationMeta, nameof(payload.Reconciliations));

            ModelState.ThrowIfInvalid();

            // SQL Validation
            var sqlErrors = await _behavior.Repository.Reconciliations_Validate__Save(
                accountId: accountId,
                agentId: agentId,
                externalEntriesForSave: payload.ExternalEntries,
                reconciliations: payload.Reconciliations,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddLocalizedErrors(sqlErrors, _localizer);
            ModelState.ThrowIfInvalid();
        }

        public async Task<List<ExternalEntryForSave>> Import(Stream fileStream, string fileName, string contentType)
        {
            await Initialize();

            // Validation
            if (fileStream == null)
            {
                throw new ServiceException(_localizer["Error_NoFileWasUploaded"]);
            }

            // Extract the raw data from the file stream
            IEnumerable<string[]> data = BaseUtil.ExtractStringsFromFile(fileStream, fileName, contentType, _localizer);
            if (data.Count() <= 1)
            {
                throw new ServiceException(_localizer["Error_UploadedFileWasEmpty"]);
            }

            // Errors
            var importErrors = new ImportErrors();

            // Result
            var result = new List<ExternalEntryForSave>();

            // Go over every row and parse
            foreach (var (row, rowIndex) in data.Select((e, i) => (e, i)).Skip(1))
            {
                var dateString = row.ElementAtOrDefault(0);
                var externalRef = row.ElementAtOrDefault(1);
                var amountString = row.ElementAtOrDefault(2);

                // Ignore empty rows
                if (string.IsNullOrWhiteSpace(dateString) && string.IsNullOrWhiteSpace(externalRef) && string.IsNullOrWhiteSpace(amountString))
                {
                    continue;
                }

                var exEntry = new ExternalEntryForSave
                {
                    ExternalReference = externalRef
                };

                // Parse date
                if (string.IsNullOrWhiteSpace(dateString))
                {
                    importErrors.AddImportError(rowIndex + 1, 1, _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Line_PostingDate"]]);
                }
                else if (DateTime.TryParse(dateString, out DateTime date))
                {
                    exEntry.PostingDate = date;
                }
                else if (double.TryParse(dateString, out double d))
                {
                    // Double indicates an OLE Automation date which typically comes from excel
                    exEntry.PostingDate = DateTime.FromOADate(d);
                }
                else
                {
                    throw new ParseException(_localizer["Error_Value0IsNotAValid1Example2", dateString, _localizer["DateTime"], DateTime.Today.ToString("yyyy-MM-dd")]);
                }

                if (string.IsNullOrWhiteSpace(amountString))
                {
                    importErrors.AddImportError(rowIndex + 1, 3, _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Entry_MonetaryValue"]]);
                }
                else if (decimal.TryParse(amountString, out decimal d))
                {
                    exEntry.MonetaryValue = Math.Abs(d);
                    exEntry.Direction = d < 0 ? (short)-1 : (short)1;
                }
                else
                {
                    throw new ParseException(_localizer["Error_Value0IsNotAValid1Example2", amountString, _localizer["Decimal"], 21502.75m]);
                }

                if (importErrors.IsValid)
                {
                    result.Add(exEntry);
                }
            }

            importErrors.ThrowIfInvalid(_localizer);

            return result;
        }

        /// <summary>
        /// Retrieves the current user's permissions that pertain to the reconciliation API.
        /// </summary>
        /// <param name="action">The permission action.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation = default) =>
            await _permissionsCache.PermissionsFromCache(
                tenantId: _behavior.TenantId,
                userId: UserId,
                version: _behavior.PermissionsVersion,
                view: View,
                action: action,
                cancellation: cancellation);

        private static UnreconciledResult MapFromOutput(UnreconciledOutput output)
        {
            return new UnreconciledResult
            (
                entriesBalance: output.EntriesBalance,
                unreconciledEntriesBalance: output.UnreconciledEntriesBalance,
                unreconciledExternalEntriesBalance: output.UnreconciledExternalEntriesBalance,
                unreconciledEntriesCount: output.UnreconciledEntriesCount,
                unreconciledExternalEntriesCount: output.UnreconciledExternalEntriesCount,
                entries: output.Entries,
                externalEntries: output.ExternalEntries
            );
        }

        private static ReconciledResult MapFromOutput(ReconciledOutput output)
        {
            return new ReconciledResult
            (
                reconciledCount: output.ReconciledCount,
                reconciliations: output.Reconciliations
            );
        }
    }
}
