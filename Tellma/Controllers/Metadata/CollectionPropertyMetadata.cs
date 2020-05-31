using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Tellma.Entities;
using Tellma.Entities.Descriptors;

namespace Tellma.Controllers
{
    /// <summary>
    /// Derives from <see cref="PropertyMetadata"/> and provides additional metadata
    /// that are specific to collection navigation properties
    /// </summary>
    public class CollectionPropertyMetadata : PropertyMetadata
    {
        private TypeMetadata _collectionEntityDescriptor; // Caching
        private readonly Func<TypeMetadata> _getCollectionTargetTypeMetadata;

        /// <summary>
        /// The <see cref="TypeMetadata"/> of the target type of the collection
        /// </summary>
        public TypeMetadata CollectionTargetTypeMetadata => _collectionEntityDescriptor ??= _getCollectionTargetTypeMetadata();

        /// <summary>
        /// Constructor
        /// </summary>
        public CollectionPropertyMetadata(
            CollectionPropertyDescriptor desc,
            Func<string> display,
            Func<Entity, object, IEnumerable<ValidationResult>> validate,
            Func<TypeMetadata> getCollectionTargetTypeMetadata) : base(desc, display, validate, null, null)
        {
            _getCollectionTargetTypeMetadata = getCollectionTargetTypeMetadata;
        }
    }
}
