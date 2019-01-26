using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Services.Identity
{
    /// <summary>
    /// A useful utility service for retrieving the current user Id 
    /// when reaching the HTTP Context is not convenient
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Returns the currently authenticated external user ID or null otherwise
        /// </summary>
        string GetUserId();

        /// <summary>
        /// Returns the currently authenticated user email, or null otherwise
        /// </summary>
        string GetUserEmail();

        /// <summary>
        /// Registers the DB User object such that it is globally available in the session
        /// </summary>
        void SetDbUser(DbUser user);

        /// <summary>
        /// Retrieves from the session the DB user that was registered with <see cref="SetDbUser(DbUser)"/>
        /// </summary>
        DbUser GetDbUser();
    }
}
