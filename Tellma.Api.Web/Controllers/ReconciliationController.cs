using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Application;
using Tellma.Services.ApiAuthentication;

namespace Tellma.Controllers
{
    [Route("api/reconciliation")]
    [AuthorizeJwtBearer]
    [ApplicationController]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ReconciliationController : ControllerBase
    {
        private readonly ReconciliationService _service;
        private readonly ILogger<GeneralSettingsController> _logger;
        private readonly IStringLocalizer<Strings> _localizer;

        public ReconciliationController(ReconciliationService service,
            ILogger<GeneralSettingsController> logger, IStringLocalizer<Strings> localizer)
        {
            _service = service;
            _logger = logger;
            _localizer = localizer;
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
                if (Request.Form.Files.Count == 0)
                {
                    throw new ServiceException(_localizer["Error_NoFileWasUploaded"]);
                }

                IFormFile formFile = Request.Form.Files[0];
                var contentType = formFile?.ContentType;
                var fileName = formFile?.FileName;
                using var fileStream = formFile?.OpenReadStream();

                var result = await _service.Import(fileStream, fileName, contentType);

                return Ok(result);
            }, _logger);
        }

    }
}
