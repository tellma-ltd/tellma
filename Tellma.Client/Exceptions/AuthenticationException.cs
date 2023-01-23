namespace Tellma.Client
{
    /// <summary>
    /// Represents failure to authenticate with Tellma's identity provider.
    /// </summary>
    public class AuthenticationException : TellmaException
    {
        public AuthenticationException() : base("Failed to authenticate with the Tellma server.")
        {
        }
    }
}
