using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Model.Common;

namespace Tellma.Api.ImportExport
{
    /// <summary>
    /// An implementation of this interface must be provided by the consumer of <see cref="DataParser"/> 
    /// to allow it to populate foreign keys from the user keys. E.g. populate ResourceId column from codes.
    /// </summary>
    public interface IApiClientForImport
    {
        /// <summary>
        /// Invokes the API that retrieves a list of entities based on the <paramref name="values"/> of a user key 
        /// property specified by <paramref name="propName"/>. The returned entities will only have two properties 
        /// hydrated: Id and the user key property.
        /// </summary>
        /// <param name="collection">The collection from which to get the entities.</param>
        /// <param name="definitionId">The definition Id of the entities.</param>
        /// <param name="propName">The name of the specific property whose values we are using to retrieve the entities.</param>
        /// <param name="values">Retrieve all entities whose specific property has a value in this collection.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>A list of <see cref="EntityWithKey"/> whose user key property has one of the given <paramref name="values"/>.</returns>
        Task<IList<EntityWithKey>> GetEntitiesByPropertyValues(string collection, int? definitionId, string propName, IEnumerable<object> values, CancellationToken cancellation);
    }
}
