using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Tellma.Client
{
    /// <summary>
    /// Indicates that the operation failed to user input validation.
    /// </summary>
    public class ValidationException : TellmaException
    {
        public ValidationException(ReadonlyValidationErrors errors) : base("The request payload did not pass validation.")
        {
            Errors = errors;
        }

        public ReadonlyValidationErrors Errors { get; }

        public override string ToString()
        {
            ;
            var errorMessages = Errors.SelectMany(pair => pair.Value.Select(msg => $"{pair.Key}: {msg}"));
            var stringifiedErrors = string.Join(Environment.NewLine, errorMessages);

            return @$"{base.ToString()}

--- Validation Errors ---
{stringifiedErrors}";
        }
    }

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
