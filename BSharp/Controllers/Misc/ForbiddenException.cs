using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.Misc
{
    /// <summary>
    /// Exception that signifies that the logged-in user performing an operation is not 
    /// authorized to do so, web controllers should translate it to a status code 403
    /// </summary>
    public class ForbiddenException : Exception
    {
    }
}
