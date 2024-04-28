using System;

namespace Tellma.Api
{
    /// <summary>
    /// Exception that signifies that the logged-in user is performing an operation is not 
    /// authorized to do so, web controllers should translate it to a status code 403.
    /// </summary>
    public class ForbiddenException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
        /// </summary>
        /// <param name="type">True if this is an error resulting from the user 
        /// trying to access a tenant that she is not a member of.</param>
        public ForbiddenException(ForbiddenReason type = ForbiddenReason.MissingPermission, string message = null) : base(message)
        {
            ForbiddenType = type;
            ErrorMessage = message;
        }

        /// <summary>
        /// True if this is an error resulting from the user 
        /// trying to access a tenant that she is not a member of.
        /// </summary>
        public ForbiddenReason ForbiddenType { get; }

        public string ErrorMessage { get; }
    }

    public enum ForbiddenReason
    {
        /// <summary>
        /// Does not have permission to perform action
        /// </summary>
        MissingPermission,

        /// <summary>
        /// Not a member of the company
        /// </summary>
        NotCompanyMember,

        /// <summary>
        /// User account violates company policy
        /// </summary>
        ViolatesCompanyPolicy,
    }
}
