using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    /// <summary>
    /// All entities in the <see cref="Entities"/> derive from this base class
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Contains metadata about the entity for client side consumption
        /// </summary>
        [NotMapped]
        public EntityMetadata EntityMetadata { get; set; } = new EntityMetadata();
    }
}
