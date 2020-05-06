using System;
using Tellma.Controllers.Utilities;

namespace Tellma.Controllers
{
    /// <summary>
    /// An exception that represents custom validation errors on DTOs
    /// web controllers would translate this to a 422 unprocessable entity response
    /// </summary>
    public class UnprocessableEntityException : Exception
    {
        public UnprocessableEntityException(ValidationErrorsDictionary validationErrors)
        {
            ModelState = validationErrors;
        }

        public ValidationErrorsDictionary ModelState { get; private set; }
    }
}
