using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/markup-templates")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class MarkupTemplatesController : CrudControllerBase<PrintingTemplateForSave, PrintingTemplate, int>
    {
        private readonly PrintingTemplatesService _service;

        public MarkupTemplatesController(PrintingTemplatesService service)
        {
            _service = service;
        }

        [HttpPut("preview-by-filter")]
        public async Task<ActionResult<MarkupPreviewResponse>> PreviewByFilter([FromBody] MarkupPreviewTemplate entity, [FromQuery] GenerateMarkupByFilterArguments<object> args, CancellationToken cancellation)
        {
            var result = await _service.PreviewByFilter(entity, args, cancellation);

            // Prepare and return the response
            var response = new MarkupPreviewResponse
            {
                Body = result.Body,
                DownloadName = result.DownloadName
            };

            return Ok(response);
        }

        [HttpPut("preview-by-id/{id}")]
        public async Task<ActionResult<MarkupPreviewResponse>> PreviewById([FromRoute] string id, [FromBody] MarkupPreviewTemplate entity, [FromQuery] GenerateMarkupByIdArguments args, CancellationToken cancellation)
        {
            var result = await _service.PreviewById(id, entity, args, cancellation);

            // Prepare and return the response
            var response = new MarkupPreviewResponse
            {
                Body = result.Body,
                DownloadName = result.DownloadName
            };

            return Ok(response);
        }

        [HttpPut("preview")]
        public async Task<ActionResult<MarkupPreviewResponse>> Preview([FromBody] MarkupPreviewTemplate entity, [FromQuery] GenerateMarkupArguments args, CancellationToken cancellation)
        {
            var result = await _service.Preview(entity, args, cancellation);

            // Prepare and return the response
            var response = new MarkupPreviewResponse
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
