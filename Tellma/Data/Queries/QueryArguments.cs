using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using Tellma.Services;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// All the information required by <see cref="Query{T}"/> and <see cref="AggregateQuery{T}"/> to execute and load data
    /// </summary>
    public class QueryArguments
    {
        public QueryArguments(SqlConnection conn, Func<Type, string> sources, int userId, DateTime? userToday, IStringLocalizer localizer, IInstrumentationService instrumentation, ILogger logger)
        {
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
            Sources = sources ?? throw new ArgumentNullException(nameof(sources));
            UserId = userId;
            UserToday = userToday;
            Localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            Instrumentation = instrumentation ?? throw new ArgumentNullException(nameof(instrumentation));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public SqlConnection Connection { get; }

        public Func<Type, string> Sources { get; }

        public int UserId { get; }

        public DateTime? UserToday { get; }

        public IStringLocalizer Localizer { get; }

        public IInstrumentationService Instrumentation { get; }

        public ILogger Logger { get; }
    }
}
