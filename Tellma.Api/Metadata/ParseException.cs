using Tellma.Utilities.Common;

namespace Tellma.Api.Metadata
{
    /// <summary>
    /// An exception that occurs during parsing of a property value from a string.
    /// </summary>
    public class ParseException : ReportableException
    {
        public ParseException(string message) : base(message)
        {
        }
    }
}
