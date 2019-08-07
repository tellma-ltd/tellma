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
    public class AggregateQueryInternal : IQueryInternal
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
            int currentUserId,
            TimeZoneInfo currentUserTimeZone)
        {
            // (1) Prepare the JOIN's clause
            var joinTree = PrepareJoin();
            var joinSql = joinTree.GetSql(sources);

            // (2) Prepare the SELECT clause
            SqlSelectGroupByClause selectClause = PrepareSelect(joinTree);
            var selectSql = selectClause.ToSelectSql();
            var groupbySql = selectClause.ToGroupBySql();

            // (3) Prepare the WHERE clause
            string whereSql = PrepareWhere(sources, joinTree, ps, currentUserId, currentUserTimeZone);

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
                Query = this,
                IsAggregate = true
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
            // Entityable analysis
            var entityablePrefixes = Select
                .Where(e => e.Property == "Id" && e.IsDimension)
                .Select(e => e.Path)
                .ToList();

            var selects = new HashSet<(string Symbol, string PropName, string aggregate)>(); // To ensure uniqueness
            var columns = new List<(string Symbol, ArraySegment<string> Path, string PropName, string Aggregate)>();

            void AddSelect(string symbol, ArraySegment<string> path, string propName, string aggregate)
            {
                if (selects.Add((symbol, propName, aggregate)))
                {
                    columns.Add((symbol, path, propName, aggregate));
                }
            }

            // Optimization: remember the joins that have been selected and don't select them again
            HashSet<JoinTree> selectedJoins = new HashSet<JoinTree>();

            foreach (var select in Select)
            {
                // Add the property
                string[] path = select.Path;

                {
                    bool isEntityable = entityablePrefixes.Any(e => e.IsPrefixOf(path));
                    var join = joinTree[path];
                    var propName = select.Property; // Can be null
                    var aggregation = isEntityable ? null : select.Aggregation; // Entityable never have aggregations
                    AddSelect(join.Symbol, path, propName, aggregation);
                }

                // In this loop we ensure all levels to the selected properties
                // have their Ids and Foreign Keys added to the select collection
                for (int i = 0; i <= path.Length; i++)
                {
                    var subpath = new ArraySegment<string>(path, 0, i);
                    var join = joinTree[subpath];
                    if (join == null)
                    {
                        // Developer mistake
                        throw new InvalidOperationException($"The path '{string.Join('/', subpath)}' was not found in the joinTree");
                    }

                    // This does not add "Id" and foreign key by default unless it is a Entityable path
                    bool isEntityable = entityablePrefixes.Any(e => e.IsPrefixOf(subpath));
                    if (isEntityable)
                    {
                        // Unless this is root, we also go one level up and add the foreign key that points to this Entity
                        if(subpath.Count > 0)
                        {
                            var prevPath = new ArraySegment<string>(subpath.Array, subpath.Offset, subpath.Count - 1);
                            var prevJoin = joinTree[prevPath];

                            AddSelect(prevJoin.Symbol, prevPath, join.ForeignKeyName, null);
                        }

                        // The Id is ALWAYS included in every returned Entity
                        AddSelect(join.Symbol, subpath, "Id", null);
                    }
                }
            }

            // Change the hash set to a list so that the order is well defined
            return new SqlSelectGroupByClause(columns, Top ?? 0);
        }

        /// <summary>
        /// Prepares the WHERE clause of the SQL query from the <see cref="Filter"/> argument: WHERE ABC
        /// </summary>
        private string PrepareWhere(Func<Type, string> sources, JoinTree joinTree, SqlStatementParameters ps, int currentUserId, TimeZoneInfo currentUserTimeZone)
        {
            string whereSql = QueryTools.FilterToSql(Filter, sources, ps, joinTree, currentUserId, currentUserTimeZone) ?? "";

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
                string orderby = QueryTools.AtomSql(symbol, atom.Property, atom.Aggregation) + $" {atom.OrderDirection.ToUpper()}";
                orderbys.Add(orderby);
            }

            string orderbySql = ""; //  "ORDER BY Id DESC"; // Default order by
            if (orderbys.Count > 0)
            {
                orderbySql = "ORDER BY " + string.Join(", ", orderbys);
            }

            return orderbySql;
        }

        #region IQueryInternal

        // None of these are used

        public QueryInternal PrincipalQuery { get; set; }

        public string ForeignKeyToPrincipalQuery { get; set; }

        public ArraySegment<string> PathToCollectionPropertyInPrincipal { get; set; }

        public bool IsAncestorExpand { get; set; }

        #endregion
    }
}
