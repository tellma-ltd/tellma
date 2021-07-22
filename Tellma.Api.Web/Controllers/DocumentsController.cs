﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Controllers.Dto;
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
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var (fileBytes, fileName) = await _service.GetAttachment(docId, attachmentId, cancellation);
                var contentType = ControllerUtilities.ContentType(fileName);
                return File(fileContents: fileBytes, contentType: contentType, fileName);
            }, _logger);
        }

        [HttpPut("assign")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Assign([FromBody] List<int> ids, [FromQuery] AssignArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Assign(ids, args);
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
            , _logger);
        }

        [HttpPut("sign-lines")]
        public async Task<ActionResult<EntitiesResponse<Document>>> SignLines([FromBody] List<int> lineIds, [FromQuery] SignArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.SignLines(lineIds, args);
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
            , _logger);
        }

        [HttpPut("unsign-lines")]
        public async Task<ActionResult<EntitiesResponse<Document>>> UnsignLines([FromBody] List<int> signatureIds, [FromQuery] ActionArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.UnsignLines(signatureIds, args);
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
            , _logger);
        }

        [HttpPut("close")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Close([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Close(ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

                if (args.ReturnEntities ?? false)
                {
                    return Ok(response);
                }
                else
                {
                    return Ok();
                }
            }, _logger);
        }

        [HttpPut("open")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Open([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Open(ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

                if (args.ReturnEntities ?? false)
                {
                    return Ok(response);
                }
                else
                {
                    return Ok();
                }
            }, _logger);
        }

        [HttpPut("cancel")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Cancel([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Cancel(ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

                if (args.ReturnEntities ?? false)
                {
                    return Ok(response);
                }
                else
                {
                    return Ok();
                }
            }, _logger);
        }

        [HttpPut("uncancel")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Uncancel([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Uncancel(ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

                if (args.ReturnEntities ?? false)
                {
                    return Ok(response);
                }
                else
                {
                    return Ok();
                }
            }, _logger);
        }

        [HttpGet("generate-lines/{lineDefId}")]
        public async Task<ActionResult<EntitiesResponse<LineForSave>>> Generate([FromRoute] int lineDefId, [FromQuery] Dictionary<string, string> args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (lines, accounts, resources, relations, entryTypes, centers, currencies, units) = await _service.Generate(lineDefId, args, cancellation);

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
            }, _logger);
        }

        protected override CrudServiceBase<DocumentForSave, Document, int> GetCrudService()
        {
            _service.SetDefinitionId(DefinitionId);
            _service.SetIncludeRequiredSignatures(IncludeRequiredSignatures());

            return _service;
        }

        private int DefinitionId => int.Parse(Request.RouteValues.GetValueOrDefault("definitionId").ToString());
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