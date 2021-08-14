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
    public class RelationsController : CrudControllerBase<RelationForSave, Relation, int>
    {
        private readonly RelationsService _service;

        public RelationsController(RelationsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpGet("{id}/image")]
        public async Task<ActionResult> GetImage(int id, CancellationToken cancellation)
        {
            var (imageId, imageBytes) = await GetService().GetImage(id, cancellation);
            Response.Headers.Add("x-image-id", imageId);
            return File(imageBytes, MimeTypes.Jpeg);
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Relation>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await GetService().Activate(ids: ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            return Ok(response);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Relation>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await GetService().Deactivate(ids: ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            return Ok(response);
        }

        [HttpGet("{docId}/attachments/{attachmentId}")]
        public async Task<ActionResult> GetAttachment(int docId, int attachmentId, CancellationToken cancellation)
        {
            var (fileBytes, fileName) = await GetService().GetAttachment(docId, attachmentId, cancellation);
            var contentType = ControllerUtilities.ContentType(fileName);

            return File(fileContents: fileBytes, contentType: contentType, fileName);
        }

        protected override CrudServiceBase<RelationForSave, Relation, int> GetCrudService()
        {
            _service.SetDefinitionId(DefinitionId);
            return _service;
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

        public RelationsGenericController(RelationsGenericService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override FactWithIdServiceBase<Relation, int> GetFactWithIdService()
        {
            return _service;
        }
    }
}