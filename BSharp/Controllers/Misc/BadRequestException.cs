using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    /// <summary>
    /// An exception that is similar to <see cref="InvalidOperationException"/>
    /// web controllers would translate this to a 400 bad request response
    /// </summary>
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}
