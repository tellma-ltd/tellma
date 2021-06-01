using System;
using Tellma.Api.Metadata;
using Tellma.Model.Common;

namespace Tellma.Api.Tests
{
    public class MockMetadataOverridesProvider : IMetadataOverridesProvider
    {
        public const int OverridingDefinitionId = 1;
        public EntityMetadataOverrides EntityOverrides(TypeDescriptor typeDesc, int? definitionId, Func<string> singularDisplay, Func<string> pluralDisplay)
        {
            return null;
        }

        public PropertyMetadataOverrides PropertyOverrides(TypeDescriptor typeDesc, int? definitionId, PropertyDescriptor propDesc, Func<string> defaultDisplay)
        {
            if (definitionId == OverridingDefinitionId)
            {
                if (typeDesc.Name == nameof(TestEntity))
                {
                    if (propDesc.Name == nameof(TestEntity.Age))
                    {
                        return new PropertyMetadataOverrides { Display = () => "Override Test Age" }; // Override display name
                    }

                    if (propDesc.Name == nameof(TestEntity.Hidden))
                    {
                        return new PropertyMetadataOverrides { Display = null }; // Should hide it
                    }
                }
            }

            return null;
        }
    }
}
