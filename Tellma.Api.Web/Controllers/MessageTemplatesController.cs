using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/message-templates")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class MessageTemplatesController : CrudControllerBase<MessageTemplateForSave, MessageTemplate, int>
    {
        private readonly MessageTemplatesService _service;

        public MessageTemplatesController(MessageTemplatesService service)
        {
            _service = service;
        }

        [HttpPut("message-entities-preview")]
        public async Task<ActionResult<PrintPreviewResponse>> MessageCommandPreviewEntities(
            [FromBody] MessageTemplate template,
            [FromQuery] PrintEntitiesArguments<int> args,
            CancellationToken cancellation)
        {
            var result = await _service.MessageCommandPreviewEntities(template, args, cancellation);
            return Ok(result);
        }

        [HttpPut("{id:int}/message-entity-preview")]
        public async Task<ActionResult<PrintPreviewResponse>> MessageCommandPreviewEntity(
            [FromRoute] string id,
            [FromBody] MessageTemplate template,
            [FromQuery] PrintEntityByIdArguments args,
            CancellationToken cancellation)
        {
            var result = await _service.MessageCommandPreviewEntity(id: id, template, args, cancellation);
            return Ok(result);
        }

        protected override CrudServiceBase<MessageTemplateForSave, MessageTemplate, int> GetCrudService()
        {
            return _service;
        }

        protected override Task OnSuccessfulSave(EntitiesResult<MessageTemplate> data)
        {
            if (data?.Data != null && data.Data.Any(e => e.IsDeployed ?? false))
            {
                Response.Headers.Set("x-definitions-version", Constants.Stale);
            }

            return base.OnSuccessfulSave(data);
        }
    }
}
