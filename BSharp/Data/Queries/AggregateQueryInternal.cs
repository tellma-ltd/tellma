using BSharp.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Data.Queries
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
        public AggregateSelectExpression Select { get; set; }

        /// <summary>
        /// The filter parameter
        /// </summary>
        public FilterExpression Filter { get; set; }

        /// <summary>
        /// The top parameter
        /// </summary>
        public int? Top { get; set; }

        /// <summary>
        /// The root data type of the query from which all the paths start. In the case of aggregate queries the actual result will be a DynamicEntity
        /// </summary>
        public Type ResultType { get; set; }

        /// <summary>
        /// Implementation of <see cref="IQueryInternal"/> 
        /// </summary>
        public SqlStatement PrepareStatement(
            Func<Type, string> sources,
            SqlStatementParameters ps,
            int userId,
            DateTime? userToday)
        {
            // (1) Prepare the JOIN's clause
            var joinTree = PrepareJoin();
            var joinSql = joinTree.GetSql(sources, fromSql: null);

            // (2) Prepare the SELECT clause
            SqlSelectGroupByClause selectClause = PrepareSelect(joinTree);
            var selectSql = selectClause.ToSelectSql();
            var groupbySql = selectClause.ToGroupBySql();

            // (3) Prepare the WHERE clause
            string whereSql = PrepareWhere(sources, joinTree, ps, userId, userToday);

            // (4) Prepare the ORDERBY clause
            string orderbySql = PrepareOrderBy(joinTree);

            // (5) Finally put together the final SQL statement and return it
            string sql = QueryTools.CombineSql(
                    selectSql: selectSql,
                    joinSql: joinSql,
                    principalQuerySql: null,
                    whereSql: whereSql,
                    orderbySql: orderbySql,
                    offsetFetchSql: null,
                    groupbySql: groupbySql
                );

            // (6) Return the result
            return new SqlStatement
            {
                Sql = sql,
                ResultType = ResultType,
                ColumnMap = selectClause.GetColumnMap(),
                Query = null, // Not used anyways
            };
        }

        /// <summary>
        /// Prepares the join tree 
        /// </summary>
        private JoinTree PrepareJoin()
        {
            // construct the join tree
            var allPaths = new List<string[]>();
            if (Select != null)
            {
                allPaths.AddRange(Select.Select(e => e.Path));
            }

            if (Filter != null)
            {
                allPaths.AddRange(Filter.Select(e => e.Path));
            }

            // This will represent the mapping from paths to symbols
            var joinTree = JoinTree.Make(ResultType, allPaths);
            return joinTree;
        }

        /// <summary>
        /// Prepares a data structure containing all the information needed to construct the SELECT and GROUP BY clauses of the aggregate query
        /// </summary>
        private SqlSelectGroupByClause PrepareSelect(JoinTree joinTree)
        {
            var selects = new HashSet<(string Symbol, string PropName, string Aggregate, string Function)>(); // To ensure uniqueness
            var columns = new List<(string Symbol, ArraySegment<string> Path, string PropName, string Aggregate, string Function)>();

            foreach (var select in Select)
            {
                // Add the property
                string[] path = select.Path;
                var join = joinTree[path];
                var symbol = join.Symbol;
                var propName = select.Property; // Can be null
                var aggregation = select.Aggregation;
                var function = select.Function;

                // If the select doesn't exist: add it, or if it is not original and it shows up again as original: upgrade it
                if (selects.Add((symbol, propName, aggregation, function)))
                {
                    columns.Add((symbol, path, propName, aggregation, function));
                }
            }

            // Change the hash set to a list so that the order is well defined
            return new SqlSelectGroupByClause(columns.ToList(), Top ?? 0);
        }

        /// <summary>
        /// Prepares the WHERE clause of the SQL query from the <see cref="Filter"/> argument: WHERE ABC
        /// </summary>
        private string PrepareWhere(Func<Type, string> sources, JoinTree joinTree, SqlStatementParameters ps, int userId, DateTime? userToday)
        {
            string whereSql = QueryTools.FilterToSql(Filter, sources, ps, joinTree, userId, userToday) ?? "";

            // Add the "WHERE" keyword
            if (!string.IsNullOrEmpty(whereSql))
            {
                whereSql = "WHERE " + whereSql;
            }

            return whereSql;
        }

        /// <summary>
        /// Prepares the ORDER BY clause of the SQL query using the <see cref="Select"/> argument: ORDER BY ABC
        /// </summary>
        private string PrepareOrderBy(JoinTree joinTree)
        {
            var orderByAtoms = Select.Where(e => !string.IsNullOrEmpty(e.OrderDirection));
            var orderByAtomsCount = orderByAtoms.Count();
            if (orderByAtomsCount == 0)
            {
                return "";
            }

            List<string> orderbys = new List<string>(orderByAtomsCount);
            foreach (var atom in orderByAtoms)
            {
                var join = joinTree[atom.Path];
                if (join == null)
                {
                    // Developer mistake
                    throw new InvalidOperationException($"The path '{string.Join('/', atom.Path)}' was not found in the joinTree");
                }
                var symbol = join.Symbol;
                string orderby = QueryTools.AtomSql(symbol, atom.Property, atom.Aggregation, atom.Function) + $" {atom.OrderDirection.ToUpper()}";
                orderbys.Add(orderby);
            }

            string orderbySql = ""; //  "ORDER BY Id DESC"; // Default order by
            if (orderbys.Count > 0)
            {
                orderbySql = "ORDER BY " + string.Join(", ", orderbys);
            }

            return orderbySql;
        }
    }
}
