using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/agents/{definitionId}")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class AgentsController : CrudControllerBase<AgentForSave, Agent, int>
    {
        private readonly AgentsService _service;

        public AgentsController(AgentsService service)
        {
            _service = service;
        }

        [HttpGet("{id}/image")]
        public async Task<ActionResult> GetImage(int id, CancellationToken cancellation)
        {
            var result = await GetService().GetImage(id, cancellation);

            Response.Headers.Add("x-image-id", result.ImageId);
            return File(result.ImageBytes, MimeTypes.Jpeg);
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Agent>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var result = await GetService().Activate(ids: ids, args);
            var response = TransformToEntitiesResponse(result, serverTime, cancellation: default);

            return Ok(response);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Agent>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var result = await GetService().Deactivate(ids: ids, args);
            var response = TransformToEntitiesResponse(result, serverTime, cancellation: default);

            return Ok(response);
        }

        [HttpGet("{docId}/attachments/{attachmentId}")]
        public async Task<ActionResult> GetAttachment(int docId, int attachmentId, CancellationToken cancellation)
        {
            var result = await GetService().GetAttachment(docId, attachmentId, cancellation);
            var contentType = ControllerUtilities.ContentType(result.FileName);

            return File(fileContents: result.FileBytes, contentType: contentType, result.FileName);
        }


        [HttpGet("message-entities-preview/{templateId:int}")]
        public async Task<ActionResult<MessageCommandPreview>> MessageCommandPreviewEntities(int templateId, [FromQuery] PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            args.Custom = Request.Query.ToDictionary(e => e.Key, e => e.Value.FirstOrDefault());

            var service = GetService();
            var result = await service.MessageCommandPreviewEntities(templateId, args, cancellation);

            return Ok(result);
        }

        [HttpPut("message-entities/{templateId:int}")]
        public async Task<ActionResult> MessageEntities(int templateId, [FromQuery] PrintEntitiesArguments<int> args, [FromQuery] string version, CancellationToken cancellation)
        {
            args.Custom = Request.Query.ToDictionary(e => e.Key, e => e.Value.FirstOrDefault());

            var service = GetService();
            var commandId = await service.SendByMessage(templateId, args, version, cancellation);

            return Ok(new IdResult { Id = commandId });
        }

        [HttpGet("{id:int}/message-entity-preview/{templateId:int}")]
        public async Task<ActionResult<MessageCommandPreview>> MessageCommandPreviewEntity(int id, int templateId, [FromQuery] PrintEntityByIdArguments args, CancellationToken cancellation)
        {
            args.Custom = Request.Query.ToDictionary(e => e.Key, e => e.Value.FirstOrDefault());

            var service = GetService();
            var result = await service.MessageCommandPreviewEntity(id, templateId, args, cancellation);

            return Ok(result);
        }

        [HttpPut("{id:int}/message-entity/{templateId:int}")]
        public async Task<ActionResult> MessageEntity(int id, int templateId, [FromQuery] PrintEntityByIdArguments args, [FromQuery] string version, CancellationToken cancellation)
        {
            args.Custom = Request.Query.ToDictionary(e => e.Key, e => e.Value.FirstOrDefault());

            var service = GetService();
            int commandId = await service.SendByMessage(id, templateId, args, version, cancellation);

            return Ok(new IdResult { Id = commandId });
        }


        protected override CrudServiceBase<AgentForSave, Agent, int> GetCrudService()
        {
            return GetService();
        }

        private AgentsService GetService()
        {
            _service.SetDefinitionId(DefinitionId);
            return _service;
        }

        protected int DefinitionId => int.Parse(Request.RouteValues.GetValueOrDefault("definitionId").ToString());
    }

    [Route("api/agents")]
    [ApplicationController]
    public class AgentsGenericController : FactWithIdControllerBase<Agent, int>
    {
        private readonly AgentsGenericService _service;

        public AgentsGenericController(AgentsGenericService service)
        {
            _service = service;
        }

        protected override FactWithIdServiceBase<Agent, int> GetFactWithIdService()
        {
            return _service;
        }
    }
}