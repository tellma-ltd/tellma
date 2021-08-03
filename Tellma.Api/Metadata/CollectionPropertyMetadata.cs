using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Api.Metadata
{
    /// <summary>
    /// Derives from <see cref="PropertyMetadata"/> and provides additional metadata
    /// that are specific to collection navigation properties.
    /// </summary>
    public class CollectionPropertyMetadata : PropertyMetadata
    {
        private TypeMetadata _collectionEntityDescriptor; // Caching
        private readonly Func<TypeMetadata> _getCollectionTargetTypeMetadata;

        /// <summary>
        /// The <see cref="TypeMetadata"/> of the target type of the collection.
        /// </summary>
        public TypeMetadata CollectionTargetTypeMetadata => _collectionEntityDescriptor ??= _getCollectionTargetTypeMetadata();

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionPropertyMetadata"/> class.
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
