using BSharp.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSharp.Services.OData
{
    /// <summary>
    /// This represents an odata query without any one-to-many path steps (e.g. expand=Order/LineItems is not allowed)
    /// </summary>
    public class ODataFlatQuery // Cannot handle many to one immediately
    {
        public ODataFlatQuery PrincipalQuery { get; set; } // Need to populate these 3 properties and use them below

        public string ForeignKeyToPrincipalQuery { get; set; }

        public ArraySegment<string> PathToCollectionPropertyInPrincipal { get; set; }

        public Type ResultType { get; set; }

        public Type KeyType { get; set; }

        // The OData arguments

        public SelectExpression Select { get; set; } // Should NOT contain collection nav properties

        public ExpandExpression Expand { get; set; } // Should NOT contain collection nav properties

        public FilterExpression Filter { get; set; }

        public OrderByExpression OrderBy { get; set; }

        public string Ids { get; set; }

        public int? Skip { get; set; }

        public int? Top { get; set; }

        // Private fields

        private string _cachedWhere = null;

        // Functionality

        /// <summary>
        /// Calling this method will keep a permanent cache of some parts of the result, therefore
        /// if the OData arguments need to change after that, a new <see cref="ODataFlatQuery"/> must be created
        /// </summary>
        public SqlStatement PrepareStatement(
            Func<Type, string> sources,
            SqlStatementParameters ps,
            int currentUserId,
            TimeZoneInfo currentUserTimeZone)
        {

            // (1) Prepare the JOIN's clause
            SqlJoinClause joinClause = PrepareJoin();
            var joinTree = joinClause.JoinTree;
            var joinSql = joinClause.ToSql(sources);

            // (2) Prepare the SELECT clause
            SqlSelectClause selectClause = PrepareSelect(joinTree);
            var selectSql = selectClause.ToSql();

            // (3) Prepare the inner join with the principal query (if any)
            string principalQuerySql = PreparePrincipalQuery(sources, ps, currentUserId, currentUserTimeZone);

            // (4) Prepare the WHERE clause
            string whereSql = PrepareWhere(sources, joinTree, ps, currentUserId, currentUserTimeZone);

            // (5) Prepare the ORDERBY clause
            string orderbySql = PrepareOrderBy(joinTree);

            // (6) Prepare the OFFSET and FETCH clauses
            string offsetFetchSql = PrepareOffsetFetch();

            // (7) Finally put together the final SQL statement and return it
            string sql = PrepareSql(
                    selectSql: selectSql,
                    joinSql: joinSql,
                    principalQuerySql: principalQuerySql,
                    whereSql: whereSql,
                    orderbySql: orderbySql,
                    offsetFetchSql: offsetFetchSql
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

        public string WhereSql(
            Func<Type, string> sources,
            JoinTree joins,
            SqlStatementParameters ps,
            int currentUserId,
            TimeZoneInfo currentUserTimeZone)
        {
            return PrepareWhere(sources, joins, ps, currentUserId, currentUserTimeZone);
        }

        public SqlJoinClause JoinSql()
        {
            return PrepareJoin();
        }


        /// <summary>
        /// Calling this method will keep a permanent cache of some parts of the result, therefore
        /// if the OData arguments need to change after that, a new <see cref="ODataFlatQuery"/> must be created
        /// </summary>
        private string PrepareStatementAsPrincipal(
            Func<Type, string> sources,
            SqlStatementParameters ps,
            ArraySegment<string> pathToCollectionProperty,
            int currentUserId,
            TimeZoneInfo currentUserTimeZone)
        {
            // (1) Prepare the JOIN's clause
            SqlJoinClause joinClause = PrepareJoin(pathToCollectionProperty);
            var joinTree = joinClause.JoinTree;
            var joinSql = joinClause.ToSql(sources);

            // (2) Prepare the SELECT clause
            SqlSelectClause selectClause = PreparePrincipalSelect(joinTree, pathToCollectionProperty);
            var selectSql = selectClause.ToSql();

            // (3) Prepare the inner join with the principal query (if any)
            string principalQuerySql = PreparePrincipalQuery(sources, ps, currentUserId, currentUserTimeZone);

            // (4) Prepare the WHERE clause
            string whereSql = PrepareWhere(sources, joinTree, ps, currentUserId, currentUserTimeZone);

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
            string sql = PrepareSql(
                    selectSql: selectSql,
                    joinSql: joinSql,
                    principalQuerySql: principalQuerySql,
                    whereSql: whereSql,
                    orderbySql: orderbySql,
                    offsetFetchSql: offsetFetchSql
                );

            // (8) Return the result
            return sql;
        }

        private string PrepareSql(string selectSql, string joinSql, string principalQuerySql, string whereSql, string orderbySql, string offsetFetchSql)
        {
            var finalSQL = new StringBuilder();

            finalSQL.AppendLine(selectSql);
            finalSQL.Append(joinSql);

            if (!string.IsNullOrWhiteSpace(principalQuerySql))
            {
                finalSQL.AppendLine();
                finalSQL.Append(principalQuerySql);
            }

            if (!string.IsNullOrWhiteSpace(whereSql))
            {
                finalSQL.AppendLine();
                finalSQL.Append(whereSql);
            }

            if (!string.IsNullOrWhiteSpace(orderbySql))
            {
                finalSQL.AppendLine();
                finalSQL.Append(orderbySql);
            }

            if (!string.IsNullOrWhiteSpace(offsetFetchSql))
            {
                finalSQL.AppendLine();
                finalSQL.Append(offsetFetchSql);
            }

            return finalSQL.ToString();
        }

        private SqlSelectClause PrepareSelect(JoinTree joinTree)
        {
            var selects = new HashSet<(string Symbol, string PropName)>();
            var columns = new List<(string Symbol, ArraySegment<string> Path, string PropName)>();
            void AddSelect(string symbol, ArraySegment<string> path, string propName)
            {
                // NULL happens when there is a select that has been segmented from the middle
                // and the first section of the segment no longer terminates with a simple property
                propName = propName ?? "Id";

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
            Expand = Expand ?? ExpandExpression.Empty;
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
                    for (int i = 0; i < path.Length; i++)
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

                        // The Id is ALWAYS included in every returned DTO
                        AddSelect(join.Symbol, subpath, "Id");

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
                var join = joinTree[path];
                AddSelect(joinTree.Symbol, path, ForeignKeyToPrincipalQuery);
            }

            // Change the hash set to a list so that the order is well defined
            return new SqlSelectClause(columns);
        }

        private SqlSelectClause PreparePrincipalSelect(JoinTree joinTree, ArraySegment<string> pathToCollection)
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
                (symbol, pathToCollectionEntity, "Id")
            };

            return new SqlSelectClause(columns);
        }

        private SqlJoinClause PrepareJoin(ArraySegment<string>? pathToCollection = null)
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
            var joinTree = JoinTree.Make(ResultType, allPaths);
            return new SqlJoinClause(joinTree);
        }

        private string PreparePrincipalQuery(Func<Type, string> sources, SqlStatementParameters ps, int currentUserId, TimeZoneInfo currentUserTimeZone)
        {
            string principalQuerySql = "";
            if (PrincipalQuery != null)
            {
                // Get the inner sql and append 4 spaces before each line for aesthetics
                string innerSql = PrincipalQuery.PrepareStatementAsPrincipal(sources, ps, PathToCollectionPropertyInPrincipal, currentUserId, currentUserTimeZone);
                innerSql = ODataTools.IndentLines(innerSql);

                principalQuerySql = $@"INNER JOIN (
{innerSql}
) As S ON [S].[Id] = [P].[{ForeignKeyToPrincipalQuery}]";
            }

            return principalQuerySql;
        }

        private string PrepareWhere(Func<Type, string> sources, JoinTree joinTree, SqlStatementParameters ps, int currentUserId, TimeZoneInfo currentUserTimeZone)
        {
            // Where is cached 
            if (_cachedWhere == null)
            {
                string FilterToSql(FilterExpression exp)
                {
                    if (exp is FilterBrackets bracketsExp)
                    {
                        return $"({FilterToSql(bracketsExp.Inner)})";
                    }

                    if (exp is FilterConjunction conExp)
                    {
                        return $"({FilterToSql(conExp.Left)}) AND ({FilterToSql(conExp.Right)})";
                    }

                    if (exp is FilterDisjunction disExp)
                    {
                        return $"({FilterToSql(disExp.Left)}) OR ({FilterToSql(disExp.Right)})";
                    }

                    if (exp is FilterAtom atom)
                    {
                        // (A) Prepare the symbol corresponding to the path, e.g. P1
                        var join = joinTree[atom.Path];
                        if (join == null)
                        {
                            // Developer mistake
                            throw new InvalidOperationException($"The path '{string.Join('/', atom.Path)}' was not found in the joinTree");
                        }
                        var symbol = join.Symbol;

                        // (B) Determine the type of the property
                        var propName = atom.Property;
                        var prop = join.Type.GetProperty(propName);
                        if (prop == null)
                        {
                            // Developer mistake
                            throw new InvalidOperationException($"Could not find property {propName} on type {join.Type}");
                        }

                        var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                        // (C) Prepare the value (e.g. "'Huntington Rd.'")
                        var valueString = atom.Value;
                        object value;
                        bool isNull = false;
                        if (valueString == "null")
                        {
                            value = null;
                            isNull = true;
                        }
                        else if (valueString?.ToLower() == "me")
                        {
                            value = currentUserId;
                        }
                        else
                        {
                            if (propType == typeof(string) || propType == typeof(char) || propType == typeof(HierarchyId))
                            {
                                if (!valueString.StartsWith("'") || !valueString.EndsWith("'"))
                                {
                                    // Developer mistake
                                    throw new InvalidOperationException($"Property {propName} is of type String, therefore the value it is compared to must be enclosed in single quotation marks");
                                }

                                valueString = valueString.Substring(1, valueString.Length - 2);
                            }

                            try
                            {
                                value = valueString.ChangeType(prop.PropertyType, currentUserTimeZone);
                            }
                            catch (ArgumentException)
                            {
                                // Developer mistake
                                throw new InvalidOperationException($"The filter value '{valueString}' could not be parsed into a valid {propType}");
                            }
                        }

                        string paramSymbol = isNull ? "NULL" : "@" + ps.AddParameter(value);

                        // (D) parse the operator (e.g. "eq")
                        string propSQL = $"[{symbol}].[{atom.Property}]";
                        switch (atom.Op?.ToLower() ?? "")
                        {
                            case "gt":
                                return $"{propSQL} > {paramSymbol}";

                            case "ge":
                                return $"{propSQL} >= {paramSymbol}";

                            case "lt":
                                return $"{propSQL} < {paramSymbol}";

                            case "le":
                                return $"{propSQL} <= {paramSymbol}";

                            case "eq":
                                string eqSql = isNull ? "IS" : "=";
                                return $"{propSQL} {eqSql} {paramSymbol}";

                            case "ne":
                                string neSql = isNull ? "IS NOT" : "<>";
                                return $"{propSQL} {neSql} {paramSymbol}";

                            case "contains": // Must be text
                                if (propType != typeof(string))
                                {
                                    // Developer mistake
                                    throw new InvalidOperationException($"Property {propName} is not of type String, therefore cannot use the operator '{atom.Op}'");
                                }

                                return $"{propSQL} LIKE N'%' + {paramSymbol} + N'%'";

                            case "ncontains": // Must be text
                                if (propType != typeof(string))
                                {
                                    // Developer mistake
                                    throw new InvalidOperationException($"Property {propName} is not of type String, therefore cannot use the operator '{atom.Op}'");
                                }

                                return $"{propSQL} NOT LIKE N'%' + {paramSymbol} + N'%'";

                            //case "childofh": // Must be hierarchy Id
                            //    if (propType != typeof(HierarchyId))
                            //    {
                            //        // Developer mistake
                            //        throw new InvalidOperationException($"Property {propName} is not of type hierarchyid, therefore cannot use the operator '{atom.Op}'");
                            //    }

                            //    return $"{propSQL}.GetAncestor(1) = {paramSymbol}";

                            case "childof": // Must be hierarchy Id
                                {
                                    if (propType != typeof(HierarchyId))
                                    {
                                        // Developer mistake
                                        throw new InvalidOperationException($"Property {propName} is not of type hierarchyid, therefore cannot use the operator '{atom.Op}'");
                                    }

                                    var treeSource = sources(join.Type);
                                    string parentNode = isNull ? "HierarchyId::GetRoot()" : 
                                        $"(SELECT [Node] FROM {treeSource} As [T] WHERE [T].[Id] = {paramSymbol})";

                                    return $"{propSQL}.GetAncestor(1) = {parentNode}";
                                }

                            //case "descendantofh": // Must be hierarchy Id
                                
                            //    if (propType != typeof(HierarchyId))
                            //    {
                            //        // Developer mistake
                            //        throw new InvalidOperationException($"Property {propName} is not of type hierarchyid, therefore cannot use the operator '{atom.Op}'");
                            //    }

                            //    return $"{propSQL}.IsDescendantOf({paramSymbol}) = 1";

                            case "descendantof": // Must be hierarchy Id
                                {
                                    if (propType != typeof(HierarchyId))
                                    {
                                        // Developer mistake
                                        throw new InvalidOperationException($"Property {propName} is not of type hierarchyid, therefore cannot use the operator '{atom.Op}'");
                                    }

                                    var treeSource = sources(join.Type);
                                    string parentNode = isNull ? "HierarchyId::GetRoot()" :
                                        $"(SELECT [Node] FROM {treeSource} As [T] WHERE [T].[Id] = {paramSymbol})";

                                    return $"{propSQL}.IsDescendantOf({parentNode}) = 1";
                                }

                            default:
                                // Developer mistake
                                throw new InvalidOperationException($"The filter operator '{atom.Op}' is not recognized");
                        }

                    }

                    // Programmer mistake
                    throw new InvalidOperationException("Unknown AST type");
                }

                string whereFilter = null;
                string whereInIds = null;

                if (Filter != null)
                {
                    whereFilter = FilterToSql(Filter);
                }

                if (Ids != null)
                {
                    string paramName = ps.AddParameter(Ids);
                    var isIntKey = (Nullable.GetUnderlyingType(KeyType) ?? KeyType) == typeof(int);
                    string value = isIntKey ? "CONVERT(INT, VALUE)" : "VALUE";
                    whereInIds = $"[P].[Id] IN (SELECT {value} AS Id FROM STRING_SPLIT(@{paramName}, ','))";
                }

                // The final WHERE clause
                string whereSql;
                if (whereFilter != null && whereInIds != null)
                {
                    whereSql = $"({whereFilter}) AND ({whereInIds})";
                }
                else if (whereFilter != null)
                {
                    whereSql = whereFilter;
                }
                else if (whereInIds != null)
                {
                    whereSql = whereInIds;
                }
                else
                {
                    whereSql = "";
                }

                if (!string.IsNullOrEmpty(whereSql))
                {
                    whereSql = "WHERE " + whereSql;
                }

                _cachedWhere = whereSql;
            }

            return _cachedWhere;
        }

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

            string orderbySql = ""; //  "ORDER BY Id DESC"; // Default order by
            if (orderbys.Count > 0)
            {
                orderbySql = "ORDER BY " + string.Join(", ", orderbys);
            }

            return orderbySql;
        }

        private string PrepareOffsetFetch()
        {
            string offsetFetchSql = "";
            if (Skip != null || Top != null)
            {
                offsetFetchSql = offsetFetchSql + $"OFFSET {Skip ?? 0} ROWS";
            }

            if (Top != null)
            {
                offsetFetchSql = offsetFetchSql + $" FETCH NEXT {Top.Value} ROWS ONLY";
            }

            return offsetFetchSql;
        }
    }
}
