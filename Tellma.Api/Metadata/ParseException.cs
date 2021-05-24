using System;

namespace Tellma.Api
{
    /// <summary>
    /// An exception that occurs during parsing of a property value from a string.
    /// </summary>
    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
        }
    }
}
