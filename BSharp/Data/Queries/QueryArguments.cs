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
        public QueryArguments(SqlConnection conn, Func<Type, SqlSource> sources, int userId, TimeZoneInfo userTimeZone, IStringLocalizer localizer)
        {
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
            Sources = sources ?? throw new ArgumentNullException(nameof(sources));
            UserId = userId;
            UserTimeZone = userTimeZone ?? throw new ArgumentNullException(nameof(userTimeZone));
            Localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        public SqlConnection Connection { get; }

        public Func<Type, SqlSource> Sources { get; }

        public int UserId { get; }

        public TimeZoneInfo UserTimeZone { get; }

        public IStringLocalizer Localizer { get; set; }
    }
}
