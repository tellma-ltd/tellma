using System;
using System.Linq;
using Tellma.Repository.Common.Queryex;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Used by <see cref="StatementLoader"/> when encountering an unhandled 
    /// exception to report the SQL statement(s) that caused the exception.
    /// Logging this is useful to the developer trying to debug the issue.
    /// <para/>
    /// Note: The contents of this exception should be logged but not reported to untrusted clients.
    /// </summary>
    public class StatementLoaderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="StatementLoaderException"/>.
        /// </summary>
        /// <param name="sql">The SQL statement(s) that caused the <paramref name="innerException"/>.</param>
        /// <param name="ps">The parameters provided to <paramref name="sql"/>.</param>
        /// <param name="innerException">The unhandled exception when attempting to load the statements.</param>
        public StatementLoaderException(
            string sql, 
            SqlStatementParameters ps,
            Exception innerException) : base(innerException.Message, innerException)
        {
            Sql = sql;
            Parameters = ps;
        }

        /// <summary>
        /// The SQL statement(s) that caused the <see cref="StatementLoaderException.InnerException"/>.
        /// </summary>
        public string Sql { get; }

        /// <summary>
        /// The parameters provided to <see cref="Sql"/>.
        /// </summary>
        public SqlStatementParameters Parameters { get; }

        public override string ToString()
        {
            var stringifiedParams = string.Join(Environment.NewLine, Parameters.Select(e => $"   DECLARE @{e.ParameterName} NVARCHAR(1024) = N'{e.Value?.ToString()?.Replace("'", "''")}';"));

            return @$"{base.ToString()}

--- Additional Information ---
/* Parameters: 
{stringifiedParams}
*/

--- SQL Statement(s):
{Sql}";
        }
    }
}
