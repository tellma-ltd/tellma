using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Tellma.IntegrationTests.Utilities
{
    public class TestCollectionOrderer : ITestCollectionOrderer
    {
        public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
        {
            return testCollections.OrderBy(collection => collection.DisplayName);
        }

        public const string TypeName = nameof(Tellma) + "." + nameof(IntegrationTests) + "." + nameof(Utilities) + "." + nameof(TestCollectionOrderer);

        public const string AssemblyName = nameof(Tellma) + "." + nameof(IntegrationTests);

    }
}
