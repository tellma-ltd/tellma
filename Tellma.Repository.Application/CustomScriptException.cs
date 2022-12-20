using Tellma.Utilities.Common;

namespace Tellma.Repository.Application
{
    /// <summary>
    /// Exception indicating that the error is coming from the execution
    /// of a custom script (e.g. preprocess script, validation script, etc)
    /// and not from the built in code. This covers both intentional errors
    /// (Number >= 50,000) and ones caused by bugs in the script.
    /// </summary>
    public class CustomScriptException : ReportableException
    {
        public CustomScriptException(string msg, int number) : base(msg)
        {
            Number = number;
        }
        public CustomScriptException(string msg, int number, int lineDefId) : this(msg, number)
        {
            LineDefinitionId = lineDefId;
        }

        /// <summary>
        /// SQL Server error number.
        /// </summary>
        public int Number { get; }

        /// <summary>
        /// True if <see cref="Number"/> is less than 50,000, indicating that
        /// this error was not raised intentionally by the script author.
        /// </summary>
        public bool IsScriptBug => Number < 50000; // RAISERROR creates an error with a number of 50,000

        /// <summary>
        /// Optional: The Id of the definition that caused the error.
        /// </summary>
        public int? LineDefinitionId { get; set; }
    }
}
