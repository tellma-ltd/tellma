using Tellma.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Tellma.Entities.Descriptors;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Responsible for creating an <see cref="SqlStatement"/> based on some query parameters.
    /// This is a helper class used internally in the implementation of <see cref="AggregateQuery{T}"/> and should not be used elsewhere in the solution
    /// </summary>
    internal class AggregateQueryInternal
    {
        /// <summary>
        /// The select parameter, should NOT contain collection nav properties or tree nav properties (Parent)
        /// </summary>
        public ExpressionAggregateSelect Select { get; set; }

        /// <summary>
        /// The orderby parameter
        /// </summary>
        public ExpressionAggregateOrderBy OrderBy { get; set; }

        /// <summary>
        /// The filter parameter
        /// </summary>
        public ExpressionFilter Filter { get; set; }

        /// <summary>
        /// The having parameter
        /// </summary>
        public ExpressionHaving Having { get; set; }

        /// <summary>
        /// The top parameter
        /// </summary>
        public int? Top { get; set; }

        /// <summary>
        /// The root data type of the query from which all the paths start. In the case of aggregate queries the actual result will be a DynamicEntity
        /// </summary>
        public TypeDescriptor ResultType { get; set; }

        /// <summary>
        /// Implementation of <see cref="IQueryInternal"/> 
        /// </summary>
        public SqlStatement PrepareStatement(
            Func<Type, string> sources,
            SqlStatementVariables vars,
            SqlStatementParameters ps,
            int userId,
            DateTime? userToday)
        {
            // (1) Prepare the JOIN's clause
            var joinTrie = PrepareJoin();
            var joinSql = joinTrie.GetSql(sources, fromSql: null);

            // Compilation context
            var today = userToday ?? DateTime.Today;
            var ctx = new QxCompilationContext(joinTrie, sources, vars, ps, today, userId);

            // (2) Prepare all the SQL clauses
            string selectSql = PrepareSelectSql(ctx);
            string groupbySql = PrepareGroupBySql(ctx);
            string whereSql = PrepareWhereSql(ctx);
            string havingSql = PrepareHavingSql(ctx);
            string orderbySql = PrepareOrderBySql(ctx);

            // (3) Put together the final SQL statement and return it
            string sql = QueryTools.CombineSql(
                    selectSql: selectSql,
                    joinSql: joinSql,
                    principalQuerySql: null,
                    whereSql: whereSql,
                    orderbySql: orderbySql,
                    offsetFetchSql: null,
                    groupbySql: groupbySql,
                    havingSql: havingSql
                );

            // (8) Return the result
            return new SqlStatement
            {
                Sql = sql,
                ResultDescriptor = ResultType
            };
        }

        /// <summary>
        /// Prepares the join tree 
        /// </summary>
        private JoinTrie PrepareJoin()
        {
            // construct the join tree
            var allPaths = new List<string[]>();
            if (Select != null)
            {
                allPaths.AddRange(Select.ColumnAccesses().Select(e => e.Path));
            }

            if (OrderBy != null)
            {
                allPaths.AddRange(OrderBy.ColumnAccesses().Select(e => e.Path));
            }

            if (Filter != null)
            {
                allPaths.AddRange(Filter.ColumnAccesses().Select(e => e.Path));
            }

            if (Having != null)
            {
                allPaths.AddRange(Having.ColumnAccesses().Select(e => e.Path));
            }

            // This will represent the mapping from paths to symbols
            var joinTree = JoinTrie.Make(ResultType, allPaths);
            return joinTree;
        }

        private string PrepareSelectSql(QxCompilationContext ctx)
        {
            string top = Top == 0 ? "" : $"TOP {Top} ";
            return $"SELECT {top}" + string.Join(", ", Select.Select(e => e.CompileToNonBoolean(ctx).DeBracket()));
        }

        private string PrepareGroupBySql(QxCompilationContext ctx)
        {
            // take all columns that are not aggregated, and group them together
            var nonAggregateSelects = Select.Where(e => !e.ContainsAggregations);
            if (nonAggregateSelects.Any())
            {
                return "GROUP BY " + string.Join(", ", nonAggregateSelects.Select(e => e.CompileToNonBoolean(ctx).DeBracket()));
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Prepares the WHERE clause of the SQL query from the <see cref="Filter"/> argument: WHERE ABC
        /// </summary>
        private string PrepareWhereSql(QxCompilationContext ctx)
        {
            string whereSql = Filter?.Expression?.CompileToBoolean(ctx)?.DeBracket();

            // Add the "WHERE" keyword
            if (!string.IsNullOrEmpty(whereSql))
            {
                whereSql = "WHERE " + whereSql;
            }

            return whereSql;
        }

        /// <summary>
        /// Prepares the WHERE clause of the SQL query from the <see cref="Filter"/> argument: WHERE ABC
        /// </summary>
        private string PrepareHavingSql(QxCompilationContext ctx)
        {
            string havingSql = Having?.Expression?.CompileToBoolean(ctx)?.DeBracket();

            // Add the "HAVING" keyword
            if (!string.IsNullOrEmpty(havingSql))
            {
                havingSql = "HAVING " + havingSql;
            }

            return havingSql;
        }

        /// <summary>
        /// Prepares the ORDER BY clause of the SQL query using the <see cref="Select"/> argument: ORDER BY ABC
        /// </summary>
        private string PrepareOrderBySql(QxCompilationContext ctx)
        {
            var orderByAtomsCount = OrderBy?.Count() ?? 0;
            if (orderByAtomsCount == 0)
            {
                return "";
            }

            List<string> orderbys = new List<string>(orderByAtomsCount);
            foreach (var expression in OrderBy)
            {
                string orderby = expression.CompileToNonBoolean(ctx);
                if (expression.IsDescending)
                {
                    orderby += " DESC";
                }

                if (expression.IsAscending)
                {
                    orderby += " ASC";
                }

                orderbys.Add(orderby);
            }

            return "ORDER BY " + string.Join(", ", orderbys);
        }
    }
}
