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
    }
}
