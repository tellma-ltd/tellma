using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Tellma.Client
{
    /// <summary>
    /// Validation errors returned by a Tellma API request.
    /// </summary>
    /// <remarks>Used for deserialization from JSON.</remarks>
    internal class ValidationErrors : Dictionary<string, List<string>>
    {
    }

    /// <summary>
    /// Validation errors returned by a Tellma API request.
    /// </summary>
    public class ReadonlyValidationErrors : ReadOnlyDictionary<string, List<string>>
    {
        internal ReadonlyValidationErrors(ValidationErrors errors) : base(errors)
        {
        }
    }
}
