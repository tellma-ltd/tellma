using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Services.ApiAuthentication;

namespace Tellma.Controllers
{
    [Route("api/reconciliation")]
    [AuthorizeJwtBearer]
    [ApplicationController]
    [ApiController]
    [ApiVersion("1.0")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ReconciliationController : ControllerBase
    {
        private readonly ReconciliationService _service;

        public ReconciliationController(ReconciliationService service)
        {
            _service = service;
        }

        [HttpGet("unreconciled")]
        public async Task<ActionResult<ReconciliationGetUnreconciledResponse>> GetUnreconciled([FromQuery] ReconciliationGetUnreconciledArguments args, CancellationToken cancellation)
        {
            var result = await _service.GetUnreconciled(args, cancellation);
            return Ok(MapFromResult(result));
        }

        [HttpGet("reconciled")]
        public async Task<ActionResult<ReconciliationGetReconciledResponse>> GetReconciled([FromQuery] ReconciliationGetReconciledArguments args, CancellationToken cancellation)
        {
            var result = await _service.GetReconciled(args, cancellation);
            return Ok(MapFromResult(result));
        }

        [HttpPost("unreconciled")]
        public async Task<ActionResult<ReconciliationGetUnreconciledResponse>> SaveAndGetUnreconciled([FromBody] ReconciliationSavePayload payload, [FromQuery] ReconciliationGetUnreconciledArguments args)
        {
            var result = await _service.SaveAndGetUnreconciled(payload, args);
            return Ok(MapFromResult(result));
        }

        [HttpPost("reconciled")]
        public async Task<ActionResult<ReconciliationGetReconciledResponse>> SaveAndGetReconciled([FromBody] ReconciliationSavePayload payload, [FromQuery] ReconciliationGetReconciledArguments args)
        {
            var result = await _service.SaveAndGetReconciled(payload, args);
            return Ok(MapFromResult(result));
        }

        [HttpPost("import"), RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
        public async Task<ActionResult<List<ExternalEntryForSave>>> Import()
        {
            string contentType = null;
            string fileName = null;
            Stream fileStream = null;

            if (Request.Form.Files.Count > 0)
            {
                var formFile = Request.Form.Files[0];

                contentType = formFile.ContentType;
                fileName = formFile.FileName;
                fileStream = formFile.OpenReadStream();
            }

            try
            {
                var result = await _service.Import(fileStream, fileName, contentType);
                return Ok(result);
            }
            finally
            {
                if (fileStream != null)
                {
                    await fileStream.DisposeAsync();
                }
            }
        }
        private static ReconciliationGetUnreconciledResponse MapFromResult(UnreconciledResult result)
        {
            return new ReconciliationGetUnreconciledResponse
            {
                EntriesBalance = result.EntriesBalance,
                UnreconciledEntriesBalance = result.UnreconciledEntriesBalance,
                UnreconciledExternalEntriesBalance = result.UnreconciledExternalEntriesBalance,
                UnreconciledEntriesCount = result.UnreconciledEntriesCount,
                UnreconciledExternalEntriesCount = result.UnreconciledExternalEntriesCount,
                Entries = result.Entries,
                ExternalEntries = result.ExternalEntries
            };
        }

        private static ReconciliationGetReconciledResponse MapFromResult(ReconciledResult result)
        {
            return new ReconciliationGetReconciledResponse
            {
                ReconciledCount = result.ReconciledCount,
                Reconciliations = result.Reconciliations
            };
        }
    }
}
