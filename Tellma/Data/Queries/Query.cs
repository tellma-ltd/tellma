using Tellma.Entities;
using Tellma.Services.Utilities;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Tellma.Entities.Descriptors;

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
        private List<ExpressionFilter> _filterConditions;
        private ExpressionSelect _select;
        private ExpressionExpand _expand;
        private ExpressionOrderBy _orderby;
        private IEnumerable<object> _ids;
        private IEnumerable<object> _parentIds;
        private string _propName;
        private IEnumerable<object> _values;
        private bool _includeRoots;
        private string _fromSql;
        private string _preSql;
        private SqlParameter[] _parameters;
        private List<SqlParameter> _additionalParameters;

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
                _ids = _ids?.ToList(),
                _parentIds = _parentIds?.ToList(),
                _propName = _propName,
                _values = _values?.ToList(),
                _includeRoots = _includeRoots,
                _fromSql = _fromSql,
                _preSql = _preSql,
                _parameters = _parameters?.ToArray(),
                _additionalParameters = _additionalParameters?.ToList()
            };

            return clone;
        }

        /// <summary>
        /// Applies a <see cref="ExpressionSelect"/> on the <see cref="Query{T}"/> to determine what columns need to be returned
        /// </summary>
        public Query<T> Select(ExpressionSelect select)
        {
            var clone = Clone();
            clone._select = select;
            return clone;
        }

        /// <summary>
        /// A version of <see cref="Select(ExpressionSelect)"/> that accepts a string
        /// </summary>
        public Query<T> Select(string select)
        {
            return Select(ExpressionSelect.Parse(select));
        }

        /// <summary>
        /// Applies an <see cref="ExpressionExpand"/> on the <see cref="Query{T}"/> to determine what related tables to
        /// include in the result, any tables touched by the <see cref="ExpressionSelect"/> will have it overriding the <see cref="ExpressionExpand"/>
        /// </summary>
        public Query<T> Expand(ExpressionExpand expand)
        {
            var clone = Clone();
            clone._expand = expand;
            return clone;
        }

        /// <summary>
        /// A version of <see cref="Expand(ExpressionExpand)" that accepts a string
        /// </summary>
        public Query<T> Expand(string expand)
        {
            return Expand(ExpressionExpand.Parse(expand));
        }

        /// <summary>
        /// Applies a <see cref="ExpressionFilter"/> to filter the result
        /// </summary>
        public Query<T> Filter(ExpressionFilter condition)
        {
            if (_top != null || _skip != null)
            {
                throw new InvalidOperationException("Cannot filter the query again after either Skip or Top have been invoked");
            }

            var clone = Clone();
            if (condition != null)
            {
                clone._filterConditions ??= new List<ExpressionFilter>();
                clone._filterConditions.Add(condition);
            }

            return clone;
        }

        /// <summary>
        /// A version of <see cref="Filter(ExpressionFilter)" that accepts a string
        /// </summary>
        public Query<T> Filter(string filter)
        {
            return Filter(ExpressionFilter.Parse(filter));
        }

        /// <summary>
        /// Restricts the <see cref="Query{T}"/> to loading the entities with the specified list of Ids
        /// </summary>
        /// <typeparam name="TKey">The type of the ids (either string or int)</typeparam>
        public Query<T> FilterByIds<TKey>(IEnumerable<TKey> ids)
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
        /// Restricts the <see cref="Query{T}"/> to loading entities that have a certain property evaluating to one of the supplied values.
        /// For example: all entities where Code IN values
        /// </summary>
        public Query<T> FilterByPropertyValues(string propName, IEnumerable<object> values)
        {
            if (string.IsNullOrWhiteSpace(propName))
            {
                throw new ArgumentNullException(nameof(propName));
            }

            var desc = TypeDescriptor.Get<T>();
            if (desc.Property(propName) == null)
            {
                throw new InvalidOperationException($"Property {propName} does not exist on type {desc.Name}");
            }

            var clone = Clone();
            clone._propName = propName;
            clone._values = values?.ToList() ?? throw new ArgumentNullException(nameof(values)); // for immutability

            return clone;
        }

        /// <summary>
        /// Applies a <see cref="ExpressionOrderBy"/> to set the order of the result, it is used in
        /// conjunction with <see cref="Top(int)"/> and <see cref="Skip(int)"/> to implement paging
        /// </summary>
        public Query<T> OrderBy(ExpressionOrderBy orderby)
        {
            var clone = Clone();
            clone._orderby = orderby;
            return clone;
        }

        /// <summary>
        /// A version of <see cref="OrderBy(ExpressionOrderBy)" that accepts a string
        /// </summary>
        public Query<T> OrderBy(string orderBy)
        {
            return OrderBy(ExpressionOrderBy.Parse(orderBy));
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
        public Query<T> AdditionalParameters(params SqlParameter[] parameters)
        {
            var clone = Clone();
            if (clone._additionalParameters == null)
            {
                clone._additionalParameters = new List<SqlParameter>();
            }

            clone._additionalParameters.AddRange(parameters);

            return clone;
        }

        /// <summary>
        /// Returns the total count of all the rows that will be returned by this query, this is usually useful before calling <see cref="Top(int)"/>
        /// </summary>
        public async Task<int> CountAsync(CancellationToken cancellation, int maximum = 0)
        {
            var (_, count) = await ToListAndCountInnerAsync(includeCount: true, maxCount: maximum, cancellation: cancellation);
            return count;
        }

        /// <summary>
        /// Executes the <see cref="Query{T}"/> against the SQL database and loads the result into memory as a <see cref="List{T}"/>
        /// </summary>
        public async Task<List<T>> ToListAsync(CancellationToken cancellation)
        {
            var (result, _) = await ToListAndCountInnerAsync(includeCount: false, maxCount: 0, cancellation: cancellation);
            return result;
        }

        /// <summary>
        /// Executes the <see cref="Query{T}"/> against the SQL database and loads the result into memory as a <see cref="List{T}"/> + their total count (without the orderby, select, expand, top or skip applied)
        /// </summary>
        public Task<(List<T> result, int count)> ToListAndCountAsync(int maxCount, CancellationToken cancellation)
        {
            return ToListAndCountInnerAsync(includeCount: true, maxCount: maxCount, cancellation: cancellation);
        }

        /// <summary>
        /// Backbone for <see cref="CountAsync(CancellationToken, int)"/>, <see cref="ToListAsync(CancellationToken)"/> and <see cref="ToListAndCountAsync(CancellationToken)"/>
        /// </summary>
        private async Task<(List<T> result, int count)> ToListAndCountInnerAsync(bool includeCount, int maxCount, CancellationToken cancellation)
        {
            var args = await _factory(cancellation);
            var conn = args.Connection;
            var sources = args.Sources;
            var userId = args.UserId;
            var userToday = args.UserToday;
            var localizer = args.Localizer;
            var i = args.Instrumentation;

            using var _ = i.Block("Query.ToListAndCountInnerAsync");
            IDisposable block;

            block = i.Block("Get Descriptor");
            var resultDesc = TypeDescriptor.Get<T>();

            block.Dispose();
            block = i.Block("Validate Paths and Props");

            _orderby ??= (IsEntityWithKey() ? ExpressionOrderBy.Parse("Id desc") :
                throw new InvalidOperationException($"Query<{resultDesc.Type.Name}> was executed without an orderby clause"));

            // Prepare all the query parameters
            ExpressionSelect selectExp = _select;
            ExpressionExpand expandExp = _expand;
            ExpressionOrderBy orderbyExp = _orderby;
            ExpressionFilter filterExp = _filterConditions?.Aggregate(
                (e1, e2) => ExpressionFilter.Conjunction(e1, e2));

            // To prevent SQL injection
            ValidatePathsAndProperties(selectExp, expandExp, filterExp, orderbyExp, resultDesc, localizer);

            // ------------------------ Step #1

            block.Dispose();
            block = i.Block("Prepare QueryInternals");

            // Segment the paths of select and expand along the one-to-many relationships, each one-to-many relationship will
            // result in a new internal query for the child collection with the original query as its principal query
            var segments = new Dictionary<ArraySegment<string>, QueryInternal>(new PathEqualityComparer());

            // Helper method for creating a an internal query, will be used later in both the select and the expand loops
            QueryInternal MakeFlatQuery(ArraySegment<string> previousFullPath, ArraySegment<string> subPath, TypeDescriptor desc)
            {
                QueryInternal principalQuery = previousFullPath == null ? null : segments[previousFullPath];
                ArraySegment<string> pathToCollectionPropertyInPrincipal = previousFullPath == null ? null : subPath;

                if (principalQuery != null && desc.KeyType == KeyType.None)
                {
                    // Programmer mistake
                    throw new InvalidOperationException($"[Bug] Type {desc.Name} has no Id property, yet it is used as a navigation collection on another entity");
                }

                string foreignKeyToPrincipalQuery = null;
                bool isAncestorExpand = false;
                if (principalQuery != null)
                {
                    // This loop retrieves the entity descriptor that has the collection property
                    TypeDescriptor collectionPropertyEntity = principalQuery.ResultDescriptor;
                    int i = 0;
                    for (; i < pathToCollectionPropertyInPrincipal.Count - 1; i++)
                    {
                        var step = pathToCollectionPropertyInPrincipal[i];
                        collectionPropertyEntity = collectionPropertyEntity.NavigationProperty(step).TypeDescriptor;
                    }

                    // Get the collection/Parent property
                    string propertyName = pathToCollectionPropertyInPrincipal[i];
                    var property = collectionPropertyEntity.Property(propertyName);

                    if (property is NavigationPropertyDescriptor navProperty && navProperty.IsParent)
                    {
                        foreignKeyToPrincipalQuery = "ParentId";
                        isAncestorExpand = true;
                    }
                    else if (property is CollectionPropertyDescriptor collProperty)
                    {
                        // Must be a collection then
                        foreignKeyToPrincipalQuery = collProperty.ForeignKeyName;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Bug: Segment along a property {property.Name} on type {collectionPropertyEntity.Name} That is neither a collection nor a parent");
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

                // This is the orderby of related queries, and the default orderby of the root query
                var defaultOrderBy = ExpressionOrderBy.Parse(
                    desc.HasProperty("Index") ? "Index" :
                    desc.HasProperty("SortKey") ? "SortKey" : "Id");

                // Prepare the flat query and return it
                var flatQuery = new QueryInternal
                {
                    PrincipalQuery = principalQuery,
                    IsAncestorExpand = isAncestorExpand,
                    PathToCollectionPropertyInPrincipal = pathToCollectionPropertyInPrincipal,
                    ForeignKeyToPrincipalQuery = foreignKeyToPrincipalQuery,
                    ResultDescriptor = desc,
                    OrderBy = defaultOrderBy
                };

                return flatQuery;
            }

            if (selectExp != null)
            {
                var selectTrie = PathTrie.Build(resultDesc, selectExp.Select(e => e.Path));
                foreach (var selectAtom in selectExp)
                {
                    // This breaks up the path into multiple segments along the one-to-many and child-parent relationship boundaries
                    var pathSegments = selectTrie.GetSegments(selectAtom.Path);
                    ArraySegment<string> previousFullPath = null;
                    foreach (var (fullPath, subPath, desc) in pathSegments.SkipLast(1))
                    {
                        if (!segments.ContainsKey(fullPath))
                        {
                            QueryInternal flatQuery = MakeFlatQuery(previousFullPath, subPath, desc);
                            segments[fullPath] = flatQuery;
                        }

                        if (previousFullPath != null)
                        {
                            var flatQuery = segments[previousFullPath];
                            if (subPath.Count >= 2) // If there is more than just the collection property, then we add a select
                            {
                                flatQuery.Select ??= new ExpressionSelect();
                                flatQuery.Select.Add(new QueryexColumnAccess(path: subPath.SkipLast(1).ToArray(), prop: null));
                            }
                        }

                        previousFullPath = fullPath;
                    }

                    // The last segment is turned into a select atom and added to the select property
                    {
                        var (_, subPath, _) = pathSegments.Last();
                        var flatQuery = segments[previousFullPath];
                        flatQuery.Select ??= new ExpressionSelect();
                        //flatQuery.Select.Add(new SelectAtom
                        //{
                        //    Path = subPath.ToArray(),
                        //    Property = selectAtom.Property
                        //});
                        flatQuery.Select.Add(new QueryexColumnAccess(path: subPath.ToArray(), prop: selectAtom.Property));
                    }
                }
            }

            expandExp ??= ExpressionExpand.RootSingleton;
            {
                var expandTree = PathTrie.Build(resultDesc, expandExp.Select(e => e.Path));
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
                                flatQuery.Expand ??= new ExpressionExpand();
                                // flatQuery.Expand.Add(new ExpandAtom { Steps = subPath.SkipLast(1).ToArray() });
                                flatQuery.Expand.Add(new QueryexColumnAccess(path: subPath.SkipLast(1).ToArray(), prop: null));
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
                            flatQuery.Expand ??= new ExpressionExpand();
                            //flatQuery.Expand.Add(new ExpandAtom
                            //{
                            //    Steps = subPath.ToArray(),
                            //});
                            flatQuery.Expand.Add(new QueryexColumnAccess(path: subPath.ToArray(), prop: null));
                        }
                    }
                }
            }

            // The remaining odata arguments are exclusive to the root
            var root = segments[new string[0]];
            root.Filter = filterExp;
            root.OrderBy = orderbyExp;
            root.Ids = _ids;
            root.ParentIds = _parentIds;
            root.PropName = _propName;
            root.Values = _values;
            root.IncludeRoots = _includeRoots;
            root.Skip = _skip;
            root.Top = _top;
            root.FromSql = _fromSql;

            // ------------------------ Step #2: Turn the tree of QueryInternal to prepare a list of statements + an optional count query

            block.Dispose();
            block = i.Block("Prepare Statemnts");

            // Prepare the parameters
            var vars = new SqlStatementVariables();
            var ps = new SqlStatementParameters(_parameters, _additionalParameters);

            // Prepare the SqlStatements
            List<SqlStatement> statements = segments.Values
                .Select(q => q.PrepareStatement(sources, vars, ps, userId, userToday))
                .ToList(); // The order matters for the Entity loader


            block.Dispose();
            block = i.Block("Prepare Count");

            // Prepare the countSQL (if any)
            string countSql = includeCount ? root.PrepareCountSql(sources, vars, ps, userId, userToday, maxCount) : null;

            block.Dispose();

            // Load the entities
            var (result, count) = await EntityLoader.LoadStatements<T>(
                statements: statements,
                preparatorySql: null,
                countSql: countSql,
                vars: vars,
                ps: ps,
                conn: conn,
                instrumentation: i,
                cancellation: cancellation);

            // Return the entities
            return (result.Cast<T>().ToList(), count);
        }

        /// <summary>
        /// Executes the <see cref="Query{T}"/> against the SQL database returning only the first row if exists and null otherwise
        /// </summary>
        public async Task<T> FirstOrDefaultAsync(CancellationToken cancellation)
        {
            // We reuse ToList for first or default
            var entities = await Top(1).ToListAsync(cancellation);
            return entities.FirstOrDefault();
        }

        /// <summary>
        /// To prevent SQL injection attacks
        /// </summary>
        private void ValidatePathsAndProperties(ExpressionSelect selectExp, ExpressionExpand expandExp, ExpressionFilter filterExp, ExpressionOrderBy orderbyExp, TypeDescriptor rootDesc, IStringLocalizer localizer)
        {
            // This is important to avoid SQL injection attacks

            // Select
            if (selectExp != null)
            {
                PathValidator validator = new PathValidator();
                foreach (var atom in selectExp)
                {
                    // AddPath(atom.Path, atom.Property);
                    validator.AddPath(atom.Path, atom.Property);
                }

                // Make sure the paths are valid (Protects against SQL injection)
                validator.Validate(rootDesc, localizer, "select",
                    allowLists: true,
                    allowSimpleTerminals: true,
                    allowNavigationTerminals: false);
            }

            // Expand
            if (expandExp != null)
            {
                PathValidator trie = new PathValidator();
                foreach (var atom in expandExp)
                {
                    trie.AddPath(atom.Path);
                }

                // Make sure the paths are valid (Protects against SQL injection)
                trie.Validate(rootDesc, localizer, "expand",
                    allowLists: true,
                    allowSimpleTerminals: false,
                    allowNavigationTerminals: true);
            }

            // Filter
            if (filterExp != null)
            {
                PathValidator trie = new PathValidator();
                foreach (var atom in filterExp.ColumnAccesses())
                {
                    // AddPath(atom.Path, atom.Property);
                    trie.AddPath(atom.Path, atom.Property);
                }

                // Make sure the paths are valid (Protects against SQL injection)
                trie.Validate(rootDesc, localizer, "filter",
                    allowLists: false,
                    allowSimpleTerminals: true,
                    allowNavigationTerminals: false);
            }

            // Order By
            if (orderbyExp != null)
            {
                PathValidator trie = new PathValidator();
                foreach (var atom in orderbyExp)
                {
                    // AddPath(atom.Path, atom.Property);
                    trie.AddPath(atom.Path, atom.Property);
                }

                trie.Validate(rootDesc, localizer, "orderby",
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
        private class PathTrie : Dictionary<string, PathTrie>
        {
            /// <summary>
            /// The type of the current step
            /// </summary>
            public TypeDescriptor Desc { get; set; }

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
            public static PathTrie Build(TypeDescriptor desc, IEnumerable<string[]> paths)
            {
                var root = new PathTrie { Desc = desc, IsList = true, IsParent = false };

                foreach (var path in paths)
                {
                    var currentTree = root;
                    foreach (var step in path)
                    {
                        if (!currentTree.ContainsKey(step))
                        {
                            var prop = currentTree.Desc.Property(step);
                            if (prop == null)
                            {
                                throw new InvalidOperationException($"Property {prop.Name} does not exist on type {currentTree.Desc.Name}");
                            }

                            TypeDescriptor propEntityDesc = prop.GetEntityDescriptor();
                            bool isParent = false;
                            bool isList = false;

                            if (prop is NavigationPropertyDescriptor navProp)
                            {
                                isParent = navProp.IsParent;
                            }
                            else if (prop is CollectionPropertyDescriptor collProp)
                            {
                                isList = true;
                            }

                            currentTree[step] = new PathTrie
                            {
                                Desc = propEntityDesc,
                                IsList = isList,
                                IsParent = isParent
                            };
                        }

                        currentTree = currentTree[step];
                    }
                }

                return root;
            }

            public IEnumerable<(ArraySegment<string> FullPath, ArraySegment<string> SubPath, TypeDescriptor Desc)> GetSegments(string[] path)
            {
                int offset = 0;
                int count = 0;

                // always return the empty paths first
                {
                    var fullPath = new ArraySegment<string>(path, 0, offset + count);
                    var subPath = new ArraySegment<string>(path, offset, count);
                    yield return (fullPath, subPath, Desc);
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

                        yield return (fullPath, subPath, currentTree.Desc);

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
