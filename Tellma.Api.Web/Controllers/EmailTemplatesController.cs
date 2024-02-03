using Asp.Versioning;
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
    [Route("api/email-templates")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class EmailTemplatesController : CrudControllerBase<EmailTemplateForSave, EmailTemplate, int>
    {
        private readonly EmailTemplatesService _service;

        public EmailTemplatesController(EmailTemplatesService service)
        {
            _service = service;
        }

        // Studio

        [HttpPut("email-entities-preview")]
        public async Task<ActionResult<EmailCommandPreview>> EmailCommandPreviewEntities(
            [FromBody] EmailTemplate template,
            [FromQuery] PrintEntitiesArguments<int> args,
            CancellationToken cancellation)
        {
            var result = await _service.EmailCommandPreviewEntities(template, args, cancellation);
            return Ok(result);
        }

        [HttpPut("email-entities-preview/{index:int}")]
        public async Task<ActionResult<EmailCommandPreview>> EmailPreviewEntities(
            [FromRoute] int index,
            [FromBody] EmailTemplate template,
            [FromQuery] PrintEntitiesArguments<int> args,
            CancellationToken cancellation)
        {
            var result = await _service.EmailPreviewEntities(template, index, args, cancellation);
            return Ok(result);
        }

        [HttpPut("{id:int}/email-entity-preview")]
        public async Task<ActionResult<EmailCommandPreview>> EmailCommandPreviewEntity(
            [FromRoute] int id,
            [FromBody] EmailTemplate template,
            [FromQuery] PrintEntityByIdArguments args,
            CancellationToken cancellation)
        {
            var result = await _service.EmailCommandPreviewEntity(id: id, template, args, cancellation);
            return Ok(result);
        }

        [HttpPut("{id:int}/email-entity-preview/{index:int}")]
        public async Task<ActionResult<EmailCommandPreview>> EmailCommandPreviewEntity(
            [FromRoute] int id,
            [FromRoute] int index,
            [FromBody] EmailTemplate template,
            [FromQuery] PrintEntityByIdArguments args,
            CancellationToken cancellation)
        {
            var result = await _service.EmailPreviewEntity(id: id, template, index, args, cancellation);
            return Ok(result);
        }

        [HttpPut("email-preview")]
        public async Task<ActionResult<EmailCommandPreview>> EmailCommandPreview(
            [FromBody] EmailTemplate template,
            [FromQuery] PrintArguments args,
            CancellationToken cancellation)
        {
            args.Custom = Request.Query.ToDictionary(e => e.Key, e => e.Value.FirstOrDefault());

            var result = await _service.EmailCommandPreview(template, args, cancellation);
            return Ok(result);
        }

        [HttpPut("email-preview/{index:int}")]
        public async Task<ActionResult<EmailCommandPreview>> EmailPreview(
            [FromRoute] int index,
            [FromBody] EmailTemplate template,
            [FromQuery] PrintArguments args,
            CancellationToken cancellation)
        {
            args.Custom = Request.Query.ToDictionary(e => e.Key, e => e.Value.FirstOrDefault());

            var result = await _service.EmailPreview(template, index, args, cancellation);
            return Ok(result);
        }

        // Standalone

        [HttpGet("email-preview-id/{templateId:int}")]
        public async Task<ActionResult<EmailCommandPreview>> EmailCommandPreviewByTemplateId(
            [FromRoute] int templateId,
            [FromQuery] PrintArguments args,
            CancellationToken cancellation
            )
        {
            args.Custom = Request.Query.ToDictionary(e => e.Key, e => e.Value.FirstOrDefault());

            var result = await _service.EmailCommandPreviewByTemplateId(templateId, args, cancellation);
            return Ok(result);
        }

        [HttpGet("email-preview-id/{templateId:int}/{index:int}")]
        public async Task<ActionResult<EmailCommandPreview>> EmailPreviewByTemplateId(
            [FromRoute] int templateId,
            [FromRoute] int index,
            [FromQuery] PrintArguments args,
            CancellationToken cancellation
            )
        {
            args.Custom = Request.Query.ToDictionary(e => e.Key, e => e.Value.FirstOrDefault());

            var result = await _service.EmailPreviewByTemplateId(templateId, index, args, cancellation);
            return Ok(result);
        }

        [HttpPut("email/{templateId:int}")]
        public async Task<ActionResult<IdResult>> SendByMessage(int templateId, [FromQuery] PrintArguments args, [FromBody] EmailCommandVersions versions, CancellationToken cancellation)
        {
            args.Custom = Request.Query.ToDictionary(e => e.Key, e => e.Value.FirstOrDefault());

            var commandId = await _service.SendByEmail(templateId, args, versions, cancellation);

            return Ok(new IdResult { Id = commandId });
        }


        protected override CrudServiceBase<EmailTemplateForSave, EmailTemplate, int> GetCrudService()
        {
            return _service;
        }

        protected override Task OnSuccessfulSave(EntitiesResult<EmailTemplate> data)
        {
            if (data?.Data != null && data.Data.Any(e => e.IsDeployed ?? false))
            {
                Response.Headers.Set("x-definitions-version", Constants.Stale);
            }

            return base.OnSuccessfulSave(data);
        }
    }
}
