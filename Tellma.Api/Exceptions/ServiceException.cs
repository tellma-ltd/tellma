using System;

namespace Tellma.Api
{
    /// <summary>
    /// A generic exception similar to <see cref="InvalidOperationException"/>
    /// web controllers would translate this to a 400 bad request response.
    /// </summary>
    public class ServiceException : Exception
    {
        public ServiceException(string message) : base(message)
        {
        }
    }
}
