using System;
using System.Reflection;

namespace Tellma.Entities.Descriptors
{
    /// <summary>
    /// Describes a collection navigation property that points to an <see cref="Entity"/> list
    /// </summary>
    public class CollectionPropertyDescriptor : PropertyDescriptor
    {
        private TypeDescriptor _collectionTypeDescriptor; // Caching
        private readonly Func<TypeDescriptor> _getCollectionTypeDescriptor;

        /// <summary>
        /// The name of the foreign key on the target type
        /// </summary>
        public string ForeignKeyName { get; set; }

        /// <summary>
        /// The <see cref="TypeDescriptor"/> of the type of entities that are held in this list
        /// </summary>
        public TypeDescriptor CollectionTypeDescriptor => _collectionTypeDescriptor ??= _getCollectionTypeDescriptor();

        /// <summary>
        /// Constructor
        /// </summary>
        public CollectionPropertyDescriptor(
            PropertyInfo propInfo,
            string name,
            Action<Entity, object> setter,
            Func<Entity, object> getter,
            string foreignKeyName,
            Func<TypeDescriptor> getCollectionTypeDescriptor) : base(propInfo, name, setter, getter)
        {
            ForeignKeyName = foreignKeyName;
            _getCollectionTypeDescriptor = getCollectionTypeDescriptor;
        }
    }
}
