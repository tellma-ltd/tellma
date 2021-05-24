namespace Tellma.Repository.Common
{
    /// <summary>
    /// Base class for statement objects. A "statement" is a single SQL SELECT statement stored
    /// in the <see cref="Sql"/> alongside some metadata that describe how to load that SQL code
    /// from the DB.
    /// </summary>
    public abstract class SqlStatementBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sql">The raw SQL code to run against the DB.</param>
        public SqlStatementBase(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new System.ArgumentException($"'{nameof(sql)}' cannot be null or whitespace.", nameof(sql));
            }

            Sql = sql;
        }

        /// <summary>
        /// The raw SQL code to run against the DB.
        /// </summary>
        public string Sql { get; }
    }
}
