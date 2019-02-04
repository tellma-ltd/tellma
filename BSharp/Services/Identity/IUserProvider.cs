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
    public interface IUserProvider
    {
        /// <summary>
        /// Returns the currently authenticated external user ID or null otherwise
        /// </summary>
        string GetUserId();

        /// <summary>
        /// Returns the currently authenticated external user email, or null otherwise
        /// </summary>
        string GetUserEmail();
    }
}
