using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Model.Common
{
    /// <summary>
    /// All entities in the model derive from this base class.
    /// </summary>
    public class Entity
    {
        private EntityMetadata _entityMetadata;

        /// <summary>
        /// Contains metadata about the entity for client side consumption
        /// </summary>
        [NotMapped]
        public EntityMetadata EntityMetadata
        {
            get { return _entityMetadata ??= new EntityMetadata(); }
            set { _entityMetadata = value; }
        }
    }
}
