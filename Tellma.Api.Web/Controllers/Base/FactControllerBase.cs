using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Model.Common;
using Tellma.Services.ApiAuthentication;

namespace Tellma.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type using OData-like parameters.
    /// </summary>
    [AuthorizeJwtBearer]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public abstract class FactControllerBase<TEntity> : ControllerBase
        where TEntity : Entity
    {
        public FactControllerBase(IServiceProvider _)
        {
        }

        [HttpGet]
        public virtual async Task<ActionResult<GetResponse<TEntity>>> GetEntities([FromQuery] GetArguments args, CancellationToken cancellation)
        {
            // Calculate server time at the very beginning for consistency
            var serverTime = DateTimeOffset.UtcNow;

            // Retrieves the raw data from the database, unflattend, untrimmed 
            var service = GetFactService();
            var (data, extras, totalCount) = await service.GetEntities(args, cancellation);

            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(data, cancellation);

            // Transform extras
            var transformedExtras = TransformExtras(extras, cancellation);

            // Prepare the result in a response object
            var result = new GetResponse<TEntity>
            {
                Skip = args.Skip,
                Top = data.Count,
                OrderBy = args.OrderBy,
                TotalCount = totalCount,
                Result = data,
                RelatedEntities = relatedEntities,
                CollectionName = ControllerUtilities.GetCollectionName(typeof(TEntity)),
                Extras = transformedExtras,
                ServerTime = serverTime
            };

            return Ok(result);
        }

        [HttpGet("fact")]
        public virtual async Task<ActionResult<GetFactResponse>> GetFact([FromQuery] GetArguments args, CancellationToken cancellation)
        {
            // Calculate server time at the very beginning for consistency
            var serverTime = DateTimeOffset.UtcNow;

            // Retrieves the raw data from the database, unflattend, untrimmed 
            var service = GetFactService();
            var (data, count) = await service.GetFact(args, cancellation);

            // Prepare the result in a response object
            var result = new GetFactResponse
            {
                ServerTime = serverTime,
                Result = data,
                TotalCount = count
            };

            return Ok(result);
        }

        [HttpGet("aggregate")]
        public virtual async Task<ActionResult<GetAggregateResponse>> GetAggregate([FromQuery] GetAggregateArguments args, CancellationToken cancellation)
        {
            // Calculate server time at the very beginning for consistency
            var serverTime = DateTimeOffset.UtcNow;

            // Sometimes select is so huge that it is passed as a header instead
            if (string.IsNullOrWhiteSpace(args.Select))
            {
                args.Select = Request.Headers["X-Select"].FirstOrDefault();
            }

            // Load the data
            var (data, ancestors) = await GetFactService().GetAggregate(args, cancellation);

            // Finally return the result
            var result = new GetAggregateResponse
            {
                ServerTime = serverTime,

                Result = data,
                DimensionAncestors = ancestors,
            };

            return Ok(result);
        }

        [HttpGet("print/{templateId}")]
        public async Task<ActionResult> PrintByFilter(int templateId, [FromQuery] PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            var service = GetFactService();
            var (fileBytes, fileName) = await service.PrintEntities(templateId, args, cancellation);
            var contentType = ControllerUtilities.ContentType(fileName);

            return File(fileContents: fileBytes, contentType: contentType, fileName);
        }

        protected abstract FactServiceBase<TEntity> GetFactService();

        /// <summary>
        /// Takes a list of <see cref="Entity"/>, and for every entity it inspects the navigation properties, if a navigation property
        /// contains an <see cref="Entity"/> with a strong type, it sets that property to null, and moves the strong entity into a separate
        /// "relatedEntities" hash set, this has several advantages: <br/>
        /// 1 - JSON.NET will not have to deal with circular references <br/>
        /// 2 - Every strong entity is mentioned once in the JSON response (smaller response size) <br/>
        /// 3 - It makes it easier for clients to store and track entities in a central workspace <br/>
        /// </summary>
        /// <returns>A hash set of strong related entity in the original result entities (excluding the result entities).</returns>
        protected Dictionary<string, IEnumerable<Entity>> FlattenAndTrim<T>(IEnumerable<T> resultEntities, CancellationToken cancellation)
            where T : Entity
        {
            return ControllerUtilities.FlattenAndTrim(resultEntities, cancellation);
        }

        protected virtual Extras TransformExtras(Extras extras, CancellationToken cancellation)
        {
            return extras;
        }
    }
}
