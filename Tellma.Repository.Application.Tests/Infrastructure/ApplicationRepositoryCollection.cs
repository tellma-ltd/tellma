using Xunit;

namespace Tellma.Repository.Application.Tests
{
    [CollectionDefinition(nameof(ApplicationRepositoryCollection))]
    public class ApplicationRepositoryCollection : ICollectionFixture<ApplicationRepositoryFixture>
    {
        // Empty class just to define the collection
    }
}
