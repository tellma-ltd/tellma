using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Application;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/printing-templates")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class PrintingTemplatesController : CrudControllerBase<PrintingTemplateForSave, PrintingTemplate, int>
    {
        private readonly PrintingTemplatesService _service;

        public PrintingTemplatesController(PrintingTemplatesService service)
        {
            _service = service;
        }

        [HttpGet("print/{templateId}")]
        public async Task<FileContentResult> Print(int templateId, [FromQuery] PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            args.Custom = Request.Query.ToDictionary(e => e.Key, e => e.Value.FirstOrDefault());

            var result = await _service.Print(templateId, args, cancellation);

            var fileBytes = result.FileBytes;
            var fileName = result.FileName;
            var contentType = ControllerUtilities.ContentType(fileName);
            Response.Headers.Add("x-filename", fileName);

            return File(fileContents: fileBytes, contentType: contentType, fileName);
        }

        [HttpPut("preview-by-filter")]
        public async Task<ActionResult<PrintPreviewResponse>> PreviewByFilter([FromBody] PrintingPreviewTemplate entity, [FromQuery] PrintEntitiesArguments<object> args, CancellationToken cancellation)
        {
            var result = await _service.PreviewByFilter(entity, args, cancellation);

            // Prepare and return the response
            var response = new PrintPreviewResponse
            {
                Body = result.Body,
                DownloadName = result.DownloadName
            };

            return Ok(response);
        }

        [HttpPut("preview-by-id/{id}")]
        public async Task<ActionResult<PrintPreviewResponse>> PreviewById([FromRoute] string id, [FromBody] PrintingPreviewTemplate entity, [FromQuery] PrintEntityByIdArguments args, CancellationToken cancellation)
        {
            var result = await _service.PreviewById(id, entity, args, cancellation);

            // Prepare and return the response
            var response = new PrintPreviewResponse
            {
                Body = result.Body,
                DownloadName = result.DownloadName
            };

            return Ok(response);
        }

        [HttpPut("preview")]
        public async Task<ActionResult<PrintPreviewResponse>> Preview([FromBody] PrintingPreviewTemplate entity, [FromQuery] PrintArguments args, CancellationToken cancellation)
        {
            args.Custom = Request.Query.ToDictionary(e => e.Key, e => e.Value.FirstOrDefault());

            var result = await _service.Preview(entity, args, cancellation);

            // Prepare and return the response
            var response = new PrintPreviewResponse
            {
                Body = result.Body,
                DownloadName = result.DownloadName
            };

            return Ok(response);
        }

        protected override CrudServiceBase<PrintingTemplateForSave, PrintingTemplate, int> GetCrudService()
        {
            return _service;
        }

        protected override Task OnSuccessfulSave(EntitiesResult<PrintingTemplate> data)
        {
            if (data?.Data != null && data.Data.Any(e => e.IsDeployed ?? false))
            {
                Response.Headers.Set("x-definitions-version", Constants.Stale);
            }

            return base.OnSuccessfulSave(data);
        }
    }
}
