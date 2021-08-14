namespace Tellma.Services.ApiAuthentication
{
    /// <summary>
    /// A useful utility service for retrieving the current user Id 
    /// and email from the HTTP web request.
    /// </summary>
    public interface IExternalUserAccessor
    {
        /// <summary>
        /// Returns the currently authenticated external user ID or null otherwise.
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// Returns the currently authenticated external user email, or null otherwise.
        /// </summary>
        string Email { get; }

        /// <summary>
        /// Returns the currently authenticated client ID or null otherwise.
        /// </summary>
        string ClientId { get; }

        /// <summary>
        /// True if the authenticated user is a service account, False if the
        /// authenticated user is a living 
        /// </summary>
        bool IsServiceAccount { get; }
    }
}
