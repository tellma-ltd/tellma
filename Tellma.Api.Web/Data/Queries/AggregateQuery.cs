using Tellma.Model.Application;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Tellma.Entities.Descriptors;
using Tellma.Services.Utilities;
using System.Text;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Used to execute GROUP BY queries on a SQL Server database
    /// </summary>
    /// <typeparam name="T">The expected type of the result</typeparam>
    public class AggregateQuery<T> where T : Entity
    {
        // From constructor
        private readonly QueryArgumentsFactory _factory;

        // Through setter methods
        private string _tempTableName;
        private int? _top;
        private ExpressionFilter _filter;
        private ExpressionHaving _having;
        private ExpressionAggregateSelect _select;
        private ExpressionAggregateOrderBy _orderby;

        private Dictionary<QueryexBase, int> _selectHash;
        private Dictionary<QueryexBase, int> SelectIndexDictionary => _selectHash ??= _select
                .Select((exp, index) => (exp, index))
                .ToDictionary(pair => pair.exp, pair => pair.index);

        /// <summary>
        /// Creates an instance of <see cref="AggregateQuery{T}"/>
        /// </summary>
        /// <param name="conn">The connection to use when loading the results</param>
        /// <param name="sources">Mapping from every type into SQL code that can be used to query that type</param>
        /// <param name="localizer">For validation error messages</param>
        /// <param name="userId">Used as context when preparing certain filter expressions</param>
        /// <param name="userTimeZone">Used as context when preparing certain filter expressions</param>
        public AggregateQuery(QueryArgumentsFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Clones the <see cref="AggregateQuery{T}"/> into a new one. Used internally
        /// </summary>
        private AggregateQuery<T> Clone()
        {
            var clone = new AggregateQuery<T>(_factory)
            {
                _top = _top,
                _filter = _filter,
                _having = _having,
                _select = _select,
                _orderby = _orderby,
            };

            return clone;
        }

        /// <summary>
        /// Applies a <see cref="ExpressionAggregateSelect"/> to specify which dimensions and measures
        /// must be returned, dimensions are specified without an aggregate function, measures do not have an aggregate function
        /// </summary>
        public AggregateQuery<T> Select(ExpressionAggregateSelect selects)
        {
            var clone = Clone();
            clone._select = selects;
            return clone;
        }

        /// <summary>
        /// Applies a <see cref="ExpressionAggregateOrderBy"/> to how to order the result
        /// </summary>
        public AggregateQuery<T> OrderBy(ExpressionAggregateOrderBy orderby)
        {
            var clone = Clone();
            clone._orderby = orderby;
            return clone;
        }

        /// <summary>
        /// Applies a <see cref="ExpressionFilter"/> to filter the result
        /// </summary>
        public AggregateQuery<T> Filter(ExpressionFilter condition)
        {
            if (_top != null)
            {
                // Programmer mistake
                throw new InvalidOperationException($"Cannot filter the query after {nameof(Top)} has been invoked");
            }

            var clone = Clone();
            if (condition != null)
            {
                if (clone._filter == null)
                {
                    clone._filter = condition;
                }
                else
                {
                    clone._filter = ExpressionFilter.Conjunction(clone._filter, condition);
                }
            }

            return clone;
        }

        /// <summary>
        /// Applies a <see cref="ExpressionHaving"/> to filter the grouped result
        /// </summary>
        public AggregateQuery<T> Having(ExpressionHaving having)
        {
            if (_top != null)
            {
                // Programmer mistake
                throw new InvalidOperationException($"Cannot apply a having argument after {nameof(Top)} has been invoked");
            }

            var clone = Clone();
            clone._having = having;
            return clone;
        }

        /// <summary>
        /// Instructs the query to load only the top N rows
        /// </summary>
        public AggregateQuery<T> Top(int top)
        {
            var clone = Clone();
            clone._top = top;
            return clone;
        }

        public async Task<(List<DynamicRow> result, IEnumerable<TreeDimensionResult> trees)> ToListAsync(CancellationToken cancellation)
        {
            var queryArgs = await _factory(cancellation);

            var conn = queryArgs.Connection;
            var sources = queryArgs.Sources;
            var userId = queryArgs.UserId;
            var userToday = queryArgs.UserToday;
            var localizer = queryArgs.Localizer;
            var logger = queryArgs.Logger;

            // ------------------------ Validation Step

            // SELECT Validation
            if (_select == null)
            {
                string message = $"The select argument is required";
                throw new InvalidOperationException(message);
            }

            // Make sure that measures are well formed: every column access is wrapped inside an aggregation function
            foreach (var exp in _select)
            {
                if (exp.ContainsAggregations) // This is a measure
                {
                    // Every column access must descend from an aggregation function
                    var exposedColumnAccess = exp.UnaggregatedColumnAccesses().FirstOrDefault();
                    if (exposedColumnAccess != null)
                    {
                        throw new QueryException($"Select parameter contains a measure with a column access {exposedColumnAccess} that is not included within an aggregation.");
                    }
                }
            }

            // ORDER BY Validation
            if (_orderby != null)
            {
                foreach (var exp in _orderby)
                {
                    // Order by cannot be a constant
                    if (!exp.ContainsAggregations && !exp.ContainsColumnAccesses)
                    {
                        throw new QueryException("OrderBy parameter cannot be a constant, every orderby expression must contain either an aggregation or a column access.");
                    }
                }
            }

            // FILTER Validation
            if (_filter != null)
            {
                var conditionWithAggregation = _filter.Expression.Aggregations().FirstOrDefault();
                if (conditionWithAggregation != null)
                {
                    throw new QueryException($"Filter contains a condition with an aggregation function: {conditionWithAggregation}");
                }
            }


            // HAVING Validation
            if (_having != null)
            {
                // Every column access must descend from an aggregation function
                var exposedColumnAccess = _having.Expression.UnaggregatedColumnAccesses().FirstOrDefault();
                if (exposedColumnAccess != null)
                {
                    throw new QueryException($"Having parameter contains a column access {exposedColumnAccess} that is not included within an aggregation.");
                }
            }

            // ------------------------ Preparation Step

            // If all is good Prepare some universal variables and parameters
            var vars = new SqlStatementVariables();
            var ps = new SqlStatementParameters();
            var today = userToday ?? DateTime.Today;
            var now = DateTimeOffset.Now;

            // ------------------------ Tree Analysis Step

            // By convention if A.B.Id AND A.B.ParentId are both in the select expression, 
            // then this is a tree dimension and we return all the ancestors of A.B, 
            // What do we select for the ancestors? All non-aggregated expressions in
            // the original select that contain column accesses exclusively starting with A.B
            var additionalNodeSelects = new List<QueryexColumnAccess>();
            var ancestorsStatements = new List<DimensionAncestorsStatement>();
            {
                // Find all column access atoms that terminate with ParentId, those are the potential tree dimensions
                var parentIdSelects = _select
                    .Where(e => e is QueryexColumnAccess ca && ca.Property == "ParentId")
                    .Cast<QueryexColumnAccess>();

                foreach (var parentIdSelect in parentIdSelects)
                {
                    var pathToTreeEntity = parentIdSelect.Path; // A.B

                    // Confirm it's a tree dimension
                    var idSelect = _select.FirstOrDefault(e => e is QueryexColumnAccess ca && ca.Property == "Id" && ca.PathEquals(pathToTreeEntity));
                    if (idSelect != null)
                    {
                        // Prepare the Join Trie
                        var treeType = TypeDescriptor.Get<T>();
                        foreach (var step in pathToTreeEntity)
                        {
                            treeType = treeType.NavigationProperty(step)?.TypeDescriptor ??
                                throw new QueryException($"Property {step} does not exist on type {treeType.Name}.");
                        }

                        // Create or Get the name of the Node column
                        string nodeColumnName = NodeColumnName(additionalNodeSelects.Count);
                        additionalNodeSelects.Add(new QueryexColumnAccess(pathToTreeEntity, "Node")); // Tell the principal query to include this node

                        // Get all atoms that contain column accesses exclusively starting with A.B
                        var principalSelectsWithMatchingPrefix = _select
                            .Where(exp => exp.ColumnAccesses().All(ca => ca.PathStartsWith(pathToTreeEntity)));

                        // Calculate the target indices
                        var targetIndices = principalSelectsWithMatchingPrefix
                            .Select(exp => SelectIndexDictionary[exp]);

                        // Remove the prefix from all column accesses
                        var ancestorSelects = principalSelectsWithMatchingPrefix
                            .Select(exp => exp.Clone(prefixToRemove: pathToTreeEntity));

                        var allPaths = ancestorSelects.SelectMany(e => e.ColumnAccesses()).Select(e => e.Path);
                        var joinTrie = JoinTrie.Make(treeType, allPaths);
                        var joinSql = joinTrie.GetSql(sources, fromSql: null);

                        // Prepare the Context
                        var ctx = new QxCompilationContext(joinTrie, sources, vars, ps, today, now, userId);

                        // Prepare the SQL components
                        var selectSql = PrepareAncestorSelectSql(ctx, ancestorSelects);
                        var principalQuerySql = PreparePrincipalQuerySql(nodeColumnName);

                        // Combine the SQL components
                        string sql = QueryTools.CombineSql(
                            selectSql: selectSql,
                            joinSql: joinSql,
                            principalQuerySql: principalQuerySql,
                            whereSql: null,
                            orderbySql: null,
                            offsetFetchSql: null,
                            groupbySql: null,
                            havingSql: null,
                            selectFromTempSql: null);

                        // Get the index of the id select
                        int idIndex = SelectIndexDictionary[idSelect];

                        // Create and add the statement object
                        var statement = new DimensionAncestorsStatement(idIndex, sql, targetIndices);
                        ancestorsStatements.Add(statement);
                    }
                }
            }


            // ------------------------ The SQL Generation Step

            // (1) Prepare the JOIN's clause
            var principalJoinTrie = PreparePrincipalJoin();
            var principalJoinSql = principalJoinTrie.GetSql(sources, fromSql: null);

            // Compilation context
            var principalCtx = new QxCompilationContext(principalJoinTrie, sources, vars, ps, today, now, userId);

            // (2) Prepare all the SQL clauses
            var (principalSelectSql, principalGroupbySql, principalColumnCount) = PreparePrincipalSelectAndGroupBySql(principalCtx, additionalNodeSelects);
            string principalWhereSql = PreparePrincipalWhereSql(principalCtx);
            string principalHavingSql = PreparePrincipalHavingSql(principalCtx);
            string principalOrderbySql = PreparePrincipalOrderBySql();
            string principalSelectFromTempSql = PrepareSelectFromTempSql();

            // (3) Put together the final SQL statement and return it
            string principalSql = QueryTools.CombineSql(
                    selectSql: principalSelectSql,
                    joinSql: principalJoinSql,
                    principalQuerySql: null,
                    whereSql: principalWhereSql,
                    orderbySql: principalOrderbySql,
                    offsetFetchSql: null,
                    groupbySql: principalGroupbySql,
                    havingSql: principalHavingSql,
                    selectFromTempSql: principalSelectFromTempSql
                );

            // ------------------------ Execute SQL and return Result
            var principalStatement = new SqlDynamicStatement(principalSql, principalColumnCount);

            var (result, trees, _) = await EntityLoader.LoadDynamicStatement(
                principalStatement: principalStatement,
                dimAncestorsStatements: ancestorsStatements,
                includeCount: false,
                vars: vars,
                ps: ps,
                conn: conn,
                logger: logger,
                cancellation: cancellation);

            return (result, trees);
        }

        public string GenerateTempTableName()
        {
            return _tempTableName ??= $"#Query_{Guid.NewGuid():N}";
        }

        /// <summary>
        /// Prepares the join tree 
        /// </summary>
        private JoinTrie PreparePrincipalJoin()
        {
            // construct the join tree
            var allPaths = new List<string[]>();
            if (_select != null)
            {
                allPaths.AddRange(_select.ColumnAccesses().Select(e => e.Path));
            }

            if (_orderby != null)
            {
                allPaths.AddRange(_orderby.ColumnAccesses().Select(e => e.Path));
            }

            if (_filter != null)
            {
                allPaths.AddRange(_filter.ColumnAccesses().Select(e => e.Path));
            }

            if (_having != null)
            {
                allPaths.AddRange(_having.ColumnAccesses().Select(e => e.Path));
            }

            // This will represent the mapping from paths to symbols
            var joinTree = JoinTrie.Make(TypeDescriptor.Get<T>(), allPaths);
            return joinTree;
        }

        private string PrepareAncestorSelectSql(QxCompilationContext ctx, IEnumerable<QueryexBase> selectAtoms)
        {
            var selects = new List<string>(selectAtoms.Count());
            foreach (var exp in selectAtoms)
            {
                var (sql, type, _) = exp.CompileNative(ctx);
                if (type == QxType.Boolean || type == QxType.Geography)
                {
                    // Those three types are not supported for loading into C#
                    throw new QueryException($"A select expression {exp} cannot have a type {type}.");
                }
                else if (type == QxType.HierarchyId)
                {
                    continue; // In the ancestors
                }
                else
                {
                    sql = sql.DeBracket(); // e.g.: [P].[Name] AS [C3]
                    selects.Add(sql);
                }
            }

            string selectSql = $"SELECT DISTINCT " + string.Join(", ", selects);
            return selectSql;
        }

        private string PreparePrincipalQuerySql(string nodeColumnName)
        {
            return @$"INNER JOIN {GenerateTempTableName()} As [S]
ON [S].{nodeColumnName}.IsDescendantOf([P].[Node]) = 1 AND [S].{nodeColumnName} <> [P].[Node]";
        }

        private (string select, string groupby, int count) PreparePrincipalSelectAndGroupBySql(QxCompilationContext ctx, List<QueryexColumnAccess> additionalNodeSelects)
        {
            List<string> selects = new List<string>(_select.Count());
            List<string> groupbys = new List<string>();

            // This is to make the group by list unique
            HashSet<QueryexBase> groupbyHash = new HashSet<QueryexBase>();

            foreach (var exp in _select)
            {
                var (sql, type, _) = exp.CompileNative(ctx);
                if (type == QxType.Boolean || type == QxType.HierarchyId || type == QxType.Geography)
                {
                    // Those three types are not supported for loading into C#
                    throw new QueryException($"A select expression {exp} cannot have a type {type}.");
                }
                else
                {
                    string columnName = ColumnName(selects.Count); // e.g.: [C3]
                    selects.Add($"{sql.DeBracket()} AS {columnName}");// e.g.: [P].[Name] AS [C3]

                    if (!exp.ContainsAggregations && exp.ContainsColumnAccesses && groupbyHash.Add(exp))
                    {
                        groupbys.Add(sql);
                    }
                }
            }

            int columnCount = selects.Count; // The columns added later will not be loaded

            // Those are used by the ancestor expand statement
            foreach (var exp in additionalNodeSelects)
            {
                var sql = exp.CompileToNonBoolean(ctx);
                string columnName = NodeColumnName(selects.Count - columnCount); // e.g.: [Node0]
                selects.Add($"{sql.DeBracket()} AS {columnName}");// e.g.: [P].[Name] AS [Node0]
                groupbys.Add(sql);
            }

            // Prepare the SQL
            string top = _top == 0 ? "" : $"TOP {_top} ";
            string selectSql = $"SELECT {top}" + string.Join(", ", selects);

            if (_tempTableName != null)
            {
                selectSql += $" INTO {_tempTableName}";
            }

            string groupbySql = "";
            if (groupbys.Count > 0)
            {
                groupbySql = "GROUP BY " + string.Join(", ", groupbys);
            }

            return (selectSql, groupbySql, columnCount);
        }

        /// <summary>
        /// Prepares the WHERE clause of the SQL query from the <see cref="_filterExp"/> argument: WHERE ABC
        /// </summary>
        private string PreparePrincipalWhereSql(QxCompilationContext ctx)
        {
            // (1) Prepare the aggregations filter
            // If all aggregatiosn have filters Sum(X, A), Count(Y, B)
            // We append a AND (A OR B) to the filter, to avoid all-Null rows
            var aggregations = _select.SelectMany(e => e.Aggregations());
            QueryexBase aggFilter = null;
            foreach (var agg in aggregations)
            {
                if (agg.Arguments.Length < 2)
                {
                    // At least one aggregation has no condition, we abandon the aggregation filter
                    aggFilter = null;
                    break;
                }
                else
                {
                    var condition = agg.Arguments[1];
                    aggFilter = aggFilter == null ? condition : new QueryexBinaryOperator("or", aggFilter, condition);
                }
            }

            // (2) Prepare the main filter
            var mainFilter = _filter?.Expression;

            // (3) Prepare the combined filter
            QueryexBase combinedFilter = null;
            if (mainFilter != null && aggFilter != null)
            {
                combinedFilter = new QueryexBinaryOperator("and", mainFilter, aggFilter);
            } 
            else if (mainFilter != null)
            {
                combinedFilter = mainFilter;
            }
            else if (aggFilter != null)
            {
                combinedFilter = aggFilter;
            }

            // Prepare the SQL
            string whereSql = combinedFilter?.CompileToBoolean(ctx)?.DeBracket();

            // Add the "WHERE" keyword
            if (!string.IsNullOrEmpty(whereSql))
            {
                whereSql = "WHERE " + whereSql;
            }

            return whereSql;
        }

        /// <summary>
        /// Prepares the WHERE clause of the SQL query from the <see cref="_filterExp"/> argument: WHERE ABC
        /// </summary>
        private string PreparePrincipalHavingSql(QxCompilationContext ctx)
        {
            string havingSql = _having?.Expression?.CompileToBoolean(ctx)?.DeBracket();

            // Add the "HAVING" keyword
            if (!string.IsNullOrEmpty(havingSql))
            {
                havingSql = "HAVING " + havingSql;
            }

            return havingSql;
        }

        /// <summary>
        /// Prepares the ORDER BY clause of the SQL query using the <see cref="_select"/> argument: ORDER BY ABC
        /// </summary>
        private string PreparePrincipalOrderBySql()
        {
            var orderByAtomsCount = _orderby?.Count() ?? 0;
            if (orderByAtomsCount == 0 || _top == null)
            {
                return "";
            }
            else
            {
                List<string> orderbys = new List<string>(orderByAtomsCount);
                foreach (var expression in _orderby)
                {
                    if (!SelectIndexDictionary.TryGetValue(expression, out int index))
                    {
                        throw new QueryException($"Orderby expression {expression} is not present in the select parameter");
                    }

                    string orderby = ColumnName(index);
                    if (expression.IsDescending)
                    {
                        orderby += " DESC";
                    }
                    else
                    {
                        orderby += " ASC";
                    }

                    orderbys.Add(orderby);
                }

                return "ORDER BY " + string.Join(", ", orderbys);
            }
        }

        private string PrepareSelectFromTempSql()
        {
            if (_tempTableName != null)
            {
                return $"SELECT * FROM {_tempTableName}";
            }
            else
            {
                return null;
            }
        }

        private string ColumnName(int index) => $"[C{index}]";

        private string NodeColumnName(int index) => $"[Node{index}]";
    }

    public class SqlDynamicColumn
    {
        public SqlDynamicColumn(QueryexBase exp, int targetIndex)
        {
            Expression = exp;
            TargetIndex = targetIndex;
        }

        /// <summary>
        /// The original <see cref="QueryexBase"/> onto which this column is based
        /// </summary>
        public QueryexBase Expression { get; }

        /// <summary>
        /// Where in the resulting <see cref="DynamicRow"/> should the result be inserted
        /// </summary>
        public int TargetIndex { get; }
    }

    public class DimensionAncestorsStatement
    {
        public DimensionAncestorsStatement(int idIndex, string sql, IEnumerable<int> targetIndices)
        {
            IdIndex = idIndex;
            Sql = sql;
            TargetIndices = targetIndices.ToList();
        }

        public int IdIndex { get; set; } // Key

        public string Sql { get; set; }

        public IEnumerable<int> TargetIndices { get; set; }
    }

    public class TreeDimensionResult
    {
        public TreeDimensionResult(int idIndex, int minIndex)
        {
            IdIndex = idIndex;
            MinIndex = minIndex;
            Result = new List<DynamicRow>();
        }

        public int IdIndex { get; }

        public int MinIndex { get; }

        public List<DynamicRow> Result { get; }
    }
}
