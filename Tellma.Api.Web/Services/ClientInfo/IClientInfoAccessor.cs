namespace Tellma.Services.ClientInfo
{
    /// <summary>
    /// Extracts information about the caller of the web request.
    /// IMPORTANT: Nothing returned by this interface should be trusted for anything security-sensitive
    /// </summary>
    public interface IClientInfoAccessor
    {
        /// <summary>
        /// Returns a <see cref="ClientInfo"/> populated with client related information
        /// </summary>
        ClientInfo GetInfo();
    }
}
