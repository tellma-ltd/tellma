using System.Collections.Generic;

namespace Tellma.Entities
{
    /// <summary>
    /// The datatype for the entity metadata that is attached to every <see cref="Entity"/>, 
    /// the metadata maps every property name in the DTO to whether it is loaded or restricted
    /// </summary>
    public class EntityMetadata : Dictionary<string, FieldMetadata>
    {
        public const string ALL_FIELDS_KEYWORD = "AllFields";
    }
}
