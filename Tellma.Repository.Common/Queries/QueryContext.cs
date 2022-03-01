using System;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Context needed to load the data from the database
    /// </summary>
    public class QueryContext
    {
        public QueryContext(int userId, DateTime? userToday = null, DateTimeOffset? userNow = null)
        {
            UserId = userId;
            UserToday = userToday;
            UserNow = userNow;
        }

        /// <summary>
        /// The Id of the user making this call in the called DB.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The current date at the caller's timezone.
        /// </summary>
        public DateTime? UserToday { get; set; }

        /// <summary>
        /// The time returned by the now() function.
        /// </summary>
        public DateTimeOffset? UserNow { get; set; }
    }
}
