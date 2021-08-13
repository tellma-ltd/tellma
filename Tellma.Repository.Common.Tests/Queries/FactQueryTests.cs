using System.Threading.Tasks;
using Xunit;

namespace Tellma.Repository.Common.Tests.Queries
{
    public class FactQueryTests
    {
        [Fact]
        public async Task ToListAsync_RunsWithoutErrors()
        {
            // Arrange
            var spy = new SpyLoader();
            var connString = "FakeConnectionString";
            var queryArgs = new QueryArguments(EntitySources.Sources, connString, spy);
            var query = new FactQuery<TestEntity>(_ => Task.FromResult(queryArgs));
            var ctx = new QueryContext(userId: 3);

            // Act
            await query
                .Select("Id,Foo + 1,Bar")
                .Filter("Foo = 2 and Bar = 'Hello'")
                .ToListAsync(ctx);

            // Assert
            Assert.NotNull(spy.DynamicArgs);

            var statement = spy.DynamicArgs.PrincipalStatement;
            Assert.NotNull(statement);
            Assert.StartsWith($"SELECT [P].[Id], [P].[Foo] + 1, [P].[Bar]", statement.Sql);

            var ancestors = spy.DynamicArgs.DimensionAncestorsStatements;
            Assert.Null(ancestors);

            Assert.Equal(3, statement.ColumnCount);            
        }
    }
}
