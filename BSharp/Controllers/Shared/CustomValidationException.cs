using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace BSharp.Controllers.Shared
{
    /// <summary>
    /// An exception that represents custom validation errors
    /// web controllers would translate this to a 400 Bad Request
    /// </summary>
    public class CustomValidationException : Exception
    {
        public CustomValidationException(ModelStateDictionary modelState)
        {
            ModelState = modelState;
        }

        public ModelStateDictionary ModelState { get; private set; }
    }
}
