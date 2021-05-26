using System;

namespace Tellma.Api
{
    /// <summary>
    /// An exception that represents custom validation errors on Entities and DTOs
    /// web controllers would translate this to a 422 Unprocessable Entity response.
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(ValidationErrorsDictionary validationErrors)
        {
            ModelState = validationErrors;
        }

        public ValidationErrorsDictionary ModelState { get; private set; }
    }
}
