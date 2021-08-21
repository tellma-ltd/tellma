using Tellma.Api.Templating;
using System;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using Tellma.Model.Common;
using System.Threading;
using System.Collections;

namespace Tellma.Api.Tests
{
    public class TemplateServiceTests
    {
        private readonly TemplateService _templateService;

        public TemplateServiceTests()
        {
            _templateService = new TemplateService(new NullStringLocalizer(), new MockClientForTemplating());
        }

        [Fact(DisplayName = "Generates markup correctly based on template")]
        public async Task GenerateMarkup_ReturnsCorrectResults()
        {
            // Arrange
            string template = "{{ *define query as Entities('TestEntity', null, null, null, null) }}{{ *foreach item in query }}Name: {{ item.Name }}, {{ *end }}";
            var templates = new (string, string)[] { (template, "text/plain") };
            var args = new MarkupArguments(templates);

            // Act
            var results = await _templateService.GenerateMarkup(args, cancellation: default);

            // Assert
            var result = Assert.Single(results);
            Assert.Equal("Name: First, Name: Second, ", result);
        }

        #region Mocks

        private class MockClientForTemplating : IApiClientForTemplating
        {
            public Task<IReadOnlyList<DynamicRow>> GetAggregate(string collection, int? definitionId, string select, string filter, string having, string orderby, int? top, CancellationToken cancellation)
            {
                throw new NotImplementedException();
            }

            public Task<IReadOnlyList<Entity>> GetEntities(string collection, int? definitionId, string select, string filter, string orderby, int? top, int? skip, CancellationToken cancellation)
            {
                IReadOnlyList<Entity> result = collection switch
                {
                    nameof(TestEntity) => new List<Entity> {
                    new TestEntity {
                        Id = 1,
                        Name = "First",
                        Age = 25,
                        EntityMetadata = {
                            [nameof(TestEntity.Name)] = FieldMetadata.Loaded,
                            [nameof(TestEntity.Age)] = FieldMetadata.Loaded,
                        }
                    },
                    new TestEntity {
                        Id = 2,
                        Name = "Second",
                        Age = 30,
                        EntityMetadata = {
                            [nameof(TestEntity.Name)] = FieldMetadata.Loaded,
                            [nameof(TestEntity.Age)] = FieldMetadata.Loaded,
                        }
                    },
                },
                    _ => throw new Exception("Unknown type"),
                };

                return Task.FromResult(result);
            }

            public Task<IReadOnlyList<EntityWithKey>> GetEntitiesByIds(string collection, int? definitionId, string select, IList ids, CancellationToken cancellation)
            {
                throw new NotImplementedException();
            }

            public Task<EntityWithKey> GetEntityById(string collection, int? definitionId, string select, object id, CancellationToken cancellation)
            {
                throw new NotImplementedException();
            }

            public Task<IReadOnlyList<DynamicRow>> GetFact(string collection, int? definitionId, string select, string filter, string orderby, int? top, int? skip, CancellationToken cancellation)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
