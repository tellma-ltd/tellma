namespace Tellma.Client
{
    /// <summary>
    /// Represents lack of appropriate permissions to perform the action.
    /// </summary>
    public class AuthorizationException : TellmaException
    {
        public AuthorizationException() : base("Your account does not have sufficient permissions to complete this request.")
        {
        }
    }
}
