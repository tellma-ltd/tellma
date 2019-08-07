using System.Collections.Generic;
using System.Data.SqlClient;

namespace BSharp.Data
{
    /// <summary>
    /// Simple class containing the information required to load a certain entity type from the database
    /// </summary>
    public class SqlSource
    {
        /// <summary>
        /// Create a new <see cref="SqlSource"/> with the provided SQL code
        /// </summary>
        public SqlSource(string sql)
        {
            SQL = sql;
        }

        /// <summary>
        /// Create a new <see cref="SqlSource"/> with the provided SQL code and <see cref="SqlParameter"/>s
        /// </summary>
        public SqlSource(string sql, IEnumerable<SqlParameter> parameters)
        {
            SQL = sql;
            Parameters = parameters;
        }

        /// <summary>
        /// The SQL script to load a certain <see cref="EntityModel.Entity"/> from the database.
        /// The script must be such that it can be included as a subquery inside another query (therefore no EXEC sp)
        /// </summary>
        public string SQL { get; }

        /// <summary>
        /// Any parameters required for the <see cref="SQL"/> to execute correctly.
        /// WARNING: The collection might be null
        /// </summary>
        public IEnumerable<SqlParameter> Parameters { get; }
    }
}
