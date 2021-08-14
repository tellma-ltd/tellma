using System.Collections.Generic;

namespace Tellma.IntegrationTests.Utilities
{
    /// <summary>
    /// The validation errors returned by ASP.NET Core can be deserialized
    /// into this structure for the test methods to easily examine them.
    /// </summary>
    public class ValidationErrors : Dictionary<string, List<string>>
    {
    }
}
