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
    /// entity type using Queryex-style arguments.
    /// </summary>
    [AuthorizeJwtBearer]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public abstract class FactControllerBase<TEntity, TEntitiesResult> : ControllerBase
        where TEntitiesResult : EntitiesResult<TEntity>
        where TEntity : Entity
    {
        [HttpGet]
        public virtual async Task<ActionResult<GetResponse<TEntity>>> GetEntities([FromQuery] GetArguments args, CancellationToken cancellation)
        {
            // Calculate server time at the very beginning for consistency
            var serverTime = DateTimeOffset.UtcNow;

            // Retrieves the raw data from the database, unflattend, untrimmed 
            var service = GetFactService();
            var result = await service.GetEntities(args, cancellation);
            var data = result.Data;
            var count = result.Count;

            // Flatten and Trim
            var relatedEntities = FlattenAndTrim(data, cancellation);
            var extras = CreateExtras(result);

            // Prepare the result in a response object
            var response = new GetResponse<TEntity>
            {
                Skip = args.Skip,
                Top = data.Count,
                OrderBy = args.OrderBy,
                TotalCount = count,
                Result = data.ToList(),
                RelatedEntities = relatedEntities,
                CollectionName = ControllerUtilities.GetCollectionName(typeof(TEntity)),
                Extras = extras,
                ServerTime = serverTime
            };

            return Ok(response);
        }

        [HttpGet("fact")]
        public virtual async Task<ActionResult<GetFactResponse>> GetFact([FromQuery] FactArguments args, CancellationToken cancellation)
        {
            // Calculate server time at the very beginning for consistency
            var serverTime = DateTimeOffset.UtcNow;

            // Retrieves the raw data from the database, unflattend, untrimmed 
            var service = GetFactService();
            var result = await service.GetFact(args, cancellation);
            var data = result.Data;
            var count = result.Count;

            // Prepare the result in a response object
            var response = new GetFactResponse
            {
                ServerTime = serverTime,
                Result = data,
                TotalCount = count
            };

            return Ok(response);
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
            var result = await GetFactService().GetAggregate(args, cancellation);
            var data = result.Data;
            var ancestors = result.Ancestors.Select(e => new DimensionAncestors
            {
                IdIndex = e.IdIndex,
                MinIndex = e.MinIndex,
                Result = e.Data.ToList()
            });

            // Finally return the result
            var response = new GetAggregateResponse
            {
                ServerTime = serverTime,
                Result = data,
                DimensionAncestors = ancestors,
            };

            return Ok(response);
        }

        [HttpGet("print/{templateId}")]
        public async Task<ActionResult> PrintByFilter(int templateId, [FromQuery] PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            var service = GetFactService();
            var result = await service.PrintEntities(templateId, args, cancellation);

            var fileBytes = result.FileBytes;
            var fileName = result.FileName;
            var contentType = ControllerUtilities.ContentType(fileName);


            return File(fileContents: fileBytes, contentType: contentType, fileName);
        }

        protected abstract FactServiceBase<TEntity, TEntitiesResult> GetFactService();

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

        protected virtual Extras CreateExtras(TEntitiesResult result) => null;
    }

    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type using Queryex-style arguments.
    /// </summary>
    public abstract class FactControllerBase<TEntity> : FactControllerBase<TEntity, EntitiesResult<TEntity>>
        where TEntity : Entity
    {
    }
}
