﻿using System.Collections.Generic;
using Tellma.Entities.Descriptors;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents a single SQL SELECT query that returns entities, contains the information required to execute
    /// the SQL SELECT query against a SQL Server database, load the entities and link them together via
    /// navigation properties. 
    /// A single <see cref="Query{T}"/> can result in multiple <see cref="SqlStatement"/>s
    /// which are executed against the database in a single statement.
    /// </summary>
    public class SqlStatement
    {
        /// <summary>
        /// The raw SQL code of the <see cref="SqlStatement"/>
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// The root type of the result
        /// </summary>
        public TypeDescriptor ResultDescriptor { get; set; }

        /// <summary>
        /// Maps every column index to a path, property, modifier and aggregation function
        /// </summary>
        public List<SqlStatementColumn> ColumnMap { get; set; }

        /// <summary>
        /// The query that produced this <see cref="SqlStatement"/>
        /// </summary>
        public QueryInternal Query { get; set; }
    }
}
