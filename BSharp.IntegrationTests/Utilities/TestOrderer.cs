using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace BSharp.IntegrationTests.Utilities
{
    /// <summary>
    /// For the way we setup integration testing, it is important to run ALL the tests in the 
    /// SAME ORDER they are defined, this test orderer ensures that
    /// </summary>
    public class TestOrderer : ITestCaseOrderer
    {
        public const string Testing = "";

        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            // Run the test cases in sequence, in the same order as defined in the class
            var result = testCases.GroupBy(e => e.Traits[Testing].Single())
                .OrderBy(g => g.Key).SelectMany(g => g);

            return result;
        }

        public const string TypeName = nameof(BSharp) + "." + nameof(IntegrationTests) + "." + nameof(Utilities) + "." + nameof(TestOrderer);

        public const string AssemblyName = nameof(BSharp) + "." + nameof(IntegrationTests);
    }
}
