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
        /// <param name="notMember">True if this is an error resulting from the user 
        /// trying to access a tenant that she is not a member of.</param>
        public ForbiddenException(bool notMember = false)
        {
            NotMember = notMember;
        }

        /// <summary>
        /// True if this is an error resulting from the user 
        /// trying to access a tenant that she is not a member of.
        /// </summary>
        public bool NotMember { get; }
    }
}
