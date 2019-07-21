using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Services.Utilities;
using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BSharp.Services.OData
{
    public class ODataQuery<T> where T : DtoBase
    {
        private readonly SqlConnection _conn;
        private readonly Func<Type, string> _sources;
        private readonly IStringLocalizer _localizer;
        private readonly int _userId;
        private readonly TimeZoneInfo _userTimeZone;
        private int? _top;
        private int? _skip;
        private List<string> _filterConditions;
        private string _select;
        private string _expand;
        private string _orderby;
        private object[] _ids;
        private string _composableSql;
        private string _preparatorySql;
        private SqlParameter[] _parameters;
        private SqlTransaction _trx;

        public ODataQuery(DbConnection conn, Func<Type, string> sources, IStringLocalizer localizer, int userId, TimeZoneInfo userTimeZone)
        {
            if (!(conn is SqlConnection))
            {
                throw new InvalidOperationException("Only Microsoft SQL Server is supported");
            }

            _conn = conn as SqlConnection;
            _sources = sources;
            _localizer = localizer;
            _userId = userId;
            _userTimeZone = userTimeZone;
        }

        public ODataQuery<T> Clone()
        {
            var clone = new ODataQuery<T>(_conn, _sources, _localizer, _userId, _userTimeZone)
            {
                _top = _top,
                _skip = _skip,
                _filterConditions = _filterConditions?.ToList(),
                _select = _select,
                _expand = _expand,
                _orderby = _orderby,
                _ids = _ids?.ToArray(),
                _composableSql = _composableSql,
                _preparatorySql = _preparatorySql,
                _parameters = _parameters?.ToArray(),
                _trx = _trx
            };

            return clone;
        }

        public ODataQuery<T> Select(string paths)
        {
            if (string.IsNullOrWhiteSpace(paths))
            {
                paths = null;
            }

            _select = paths;
            return this;
        }

        public ODataQuery<T> Expand(string paths)
        {
            if (string.IsNullOrWhiteSpace(paths))
            {
                paths = null;
            }

            _expand = paths;
            return this;
        }

        public ODataQuery<T> Filter(string condition)
        {
            if (!string.IsNullOrWhiteSpace(condition))
            {
                _filterConditions = _filterConditions ?? new List<string>();
                _filterConditions.Add(condition);
            }

            if (_top != null || _skip != null)
            {
                throw new InvalidOperationException("Cannot filter the query again after either Skip or Top have been invoked");
            }

            return this;
        }

        public ODataQuery<T> FilterByIds(params object[] ids)
        {
            if(typeof(T).GetProperty("Id") == null)
            {
                // Developer mistake
                throw new InvalidOperationException($"Type {typeof(T).Name} does not have an Id, yet 'FilterByIds' has been invoked on an ODataQuery with {typeof(T).Name} as a generic argument");
            }

            _ids = ids;
            return this;
        }

        public ODataQuery<T> OrderBy(string paths)
        {
            if (string.IsNullOrWhiteSpace(paths))
            {
                paths = null;
            }

            _orderby = paths;
            return this;
        }

        public ODataQuery<T> Top(int top)
        {
            _top = top;
            return this;
        }

        public ODataQuery<T> Skip(int skip)
        {
            _skip = skip;
            return this;
        }

        public ODataQuery<T> FromSql(string composableSql, string preparatorySql = null, params SqlParameter[] parameters)
        {
            _composableSql = composableSql;
            _preparatorySql = preparatorySql;
            _parameters = parameters;

            return this;
        }

        public ODataQuery<T> UseTransaction(DbTransaction trx)
        {
            if (!(trx is SqlTransaction))
            {
                throw new InvalidOperationException("Only Microsoft SQL Server is supported");
            }

            _trx = trx as SqlTransaction;

            return this;
        }

        public async Task<int> CountAsync()
        {
            SelectExpression selectExp = SelectExpression.Parse(typeof(T).GetProperty("Id") != null ? "Id" : _select);
            FilterExpression filterExp = _filterConditions?.Select(c => FilterExpression.Parse(c))
                .Aggregate((e1, e2) => new FilterConjunction { Left = e1, Right = e2 });

            ValidatePathsAndProperties(selectExp, null, filterExp, null);

            var flatQuery = new ODataQueryInternal
            {
                ResultType = typeof(T),
                KeyType = typeof(T).GetProperty("Id")?.PropertyType,
                Select = selectExp,
                Filter = filterExp,
                Ids = _ids == null ? null : string.Join(",", _ids.Select(e => e)),
                Skip = _skip,
                Top = _top
            };

            var ps = new SqlStatementParameters();
            if (_parameters != null)
            {
                // If explicit parameters were provided in a FromSql call, use them
                foreach (var p in _parameters)
                {
                    ps.AddParameter(p);
                }
            }

            var sources = _sources;
            if (!string.IsNullOrWhiteSpace(_composableSql))
            {
                // If a composable SQL is provided in FromSql call, use it as the source of T rather then the default
                sources = (t) =>
                {
                    if (t == typeof(T))
                    {
                        return $@"({_composableSql})";
                    }
                    else
                    {
                        return sources(t);
                    }
                };
            }

            var sql = flatQuery.PrepareStatement(_sources, ps, _userId, _userTimeZone).Sql;
            sql = ODataTools.IndentLines(sql);
            sql = $@"SELECT COUNT(*) As [Count] FROM (
{sql}
) As [S]";

            // always append the preparatory sql to the beginning
            if (!string.IsNullOrWhiteSpace(_preparatorySql))
            {
                sql = $@"{_preparatorySql}

{sql}";
            }

            using (var cmd = _conn.CreateCommand())
            {
                if(_trx != null)
                {
                    cmd.Transaction = _trx;
                }

                // Prepare the SQL command
                cmd.CommandText = sql;
                foreach (var parameter in ps)
                {
                    cmd.Parameters.Add(parameter);
                }

                bool ownsConnection = _conn.State != System.Data.ConnectionState.Open;
                if (ownsConnection)
                {
                    _conn.Open();
                }

                try
                {
                    int count = (int)await cmd.ExecuteScalarAsync();
                    return count;
                }
                finally
                {
                    if (ownsConnection)
                    {
                        _conn.Close();
                        _conn.Dispose();
                    }
                }
            }
        }

        public async Task<(List<T>, EntitiesMap)> ToListAsync()
        {
            if(string.IsNullOrWhiteSpace(_orderby))
            {
                // Developer mistake
                throw new InvalidOperationException($"ODataQuery of type {typeof(T).Name} was executed without an orderby clause");
            }

            // Create the expressions, for filter, turn all the filters into expressions and AND them together
            SelectExpression selectExp = SelectExpression.Parse(_select);
            ExpandExpression expandExp = ExpandExpression.Parse(_expand);
            OrderByExpression orderbyExp = OrderByExpression.Parse(_orderby);
            FilterExpression filterExp = _filterConditions?.Select(c => FilterExpression.Parse(c))
                .Aggregate((e1, e2) => new FilterConjunction { Left = e1, Right = e2 });

            // To prevent SQL injection
            ValidatePathsAndProperties(selectExp, expandExp, filterExp, orderbyExp);

            // ------------------------

            var segments = new Dictionary<ArraySegment<string>, ODataQueryInternal>(new PathEqualityComparer());

            // Helper method, will be used later in both the select and the expand loops
            ODataQueryInternal MakeFlatQuery(ArraySegment<string> previousFullPath, ArraySegment<string> subPath, Type type)
            {
                ODataQueryInternal principalQuery = previousFullPath == null ? null : segments[previousFullPath];
                ArraySegment<string> pathToCollectionPropertyInPrincipal = previousFullPath == null ? null : subPath;
                Type keyType = type.GetProperty("Id")?.PropertyType;

                if(principalQuery != null && keyType == null)
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

                    if(collectionProperty.IsParent())
                    {
                        foreignKeyToPrincipalQuery = "ParentId";
                        isAncestorExpand = true;
                    }
                    else
                    {
                        // Must be a collection then
                        foreignKeyToPrincipalQuery = collectionProperty.GetCustomAttribute<NavigationPropertyAttribute>()?.ForeignKey;
                        if (string.IsNullOrWhiteSpace(foreignKeyToPrincipalQuery))
                        {
                            // Programmer mistake
                            throw new InvalidOperationException($"Navigation collection {collectionProperty.Name} on type {collectionProperty.DeclaringType} is not adorned with the associated foreign key");
                        }
                    }                    
                }

                if(isAncestorExpand)
                {
                    // the path to parent entity is the path above minus the "Parent"
                    var pathToParentEntity = new ArraySegment<string>(
                        array: pathToCollectionPropertyInPrincipal.Array, 
                        offset: 0, 
                        count: pathToCollectionPropertyInPrincipal.Count - 1);

                    // Adding this causes the principal query to always include ParentId in the select clause
                    principalQuery.PathsToParentEntitiesWithExpandedAncestors.Add(pathToParentEntity);
                }

                var flatQuery = new ODataQueryInternal
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
                            ODataQueryInternal flatQuery = MakeFlatQuery(previousFullPath, subPath, type);

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

            expandExp = expandExp ?? ExpandExpression.RootSingleton;
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
                            ODataQueryInternal flatQuery = MakeFlatQuery(previousFullPath, subPath, type);
                            segments[fullPath] = flatQuery;
                        }

                        {
                            var flatQuery = segments[fullPath];
                            if (flatQuery.Expand == null && subPath.Count >= 2) // If there is more than just the collection property, then we add an expand
                            {
                                flatQuery.Expand = new ExpandExpression
                                {
                                    new ExpandAtom
                                    {
                                        Path = subPath.SkipLast(1).ToArray()
                                    }
                                };
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
                            flatQuery.Expand = flatQuery.Expand ?? new ExpandExpression();
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
            root.Ids = _ids == null ? null : string.Join(",", _ids.Select(e => e.ToString()));
            root.Skip = _skip;
            root.Top = _top;

            var ps = new SqlStatementParameters();
            if (_parameters != null)
            {
                // If explicit parameters were provided in a FromSql call, use them
                foreach (var p in _parameters)
                {
                    ps.AddParameter(p);
                }
            }

            var sources = _sources;
            if (!string.IsNullOrWhiteSpace(_composableSql))
            {
                // If a composable SQL is provided in FromSql call, use it as the source of T rather then the default
                sources = (t) =>
                {
                    if (t == typeof(T))
                    {
                        return $@"({_composableSql})";
                    }
                    else
                    {
                        return sources(t);
                    }
                };
            }

            var results = segments.ToDictionary(e => (IQueryInternal)e.Value, e => new List<DtoBase>());

            List<(IQueryInternal, SqlStatement)> queries = results.Keys.Select(q => 
                (q, q.PrepareStatement(_sources, ps, _userId, _userTimeZone))).ToList(); // The order matters for the hydration step later

            var result = await ObjectLoader.LoadStatements<T>(
                queries: queries,
                preparatorySql: null,
                ps: ps,
                conn: _conn,
                trx: _trx);


            return (result.Result.Cast<T>().ToList(), result.RelatedEntities);
        }

        public async Task<(T, EntitiesMap)> FirstOrDefaultAsync()
        {
            //if(string.IsNullOrWhiteSpace(_orderby))
            //{
            //    OrderBy("Id");
            //}

            Top(1);
            var (data, related) = await ToListAsync();
            return (data.FirstOrDefault(), related);
        }

        /// <summary>
        /// Useful for RLS security enforcement, this method takes a list of permission criteria each associated with an index
        /// and returns a mapping from every entity Id in the original query to all the criteria that are satified by this entity
        /// </summary>
        public async Task<IEnumerable<IdIndex>> GetIndexToIdMap(IEnumerable<IndexAndCriteria> criteriaIndexes)
        {
            var idType = typeof(T).GetProperty("Id")?.PropertyType;
            if(idType == null)
            {
                // Programmer mistake
                throw new InvalidOperationException("");
            }

            return await GetIndexMapInner(criteriaIndexes, idType);
        }

        /// <summary>
        /// Useful for RLS security enforcement, this method takes a list of permission criteria each associated with an index
        /// and returns a mapping from every entity Id (which is specifically and INT) in the original query to all the criteria
        /// that are satified by this entity, this is different from <see cref="GetIndexToIdMap(IEnumerable{IndexAndCriteria})"/>
        /// in that it is useful when the origianl query is dynamically constructed using <see cref="FromSql(string, string, SqlStatementParameter[])"/>
        /// by passing in new data that doesn't have Ids, and hence the indexes of the items in the memory list are used as Ids instead
        /// </summary>
        public async Task<IEnumerable<IdIndex>> GetIndexToIndexMap(IEnumerable<IndexAndCriteria> criteriaIndexes)
        {
            return await GetIndexMapInner(criteriaIndexes, typeof(int));
        }

        /// <summary>
        /// Internal Implementation
        /// </summary>
        private async Task<IEnumerable<IdIndex>> GetIndexMapInner(IEnumerable<IndexAndCriteria> criteriaIndexes, Type idType)
        {
            if (!string.IsNullOrWhiteSpace(_select) || !string.IsNullOrWhiteSpace(_expand))
            {
                throw new InvalidOperationException($"Cannot call {nameof(GetIndexToIdMap)} when select or expand are specified");
            }

            var orderByExp = OrderByExpression.Parse(_orderby);
            FilterExpression filterExp = _filterConditions?.Select(c => FilterExpression.Parse(c))
                .Aggregate((e1, e2) => new FilterConjunction { Left = e1, Right = e2 });

            ValidatePathsAndProperties(null, null, filterExp, orderByExp);

            var flatQuery = new ODataQueryInternal
            {
                ResultType = typeof(T),
                KeyType = idType,
                Filter = filterExp,
                Ids = _ids == null ? null : string.Join(",", _ids.Select(e => e)),
                OrderBy = orderByExp,
                Skip = _skip,
                Top = _top
            };

            var ps = new SqlStatementParameters();
            if (_parameters != null)
            {
                // If explicit parameters were provided in a FromSql call, use them
                foreach (var p in _parameters)
                {
                    ps.AddParameter(p);
                }
            }

            var sources = _sources;
            if (!string.IsNullOrWhiteSpace(_composableSql))
            {
                // If a composable SQL is provided in FromSql call, use it as the source of T rather then the default
                sources = (t) =>
                {
                    if (t == typeof(T))
                    {
                        return $@"({_composableSql})";
                    }
                    else
                    {
                        return _sources(t);
                    }
                };
            }

            var sourceSql = flatQuery.PrepareStatement(sources, ps, _userId, _userTimeZone).Sql;

            string SourceOverride(Type t)
            {
                if (t == typeof(T))
                {
                    return $@"({sourceSql})";
                }
                else
                {
                    return sources(t);
                }
            }

            List<StringBuilder> toBeUnioned = new List<StringBuilder>(criteriaIndexes.Count());

            foreach (var criteriaIndex in criteriaIndexes)
            {
                int index = criteriaIndex.Index;
                string criteria = criteriaIndex.Criteria;

                var criteriaExp = FilterExpression.Parse(criteria);
                ValidatePathsAndProperties(null, null, criteriaExp, null);

                var criteriaQuery = new ODataQueryInternal
                {
                    ResultType = typeof(T),
                    KeyType = idType,
                    Filter = criteriaExp,
                };

                SqlJoinClause joinClause = criteriaQuery.JoinSql();
                string joinSql = joinClause.ToSql(SourceOverride);
                string whereSql = criteriaQuery.WhereSql(sources, joinClause.JoinTree, ps, _userId, _userTimeZone);


                var sqlBuilder = new StringBuilder();
                sqlBuilder.AppendLine($"SELECT [P].[Id], {index} As [Index]");
                sqlBuilder.AppendLine(joinSql);
                sqlBuilder.AppendLine(whereSql);

                toBeUnioned.Add(sqlBuilder);
            }

            string sql = toBeUnioned.Select(s => s.ToString()).Aggregate((s1, s2) => $@"{s1}

UNION

{s2}");

            if(!string.IsNullOrWhiteSpace(_preparatorySql))
            {
                sql = $@"{_preparatorySql}

{sql}";
            }

            using (var cmd = _conn.CreateCommand())
            {
                if (_trx != null)
                {
                    cmd.Transaction = _trx;
                }

                // Prepare the SQL command
                cmd.CommandText = sql;
                foreach (var parameter in ps)
                {
                    cmd.Parameters.Add(parameter);
                }

                bool ownsConnection = _conn.State != System.Data.ConnectionState.Open;
                if (ownsConnection)
                {
                    _conn.Open();
                }

                try
                {
                    var result = new List<IdIndex>();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // Loop over the result from the database
                        while (await reader.ReadAsync())
                        {
                            var dbId = reader["Id"];
                            var dbIndex = reader["Index"];

                            result.Add(new IdIndex
                            {

                                Id = dbId,
                                Index = (int)dbIndex
                            });
                        }
                    }

                    return result;
                }
                finally
                {
                    if (ownsConnection)
                    {
                        _conn.Close();
                        _conn.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// To prevent SQL injection attacks
        /// </summary>
        private void ValidatePathsAndProperties(SelectExpression selectExp, ExpandExpression expandExp, FilterExpression filterExp, OrderByExpression orderbyExp)
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
                selectPathValidator.Validate(typeof(T), _localizer, "select",
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
                    // AddPath(atom.Path);
                    expandPathTree.AddPath(atom.Path);
                }

                // Make sure the paths are valid (Protects against SQL injection)
                expandPathTree.Validate(typeof(T), _localizer, "expand",
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
                filterPathTree.Validate(typeof(T), _localizer, "filter",
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

                orderbyPathTree.Validate(typeof(T), _localizer, "orderby",
                    allowLists: false,
                    allowSimpleTerminals: true,
                    allowNavigationTerminals: false);
            }
        }

        /// <summary>
        /// For efficiently segmenting SELECT and EXPAND arguments along the one-to-many relations
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
                        offset = offset + count;
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

        /// <summary>
        /// For efficient traversal of the result column map
        /// </summary>
    }


    public class IdIndex
    {
        public object Id { get; set; }

        public int Index { get; set; }
    }

    public class IndexAndCriteria
    {
        public string Criteria { get; set; }

        public int Index { get; set; }
    }
}
