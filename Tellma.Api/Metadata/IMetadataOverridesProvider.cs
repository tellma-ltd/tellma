using System;
using Tellma.Model.Common;

namespace Tellma.Api.Metadata
{
    public interface IMetadataOverridesProvider
    {
        public EntityMetadataOverrides EntityOverrides(TypeDescriptor typeDesc, int? definitionId, Func<string> singularDisplay, Func<string> pluralDisplay);

        public PropertyMetadataOverrides PropertyOverrides(TypeDescriptor typeDesc, int? definitionId, PropertyDescriptor propDesc, Func<string> defaultDisplay);
    }
}
