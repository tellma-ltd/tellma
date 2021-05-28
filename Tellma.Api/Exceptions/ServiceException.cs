using System;

namespace Tellma.Controllers
{
    /// <summary>
    /// A generic exception similar to <see cref="InvalidOperationException"/>
    /// web controllers would translate this to a 400 bad request response and
    /// supply the exception message in the response body.
    /// </summary>
    public class ServiceException : Exception
    {
        public ServiceException(string message) : base(message)
        {
        }
    }
}
