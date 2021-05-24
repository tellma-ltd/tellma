using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Api
{
    /// <summary>
    /// Derives from <see cref="PropertyMetadata"/> and provides additional metadata
    /// that are specific to navigation properties.
    /// </summary>
    public class NavigationPropertyMetadata : PropertyMetadata
    {
        private TypeMetadata _entityMetadata; // Caching
        private readonly Func<TypeMetadata> _getTargetTypeMetadata;

        /// <summary>
        /// The <see cref="PropertyMetadata"/> of the foreign key associated with this navigation property.
        /// it should be another simple property in the same <see cref="TypeMetadata"/>.
        /// </summary>
        public PropertyMetadata ForeignKey { get; }

        /// <summary>
        /// The <see cref="TypeMetadata"/> of the target type of the navigation.
        /// </summary>
        public TypeMetadata TargetTypeMetadata => _entityMetadata ??= _getTargetTypeMetadata();

        /// <summary>
        /// Constructor.
        /// </summary>
        public NavigationPropertyMetadata(
            NavigationPropertyDescriptor desc,
            Func<string> display,
            Func<Entity, object, IEnumerable<ValidationResult>> validate,
            PropertyMetadata foreignKey,
            Func<TypeMetadata> getTargetTypeMetadata) : base(desc, display, validate, null, null)
        {
            _getTargetTypeMetadata = getTargetTypeMetadata;
            ForeignKey = foreignKey;
        }
    }
}
