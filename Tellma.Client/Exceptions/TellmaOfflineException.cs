namespace Tellma.Client
{
    /// <summary>
    /// Indicates that the Tellma server is currently unreachable from the client program.
    /// </summary>
    public class TellmaOfflineException : TellmaException
    {
        public TellmaOfflineException() : base("You are currently offline.")
        {
        }
    }
}
