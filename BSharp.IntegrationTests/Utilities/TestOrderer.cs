using System.Collections.Generic;
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
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            // Run the test cases in sequence, in the same order as defined in the class
            return testCases;
        }

        public const string TypeName = nameof(BSharp) + "." + nameof(IntegrationTests) + "." + nameof(Utilities) + "." + nameof(TestOrderer);

        public const string AssemblyName = nameof(BSharp) + "." + nameof(IntegrationTests);
    }
}
