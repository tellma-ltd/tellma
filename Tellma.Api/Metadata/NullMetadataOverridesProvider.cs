using System;
using Tellma.Model.Common;

namespace Tellma.Api.Metadata
{
    public class NullMetadataOverridesProvider : IMetadataOverridesProvider
    {
        public EntityMetadataOverrides EntityOverrides(TypeDescriptor typeDesc, int? definitionId, Func<string> defaultSingularDisplay, Func<string> defaultSingularDisplay)
        {
            return null;
        }

        public PropertyMetadataOverrides PropertyOverrides(TypeDescriptor typeDesc, int? definitionId, PropertyDescriptor propDesc, Func<string> defaultDisplay)
        {
            return null;
        }
    }
}
