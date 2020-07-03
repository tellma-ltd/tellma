using System.Collections.Generic;

namespace Tellma.Entities
{
    /// <summary>
    /// The datatype for the entity metadata that is attached to every <see cref="Entity"/>, 
    /// the metadata maps every property name in the DTO to whether it is loaded or restricted
    /// </summary>
    public class EntityMetadata : Dictionary<string, FieldMetadata>
    {
        /// <summary>
        /// Returns true if the given property name is marked as loaded in this <see cref="EntityMetadata"/>
        /// </summary>
        /// <param name="propName">The name of the property to inspect</param>
        /// <returns>True if the given property name is marked as loaded in this <see cref="EntityMetadata"/>, false otherwise.</returns>
        public bool IsLoaded(string propName) => propName == "Id" || TryGetValue(propName, out FieldMetadata meta) && meta == FieldMetadata.Loaded;

        /// <summary>
        /// Used for import/export to map entities to the row they came from.
        /// Since the containing entity is a dictionary, JSON.NET will not serialize this value
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// Stores the code or name of the parent, used by the import logic when
        /// an entity's user key may refer to another entity in the imported list
        /// </summary>
        public object ParentUserKey { get; set; }

        /// <summary>
        /// Used by the import logic
        /// </summary>
        public IEnumerable<EntityWithKey> ParentMatches { get; set; }

        /// <summary>
        /// Used by the flatten and trim logic to remember which entities have already been flattened and trimmed
        /// </summary>
        public bool FlattenedAndTrimmed { get; set; }

        /// <summary>
        /// Used to store any error message when parsing <see cref="ILocationEntityForSave.LocationJson"/>
        /// </summary>
        public string LocationJsonParseError { get; set; }
    }
}
