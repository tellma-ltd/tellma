using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
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
using Tellma.Model.Common;

namespace Tellma.Controllers
{
    [Route("api/documents/{definitionId:int}")]
    [ApplicationController]
    public class DocumentsController : CrudControllerBase<DocumentForSave, Document, int>
    {
        private readonly DocumentsService _service;

        public DocumentsController(DocumentsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpGet("{docId}/attachments/{attachmentId}")]
        public async Task<ActionResult> GetAttachment(int docId, int attachmentId, CancellationToken cancellation)
        {
            var (fileBytes, fileName) = await GetService().GetAttachment(docId, attachmentId, cancellation);
            var contentType = ControllerUtilities.ContentType(fileName);
            return File(fileContents: fileBytes, contentType: contentType, fileName);

        }

        [HttpPut("assign")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Assign([FromBody] List<int> ids, [FromQuery] AssignArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await GetService().Assign(ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            if (args.ReturnEntities ?? false)
            {
                return Ok(response);
            }
            else
            {
                return Ok();
            }
        }

        [HttpPut("update-assignment")]
        public async Task<ActionResult<GetByIdResponse<Document>>> UpdateAssignment([FromQuery] UpdateAssignmentArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (entity, extras) = await GetService().UpdateAssignment(args);

            if (args.ReturnEntities ?? false)
            {
                // Flatten and Trim
                var singleton = new List<Document> { entity };
                var relatedEntities = FlattenAndTrim(singleton, cancellation: default);

                // Prepare the result in a response object
                var response = new GetByIdResponse<Document>
                {
                    Result = entity,
                    RelatedEntities = relatedEntities,
                    CollectionName = ControllerUtilities.GetCollectionName(typeof(Document)),
                    Extras = TransformExtras(extras, cancellation: default),
                    ServerTime = serverTime,
                };

                return Ok(response);
            }
            else
            {
                return Ok();
            }
        }

        [HttpPut("sign-lines")]
        public async Task<ActionResult<EntitiesResponse<Document>>> SignLines([FromBody] List<int> lineIds, [FromQuery] SignArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await GetService().SignLines(lineIds, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            if (args.ReturnEntities ?? false)
            {
                return Ok(response);
            }
            else
            {
                return Ok();
            }
        }

        [HttpPut("unsign-lines")]
        public async Task<ActionResult<EntitiesResponse<Document>>> UnsignLines([FromBody] List<int> signatureIds, [FromQuery] ActionArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await GetService().UnsignLines(signatureIds, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            if (args.ReturnEntities ?? false)
            {
                return Ok(response);
            }
            else
            {
                return Ok();
            }
        }

        [HttpPut("close")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Close([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await GetService().Close(ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            if (args.ReturnEntities ?? false)
            {
                return Ok(response);
            }
            else
            {
                return Ok();
            }
        }

        [HttpPut("open")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Open([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await GetService().Open(ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            if (args.ReturnEntities ?? false)
            {
                return Ok(response);
            }
            else
            {
                return Ok();
            }
        }

        [HttpPut("cancel")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Cancel([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await GetService().Cancel(ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            if (args.ReturnEntities ?? false)
            {
                return Ok(response);
            }
            else
            {
                return Ok();
            }
        }

        [HttpPut("uncancel")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Uncancel([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (data, extras) = await GetService().Uncancel(ids, args);
            var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

            if (args.ReturnEntities ?? false)
            {
                return Ok(response);
            }
            else
            {
                return Ok();
            }
        }

        [HttpGet("generate-lines/{lineDefId}")]
        public async Task<ActionResult<EntitiesResponse<LineForSave>>> Generate([FromRoute] int lineDefId, [FromQuery] Dictionary<string, string> args, CancellationToken cancellation)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var (lines, accounts, resources, relations, entryTypes, centers, currencies, units) = await GetService().Generate(lineDefId, args, cancellation);

            // Related entitiess
            var relatedEntities = new Dictionary<string, IEnumerable<Entity>>
                {
                    { ControllerUtilities.GetCollectionName(typeof(Account)), accounts },
                    { ControllerUtilities.GetCollectionName(typeof(Resource)), resources },
                    { ControllerUtilities.GetCollectionName(typeof(Relation)), relations },
                    { ControllerUtilities.GetCollectionName(typeof(EntryType)), entryTypes },
                    { ControllerUtilities.GetCollectionName(typeof(Center)), centers },
                    { ControllerUtilities.GetCollectionName(typeof(Currency)), currencies },
                    { ControllerUtilities.GetCollectionName(typeof(Unit)), units }
                };

            // Prepare the result in a response object
            var response = new EntitiesResponse<LineForSave>
            {
                Result = lines,
                RelatedEntities = relatedEntities,
                CollectionName = "", // Not important
                ServerTime = serverTime,
            };

            // Return
            return Ok(response);
        }

        protected override CrudServiceBase<DocumentForSave, Document, int> GetCrudService()
        {
            _service.SetDefinitionId(DefinitionId);
            _service.SetIncludeRequiredSignatures(IncludeRequiredSignatures());

            return _service;
        }

        private DocumentsService GetService()
        {
            _service.SetDefinitionId(DefinitionId);
            _service.SetIncludeRequiredSignatures(IncludeRequiredSignatures());

            return _service;
        }

        protected int DefinitionId => int.Parse(Request.RouteValues.GetValueOrDefault("definitionId").ToString());

        private bool IncludeRequiredSignatures()
        {
            const string paramName = "includeRequiredSignatures";

            return Request.Query.TryGetValue(paramName, out StringValues value) 
                && value.FirstOrDefault()?.ToLower() == "true";
        }

        protected override Extras TransformExtras(Extras extras, CancellationToken cancellation)
        {
            if (extras != null && extras.TryGetValue("RequiredSignatures", out object requiredSignaturesObj))
            {
                var requiredSignatures = requiredSignaturesObj as List<RequiredSignature>;

                var relatedEntities = FlattenAndTrim(requiredSignatures, cancellation);
                requiredSignatures.ForEach(rs => rs.EntityMetadata = null); // Smaller response size

                extras["RequiredSignaturesRelatedEntities"] = relatedEntities;
            }

            return extras;
        }
    }

    [Route("api/documents")]
    [ApplicationController]
    public class DocumentsGenericController : FactWithIdControllerBase<Document, int>
    {
        private readonly DocumentsGenericService _service;

        public DocumentsGenericController(DocumentsGenericService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override FactWithIdServiceBase<Document, int> GetFactWithIdService()
        {
            return _service;
        }
    }
}