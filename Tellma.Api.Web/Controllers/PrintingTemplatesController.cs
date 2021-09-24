using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Application;

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
            var service = GetFactService();
            var result = await service.PrintEntities(templateId, args, cancellation);

            var fileBytes = result.FileBytes;
            var fileName = result.FileName;
            var contentType = ControllerUtilities.ContentType(fileName);

            return File(fileContents: fileBytes, contentType: contentType, fileName);
        }

        [HttpPut("preview-by-filter")]
        public async Task<ActionResult<PrintPreviewResponse>> PreviewByFilter([FromBody] PrintingPreviewTemplate entity, [FromQuery] PrintEntitiesPreviewArguments<object> args, CancellationToken cancellation)
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
        public async Task<ActionResult<PrintPreviewResponse>> PreviewById([FromRoute] string id, [FromBody] PrintingPreviewTemplate entity, [FromQuery] PrintEntityByIdPreviewArguments args, CancellationToken cancellation)
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
        public async Task<ActionResult<PrintPreviewResponse>> Preview([FromBody] PrintingPreviewTemplate entity, [FromQuery] PrintPreviewArguments args, CancellationToken cancellation)
        {
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
    }
}
