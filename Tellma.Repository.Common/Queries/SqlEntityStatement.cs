using System.Collections.Generic;
using Tellma.Model.Common;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Represents a single SQL SELECT query that returns entities, contains the information required to execute
    /// the SQL SELECT query against a SQL Server database, load the entities and link them together via
    /// navigation properties. 
    /// A single <see cref="EntityQuery{T}"/> can result in multiple <see cref="SqlEntityStatement"/>s
    /// which are executed against the database in a single statement.
    /// </summary>
    public class SqlEntityStatement : SqlStatementBase
    {
        public SqlEntityStatement(string sql, TypeDescriptor resultDescriptor, List<SqlEntityStatementColumn> columnMap, EntityQueryInternal query) : base(sql)
        {
            ResultDescriptor = resultDescriptor;
            ColumnMap = columnMap;
            Query = query;
        }

        /// <summary>
        /// The root type of the result
        /// </summary>
        public TypeDescriptor ResultDescriptor { get; }

        /// <summary>
        /// Maps every column index to a path and a property
        /// </summary>
        public List<SqlEntityStatementColumn> ColumnMap { get; }

        /// <summary>
        /// The query that produced this <see cref="SqlEntityStatement"/>
        /// </summary>
        public EntityQueryInternal Query { get; }
    }
}
