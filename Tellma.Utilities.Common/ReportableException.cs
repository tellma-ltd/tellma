using System;

namespace Tellma.Utilities.Common
{
    /// <summary>
    /// Base class for expected error exceptions that aren't bugs or defects in the software.
    /// Such exceptions do not reveal the internal workings of the software and therefore 
    /// <see cref="Exception.Message"/> can be safely reported to an untrusted client.
    /// </summary>
    public class ReportableException : Exception
    {
        public ReportableException(string msg) : base(msg)
        {
        }
    }
}
