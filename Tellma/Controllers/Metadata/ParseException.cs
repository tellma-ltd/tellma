using System;

namespace Tellma.Controllers
{
    /// <summary>
    /// An exception that occurs during parsing of a property value from string
    /// </summary>
    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
        }
    }
}
