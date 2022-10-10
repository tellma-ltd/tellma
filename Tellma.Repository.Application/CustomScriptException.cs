using Tellma.Utilities.Common;

namespace Tellma.Repository.Application
{
    /// <summary>
    /// Exception indicating that the error is coming from the execution
    /// of a custom script (e.g. preprocess script, validation script, etc)
    /// and not from the built in code.
    /// </summary>
    public class CustomScriptException : ReportableException
    {
        public CustomScriptException(string msg, int number, int lineNumber = 0) : base(msg)
        {
            Number = number;
            LineNumber = lineNumber;
        }

        public int Number { get; }
        public int LineNumber { get; }
    }
}
