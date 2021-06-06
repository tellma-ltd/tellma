using System;
using Tellma.Api.Metadata;
using Tellma.Model.Common;
using Xunit;

namespace Tellma.Api.Tests
{
    public class MetadataProviderTests
    {
        private readonly MetadataProvider _provider;
        private readonly IMetadataOverridesProvider _overrides;

        public MetadataProviderTests()
        {
            var localizer = new NullStringLocalizer();
            _provider = new MetadataProvider(localizer);
            _overrides = new MockMetadataOverridesProvider();
        }

        [Fact]
        public void GetMetadata_ReturnsCorrectResults()
        {
            // Arrange
            int tenantId = 1; // Doesn't matter
            int definitionId = MockMetadataOverridesProvider.OverridingDefinitionId + 1000; // Don't override

            // Act
            var meta = _provider.GetMetadata(tenantId, typeof(TestEntity), definitionId, _overrides);
            var idProp = meta.Property(nameof(TestEntity.Id));
            var nameProp = meta.Property(nameof(TestEntity.Name));
            var ageProp = meta.Property(nameof(TestEntity.Age));
            var hiddenProp = meta.Property(nameof(TestEntity.Hidden));

            // Assert
            Assert.NotNull(idProp);
            Assert.NotNull(nameProp);
            Assert.NotNull(ageProp);
            Assert.NotNull(hiddenProp);

            Assert.Equal("Test Name", nameProp.Display());
            Assert.Equal("Test Age", ageProp.Display());
            Assert.Equal("Test Hidden", hiddenProp.Display());
        }

        [Fact]
        public void GetMetadata_OverridingWorks()
        {
            // Arrange
            int tenantId = 1; // Doesn't matter
            int definitionId = MockMetadataOverridesProvider.OverridingDefinitionId;

            // Act
            var meta = _provider.GetMetadata(tenantId, typeof(TestEntity), definitionId, _overrides);
            var idProp = meta.Property(nameof(TestEntity.Id));
            var nameProp = meta.Property(nameof(TestEntity.Name));
            var ageProp = meta.Property(nameof(TestEntity.Age));
            var hiddenProp = meta.Property(nameof(TestEntity.Hidden));

            // Assert
            Assert.NotNull(idProp);
            Assert.NotNull(nameProp);
            Assert.NotNull(ageProp);
            Assert.Null(hiddenProp); // Removed by overrides

            Assert.Equal("Test Name", nameProp.Display());
            Assert.Equal("Override Test Age", ageProp.Display()); // Changed by overrides
        }

        #region Mocks

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

        #endregion
    }
}
