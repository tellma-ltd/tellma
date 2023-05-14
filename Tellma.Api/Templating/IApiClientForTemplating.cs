using DocumentFormat.OpenXml.Office2013.Excel;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Common;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// An implementation of this interface must be provided by the consumer of <see cref="TemplateService"/> 
    /// to allow it to evaluate template expressions that invoke API queries.
    /// </summary>
    public interface IApiClientForTemplating
    {
        /// <summary>
        /// Invokes the API that retrieves a list of entities based on Queryex-style query arguments.
        /// </summary>
        /// <param name="collection">The collection from which to get the entities.</param>
        /// <param name="definitionId">The definition Id of the entities.</param>
        /// <param name="select">The properties to include from the entities and their related entities.</param>
        /// <param name="filter">The filter to apply to the entities.</param>
        /// <param name="orderby">How to order the entities.</param>
        /// <param name="top">How many entities to return.</param>
        /// <param name="skip">How many entities to skip from the beginning of the query.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>A list of <see cref="Entity"/> based on the arguments.</returns>
        Task<IReadOnlyList<Entity>> GetEntities(string collection, int? definitionId, string select, string filter, string orderby, int? top, int? skip, DateTimeOffset? now, CancellationToken cancellation);

        /// <summary>
        /// Invokes the API that retrieves a list of entities based on a list of Ids.
        /// </summary>
        /// <param name="collection">The collection from which to get the entities.</param>
        /// <param name="definitionId">The definition Id of the entities.</param>
        /// <param name="select">The properties to include from the entities and their related entities.</param>
        /// <param name="ids">The ids of the entities to return, in the order to return the entities in.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>A list of <see cref="EntityWithKey"/> based on the provided list of Ids.</returns>
        Task<IReadOnlyList<EntityWithKey>> GetEntitiesByIds(string collection, int? definitionId, string select, IList ids, DateTimeOffset? now, CancellationToken cancellation);

        /// <summary>
        /// Invokes the API that retrieves a single entity based on an Id.
        /// </summary>
        /// <param name="collection">The collection from which to get the entity.</param>
        /// <param name="definitionId">The definition Id of the entity.</param>
        /// <param name="select">The properties to include from the entity and its related entities.</param>
        /// <param name="id">The id of the entity to return.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>A single <see cref="EntityWithKey"/> which has the provided Id value.</returns>
        Task<EntityWithKey> GetEntityById(string collection, int? definitionId, string select, object id, DateTimeOffset? now, CancellationToken cancellation);

        /// <summary>
        /// Invokes the API that retrieves a list of dynamic rows based on Queryex-style query arguments.
        /// </summary>
        /// <param name="collection">The root collection of the dynamic query.</param>
        /// <param name="definitionId">The root definition Id of the dynamic query.</param>
        /// <param name="select">The expressions to load in the dynamic rows.</param>
        /// <param name="filter">The filter to apply to the dynamic query.</param>
        /// <param name="orderby">How to order the dynamic rows.</param>
        /// <param name="top">How many dynamic rows to return.</param>
        /// <param name="skip">How many dynamic rows to skip from the beginning of the query.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>A list of <see cref="DynamicRow"/>s based on the arguments.</returns>
        Task<IReadOnlyList<DynamicRow>> GetFact(string collection, int? definitionId, string select, string filter, string orderby, int? top, int? skip, DateTimeOffset? now, CancellationToken cancellation);

        /// <summary>
        /// Invokes the API that retrieves an aggregated list of dynamic rows based on Queryex-style query arguments.
        /// </summary>
        /// <param name="collection">The root collection of the dynamic query.</param>
        /// <param name="definitionId">The root definition Id of the dynamic query.</param>
        /// <param name="select">The expressions (both aggregated and non-aggregated) to load in the dynamic rows.</param>
        /// <param name="filter">The filter to apply to the dynamic query.</param>
        /// <param name="having">The filter to apply to the dynamic query after aggregation.</param>
        /// <param name="orderby">How to order the dynamic rows.</param>
        /// <param name="top">How many dynamic rows to return.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>A list of aggregated <see cref="DynamicRow"/>s based on the arguments.</returns>
        Task<IReadOnlyList<DynamicRow>> GetAggregate(string collection, int? definitionId, string select, string filter, string having, string orderby, int? top, DateTimeOffset? now, CancellationToken cancellation);

        /// <summary>
        /// Invokes the API that retrieves the image of the entity with the given collection, definitionId, and Id.
        /// </summary>
        /// <param name="collection">The collection from which to get the entity.</param>
        /// <param name="definitionId">The definition Id of the entity.</param>
        /// <param name="id">The id of the entity whose image to retrieve.</param>
        /// <returns>The <see cref="ImageResult"/> of the entity with the given collection, definitionId and Id.</returns>
        Task<ImageResult> GetImage(string collection, int? definitionId, int id, CancellationToken cancellation);
    }
}
