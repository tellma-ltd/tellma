using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace BSharp.Controllers.Misc
{
    /// <summary>
    /// An exception that represents custom validation errors on DTOs
    /// web controllers would translate this to a 422 unprocessable entity response
    /// </summary>
    public class UnprocessableEntityException : Exception
    {
        public UnprocessableEntityException(ModelStateDictionary modelState)
        {
            ModelState = modelState;
        }

        public ModelStateDictionary ModelState { get; private set; }
    }
}
