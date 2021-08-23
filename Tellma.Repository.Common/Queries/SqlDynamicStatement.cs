using System;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Represents a single SQL SELECT query that returns rows with dynamic columns. Contains the information required
    /// to execute the SQL SELECT query against a SQL Server database and load the data in dynamic rows.
    public class SqlDynamicStatement : SqlStatementBase
    {
        public SqlDynamicStatement(string sql, int columnCount) : base(sql)
        {
            if (columnCount == 0)
            {
                throw new ArgumentException($"'{nameof(columnCount)}' cannot be zero", nameof(columnCount));
            }

            ColumnCount = columnCount;
        }

        /// <summary>
        /// The number of columns.
        /// </summary>
        public int ColumnCount { get; }
    }
}
