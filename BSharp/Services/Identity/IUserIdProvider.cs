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
    public interface IUserIdProvider
    {
        /// <summary>
        /// Returns the currently authenticated User Id, or null otherwise
        /// </summary>
        /// <returns></returns>
        string GetUserId();
    }
}
