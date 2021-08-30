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
    [ApiVersion("1.0")]
    public class DocumentsController : CrudControllerBase<DocumentForSave, Document, int, DocumentsResult, DocumentResult>
    {
        private readonly DocumentsService _service;

        public DocumentsController(DocumentsService service)
        {
            _service = service;
        }

        [HttpGet("{docId}/attachments/{attachmentId}")]
        public async Task<ActionResult> GetAttachment(int docId, int attachmentId, CancellationToken cancellation)
        {
            var result = await GetService().GetAttachment(docId, attachmentId, cancellation);
            var contentType = ControllerUtilities.ContentType(result.FileName);

            return File(fileContents: result.FileBytes, contentType: contentType, result.FileName);
        }

        [HttpPut("assign")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Assign([FromBody] List<int> ids, [FromQuery] AssignArguments args)
        {
            var serverTime = DateTimeOffset.UtcNow;
            var result = await GetService().Assign(ids, args);

            if (args.ReturnEntities ?? false)
            {
                var response = TransformToEntitiesResponse(result, serverTime, cancellation: default);
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
            var result = await GetService().UpdateAssignment(args);

            if (args.ReturnEntities ?? false)
            {
                var entity = result.Entity;

                // Flatten and Trim
                var singleton = new List<Document> { entity };
                var relatedEntities = Flatten(singleton, cancellation: default);

                // Prepare the result in a response object
                var response = new GetByIdResponse<Document>
                {
                    Result = entity,
                    RelatedEntities = relatedEntities,
                    CollectionName = ControllerUtilities.GetCollectionName(typeof(Document)),
                    Extras = CreateExtras(result),
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
            var result = await GetService().SignLines(lineIds, args);

            if (args.ReturnEntities ?? false)
            {
                var response = TransformToEntitiesResponse(result, serverTime, cancellation: default);
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
            var result = await GetService().UnsignLines(signatureIds, args);

            if (args.ReturnEntities ?? false)
            {
                var response = TransformToEntitiesResponse(result, serverTime, cancellation: default);
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
            var result = await GetService().Close(ids, args);

            if (args.ReturnEntities ?? false)
            {
                var response = TransformToEntitiesResponse(result, serverTime, cancellation: default);
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
            var result = await GetService().Open(ids, args);

            if (args.ReturnEntities ?? false)
            {
                var response = TransformToEntitiesResponse(result, serverTime, cancellation: default);
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
            var result = await GetService().Cancel(ids, args);

            if (args.ReturnEntities ?? false)
            {
                var response = TransformToEntitiesResponse(result, serverTime, cancellation: default);
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
            var result = await GetService().Uncancel(ids, args);

            if (args.ReturnEntities ?? false)
            {
                var response = TransformToEntitiesResponse(result, serverTime, cancellation: default);
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
            var (lines, accounts, resources, agents, entryTypes, centers, currencies, units) = await GetService().Generate(lineDefId, args, cancellation);

            // Related entitiess
            var relatedEntities = new Dictionary<string, IEnumerable<EntityWithKey>>
                {
                    { ControllerUtilities.GetCollectionName(typeof(Account)), accounts },
                    { ControllerUtilities.GetCollectionName(typeof(Resource)), resources },
                    { ControllerUtilities.GetCollectionName(typeof(Agent)), agents },
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

        protected override CrudServiceBase<DocumentForSave, Document, int, DocumentsResult, DocumentResult> GetCrudService()
        {
            return GetService();
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

        protected override Extras CreateExtras(DocumentResult result)
        {
            return CreateExtras(result.RequiredSignatures);
        }

        protected override Extras CreateExtras(DocumentsResult result)
        {
            return CreateExtras(result.RequiredSignatures);
        }

        protected Extras CreateExtras(IReadOnlyList<RequiredSignature> requiredSignatures)
        {
            if (requiredSignatures == null)
            {
                return null;
            }
            else
            {
                var extras = new Extras();

                var relatedEntities = Flatten(requiredSignatures, cancellation: default);
                foreach (var rs in requiredSignatures)
                {
                    rs.EntityMetadata = null; // Smaller response size
                }

                extras["RequiredSignatures"] = requiredSignatures;
                extras["RequiredSignaturesRelatedEntities"] = relatedEntities;

                return extras;
            }
        }
    }

    [Route("api/documents")]
    [ApplicationController]
    public class DocumentsGenericController : FactWithIdControllerBase<Document, int>
    {
        private readonly DocumentsGenericService _service;

        public DocumentsGenericController(DocumentsGenericService service)
        {
            _service = service;
        }

        protected override FactWithIdServiceBase<Document, int> GetFactWithIdService()
        {
            return _service;
        }
    }
}