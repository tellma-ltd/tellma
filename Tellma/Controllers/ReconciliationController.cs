using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Services.ApiAuthentication;
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
        private readonly ILogger<SettingsController> _logger;

        public ReconciliationController(ReconciliationService service,
            ILogger<SettingsController> logger)
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

        //[HttpGet("report")]
        //public async Task<ActionResult<ReconciliationLoadReconciledResponse>> LoadReport([FromQuery] ReconciliationLoadReconciledArguments args, CancellationToken cancellation)
        //{
        //    return await ControllerUtilities.InvokeActionImpl(async () =>
        //    {
        //        var result = await _service.GetReconciled(args, cancellation);
        //        return Ok(result);
        //    },
        //    _logger);
        //}

        [HttpPost("unreconciled")]
        public async Task<ActionResult<SaveSettingsResponse>> SaveAndGetUnreconciled([FromBody] ReconciliationSavePayload payload, [FromQuery] ReconciliationGetUnreconciledArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await _service.SaveAndGetUnreconciled(payload, args);
                return Ok(result);
            },
            _logger);
        }

        [HttpPost("reconciled")]
        public async Task<ActionResult<SaveSettingsResponse>> SaveAndGetReconciled([FromBody] ReconciliationSavePayload payload, [FromQuery] ReconciliationGetReconciledArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await _service.SaveAndGetReconciled(payload, args);
                return Ok(result);
            },
            _logger);
        }
    }

    public class ReconciliationService : ServiceBase
    {
        public const string VIEW = "reconciliation";

        private readonly ApplicationRepository _repo;
        private readonly IStringLocalizer _localizer;

        public ReconciliationService(ApplicationRepository repo,
            IStringLocalizer<Strings> localizer)
        {
            _repo = repo;
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
            // Authorized access (Criteria are not supported here)
            var permissions = await _repo.PermissionsFromCache(VIEW, Constants.Update, default);
            if (!permissions.Any())
            {
                throw new ForbiddenException();
            }

            return null;
        }

        public async Task<ReconciliationGetUnreconciledResponse> SaveAndGetReconciled(ReconciliationSavePayload payload, ReconciliationGetReconciledArguments args)
        {
            // Authorized access (Criteria are not supported here)
            var permissions = await _repo.PermissionsFromCache(VIEW, Constants.Update, default);
            if (!permissions.Any())
            {
                throw new ForbiddenException();
            }

            return null;
        }
    }
}
