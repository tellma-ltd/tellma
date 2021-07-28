using System;
using Tellma.Model.Common;

namespace Tellma.Api.Metadata
{
    public interface IMetadataOverridesProvider
    {
        public EntityMetadataOverrides EntityOverrides(TypeDescriptor typeDesc, int? definitionId, Func<string> singularDisplay, Func<string> pluralDisplay);

        /// <summary>
        /// Implementation can override (1) the display label of the property, (2) make a non-required
        /// property required or (3) specify the definition Id of the target of a navigation property.
        /// </summary>
        /// <param name="typeDesc">The <see cref="TypeDescriptor"/> of the type that defines the property-to-override.</param>
        /// <param name="definitionId">The definition Id of the entity where the property resides.</param>
        /// <param name="propDesc">The <see cref="PropertyDescriptor"/> of the property to override.</param>
        /// <param name="defaultDisplay">The default display function (localizing) as determined by the property attributes.</param>
        /// <returns>The <see cref="PropertyMetadataOverrides"/> that specifies what to override. Or null if there is nothing to override.</returns>
        /// <remarks>
        /// - If you don't want to override anything return null. <br/>
        /// - To override the display function, return a result with a set <see cref="PropertyMetadataOverrides.Display"/>. <br/>
        /// - If you want to remove the property completely, return a result with <see cref="PropertyMetadataOverrides.Display"/> = null. <br/>
        /// - To specify a property as required, return a result with <see cref="PropertyMetadataOverrides.IsRequired"/> = true. If it's false it will be ignored, ie properties determined to be required from their attributes will always be required. <br/>
        /// </remarks>
        public PropertyMetadataOverrides PropertyOverrides(TypeDescriptor typeDesc, int? definitionId, PropertyDescriptor propDesc, Func<string> defaultDisplay);
    }
}
