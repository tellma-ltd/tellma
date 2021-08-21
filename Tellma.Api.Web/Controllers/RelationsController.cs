using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Application;

namespace Tellma.Controllers
{
    [Route("api/relations/{definitionId}")]
    [ApplicationController]
    [ApiVersion("1.0")]
    public class RelationsController : CrudControllerBase<RelationForSave, Relation, int>
    {
        private readonly RelationsService _service;

        public RelationsController(RelationsService service)
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
        public async Task<ActionResult<EntitiesResponse<Relation>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var result = await GetService().Activate(ids: ids, args);
            var response = TransformToEntitiesResponse(result, serverTime, cancellation: default);

            return Ok(response);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Relation>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
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

        protected override CrudServiceBase<RelationForSave, Relation, int> GetCrudService()
        {
            return GetService();
        }

        private RelationsService GetService()
        {
            _service.SetDefinitionId(DefinitionId);
            return _service;
        }

        protected int DefinitionId => int.Parse(Request.RouteValues.GetValueOrDefault("definitionId").ToString());
    }

    [Route("api/relations")]
    [ApplicationController]
    public class RelationsGenericController : FactWithIdControllerBase<Relation, int>
    {
        private readonly RelationsGenericService _service;

        public RelationsGenericController(RelationsGenericService service)
        {
            _service = service;
        }

        protected override FactWithIdServiceBase<Relation, int> GetFactWithIdService()
        {
            return _service;
        }
    }
}