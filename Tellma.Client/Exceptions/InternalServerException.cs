namespace Tellma.Client
{
    /// <summary>
    /// Represents an unkown problem on Tellma.
    /// </summary>
    public class InternalServerException : TellmaException
    {
        public InternalServerException(string traceIdentifier) :
            base($"An unknown error occurred on the server." + traceIdentifier != null ? $" Trace Identier {traceIdentifier}" : "")
        {
            TraceIdentifier = traceIdentifier;
        }

        public string TraceIdentifier { get; }

        public override string ToString()
        {
            return @$"{base.ToString()}

--- Trace Identifier ---
{TraceIdentifier}";
        }
    }
}
