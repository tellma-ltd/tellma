using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Data.DbModel
{
    /// <summary>
    /// A Query type https://docs.microsoft.com/en-us/ef/core/modeling/query-types
    /// represents a global user from the manager db which matches a certain email
    /// </summary>
    public class GlobalUsersMatch
    {
        public string Email { get; set; }

        public string ExternalId { get; set; }
    }
}
