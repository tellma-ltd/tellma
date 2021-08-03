using System;

namespace Tellma.Repository.Common.Queryex
{
    /// <summary>
    /// Contains all the information needed to compile the abstract syntax tree
    /// of <see cref="QueryexBase"/> nodes into an SQL string.
    /// </summary>
    public class QxCompilationContext
    {
        public QxCompilationContext(JoinTrie joins, Func<Type, string> sources, SqlStatementVariables vars, SqlStatementParameters ps, DateTime today, DateTimeOffset now, int userId)
        {
            Joins = joins ?? throw new ArgumentNullException(nameof(joins));
            Sources = sources ?? throw new ArgumentNullException(nameof(sources));
            Variables = vars ?? throw new ArgumentNullException(nameof(vars));
            Parameters = ps ?? throw new ArgumentNullException(nameof(ps));
            Today = today;
            Now = now;
            UserId = userId;
        }

        public JoinTrie Joins { get; }

        public Func<Type, string> Sources { get; }

        public SqlStatementVariables Variables { get; }

        public SqlStatementParameters Parameters { get; }

        public DateTime Today { get; }

        public DateTimeOffset Now { get; }

        public int UserId { get; }
    }
}
