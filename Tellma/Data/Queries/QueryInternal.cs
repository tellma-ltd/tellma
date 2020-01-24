using Tellma.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// This represents a query without any one-to-many path steps (e.g. expand=Order/LineItems is not allowed).
    /// This is a helper class used internally in the implementation of <see cref="Query{T}"/> and should not be used elsewhere in the solution
    /// </summary>
    public class QueryInternal
    {
        /// <summary>
        /// For a <see cref="QueryInternal"/> X that is created for the purpose of loading a collection
        /// navigation property of another <see cref="QueryInternal"/> Y, Y is the principal query of X
        /// </summary>
        public QueryInternal PrincipalQuery { get; set; }

        /// <summary>
        /// For a <see cref="QueryInternal"/> X that is created for the purpose of loading a collection
        /// navigation property of another <see cref="QueryInternal"/> Y, this is the foreign key property on X that points to Y
        /// </summary>
        public string ForeignKeyToPrincipalQuery { get; set; }

        /// <summary>
        /// For a <see cref="QueryInternal"/> X that is created for the purpose of loading a collection
        /// navigation property of another <see cref="QueryInternal"/> Y, this is the path in Y that leads
        /// to the collection navigation property
        /// </summary>
        public ArraySegment<string> PathToCollectionPropertyInPrincipal { get; set; }

        /// <summary>
        /// The expected type of the result of this <see cref="QueryInternal"/>
        /// </summary>
        public Type ResultType { get; set; }

        /// <summary>
        /// The type of the key of the entities of the result
        /// </summary>
        public Type KeyType { get; set; }

        // Dealing with expanding tree ancestors

        /// <summary>
        /// Set to true if this query is expanding the ancestors of nodes in the principal query
        /// </summary>
        public bool IsAncestorExpand { get; set; }

        /// <summary>
        /// If the query contains any tree entities with expanded parents, this collection will contain them
        /// </summary>
        public List<ArraySegment<string>> PathsToParentEntitiesWithExpandedAncestors { get; set; } = new List<ArraySegment<string>>();

        public SelectExpression Select { get; set; } // Should NOT contain collection nav properties

        public ExpandExpression Expand { get; set; } // Should NOT contain collection nav properties

        public FilterExpression Filter { get; set; }

        public OrderByExpression OrderBy { get; set; }

        public IEnumerable<object> Ids { get; set; }

        public IEnumerable<object> ParentIds { get; set; }

        public bool IncludeRoots { get; set; }

        public int? Skip { get; set; }

        public int? Top { get; set; }

        public string FromSql { get; set; }

        // Private fields

        private string _cachedWhere = null;

        // Functionality

        /// <summary>
        /// Create a <see cref="SqlStatement"/> that contains all the needed information to execute the query against a SQL Server database and load and hydrate the entities
        /// IMPORTANT: Calling this method will keep a permanent cache of some parts of the result, therefore if the arguments need to change after
        /// that, a new <see cref="QueryInternal"/> must be created
        /// </summary>
        public SqlStatement PrepareStatement(
            Func<Type, string> sources,
            SqlStatementParameters ps,
            int userId,
            DateTime? userToday)
        {
            // (1) Prepare the JOIN's clause
            var joinTree = PrepareJoin();
            var joinSql = joinTree.GetSql(sources, FromSql);

            // (2) Prepare the SELECT clause
            SqlSelectClause selectClause = PrepareSelect(joinTree);
            var selectSql = selectClause.ToSql(IsAncestorExpand);

            // (3) Prepare the inner join with the principal query (if any)
            string principalQuerySql = PreparePrincipalQuery(sources, ps, userId, userToday);

            // (4) Prepare the WHERE clause
            string whereSql = PrepareWhere(sources, joinTree, ps, userId, userToday);

            // (5) Prepare the ORDERBY clause
            string orderbySql = PrepareOrderBy(joinTree);

            // (6) Prepare the OFFSET and FETCH clauses
            string offsetFetchSql = PrepareOffsetFetch();

            // (7) Finally put together the final SQL statement and return it
            string sql = QueryTools.CombineSql(
                    selectSql: selectSql,
                    joinSql: joinSql,
                    principalQuerySql: principalQuerySql,
                    whereSql: whereSql,
                    orderbySql: orderbySql,
                    offsetFetchSql: offsetFetchSql,
                    groupbySql: null
                );

            // (8) Return the result
            return new SqlStatement
            {
                Sql = sql,
                ResultType = ResultType,
                ColumnMap = selectClause.GetColumnMap(),
                Query = this,
            };
        }

        /// <summary>
        /// Creates the SQL WHERE clause of the current query
        /// </summary>
        public string WhereSql(
            Func<Type, string> sources,
            JoinTree joins,
            SqlStatementParameters ps,
            int userId,
            DateTime? userToday)
        {
            return PrepareWhere(sources, joins, ps, userId, userToday);
        }

        /// <summary>
        /// Creates the <see cref="JoinTree"/> of the current query
        /// </summary>
        public JoinTree JoinSql()
        {
            return PrepareJoin();
        }


        /// <summary>
        /// Create a <see cref="SqlStatement"/> that contains all the needed information to execute the query
        /// as an INNER JOIN of any one of the other queries that uses it as a principal query
        /// IMPORTANT: Calling this method will keep a permanent cache of some parts of the result, therefore
        /// if the arguments need to change after that, a new <see cref="QueryInternal"/> must be created
        /// </summary>
        private string PrepareStatementAsPrincipal(
            Func<Type, string> sources,
            SqlStatementParameters ps,
            bool isAncestorExpand,
            ArraySegment<string> pathToCollectionProperty,
            int userId,
            DateTime? userToday)
        {
            // (1) Prepare the JOIN's clause
            JoinTree joinTree = PrepareJoin(pathToCollectionProperty);
            var joinSql = joinTree.GetSql(sources, FromSql);

            // (2) Prepare the SELECT clause
            SqlSelectClause selectClause = PrepareSelectAsPrincipal(joinTree, pathToCollectionProperty, isAncestorExpand);
            var selectSql = selectClause.ToSql(IsAncestorExpand);

            // (3) Prepare the inner join with the principal query (if any)
            string principalQuerySql = PreparePrincipalQuery(sources, ps, userId, userToday);

            // (4) Prepare the WHERE clause
            string whereSql = PrepareWhere(sources, joinTree, ps, userId, userToday);

            // (5) Prepare the ORDERBY clause
            string orderbySql = PrepareOrderBy(joinTree);

            // (6) Prepare the OFFSET and FETCH clauses
            string offsetFetchSql = PrepareOffsetFetch();

            if (string.IsNullOrWhiteSpace(offsetFetchSql))
            {
                // In a principal query, order by is only added if there is an offset-fetch (usually in the root query)
                orderbySql = "";
            }

            // (7) Finally put together the final SQL statement and return it
            string sql = QueryTools.CombineSql(
                    selectSql: selectSql,
                    joinSql: joinSql,
                    principalQuerySql: principalQuerySql,
                    whereSql: whereSql,
                    orderbySql: orderbySql,
                    offsetFetchSql: offsetFetchSql,
                    groupbySql: null
                );

            // (8) Return the result
            return sql;
        }

        /// <summary>
        /// Prepares the SELECT statement and the column map, using the <see cref="Select"/> argument
        /// </summary>
        private SqlSelectClause PrepareSelect(JoinTree joinTree)
        {
            var selects = new HashSet<(string Symbol, string PropName)>();
            var columns = new List<(string Symbol, ArraySegment<string> Path, string PropName)>();
            void AddSelect(string symbol, ArraySegment<string> path, string propName)
            {
                // NULL happens when there is a select that has been segmented from the middle
                // and the first section of the segment no longer terminates with a simple property
                // propName = propName ?? "Id";
                if (propName == null)
                {
                    return;
                }

                if (selects.Add((symbol, propName)))
                {
                    columns.Add((symbol, path, propName));
                }
            }

            // Any path step that is touched by a select (which has a property) ignores the expand, the joinTree below
            // allows us to efficiently check if any particular step is touched by a select
            JoinTree overridingSelectTree = Select == null ? null : JoinTree.Make(ResultType, Select.Select(e => e.Path)); // Overriding select paths

            // Optimization: remember the joins that have been selected and don't select them again
            HashSet<JoinTree> selectedJoins = new HashSet<JoinTree>();

            // For every expanded entity that has not been tainted by a select argument, we add all its properties to the list of selects
            Expand ??= ExpandExpression.Empty;
            foreach (var expand in Expand.Union(ExpandExpression.RootSingleton))
            {
                string[] path = expand.Path;
                for (int i = 0; i <= path.Length; i++)
                {
                    var subpath = new ArraySegment<string>(path, 0, i);
                    var selectMatch = overridingSelectTree?[subpath];
                    if (selectMatch == null) // This expand is not overridden by a select
                    {
                        var join = joinTree[subpath];
                        if (join == null)
                        {
                            // Developer mistake
                            throw new InvalidOperationException($"The path '{string.Join('/', subpath)}' was not found in the joinTree");
                        }
                        else if (selectedJoins.Contains(join))
                        {
                            continue;
                        }
                        else
                        {
                            selectedJoins.Add(join);
                        }

                        foreach (var prop in join.Type.GetMappedProperties())
                        {
                            AddSelect(join.Symbol, subpath, prop.Name);
                        }
                    }
                }
            }

            if (Select != null)
            {
                foreach (var select in Select)
                {
                    // Add the property
                    string[] path = select.Path;
                    {
                        var join = joinTree[path];
                        var propName = select.Property; // Can be null
                        AddSelect(join.Symbol, path, propName);
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
                        else if (selectedJoins.Contains(join))
                        {
                            // All properties were added earlier in an expand
                            continue;
                        }
                        else
                        {
                            selectedJoins.Add(join);
                        }

                        // The Id is ALWAYS required in every EntityWithKey
                        if (join.Type.IsSubclassOf(typeof(EntityWithKey)))
                        {
                            AddSelect(join.Symbol, subpath, "Id");
                        }

                        // Add all the foreign keys to the next level down
                        foreach (var nextJoin in join.Values)
                        {
                            AddSelect(join.Symbol, subpath, nextJoin.ForeignKeyName);
                        }
                    }
                }
            }

            // If the foreign key to the principal query is specified, then always include that
            // otherwise there will be no way to link the collection to the principal query once we load the data
            if (!string.IsNullOrWhiteSpace(ForeignKeyToPrincipalQuery))
            {
                var path = new string[0];
                AddSelect(joinTree.Symbol, path, ForeignKeyToPrincipalQuery);
            }

            // Deals with trees
            foreach (var path in PathsToParentEntitiesWithExpandedAncestors)
            {
                var join = joinTree[path];
                AddSelect(join.Symbol, path, "ParentId");
            }

            if (IsAncestorExpand)
            {
                var path = new string[0];
                AddSelect(joinTree.Symbol, path, "ParentId");
            }

            // Change the hash set to a list so that the order is well defined
            return new SqlSelectClause(columns);
        }

        /// <summary>
        /// Prepares the SELECT statement and the column map, as it would appear in the INNER JOIN of another query that relies on this as its principal
        /// </summary>
        private SqlSelectClause PrepareSelectAsPrincipal(JoinTree joinTree, ArraySegment<string> pathToCollection, bool isAncestorExpand)
        {
            // Take the segment without the last item
            var pathToCollectionEntity = new ArraySegment<string>(
                pathToCollection.Array,
                pathToCollection.Offset,
                pathToCollection.Count - 1);

            string symbol = joinTree[pathToCollectionEntity]?.Symbol;
            if (string.IsNullOrWhiteSpace(symbol))
            {
                // Developer mistake
                throw new InvalidOperationException($"Could not find the path {string.Join("/", pathToCollectionEntity)} in the joinTree");
            }

            var columns = new List<(string Symbol, ArraySegment<string> Path, string PropName)>
            {
                (symbol, pathToCollectionEntity, isAncestorExpand ? "Node" : "Id")
            };

            return new SqlSelectClause(columns);
        }

        /// <summary>
        /// Create the <see cref="JoinTree"/> from the paths in all the arguments
        /// </summary>
        private JoinTree PrepareJoin(ArraySegment<string>? pathToCollection = null)
        {
            // construct the join tree
            var allPaths = new List<string[]>();
            if (Select != null)
            {
                allPaths.AddRange(Select.Select(e => e.Path));
            }

            if (Expand != null)
            {
                allPaths.AddRange(Expand.Select(e => e.Path));
            }

            if (Filter != null)
            {
                allPaths.AddRange(Filter.Select(e => e.Path));
            }

            if (OrderBy != null)
            {
                allPaths.AddRange(OrderBy.Select(e => e.Path));
            }

            if (pathToCollection != null)
            {
                var pathToCollectionEntity = new ArraySegment<string>(
                    pathToCollection.Value.Array,
                    pathToCollection.Value.Offset,
                    pathToCollection.Value.Count - 1);

                allPaths.Add(pathToCollectionEntity.ToArray());
            }

            // This will represent the mapping from paths to symbols
            return JoinTree.Make(ResultType, allPaths);
        }

        /// <summary>
        /// If this query has a principal query, this method returns the SQL of the principal query in the form of an
        /// INNER JOIN to restrict the result to those entities that are related to the principal query
        /// </summary>
        private string PreparePrincipalQuery(Func<Type, string> sources, SqlStatementParameters ps, int userId, DateTime? userToday)
        {
            string principalQuerySql = "";
            if (PrincipalQuery != null)
            {
                // Get the inner sql and append 4 spaces before each line for aesthetics
                string innerSql = PrincipalQuery.PrepareStatementAsPrincipal(sources, ps, IsAncestorExpand, PathToCollectionPropertyInPrincipal, userId, userToday);
                innerSql = QueryTools.IndentLines(innerSql);

                if (IsAncestorExpand)
                {
                    principalQuerySql = $@"INNER JOIN (
{innerSql}
) As [S] ON [S].[Node].IsDescendantOf([P].[Node]) = 1 AND [S].[Node] <> [P].[Node]";
                }
                else
                {
                    principalQuerySql = $@"INNER JOIN (
{innerSql}
) As [S] ON [S].[Id] = [P].[{ForeignKeyToPrincipalQuery}]";
                }
            }

            return principalQuerySql;
        }

        /// <summary>
        /// Prepares the WHERE clause of the SQL query from the <see cref="Filter"/> argument: WHERE ABC
        /// </summary>
        private string PrepareWhere(Func<Type, string> sources, JoinTree joinTree, SqlStatementParameters ps, int userId, DateTime? userToday)
        {
            // WHERE is cached 
            if (_cachedWhere == null)
            {
                string whereFilter = null;
                string whereInIds = null;
                string whereInParentIds = null;

                if (Filter != null)
                {
                    whereFilter = QueryTools.FilterToSql(Filter, sources, ps, joinTree, userId, userToday);
                }

                if (Ids != null && Ids.Count() >= 1)
                {
                    if (Ids.Count() == 1)
                    {
                        string paramName = ps.AddParameter(Ids.Single());
                        whereInIds = $"[P].[Id] = @{paramName}";
                    }
                    else
                    {
                        var isIntKey = (Nullable.GetUnderlyingType(KeyType) ?? KeyType) == typeof(int);
                        var isStringKey = KeyType == typeof(string);

                        // Prepare the ids table
                        DataTable idsTable = isIntKey ? RepositoryUtilities.DataTable(Ids.Select(id => new { Id = (int)id }))
                            : isStringKey ? RepositoryUtilities.DataTable(Ids.Select(id => new { Id = id.ToString() }))
                            : throw new InvalidOperationException("Only string and Integer Ids are supported");

                        // 
                        var idsTvp = new SqlParameter("@Ids", idsTable)
                        {
                            TypeName = isIntKey ? "[dbo].[IdList]" : isStringKey ? "[dbo].[StringList]" : throw new InvalidOperationException("Only string and Integer Ids are supported"),
                            SqlDbType = SqlDbType.Structured
                        };

                        ps.AddParameter(idsTvp);
                        whereInIds = $"[P].[Id] IN (SELECT Id FROM @Ids)";
                    }
                }

                if (ParentIds != null)
                {
                    if (!ParentIds.Any())
                    {
                        if (IncludeRoots)
                        {
                            whereInParentIds = $"[P].[ParentId] IS NULL";
                        }
                    }
                    else if (ParentIds.Count() == 1)
                    {
                        string paramName = ps.AddParameter(ParentIds.Single());
                        whereInParentIds = $"[P].[ParentId] = @{paramName}";
                        if (IncludeRoots)
                        {
                            whereInParentIds += " OR [P].[ParentId] IS NULL";
                        }
                    }
                    else
                    {
                        var isIntKey = (Nullable.GetUnderlyingType(KeyType) ?? KeyType) == typeof(int);
                        var isStringKey = KeyType == typeof(string);

                        // Prepare the data table
                        DataTable parentIdsTable = new DataTable();
                        string propName = "Id";
                        var column = new DataColumn(propName, KeyType);
                        if (isStringKey)
                        {
                            column.MaxLength = 450; // Just for performance
                        }
                        parentIdsTable.Columns.Add(column);
                        foreach (var id in ParentIds.Where(e => e != null))
                        {
                            DataRow row = parentIdsTable.NewRow();
                            row[propName] = id;
                            parentIdsTable.Rows.Add(row);
                        }

                        // Prepare the TVP
                        var parentIdsTvp = new SqlParameter("@ParentIds", parentIdsTable)
                        {
                            TypeName = isIntKey ? "[dbo].[IdList]" : isStringKey ? "[dbo].[StringList]" : throw new InvalidOperationException("Only string and Integer ParentIds are supported"),
                            SqlDbType = SqlDbType.Structured
                        };

                        ps.AddParameter(parentIdsTvp);
                        whereInParentIds = $"[P].[ParentId] IN (SELECT Id FROM @ParentIds)";
                        if (IncludeRoots)
                        {
                            whereInParentIds += " OR [P].[ParentId] IS NULL";
                        }
                    }
                }

                // The final WHERE clause (if any)
                string whereSql = "";

                var clauses = new List<string> { whereFilter, whereInIds, whereInParentIds }.Where(e => e != null);
                if (clauses.Any())
                {
                    whereSql = clauses.Aggregate((c1, c2) => $"{c1}) AND ({c2}");
                    whereSql = $"WHERE ({whereSql})";
                }

                _cachedWhere = whereSql;
            }

            return _cachedWhere;
        }

        /// <summary>
        /// Prepares the ORDER BY clause of the SQL query using the <see cref="OrderBy"/> argument: ORDER BY ABC
        /// </summary>
        private string PrepareOrderBy(JoinTree joinTree)
        {
            List<string> orderbys = new List<string>(OrderBy?.Count() ?? 0);
            if (OrderBy != null)
            {
                foreach (var atom in OrderBy)
                {
                    var join = joinTree[atom.Path];
                    if (join == null)
                    {
                        // Developer mistake
                        throw new InvalidOperationException($"The path '{string.Join('/', atom.Path)}' was not found in the joinTree");
                    }
                    var symbol = join.Symbol;
                    string orderby = $"[{symbol}].[{atom.Property}] {(atom.Desc ? "DESC" : "ASC")}";
                    orderbys.Add(orderby);
                }
            }

            string orderbySql = ""; // Default order by
            if (orderbys.Count > 0)
            {
                orderbySql = "ORDER BY " + string.Join(", ", orderbys);
            }

            return orderbySql;
        }

        /// <summary>
        /// Prepares the "OFFSET X ROWS FETCH NEXT Y ROWS ONLY" clause using <see cref="Skip"/> and <see cref="Top"/> arguments
        /// </summary>
        private string PrepareOffsetFetch()
        {
            string offsetFetchSql = "";
            if (Skip != null || Top != null)
            {
                offsetFetchSql += $"OFFSET {Skip ?? 0} ROWS";
            }

            if (Top != null)
            {
                offsetFetchSql += $" FETCH NEXT {Top.Value} ROWS ONLY";
            }

            return offsetFetchSql;
        }
    }
}
