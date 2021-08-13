using System.Net.Http;
using Xunit;

namespace Tellma.IntegrationTests.Scenario_01
{
    [Collection(nameof(Scenario_01))]
    public abstract class Scenario_01
    {
        private readonly Scenario_01_WebApplicationFactory _factory;

        public Scenario_01(Scenario_01_WebApplicationFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// The <see cref="HttpClient"/> used by all integration tests.
        /// </summary>
        protected HttpClient Client => _factory.GetClient();
    }
}
