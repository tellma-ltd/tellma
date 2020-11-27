using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.ImportExport;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Entities;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.MultiTenancy;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/" + ReconciliationService.VIEW)]
    [AuthorizeJwtBearer]
    [ApplicationController]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ReconciliationController : ControllerBase
    {
        private readonly ReconciliationService _service;
        private readonly ILogger<GeneralSettingsController> _logger;

        public ReconciliationController(ReconciliationService service,
            ILogger<GeneralSettingsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("unreconciled")]
        public async Task<ActionResult<ReconciliationGetUnreconciledResponse>> GetUnreconciled([FromQuery] ReconciliationGetUnreconciledArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await _service.GetUnreconciled(args, cancellation);
                return Ok(result);
            },
            _logger);
        }

        [HttpGet("reconciled")]
        public async Task<ActionResult<ReconciliationGetReconciledResponse>> GetReconciled([FromQuery] ReconciliationGetReconciledArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await _service.GetReconciled(args, cancellation);
                return Ok(result);
            },
            _logger);
        }

        [HttpPost("unreconciled")]
        public async Task<ActionResult<ReconciliationGetUnreconciledResponse>> SaveAndGetUnreconciled([FromBody] ReconciliationSavePayload payload, [FromQuery] ReconciliationGetUnreconciledArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await _service.SaveAndGetUnreconciled(payload, args);
                return Ok(result);
            },
            _logger);
        }

        [HttpPost("reconciled")]
        public async Task<ActionResult<ReconciliationGetReconciledResponse>> SaveAndGetReconciled([FromBody] ReconciliationSavePayload payload, [FromQuery] ReconciliationGetReconciledArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await _service.SaveAndGetReconciled(payload, args);
                return Ok(result);
            },
            _logger);
        }

        [HttpPost("import"), RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
        public async Task<ActionResult<List<ExternalEntryForSave>>> Import()
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                IFormFile formFile = Request.Form.Files.FirstOrDefault();
                var contentType = formFile?.ContentType;
                var fileName = formFile?.FileName;
                using var fileStream = formFile?.OpenReadStream();

                var result = await _service.Import(fileStream, fileName, contentType);

                return Ok(result);
            }, _logger);
        }

    }

    public class ReconciliationService : ServiceBase
    {
        public const string VIEW = "reconciliation";

        private readonly ApplicationRepository _repo;
        private readonly MetadataProvider _metadata;
        private readonly ITenantIdAccessor _tenantIdAccessor;
        private readonly IStringLocalizer _localizer;

        public ReconciliationService(ApplicationRepository repo, MetadataProvider metadata,
            ITenantIdAccessor tenantIdAccessor, IStringLocalizer<Strings> localizer)
        {
            _repo = repo;
            _metadata = metadata;
            _tenantIdAccessor = tenantIdAccessor;
            _localizer = localizer;
        }

        public async Task<ReconciliationGetUnreconciledResponse> GetUnreconciled(ReconciliationGetUnreconciledArguments args, CancellationToken cancellation)
        {
            // Authorized access (Criteria are not supported here)
            var permissions = await _repo.PermissionsFromCache(VIEW, Constants.Read, cancellation);
            if (!permissions.Any())
            {
                throw new ForbiddenException();
            }

            var (
                entriesBalance,
                unreconciledEntriesBalance,
                unreconciledExternalEntriesBalance,
                unreconciledEntriesCount,
                unreconciledExternalEntriesCount,
                entries,
                externalEntries
            ) = await _repo.Reconciliation__Load_Unreconciled(
                accountId: args.AccountId,
                custodyId: args.CustodyId,
                asOfDate: args.AsOfDate,
                top: args.EntriesTop,
                skip: args.EntriesSkip,
                topExternal: args.ExternalEntriesTop,
                skipExternal: args.ExternalEntriesSkip, cancellation);

            return new ReconciliationGetUnreconciledResponse
            {
                EntriesBalance = entriesBalance,
                UnreconciledEntriesBalance = unreconciledEntriesBalance,
                UnreconciledExternalEntriesBalance = unreconciledExternalEntriesBalance,
                UnreconciledEntriesCount = unreconciledEntriesCount,
                UnreconciledExternalEntriesCount = unreconciledExternalEntriesCount,
                Entries = entries,
                ExternalEntries = externalEntries
            };
        }

        public async Task<ReconciliationGetReconciledResponse> GetReconciled(ReconciliationGetReconciledArguments args, CancellationToken cancellation)
        {
            // Authorized access (Criteria are not supported here)
            var permissions = await _repo.PermissionsFromCache(VIEW, Constants.Read, cancellation);
            if (!permissions.Any())
            {
                throw new ForbiddenException();
            }

            var (
                reconciledCount,
                reconciliations
            ) = await _repo.Reconciliation__Load_Reconciled(
                accountId: args.AccountId,
                custodyId: args.CustodyId,
                fromDate: args.FromDate,
                toDate: args.ToDate,
                fromAmount: args.FromAmount,
                toAmount: args.ToAmount,
                externalReferenceContains: args.ExternalReferenceContains,
                top: args.Top,
                skip: args.Skip,
                cancellation);

            return new ReconciliationGetReconciledResponse
            {
                ReconciledCount = reconciledCount,
                Reconciliations = reconciliations
            };
        }

        public async Task<ReconciliationGetUnreconciledResponse> SaveAndGetUnreconciled(ReconciliationSavePayload payload, ReconciliationGetUnreconciledArguments args)
        {
            // Start transaction
            using var trx = ControllerUtilities.CreateTransaction();

            // Preprocess and Validate
            await PermissionsPreprocessAndValidate(args.AccountId, args.CustodyId, payload);

            // Save
            var (
                entriesBalance,
                unreconciledEntriesBalance,
                unreconciledExternalEntriesBalance,
                unreconciledEntriesCount,
                unreconciledExternalEntriesCount,
                entries,
                externalEntries
            ) = await _repo.Reconciliations__SaveAndLoad_Unreconciled(
                accountId: args.AccountId,
                custodyId: args.CustodyId,
                externalEntriesForSave: payload.ExternalEntries,
                reconciliations: payload.Reconciliations,
                deletedExternalEntryIds: payload.DeletedExternalEntryIds,
                deletedReconciliationIds: payload.DeletedReconciliationIds,
                asOfDate: args.AsOfDate,
                top: args.EntriesTop,
                skip: args.EntriesSkip,
                topExternal: args.ExternalEntriesTop,
                skipExternal: args.ExternalEntriesSkip);

            trx.Complete();

            return new ReconciliationGetUnreconciledResponse
            {
                EntriesBalance = entriesBalance,
                UnreconciledEntriesBalance = unreconciledEntriesBalance,
                UnreconciledExternalEntriesBalance = unreconciledExternalEntriesBalance,
                UnreconciledEntriesCount = unreconciledEntriesCount,
                UnreconciledExternalEntriesCount = unreconciledExternalEntriesCount,
                Entries = entries,
                ExternalEntries = externalEntries
            };
        }

        public async Task<ReconciliationGetReconciledResponse> SaveAndGetReconciled(ReconciliationSavePayload payload, ReconciliationGetReconciledArguments args)
        {
            // Start transaction
            using var trx = ControllerUtilities.CreateTransaction();

            // Preprocess and Validate
            await PermissionsPreprocessAndValidate(args.AccountId, args.CustodyId, payload);

            // Save
            var (
                reconciledCount,
                reconciliations
            ) = await _repo.Reconciliations__SaveAndLoad_Reconciled(
                accountId: args.AccountId,
                custodyId: args.CustodyId,
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
                skip: args.Skip);

            trx.Complete();

            return new ReconciliationGetReconciledResponse
            {
                ReconciledCount = reconciledCount,
                Reconciliations = reconciliations
            };
        }

        private async Task PermissionsPreprocessAndValidate(int accountId, int custodyId, ReconciliationSavePayload payload)
        {
            // Authorized access (Criteria are not supported here)
            var permissions = await _repo.PermissionsFromCache(VIEW, Constants.Update, default);
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

            // C# Validation
            int? tenantId = _tenantIdAccessor.GetTenantIdIfAny();

            var exEntryMeta = _metadata.GetMetadata(tenantId, typeof(ExternalEntryForSave));
            ValidateList(payload.ExternalEntries, exEntryMeta, "ExternalEntries");

            var reconciliationMeta = _metadata.GetMetadata(tenantId, typeof(ReconciliationForSave));
            ValidateList(payload.Reconciliations, reconciliationMeta, "Reconciliation");

            ModelState.ThrowIfInvalid();

            // SQL Validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Reconciliations_Validate__Save(
                accountId: accountId,
                custodyId: custodyId,
                externalEntriesForSave: payload.ExternalEntries,
                reconciliations: payload.Reconciliations,
                top: ModelState.MaxAllowedErrors);

            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
            ModelState.ThrowIfInvalid();
        }

        public Task<List<ExternalEntryForSave>> Import(Stream fileStream, string fileName, string contentType)
        {
            // Validation
            if (fileStream == null)
            {
                throw new BadRequestException(_localizer["Error_NoFileWasUploaded"]);
            }

            // Extract the raw data from the file stream
            IEnumerable<string[]> data = ControllerUtilities.ExtractStringsFromFile(fileStream, fileName, contentType, _localizer);
            if (data.Count() <= 1)
            {
                throw new BadRequestException(_localizer["Error_UploadedFileWasEmpty"]);
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
                    importErrors.AddImportError(rowIndex + 1, 1, _localizer[Constants.Error_Field0IsRequired, _localizer["Line_PostingDate"]]);
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
                    importErrors.AddImportError(rowIndex + 1, 3, _localizer[Constants.Error_Field0IsRequired, _localizer["Entry_MonetaryValue"]]);
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

            return Task.FromResult(result);
        }
    }
}
