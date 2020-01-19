using Microsoft.Extensions.Localization;
using System;
using System.Data.SqlClient;

namespace BSharp.Data.Queries
{
    /// <summary>
    /// All the information required by <see cref="Query{T}"/> and <see cref="AggregateQuery{T}"/> to execute and load data
    /// </summary>
    public class QueryArguments
    {
        public QueryArguments(SqlConnection conn, Func<Type, string> sources, int userId, DateTime? userToday, IStringLocalizer localizer)
        {
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
            Sources = sources ?? throw new ArgumentNullException(nameof(sources));
            UserId = userId;
            UserToday = userToday;
            Localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        public SqlConnection Connection { get; }

        public Func<Type, string> Sources { get; }

        public int UserId { get; }

        public DateTime? UserToday { get; }

        public IStringLocalizer Localizer { get; set; }
    }
}
