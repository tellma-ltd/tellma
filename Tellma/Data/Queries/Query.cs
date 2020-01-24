using Tellma.Entities;
using Tellma.Services.Utilities;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Used to execute flat SELECT queries on a SQL Server database
    /// </summary>
    /// <typeparam name="T">The expected type of the result</typeparam>
    public class Query<T> where T : Entity
    {
        // From constructor
        private readonly QueryArgumentsFactory _factory;

        // From setter methods
        private int? _top;
        private int? _skip;
        private List<FilterExpression> _filterConditions;
        private SelectExpression _select;
        private ExpandExpression _expand;
        private OrderByExpression _orderby;
        private IEnumerable<object> _ids;
        private IEnumerable<object> _parentIds;
        private bool _includeRoots;
        private string _fromSql;
        private string _preSql;
        private SqlParameter[] _parameters;
        private List<(string ParamName, object Value)> _additionalParameters;

        /// <summary>
        /// Creates a new instance of <see cref="Query"/>
        /// </summary>
        /// <param name="factory">Delegate that can asynchronously returns the <see cref="QueryArguments"/></param>
        public Query(QueryArgumentsFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Clones the <see cref="Query{T}"/> into a new one. Used internally
        /// </summary>
        private Query<T> Clone()
        {
            var clone = new Query<T>(_factory)
            {
                _top = _top,
                _skip = _skip,
                _filterConditions = _filterConditions?.ToList(),
                _select = _select,
                _expand = _expand,
                _orderby = _orderby,
                _ids = _ids == null ? null : new List<object>(_ids),
                _parentIds = _parentIds == null ? null : new List<object>(_parentIds),
                _includeRoots = _includeRoots,
                _fromSql = _fromSql,
                _preSql = _preSql,
                _parameters = _parameters?.ToArray(),
                _additionalParameters = _additionalParameters?.ToList()
            };

            return clone;
        }

        /// <summary>
        /// Applies a <see cref="SelectExpression"/> on the <see cref="Query{T}"/> to determine what columns need to be returned
        /// </summary>
        public Query<T> Select(SelectExpression select)
        {
            var clone = Clone();
            clone._select = select;
            return clone;
        }

        /// <summary>
        /// A version of <see cref="Select(SelectExpression)"/> that accepts a string
        /// </summary>
        public Query<T> Select(string select)
        {
            return Select(SelectExpression.Parse(select));
        }

        /// <summary>
        /// Applies an <see cref="ExpandExpression"/> on the <see cref="Query{T}"/> to determine what related tables to
        /// include in the result, any tables touched by the <see cref="SelectExpression"/> will have it overriding the <see cref="ExpandExpression"/>
        /// </summary>
        public Query<T> Expand(ExpandExpression expand)
        {
            var clone = Clone();
            clone._expand = expand;
            return clone;
        }

        /// <summary>
        /// A version of <see cref="Expand(ExpandExpression)" that accepts a string
        /// </summary>
        public Query<T> Expand(string expand)
        {
            return Expand(ExpandExpression.Parse(expand));
        }

        /// <summary>
        /// Applies a <see cref="FilterExpression"/> to filter the result
        /// </summary>
        public Query<T> Filter(FilterExpression condition)
        {
            if (_top != null || _skip != null)
            {
                throw new InvalidOperationException("Cannot filter the query again after either Skip or Top have been invoked");
            }

            var clone = Clone();
            if (condition != null)
            {
                clone._filterConditions ??= new List<FilterExpression>();
                clone._filterConditions.Add(condition);
            }

            return clone;
        }

        /// <summary>
        /// A version of <see cref="Filter(FilterExpression)" that accepts a string
        /// </summary>
        public Query<T> Filter(string filter)
        {
            return Filter(FilterExpression.Parse(filter));
        }

        /// <summary>
        /// Restricts the <see cref="Query{T}"/> to loading the entities with the specified list of Ids
        /// </summary>
        /// <typeparam name="TKey">The type of the ids (either string or int)</typeparam>
        public Query<T> FilterByIds<TKey>(params TKey[] ids)
        {
            if (!IsEntityWithKey())
            {
                // Developer mistake
                throw new InvalidOperationException($"Type {typeof(T).Name} does not inherit from {typeof(EntityWithKey).Name}, yet '{nameof(FilterByIds)}' has been invoked on a Query<{typeof(T).Name}>");
            }

            var clone = Clone();
            clone._ids = ids.Cast<object>();
            return clone;
        }

        /// <summary>
        /// Restricts the <see cref="Query{T}"/> to loading the children of the entities with the specified list of Ids,
        /// and the root nodes if includeRoots is set to true, this is only available on tree types (containing a property ParentId)
        /// </summary>
        /// <typeparam name="TKey">The type of the parent ids (either string or int)</typeparam>
        public Query<T> FilterByParentIds<TKey>(List<TKey> parentIds, bool includeRoots)
        {
            if (!IsEntityWithKey())
            {
                // Developer mistake
                throw new InvalidOperationException($"Type {typeof(T).Name} does not inherit from {typeof(EntityWithKey).Name}, yet '{nameof(FilterByParentIds)}' has been invoked on a Query<{typeof(T).Name}>");
            }

            if (!typeof(T).HasProperty("ParentId"))
            {
                // Developer mistake
                throw new InvalidOperationException($"Type {typeof(T).Name} does not have a ParentId property, yet '{nameof(FilterByParentIds)}' has been invoked on a Query<{typeof(T).Name}>");
            }

            var t1 = typeof(T).GetProperty("ParentId").PropertyType;
            var t2 = typeof(T).GetProperty("Id").PropertyType;
            if ((Nullable.GetUnderlyingType(t1) ?? t1) != (Nullable.GetUnderlyingType(t2) ?? t2))
            {
                // Developer mistake
                throw new InvalidOperationException($"Type {typeof(T).Name} has an Id and ParentId properties with mismatching types");
            }

            var clone = Clone();
            clone._parentIds = parentIds.Cast<object>();
            clone._includeRoots = includeRoots;
            return clone;
        }

        /// <summary>
        /// Applies a <see cref="OrderByExpression"/> to set the order of the result, it is used in
        /// conjunction with <see cref="Top(int)"/> and <see cref="Skip(int)"/> to implement paging
        /// </summary>
        public Query<T> OrderBy(OrderByExpression orderby)
        {
            var clone = Clone();
            clone._orderby = orderby;
            return clone;
        }

        /// <summary>
        /// A version of <see cref="OrderBy(OrderByExpression)" that accepts a string
        /// </summary>
        public Query<T> OrderBy(string orderBy)
        {
            return OrderBy(OrderByExpression.Parse(orderBy));
        }

        /// <summary>
        /// Applies a top argument instructing the query to load only the top N results
        /// </summary>
        public Query<T> Top(int top)
        {
            var clone = Clone();
            clone._top = top;
            return clone;
        }

        /// <summary>
        /// Applies a skip argument instructing the query to skip N results 
        /// </summary>
        /// <param name="skip"></param>
        /// <returns></returns>
        public Query<T> Skip(int skip)
        {
            var clone = Clone();
            clone._skip = skip;
            return clone;
        }

        /// <summary>
        /// Low level API, instructs the <see cref="Query{T}"/> to use the provided SQL code for the root source (the part in the SQL statement after FROM ...)
        /// </summary>
        /// <param name="fromSql">The SQL to use after FROM, can be either a table-valued function call, or a SELECT statement surrounded in parentheses</param>
        /// <param name="preSql">Any SQL to be prepended at the beginning before the main SQL script. This cannot contain any SELECT query or any code that returns a result set</param>
        /// <param name="parameters">An array of SQL parameters to include in the result</param>
        public Query<T> FromSql(string fromSql, string preSql = null, params SqlParameter[] parameters)
        {
            var clone = Clone();
            clone._fromSql = fromSql;
            clone._preSql = preSql;
            clone._parameters = parameters;

            return clone;
        }

        /// <summary>
        /// If the Query is for a parametered fact table such as <see cref="SummaryEntry"/>, the parameters
        /// must be supplied this method must be supplied through this method before loading any data
        /// </summary>
        public Query<T> AdditionalParameters(params (string ParamName, object Value)[] parameters)
        {
            var clone = Clone();
            if (clone._additionalParameters == null)
            {
                clone._additionalParameters = new List<(string ParamName, object Value)>();
            }

            clone._additionalParameters.AddRange(parameters);           

            return clone;
        }

        /// <summary>
        /// Returns the total count of all the rows that will be returned by this query, this is usually useful before calling <see cref="Top(int)"/>
        /// </summary>
        public async Task<int> CountAsync()
        {
            var args = await _factory();
            var conn = args.Connection;
            var sources = args.Sources;
            var userId = args.UserId;
            var userToday = args.UserToday;
            var localizer = args.Localizer;

            SelectExpression selectExp = IsEntityWithKey() ? SelectExpression.Parse("Id") : _select;
            FilterExpression filterExp = _filterConditions?.Aggregate(
                (e1, e2) => new FilterConjunction { Left = e1, Right = e2 });

            // To prevent SQL injection
            ValidatePathsAndProperties(selectExp, null, filterExp, null, localizer);

            // Prepare the query
            var flatQuery = new QueryInternal
            {
                ResultType = typeof(T),
                KeyType = typeof(T).GetProperty("Id")?.PropertyType, // NULL if there is no key
                Select = selectExp,
                Filter = filterExp,
                Ids = _ids,
                ParentIds = _parentIds,
                IncludeRoots = _includeRoots,
                Skip = _skip,
                Top = _top,
                FromSql = _fromSql
            };

            // Prepare the parameters
            var ps = new SqlStatementParameters();

            // Add the from sql parameters
            if(_parameters != null)
            {
                foreach(var p in _parameters)
                {
                    ps.AddParameter(p);
                }
            }

            if (_additionalParameters != null)
            {
                foreach (var (paramName, value) in _additionalParameters)
                {
                    ps.AddParameter(new SqlParameter
                    {
                        ParameterName = paramName,
                        Value = value ?? DBNull.Value
                    });
                }
            }

            // Load the statement
            var sql = flatQuery.PrepareStatement(sources, ps, userId, userToday).Sql;
            sql = QueryTools.IndentLines(sql);
            sql = $@"SELECT COUNT(*) As [Count] FROM (
{sql}
) As [S]";

            // always prepend the preparatory sql to the beginning
            if (!string.IsNullOrWhiteSpace(_preSql))
            {
                sql = $@"{_preSql}

{sql}";
            }

            // Execute the SqlStatement
            using var cmd = conn.CreateCommand();

            // Prepare the SQL command
            cmd.CommandText = sql;
            foreach (var parameter in ps)
            {
                cmd.Parameters.Add(parameter);
            }

            // It is alawys closed, but we add this code anyways for robustness
            bool ownsConnection = conn.State != System.Data.ConnectionState.Open;
            if (ownsConnection)
            {
                conn.Open();
            }

            try
            {
                // Execute the query and return the result
                int count = (int)await cmd.ExecuteScalarAsync();
                return count;
            }
            finally
            {
                // Otherwise we might get an error
                cmd.Parameters.Clear();

                // This block is never entered, but we put anyways for robustness
                if (ownsConnection)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        /// <summary>
        /// Executes the <see cref="Query{T}"/> against the SQL database and loads the result into memory as a <see cref="List{T}"/>
        /// </summary>
        public async Task<List<T>> ToListAsync()
        {
            var args = await _factory();
            var conn = args.Connection;
            var sources = args.Sources;
            var userId = args.UserId;
            var userTimeZone = args.UserToday;
            var localizer = args.Localizer;

            _orderby ??= (typeof(T).GetProperty("Id") != null ? OrderByExpression.Parse("Id desc") :
                throw new InvalidOperationException($"Query<{typeof(T).Name}> was executed without an orderby clause"));

            // Prepare all the query parameters
            SelectExpression selectExp = _select;
            ExpandExpression expandExp = _expand;
            OrderByExpression orderbyExp = _orderby;
            FilterExpression filterExp = _filterConditions?.Aggregate(
                (e1, e2) => new FilterConjunction { Left = e1, Right = e2 });

            // To prevent SQL injection
            ValidatePathsAndProperties(selectExp, expandExp, filterExp, orderbyExp, localizer);

            // ------------------------ Step #1

            // Segment the paths of select and expand along the one-to-many relationships, each one-to-many relationship will
            // result in a new internal query for the child collection with the original query as its principal query
            var segments = new Dictionary<ArraySegment<string>, QueryInternal>(new PathEqualityComparer());

            // Helper method for creating a an internal query, will be used later in both the select and the expand loops
            QueryInternal MakeFlatQuery(ArraySegment<string> previousFullPath, ArraySegment<string> subPath, Type type)
            {
                QueryInternal principalQuery = previousFullPath == null ? null : segments[previousFullPath];
                ArraySegment<string> pathToCollectionPropertyInPrincipal = previousFullPath == null ? null : subPath;
                Type keyType = type.GetProperty("Id")?.PropertyType;

                if (principalQuery != null && keyType == null)
                {
                    // Programmer mistake
                    throw new InvalidOperationException($"Type {type.Name} has no Id property, yet it is used as a navigation collection on another entity");
                }

                string foreignKeyToPrincipalQuery = null;
                bool isAncestorExpand = false;
                if (principalQuery != null)
                {
                    // This loop retrieves the collection property
                    var collectionProperty = principalQuery.ResultType.GetProperty(pathToCollectionPropertyInPrincipal[0]);
                    for (int i = 1; i < pathToCollectionPropertyInPrincipal.Count; i++)
                    {
                        var step = pathToCollectionPropertyInPrincipal[i];
                        collectionProperty = collectionProperty.PropertyType.GetProperty(step);
                    }

                    if (collectionProperty.IsParent())
                    {
                        foreignKeyToPrincipalQuery = "ParentId";
                        isAncestorExpand = true;
                    }
                    else
                    {
                        // Must be a collection then
                        foreignKeyToPrincipalQuery = collectionProperty.GetCustomAttribute<ForeignKeyAttribute>()?.Name;
                        if (string.IsNullOrWhiteSpace(foreignKeyToPrincipalQuery))
                        {
                            // Programmer mistake
                            throw new InvalidOperationException($"Navigation collection {collectionProperty.Name} on type {collectionProperty.DeclaringType} is not adorned with the associated foreign key");
                        }
                    }
                }

                if (isAncestorExpand)
                {
                    // the path to parent entity is the path above minus the "Parent"
                    var pathToParentEntity = new ArraySegment<string>(
                        array: pathToCollectionPropertyInPrincipal.Array,
                        offset: 0,
                        count: pathToCollectionPropertyInPrincipal.Count - 1);

                    // Adding this causes the principal query to always include ParentId in the select clause
                    principalQuery.PathsToParentEntitiesWithExpandedAncestors.Add(pathToParentEntity);
                }

                var flatQuery = new QueryInternal
                {
                    PrincipalQuery = principalQuery,
                    IsAncestorExpand = isAncestorExpand,
                    PathToCollectionPropertyInPrincipal = pathToCollectionPropertyInPrincipal,
                    ForeignKeyToPrincipalQuery = foreignKeyToPrincipalQuery,
                    ResultType = type,
                    KeyType = keyType,
                    OrderBy = OrderByExpression.Parse("Id")
                };

                return flatQuery;
            }

            if (selectExp != null)
            {
                var selectTree = PathTree.Build(typeof(T), selectExp.Select(e => e.Path));
                foreach (var selectAtom in selectExp)
                {
                    // This breaks up the path into multiple segments along the one-to-many and child-parent relationship boundaries
                    var pathSegments = selectTree.GetSegments(selectAtom.Path);
                    ArraySegment<string> previousFullPath = null;
                    foreach (var (fullPath, subPath, type) in pathSegments.SkipLast(1))
                    {
                        if (!segments.ContainsKey(fullPath))
                        {
                            QueryInternal flatQuery = MakeFlatQuery(previousFullPath, subPath, type);

                            flatQuery.Select = new SelectExpression
                            {
                                new SelectAtom
                                {
                                    Path = subPath.SkipLast(1).ToArray(),
                                    Property = null
                                }
                            };

                            segments[fullPath] = flatQuery;
                        }

                        previousFullPath = fullPath;
                    }

                    // The last segment is turned into a select atom and added to the select property
                    {
                        var (fullPath, subPath, _) = pathSegments.Last();
                        segments[previousFullPath].Select.Add(new SelectAtom
                        {
                            Path = subPath.ToArray(),
                            Property = selectAtom.Property
                        });
                    }
                }
            }

            expandExp ??= ExpandExpression.RootSingleton;
            {
                var expandTree = PathTree.Build(typeof(T), expandExp.Select(e => e.Path));
                foreach (var expandAtom in expandExp)
                {
                    var pathSegments = expandTree.GetSegments(expandAtom.Path);
                    ArraySegment<string> previousFullPath = null;
                    foreach (var (fullPath, subPath, type) in pathSegments.SkipLast(1))
                    {
                        if (!segments.ContainsKey(fullPath))
                        {
                            QueryInternal flatQuery = MakeFlatQuery(previousFullPath, subPath, type);
                            segments[fullPath] = flatQuery;
                        }

                        if (previousFullPath != null)
                        {
                            var flatQuery = segments[previousFullPath];
                            if (subPath.Count >= 2) // If there is more than just the collection property, then we add an expand
                            {
                                flatQuery.Expand ??= new ExpandExpression();
                                flatQuery.Expand.Add(new ExpandAtom { Path = subPath.SkipLast(1).ToArray() });
                            }
                        }
                        previousFullPath = fullPath;
                    }

                    // The last segment is turned into a expand atom
                    {
                        var (_, subPath, _) = pathSegments.Last();
                        if (subPath.Count > 0)
                        {
                            var flatQuery = segments[previousFullPath];
                            flatQuery.Expand ??= new ExpandExpression();
                            flatQuery.Expand.Add(new ExpandAtom
                            {
                                Path = subPath.ToArray(),
                            });
                        }
                    }
                }
            }

            // The remaining odata arguments are exclusvie to the root
            var root = segments[new string[0]];
            root.Filter = filterExp;
            root.OrderBy = orderbyExp;
            root.Ids = _ids;
            root.ParentIds = _parentIds;
            root.IncludeRoots = _includeRoots;
            root.Skip = _skip;
            root.Top = _top;
            root.FromSql = _fromSql;

            // ------------------------ Step #2

            // Prepare the parameters
            var ps = new SqlStatementParameters();

            // Add the fromSql parameters
            if (_parameters != null)
            {
                foreach (var p in _parameters)
                {
                    ps.AddParameter(p);
                }
            }

            if (_additionalParameters != null)
            {
                foreach (var (paramName, value) in _additionalParameters)
                {
                    ps.AddParameter(new SqlParameter
                    {
                        ParameterName = paramName,
                        Value = value ?? DBNull.Value
                    });
                }
            }

            // Prepare the SqlStatements
            List<SqlStatement> statements = segments.Values
                .Select(q => q.PrepareStatement(sources, ps, userId, userTimeZone))
                .ToList(); // The order matters for the Entity loader

            // Load the entities
            var result = await EntityLoader.LoadStatements<T>(
                statements: statements,
                preparatorySql: null,
                ps: ps,
                conn: conn);

            // Return the entities
            return result.Cast<T>().ToList();
        }

        /// <summary>
        /// Executes the <see cref="Query{T}"/> against the SQL database returning only the first row if exists and null otherwise
        /// </summary>
        public async Task<T> FirstOrDefaultAsync()
        {
            var query = this;
            if (_orderby == null)
            {
                if (!IsEntityWithKey())
                {
                    // Programmer mistake
                    throw new InvalidOperationException($"{nameof(FirstOrDefaultAsync)} was invoked without an orderby parameter to retrieve a type {typeof(T).Name} which doesn't inherit from {typeof(EntityWithKey)}");
                }
                else
                {
                    query = query.OrderBy(OrderByExpression.Parse("Id"));
                }
            }

            // We reuse ToList for first or default
            var entities = await query.Top(1).ToListAsync();
            return entities.FirstOrDefault();
        }

        /// <summary>
        /// Useful for RLS security enforcement, this method takes a list of permission criteria each associated with an index
        /// and returns a mapping from every entity Id in the original query to all the criteria that are satified by this entity
        /// </summary>
        public async Task<IEnumerable<IndexedId<TKey>>> GetIndexToIdMap<TKey>(IEnumerable<IndexAndCriteria> criteriaIndexes)
        {
            return await GetIndexMapInner<TKey>(criteriaIndexes);
        }

        /// <summary>
        /// Useful for RLS security enforcement, this method takes a list of permission criteria each associated with an index
        /// and returns a mapping from every entity Id (which is specifically an INT) in the original query to all the criteria
        /// that are satified by this entity, this is different from <see cref="GetIndexToIdMap(IEnumerable{IndexAndCriteria})"/>
        /// in that it is useful when the origianl query is dynamically constructed using <see cref="FromSql(string, string, SqlStatementParameter[])"/>
        /// by passing in new data that doesn't have Ids, and hence the indexes of the items in the memory list are used as Ids instead
        /// </summary>
        public async Task<IEnumerable<IndexedId<int>>> GetIndexToIndexMap(IEnumerable<IndexAndCriteria> criteriaIndexes)
        {
            return await GetIndexMapInner<int>(criteriaIndexes);
        }

        /// <summary>
        /// Internal Implementation
        /// </summary>
        private async Task<IEnumerable<IndexedId<TKey>>> GetIndexMapInner<TKey>(IEnumerable<IndexAndCriteria> criteriaIndexes)
        {
            var args = await _factory();
            var conn = args.Connection;
            var sources = args.Sources;
            var userId = args.UserId;
            var userTimeZone = args.UserToday;
            var localizer = args.Localizer;

            if (_select != null || _expand != null)
            {
                // Programmer mistake
                throw new InvalidOperationException($"Cannot call {nameof(GetIndexToIdMap)} when select or expand are specified");
            }

            var orderByExp = _orderby;
            FilterExpression filterExp = _filterConditions?.Aggregate(
                (e1, e2) => new FilterConjunction { Left = e1, Right = e2 });

            // To prevent SQL injection
            ValidatePathsAndProperties(null, null, filterExp, orderByExp, localizer);

            // Prepare the internal query
            var flatQuery = new QueryInternal
            {
                ResultType = typeof(T),
                KeyType = typeof(TKey),
                Filter = filterExp,
                Ids = _ids,
                ParentIds = _parentIds,
                IncludeRoots = _includeRoots,
                OrderBy = orderByExp,
                Skip = _skip,
                Top = _top,
                FromSql = _fromSql
            };

            // Prepare the parameters
            var ps = new SqlStatementParameters();

            // Add the fromSql parameters
            if (_parameters != null)
            {
                foreach (var p in _parameters)
                {
                    ps.AddParameter(p);
                }
            }

            if (_additionalParameters != null)
            {
                foreach (var (paramName, value) in _additionalParameters)
                {
                    ps.AddParameter(new SqlParameter
                    {
                        ParameterName = paramName,
                        Value = value ?? DBNull.Value
                    });
                }
            }

            // Use the internal query to create the SQL
            var sourceSql = flatQuery.PrepareStatement(sources, ps, userId, userTimeZone).Sql;

            List<StringBuilder> toBeUnioned = new List<StringBuilder>(criteriaIndexes.Count());
            foreach (var criteriaIndex in criteriaIndexes)
            {
                int index = criteriaIndex.Index;
                string criteria = criteriaIndex.Criteria;

                var criteriaExp = FilterExpression.Parse(criteria);
                ValidatePathsAndProperties(null, null, criteriaExp, null, localizer);

                var criteriaQuery = new QueryInternal
                {
                    ResultType = typeof(T),
                    KeyType = typeof(TKey),
                    Filter = criteriaExp
                };

                JoinTree joinTree = criteriaQuery.JoinSql();
                string joinSql = joinTree.GetSql(sources, fromSql: $@"({sourceSql})");
                string whereSql = criteriaQuery.WhereSql(sources, joinTree, ps, userId, userTimeZone);


                var sqlBuilder = new StringBuilder();
                sqlBuilder.AppendLine($"SELECT [P].[Id], {index} As [Index]");
                sqlBuilder.AppendLine(joinSql);
                sqlBuilder.AppendLine(whereSql);

                toBeUnioned.Add(sqlBuilder);
            }

            string sql = toBeUnioned.Select(s => s.ToString()).Aggregate((s1, s2) => $@"{s1}

UNION

{s2}");

            if (!string.IsNullOrWhiteSpace(_preSql))
            {
                sql = $@"{_preSql}

{sql}";
            }

            using var cmd = conn.CreateCommand();

            // Prepare the SQL command
            cmd.CommandText = sql;
            foreach (var parameter in ps)
            {
                cmd.Parameters.Add(parameter);
            }

            // This block is never entered, but we add it anyways for robustness sake
            bool ownsConnection = conn.State != System.Data.ConnectionState.Open;
            if (ownsConnection)
            {
                conn.Open();
            }

            try
            {
                var result = new List<IndexedId<TKey>>();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    // Loop over the result from the database
                    while (await reader.ReadAsync())
                    {
                        var dbId = reader["Id"];
                        var dbIndex = reader.GetInt32(1);

                        result.Add(new IndexedId<TKey>
                        {
                            Id = (TKey)dbId,
                            Index = dbIndex
                        });
                    }
                }

                return result;
            }
            finally
            {
                // Otherwise we might get an error
                cmd.Parameters.Clear();

                // This block is never entered but we add it here for robustness
                if (ownsConnection)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        /// <summary>
        /// To prevent SQL injection attacks
        /// </summary>
        private void ValidatePathsAndProperties(SelectExpression selectExp, ExpandExpression expandExp, FilterExpression filterExp, OrderByExpression orderbyExp, IStringLocalizer localizer)
        {
            // This is important to avoid SQL injection attacks

            // Select
            if (selectExp != null)
            {
                PathValidator selectPathValidator = new PathValidator();
                foreach (var atom in selectExp)
                {
                    // AddPath(atom.Path, atom.Property);
                    selectPathValidator.AddPath(atom.Path, atom.Property);
                }

                // Make sure the paths are valid (Protects against SQL injection)
                selectPathValidator.Validate(typeof(T), localizer, "select",
                    allowLists: true,
                    allowSimpleTerminals: true,
                    allowNavigationTerminals: false);
            }

            // Expand
            if (expandExp != null)
            {
                PathValidator expandPathTree = new PathValidator();
                foreach (var atom in expandExp)
                {
                    expandPathTree.AddPath(atom.Path);
                }

                // Make sure the paths are valid (Protects against SQL injection)
                expandPathTree.Validate(typeof(T), localizer, "expand",
                    allowLists: true,
                    allowSimpleTerminals: false,
                    allowNavigationTerminals: true);
            }

            // Filter
            if (filterExp != null)
            {
                PathValidator filterPathTree = new PathValidator();
                foreach (var atom in filterExp)
                {
                    // AddPath(atom.Path, atom.Property);
                    filterPathTree.AddPath(atom.Path, atom.Property);
                }

                // Make sure the paths are valid (Protects against SQL injection)
                filterPathTree.Validate(typeof(T), localizer, "filter",
                    allowLists: false,
                    allowSimpleTerminals: true,
                    allowNavigationTerminals: false);
            }

            // Order By
            if (orderbyExp != null)
            {
                PathValidator orderbyPathTree = new PathValidator();
                foreach (var atom in orderbyExp)
                {
                    // AddPath(atom.Path, atom.Property);
                    orderbyPathTree.AddPath(atom.Path, atom.Property);
                }

                orderbyPathTree.Validate(typeof(T), localizer, "orderby",
                    allowLists: false,
                    allowSimpleTerminals: true,
                    allowNavigationTerminals: false);
            }
        }

        /// <summary>
        /// Simply checks if the type argument <see cref="T"/> inherites from <see cref="EntityWithKey"/>
        /// </summary>
        private bool IsEntityWithKey()
        {
            return typeof(T).IsSubclassOf(typeof(EntityWithKey));
        }

        /// <summary>
        /// Data structure to help efficiently segment the SELECT and EXPAND arguments along the one-to-many relationships
        /// </summary>
        private class PathTree : Dictionary<string, PathTree>
        {
            /// <summary>
            /// The type of the current step
            /// </summary>
            public Type Type { get; set; }

            /// <summary>
            /// Indicates that the current step represents a list navigation property
            /// </summary>
            public bool IsList { get; set; }

            /// <summary>
            /// Indicates that the current step represents a parent property in a tree data structure
            /// </summary>
            public bool IsParent { get; set; }

            /// <summary>
            /// Create a path tree using the provided rood type and collection of paths
            /// </summary>
            public static PathTree Build(Type type, IEnumerable<string[]> paths)
            {
                var root = new PathTree { Type = type, IsList = true, IsParent = false };

                foreach (var path in paths)
                {
                    var currentTree = root;
                    foreach (var step in path)
                    {
                        if (!currentTree.ContainsKey(step))
                        {
                            var prop = currentTree.Type.GetProperty(step);
                            if (prop == null)
                            {
                                throw new InvalidOperationException($"Property {prop.Name} does not exist on type {currentTree.Type}");
                            }

                            var isParent = prop.Name == "Parent" && currentTree.Type.GetProperty("Node")?.PropertyType == typeof(HierarchyId);
                            var isList = prop.PropertyType.IsList();
                            var propType = isList ? prop.PropertyType.GenericTypeArguments[0] : prop.PropertyType;

                            currentTree[step] = new PathTree
                            {
                                Type = propType,
                                IsList = isList,
                                IsParent = isParent
                            };
                        }

                        currentTree = currentTree[step];
                    }
                }

                return root;
            }

            public IEnumerable<(ArraySegment<string> FullPath, ArraySegment<string> SubPath, Type Type)> GetSegments(string[] path)
            {
                int offset = 0;
                int count = 0;

                // always return the empty paths first
                {
                    var fullPath = new ArraySegment<string>(path, 0, offset + count);
                    var subPath = new ArraySegment<string>(path, offset, count);
                    yield return (fullPath, subPath, Type);
                }

                // Return the segments if any
                var currentTree = this;
                for (var i = 0; i < path.Length; i++)
                {
                    var step = path[i];
                    currentTree = currentTree[step];
                    count++;

                    if (currentTree.IsList || currentTree.IsParent)
                    {
                        var fullPath = new ArraySegment<string>(path, 0, offset + count);
                        var subPath = new ArraySegment<string>(path, offset, count);

                        yield return (fullPath, subPath, currentTree.Type);

                        // Add the count to the offset and then zero the count
                        offset += count;
                        count = 0;
                    }
                }

                // always return the rest of the path last, even if it was empty
                {
                    var fullPath = new ArraySegment<string>(path, 0, offset + count);
                    var subPath = new ArraySegment<string>(path, offset, count);

                    yield return (fullPath, subPath, null);
                }
            }
        }
    }
}
