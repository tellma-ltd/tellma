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
        string GetUserId();

        /// <summary>
        /// Returns the currently authenticated external user email, or null otherwise.
        /// </summary>
        string GetUserEmail();

        /// <summary>
        /// True if the authenticated user is a service account, false if it's a regular user.
        /// </summary>
        public bool IsService { get; }
    }
}
