using Xunit;

namespace BSharp.IntegrationTests.Scenario_01
{
    [CollectionDefinition(nameof(Scenario_01))]
    public class Scenario_01_Collection : ICollectionFixture<Scenario_01_WebApplicationFactory>
    {
    }
}
