using System;
using System.Collections.Generic;
using Tellma.Entities.Descriptors;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents a single SQL SELECT query that returns dynamic rows, contains the information required
    /// to execute the SQL SELECT query against a SQL Server database, load the data in dynamic rows
    public class SqlDynamicStatement
    {
        public SqlDynamicStatement(string sql, int columnCount)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException($"'{nameof(sql)}' cannot be null or whitespace", nameof(sql));
            }

            if (columnCount == 0)
            {
                throw new ArgumentException($"'{nameof(columnCount)}' cannot be 0", nameof(columnCount));
            }

            Sql = sql;
            ColumnCount = columnCount;
        }

        /// <summary>
        /// The raw SQL code of the <see cref="SqlStatement"/>
        /// </summary>
        public string Sql { get; }

        /// <summary>
        /// Maps every column index to a path, property, modifier and aggregation function
        /// </summary>
        public int ColumnCount { get; }
    }
}
