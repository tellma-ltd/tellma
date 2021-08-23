using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Tellma.Model.Common;
using Tellma.Repository.Common.Queryex;
using Tellma.Utilities.Common;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Used to build and run select queries to an SQL Server database which return dynamic
    /// columns each based on an arbitrary expression.
    /// </summary>
    /// <typeparam name="T">The root type of the query.</typeparam>
    public class FactQuery<T> where T : Entity
    {
        // From constructor
        private readonly QueryArgumentsFactory _factory;

        // Through setter methods
        private int? _top;
        private int? _skip;
        private ExpressionFilter _filter;
        private ExpressionFactSelect _select;
        private ExpressionOrderBy _orderby;

        private Dictionary<QueryexBase, int> _selectHash;
        private Dictionary<QueryexBase, int> SelectIndexDictionary => _selectHash ??= _select
                .Select((exp, index) => (exp, index))
                .ToDictionary(pair => pair.exp, pair => pair.index);

        /// <summary>
        /// Creates an instance of <see cref="FactQuery{T}"/>.
        /// </summary>
        /// <param name="factory">Function that can asynchronously return the <see cref="QueryArguments"/> when loading data.</param>
        public FactQuery(QueryArgumentsFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Clones the <see cref="FactQuery{T}"/> into a new one.
        /// </summary>
        private FactQuery<T> Clone()
        {
            var clone = new FactQuery<T>(_factory)
            {
                _top = _top,
                _skip = _skip,
                _filter = _filter,
                _select = _select,
                _orderby = _orderby
            };

            return clone;
        }

        /// <summary>
        /// Applies a <see cref="ExpressionFactSelect"/> to specify which expressions to select from the table.
        /// </summary>
        public FactQuery<T> Select(ExpressionFactSelect selects)
        {
            var clone = Clone();
            clone._select = selects;
            return clone;
        }

        /// <summary>
        /// A version of <see cref="Select(ExpressionFactSelect)"/> that accepts a string.
        /// </summary>
        public FactQuery<T> Select(string selects)
        {
            return Select(ExpressionFactSelect.Parse(selects));
        }

        /// <summary>
        /// Applies a <see cref="ExpressionOrderBy"/> to specify how to order the result.
        /// </summary>
        public FactQuery<T> OrderBy(ExpressionOrderBy orderby)
        {
            var clone = Clone();
            clone._orderby = orderby;
            return clone;
        }

        /// <summary>
        /// A version of <see cref="OrderBy(ExpressionOrderBy)"/> that accepts a string.
        /// </summary>
        public FactQuery<T> OrderBy(string orderby)
        {
            return OrderBy(ExpressionOrderBy.Parse(orderby));
        }

        /// <summary>
        /// Applies a <see cref="ExpressionFilter"/> to filter the result.
        /// </summary>
        public FactQuery<T> Filter(ExpressionFilter condition)
        {
            if (_top != null || _skip != null)
            {
                // Programmer mistake
                throw new InvalidOperationException($"Cannot filter the query again after either {nameof(Skip)} or {nameof(Top)} have been invoked.");
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
        /// A version of <see cref="Filter(ExpressionFilter)"/> that accepts a string.
        /// </summary>
        public FactQuery<T> Filter(string condition)
        {
            return Filter(ExpressionFilter.Parse(condition));
        }

        /// <summary>
        /// Instructs the query to load only the top N rows.
        /// </summary>
        public FactQuery<T> Top(int top)
        {
            var clone = Clone();
            clone._top = top;
            return clone;
        }

        /// <summary>
        /// Instructs the query to skip the first N rows.
        /// </summary>
        public FactQuery<T> Skip(int skip)
        {
            var clone = Clone();
            clone._skip = skip;
            return clone;
        }

        /// <summary>
        /// Executes the <see cref="FactQuery"/> against the SQL database and loads the result into memory as a <see cref="List{T}"/>
        /// </summary>
        public async Task<List<DynamicRow>> ToListAsync(QueryContext ctx, CancellationToken cancellation = default)
        {
            var (result, _) = await ToListAndCountInnerAsync(includeCount: false, maxCount: 0, ctx, cancellation: cancellation);
            return result;
        }

        /// <summary>
        /// Executes the <see cref="FactQuery"/> against the SQL database and loads the result into memory as a <see cref="List{T}"/> + their total count (without the orderby, select, expand, top or skip applied)
        /// </summary>
        public Task<(List<DynamicRow> result, int count)> ToListAndCountAsync(int maxCount, QueryContext ctx, CancellationToken cancellation = default)
        {
            return ToListAndCountInnerAsync(includeCount: true, maxCount: maxCount, ctx, cancellation: cancellation);
        }

        private async Task<(List<DynamicRow>, int count)> ToListAndCountInnerAsync(bool includeCount, int maxCount, QueryContext ctx2, CancellationToken cancellation)
        {
            var queryArgs = await _factory(cancellation);

            var connString = queryArgs.ConnectionString;
            var sources = queryArgs.Sources;
            var loader = queryArgs.Loader;

            var userId = ctx2.UserId;
            var userToday = ctx2.UserToday;

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
                var aggregation = exp.Aggregations().FirstOrDefault();
                if (aggregation != null)
                {
                    throw new QueryException($"Select cannot contain an aggregation function like: {aggregation.Name}.");
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
                    throw new QueryException($"Filter contains a condition with an aggregation function: {conditionWithAggregation}.");
                }
            }

            // ------------------------ Preparation Step

            // If all is good Prepare some universal variables and parameters
            var vars = new SqlStatementVariables();
            var ps = new SqlStatementParameters();
            var today = userToday ?? DateTime.Today;
            var now = DateTimeOffset.Now;

            // ------------------------ The SQL Generation Step

            // (1) Prepare the JOIN's clause
            var joinTrie = PrepareJoin();
            var joinSql = joinTrie.GetSql(sources);

            // Compilation context
            var ctx = new QxCompilationContext(joinTrie, sources, vars, ps, today, now, userId);

            // (2) Prepare all the SQL clauses
            var (selectSql, columnCount) = PrepareSelectSql(ctx);
            string whereSql = PrepareWhereSql(ctx);
            string orderbySql = PrepareOrderBySql(ctx);
            string offsetFetchSql = PrepareOffsetFetch();

            // (3) Put together the final SQL statement and return it
            string sql = QueryTools.CombineSql(
                    selectSql: selectSql,
                    joinSql: joinSql,
                    principalQuerySql: null,
                    whereSql: whereSql,
                    orderbySql: orderbySql,
                    offsetFetchSql: offsetFetchSql,
                    groupbySql: null,
                    havingSql: null,
                    selectFromTempSql: null
                );

            // ------------------------ Prepare the Count SQL
            string countSql = null;
            if (includeCount)
            {
                string countSelectSql = maxCount > 0 ? $"SELECT TOP {maxCount} [P].*" : "SELECT [P].*";

                countSql = QueryTools.CombineSql(
                       selectSql: countSelectSql,
                       joinSql: joinSql,
                       principalQuerySql: null,
                       whereSql: whereSql,
                       orderbySql: null,
                       offsetFetchSql: null,
                       groupbySql: null,
                       havingSql: null,
                       selectFromTempSql: null
                   );

                countSql = $@"SELECT COUNT(*) As [Count] FROM (
{countSql.IndentLines()}
) AS [Q]";
            }

            // ------------------------ Execute SQL and return Result
            
            var principalStatement = new SqlDynamicStatement(sql, columnCount);
            var args = new DynamicLoaderArguments
            {
                CountSql = countSql, // No counting in aggregate functions
                PrincipalStatement = principalStatement,
                DimensionAncestorsStatements = null, // No ancestors
                Variables = vars,
                Parameters = ps,
            };

            var result = await loader.LoadDynamic(connString, args, cancellation);
            return (result.Rows, result.Count);
        }

        /// <summary>
        /// Prepares the join tree.
        /// </summary>
        private JoinTrie PrepareJoin()
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

            // This will represent the mapping from paths to symbols
            var joinTree = JoinTrie.Make(TypeDescriptor.Get<T>(), allPaths);
            return joinTree;
        }

        private (string select, int count) PrepareSelectSql(QxCompilationContext ctx)
        {
            var selects = new List<string>(_select.Count());

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
                    selects.Add(sql.DeBracket());// e.g.: [P].[Name]
                }
            }

            var columnCount = selects.Count; // The columns added later will not be loaded
            var selectSql = $"SELECT " + string.Join(", ", selects);

            return (selectSql, columnCount);
        }

        /// <summary>
        /// Prepares the WHERE clause of the SQL query from the <see cref="_filterExp"/> argument: WHERE ABC
        /// </summary>
        private string PrepareWhereSql(QxCompilationContext ctx)
        {
            string whereSql = _filter?.Expression?.CompileToBoolean(ctx)?.DeBracket();

            // Add the "WHERE" keyword
            if (!string.IsNullOrEmpty(whereSql))
            {
                whereSql = "WHERE " + whereSql;
            }

            return whereSql;
        }

        /// <summary>
        /// Prepares the ORDER BY clause of the SQL query using the <see cref="_select"/> argument: ORDER BY ABC
        /// </summary>
        private string PrepareOrderBySql(QxCompilationContext ctx)
        {
            var orderByAtomsCount = _orderby?.Count() ?? 0;
            if (orderByAtomsCount == 0 || _top == null)
            {
                return "";
            }
            else
            {
                var orderbys = new List<string>(orderByAtomsCount);
                foreach (var expression in _orderby)
                {
                    var orderby = expression.CompileToNonBoolean(ctx);
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

                // If Entity has Id, we always make sure Id is in the OrderBy
                // clause to guarantee a deterministic order when paging
                if (ctx.Joins.EntityDescriptor.HasId)
                {
                    static bool IsId(QueryexBase ex)
                    {
                        return ex is QueryexColumnAccess ca && ca.Path.Length == 0 && ca.Property == "Id";
                    }

                    if (_orderby.All(ex => !IsId(ex)))
                    {
                        var exp = ExpressionOrderBy.Parse("Id asc").Single();
                        var orderbyId = exp.CompileToNonBoolean(ctx);
                        orderbys.Add(orderbyId);
                    }
                }

                return "ORDER BY " + string.Join(", ", orderbys);
            }
        }

        /// <summary>
        /// Prepares the "OFFSET X ROWS FETCH NEXT Y ROWS ONLY" clause using <see cref="_skip"/> and <see cref="_top"/> arguments
        /// </summary>
        private string PrepareOffsetFetch()
        {
            string sql = "";
            if (_skip != null || _top != null)
            {
                sql += $"OFFSET {_skip ?? 0} ROWS";
            }

            if (_top != null)
            {
                sql += $" FETCH NEXT {_top} ROWS ONLY";
            }

            return sql;
        }
    }
}
