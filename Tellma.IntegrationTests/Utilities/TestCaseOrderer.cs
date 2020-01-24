using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Tellma.IntegrationTests.Utilities
{
    /// <summary>
    /// For the way we setup integration testing, it is important to run ALL the tests in the 
    /// SAME ORDER they are defined, this test orderer ensures that
    /// </summary>
    public class TestCaseOrderer : ITestCaseOrderer
    {
        public const string Testing = "";

        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            //// Run the test cases in sequence, in the same order as defined in the class
            //var result = testCases.GroupBy(e => e.Traits[Testing].Single())
            //    .OrderBy(g => g.Key).SelectMany(g => g);

            //return result;

            return testCases;
        }

        public const string TypeName = nameof(Tellma) + "." + nameof(IntegrationTests) + "." + nameof(Utilities) + "." + nameof(TestCaseOrderer);

        public const string AssemblyName = nameof(Tellma) + "." + nameof(IntegrationTests);
    }
}
