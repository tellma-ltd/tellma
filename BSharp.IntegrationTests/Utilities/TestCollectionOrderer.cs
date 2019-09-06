using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace BSharp.IntegrationTests.Utilities
{
    public class TestCollectionOrderer : ITestCollectionOrderer
    {
        public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
        {
            return testCollections.OrderBy(collection => collection.DisplayName);
        }

        public const string TypeName = nameof(BSharp) + "." + nameof(IntegrationTests) + "." + nameof(Utilities) + "." + nameof(TestCollectionOrderer);

        public const string AssemblyName = nameof(BSharp) + "." + nameof(IntegrationTests);

    }
}
