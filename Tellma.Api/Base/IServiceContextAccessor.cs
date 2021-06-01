using System;
using System.Threading;

namespace Tellma.Api.Base
{
    /// <summary>
    /// Provides identity and contextual information needed by API services to execute requests.
    /// </summary>
    public interface IServiceContextAccessor
    {
        /// <summary>
        /// The external Id of the authenticated user on behalf of which the service
        /// is used, this value must be trusted as security is based on it.
        /// </summary>
        string ExternalUserId { get; }

        /// <summary>
        /// The external email of the authenticated user on behalf of which the service
        /// is used, this value must be trusted as security is based on it.
        /// </summary>
        string ExternalEmail { get; }

        /// <summary>
        /// The id of the database the requester is attempting to access, this value is
        /// not trusted. I.e the service verifies that the requesting is a member of this 
        /// database.
        /// </summary>
        int? TenantId { get; }

        /// <summary>
        /// Whether or not to update the user's LastActive value.
        /// </summary>
        bool IsSilent { get; }

        /// <summary>
        /// The current date at the client's time zone.
        /// </summary>
        DateTime Today { get; }

        /// <summary>
        /// The cancellation instruction for the service request.
        /// </summary>
        CancellationToken Cancellation { get; }
    }
}
