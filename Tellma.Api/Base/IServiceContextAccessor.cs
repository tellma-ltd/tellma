using System;

namespace Tellma.Api.Base
{
    /// <summary>
    /// Provides identity and contextual information needed by API services to execute requests.
    /// </summary>
    public interface IServiceContextAccessor
    {
        ///// <summary>
        ///// True when the caller is a trusted background job that can bypass authorization checks
        ///// </summary>
        //public bool Userless { get; }

        bool IsServiceAccount { get; }

        /// <summary>
        /// The external Id of the authenticated user on behalf of which the service
        /// is used, this value must be trusted as security is based on it.
        /// </summary>
        /// <remarks>
        /// If <see cref="IsServiceAccount"/> = False, this property is required.
        /// </remarks>
        string ExternalUserId { get; }

        /// <summary>
        /// The external email of the authenticated user on behalf of which the service
        /// is used, this value must be trusted as security is based on it.
        /// </summary>
        /// <remarks>
        /// If <see cref="IsServiceAccount"/> = False, this property is required.
        /// </remarks>
        string ExternalEmail { get; }

        /// <summary>
        /// The external Id of the client accessing the service, this value must be 
        /// trusted as security is based on it.
        /// </summary>
        /// <remarks>
        /// This property is required.
        /// </remarks>
        string ExternalClientId { get; }

        /// <summary>
        /// The id of the database the requester is attempting to access, this value is
        /// not trusted. I.e the service verifies that the requesting user is a member of 
        /// this database.
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
        /// The current calendar used to display dates on the client.
        /// </summary>
        string Calendar { get; }
    }
}
