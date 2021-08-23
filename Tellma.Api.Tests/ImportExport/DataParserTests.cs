using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.ImportExport;
using Tellma.Api.Metadata;
using Tellma.Model.Common;
using Xunit;

namespace Tellma.Api.Tests.ImportExport
{
    public class DataParserTests
    {
        private readonly DataParser _parser;
        private readonly MetadataProvider _provider;

        public DataParserTests()
        {
            _parser = new DataParser(new MockClientForImport(), new NullStringLocalizer());
            _provider = new MetadataProvider(new NullStringLocalizer());
        }

        [Fact(DisplayName = "Parses raw data to entities correctly")]
        public async Task ParseAsync_ReturnsCorrectResults()
        {
            // Arrange
            var dataWithoutHeader = new List<string[]>
            {
                new[] { "Mark", "23" },
                new[] { "Thomas", "27" },
            };

            var testEntityMeta = _provider.GetMetadata(1, typeof(TestEntity), null, null);
            var nameMeta = testEntityMeta.Property(nameof(TestEntity.Name));
            var ageMeta = testEntityMeta.Property(nameof(TestEntity.Age));

            var nameMapping = new PropertyMappingInfo(nameMeta, nameMeta) { Index = 0 };
            var ageMapping = new PropertyMappingInfo(ageMeta, ageMeta) { Index = 1 };

            var simpleProps = new List<PropertyMappingInfo> { nameMapping, ageMapping };
            var collectionProps = new List<MappingInfo> { };
            var mapping = new MappingInfo(testEntityMeta, testEntityMeta, simpleProps, collectionProps, null, null);
            var errors = new ImportErrors();

            // Act
            var entities = await _parser.ParseAsync<TestEntity>(dataWithoutHeader, mapping, errors);

            // Assert
            Assert.Collection(entities,
                entity =>
                {
                    Assert.Equal("Mark", entity.Name);
                    Assert.Equal(23, entity.Age);
                },
                entity =>
                {
                    Assert.Equal("Thomas", entity.Name);
                    Assert.Equal(27, entity.Age);
                }
            );
        }

        private class MockClientForImport : IApiClientForImport
        {
            public Task<IReadOnlyList<EntityWithKey>> GetEntitiesByPropertyValues(string collection, int? definitionId, string propName, IEnumerable<object> values, CancellationToken cancellation)
            {
                throw new NotImplementedException();
            }
        }
    }
}
