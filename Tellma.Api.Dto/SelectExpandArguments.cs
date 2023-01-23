using System;

namespace Tellma.Api.Dto
{
    /// <summary>
    /// Specifies which properties to hydrate on the returned entity or entities.
    /// </summary>
    /// <remarks>
    /// Almost all API handlers that return an entity or entities will expect arguments 
    /// that contain "select" and "expand" to specify what should be returned. All such 
    /// argument DTOs inherit from <see cref="SelectExpandArguments"/>.
    /// </remarks>
    public class SelectExpandArguments
    {
        /// <summary>
        /// A comma-separated list of simple property paths to hydrate on the returned principal entities
        /// or their related entities. Example: 
        ///     <code>"Name,Code,RelatedEntity.Name,RelatedCollection.Name"</code>
        /// The Id of the principal entities an any related entities is always selected.
        /// </summary>
        /// <remarks>
        /// This is analogous to LINQ's "Select". If left empty then all simple properties will be hydrated
        /// on the principal entity or entities as well as all the related entities specified in <see cref="Expand"/>. <br/>
        /// The <see cref="Select"/> argument may contain special "shorthands" that are understood by
        /// the API service and expanded into a proper select list, useful in scenarios where the required
        /// select parameter is enormous and unweildly yet very common.
        /// </remarks>
        public string Select { get; set; }

        /// <summary>
        /// A comma separated list of navigation property paths to hydrate with related entities. Example:
        /// <code>"RelatedEntity,RelatedCollection"</code>
        /// </summary>
        /// <remarks>
        /// This is analogous to LINQ's "Include". If left empty then only the principal entities are
        /// returned without any related entities. A <see cref="Select"/> path takes priority over an
        /// <see cref="Expand"/> path in the case of intersection so it is recommended to use one or
        /// the other.
        /// </remarks>
        public string Expand { get; set; }

        /// <summary>
        /// The time to return from the now() queryex function.
        /// </summary>
        public DateTimeOffset? Now { get; set; }
    }
}
