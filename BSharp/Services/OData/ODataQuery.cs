using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using BSharp.Services.Utilities;
using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BSharp.Services.OData
{
    public class ODataQuery<T, TKey> where T : DtoKeyBase<TKey>
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
        private TKey[] _ids;
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

        public ODataQuery<T, TKey> Clone()
        {
            var clone = new ODataQuery<T, TKey>(_conn, _sources, _localizer, _userId, _userTimeZone)
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

        public ODataQuery<T, TKey> Select(string paths)
        {
            if (string.IsNullOrWhiteSpace(paths))
            {
                paths = null;
            }

            _select = paths;
            return this;
        }

        public ODataQuery<T, TKey> Expand(string paths)
        {
            if (string.IsNullOrWhiteSpace(paths))
            {
                paths = null;
            }

            _expand = paths;
            return this;
        }

        public ODataQuery<T, TKey> Filter(string condition)
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

        public ODataQuery<T, TKey> FilterByIds(params TKey[] ids)
        {
            _ids = ids;
            return this;
        }

        public ODataQuery<T, TKey> OrderBy(string paths)
        {
            if (string.IsNullOrWhiteSpace(paths))
            {
                paths = null;
            }

            _orderby = paths;
            return this;
        }

        public ODataQuery<T, TKey> Top(int top)
        {
            _top = top;
            return this;
        }

        public ODataQuery<T, TKey> Skip(int skip)
        {
            _skip = skip;
            return this;
        }

        public ODataQuery<T, TKey> FromSql(string composableSql, string preparatorySql = null, params SqlParameter[] parameters)
        {
            _composableSql = composableSql;
            _preparatorySql = preparatorySql;
            _parameters = parameters;

            return this;
        }

        public ODataQuery<T, TKey> UseTransaction(DbTransaction trx)
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
            SelectExpression selectExp = SelectExpression.Parse("Id");
            FilterExpression filterExp = _filterConditions?.Select(c => FilterExpression.Parse(c))
                .Aggregate((e1, e2) => new FilterConjunction { Left = e1, Right = e2 });

            ValidatePathsAndProperties(null, null, filterExp, null);

            var flatQuery = new ODataFlatQuery
            {
                ResultType = typeof(T),
                KeyType = typeof(TKey),
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

        public async Task<List<T>> ToListAsync()
        {
            OrderByExpression orderbyId = OrderByExpression.Parse("Id");

            // Create the expressions, for filter, turn all the filters into expressions and AND them together
            SelectExpression selectExp = SelectExpression.Parse(_select);
            ExpandExpression expandExp = ExpandExpression.Parse(_expand);
            OrderByExpression orderbyExp = OrderByExpression.Parse(_orderby) ?? orderbyId;
            FilterExpression filterExp = _filterConditions?.Select(c => FilterExpression.Parse(c))
                .Aggregate((e1, e2) => new FilterConjunction { Left = e1, Right = e2 });

            // To prevent SQL injection
            ValidatePathsAndProperties(selectExp, expandExp, filterExp, orderbyExp);

            // ------------------------

            var segments = new Dictionary<ArraySegment<string>, ODataFlatQuery>(new PathEqualityComparer());

            // Helper method, will be used later in both the select and the expand loops
            ODataFlatQuery MakeFlatQuery(ArraySegment<string> previousFullPath, ArraySegment<string> subPath, Type type)
            {
                ODataFlatQuery principalQuery = previousFullPath == null ? null : segments[previousFullPath];
                ArraySegment<string> pathToCollectionPropertyInPrincipal = previousFullPath == null ? null : subPath;
                Type keyType = type.GetProperty("Id").PropertyType;

                // This loop retrieves the collection property
                string foreignKeyToPrincipalQuery = null;
                if (principalQuery != null)
                {
                    var collectionProperty = principalQuery.ResultType.GetProperty(pathToCollectionPropertyInPrincipal[0]);
                    for (int i = 1; i < pathToCollectionPropertyInPrincipal.Count; i++)
                    {
                        var step = pathToCollectionPropertyInPrincipal[i];
                        collectionProperty = collectionProperty.PropertyType.GetProperty(step);
                    }

                    foreignKeyToPrincipalQuery = collectionProperty.GetCustomAttribute<NavigationPropertyAttribute>()?.ForeignKey;
                    if (string.IsNullOrWhiteSpace(foreignKeyToPrincipalQuery))
                    {
                        // Programmer mistake
                        throw new InvalidOperationException($"Navigation collection {collectionProperty.Name} on type {collectionProperty.DeclaringType} is not adorned with the associated foreign key");
                    }
                }

                var flatQuery = new ODataFlatQuery
                {
                    PrincipalQuery = principalQuery,
                    PathToCollectionPropertyInPrincipal = pathToCollectionPropertyInPrincipal,
                    ForeignKeyToPrincipalQuery = foreignKeyToPrincipalQuery,
                    ResultType = type,
                    KeyType = keyType,
                    OrderBy = orderbyId
                };

                return flatQuery;
            }

            if (selectExp != null)
            {
                var selectTree = PathTree.Build(typeof(T), selectExp.Select(e => e.Path));
                foreach (var selectAtom in selectExp)
                {
                    // This breaks up the path into multiple segments along the one-to-many relationship boundaries
                    var pathSegments = selectTree.GetSegments(selectAtom.Path);
                    ArraySegment<string> previousFullPath = null;
                    foreach (var (fullPath, subPath, type) in pathSegments.SkipLast(1))
                    {
                        if (!segments.ContainsKey(fullPath))
                        {
                            ODataFlatQuery flatQuery = MakeFlatQuery(previousFullPath, subPath, type);

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
                            ODataFlatQuery flatQuery = MakeFlatQuery(previousFullPath, subPath, type);

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

            var results = segments.ToDictionary(e => e.Value, e => new List<DtoBase>());

            List<SqlStatement> statements = results.Keys
                .Select(q => q.PrepareStatement(_sources, ps, _userId, _userTimeZone)).ToList(); // The order matters for the hydration step later

            StringBuilder sql = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(_preparatorySql))
            {
                sql.AppendLine(_preparatorySql);
                sql.AppendLine(); // Just for aesthetics
            }

            foreach (var statement in statements)
            {
                sql.AppendLine(statement.Sql);
                sql.AppendLine(); // Just for aesthetics
            }

            using (var cmd = _conn.CreateCommand())
            {
                if (_trx != null)
                {
                    cmd.Transaction = _trx;
                }

                // Prepare the SQL command
                cmd.CommandText = sql.ToString();
                foreach (var parameter in ps)
                {
                    cmd.Parameters.Add(parameter);
                }

                bool ownsConnection = _conn.State != System.Data.ConnectionState.Open;
                if (ownsConnection)
                {
                    _conn.Open();
                }

                // These dictionaries will contain the result
                var memory = new HashSet<(string, ColumnMapTree)>(); // Remembers which (Id, tree) combinations have been added already
                var allEntities = new Dictionary<Type, Dictionary<string, DtoBase>>(); // int ids are cast to string

                DtoBase AddEntity(SqlDataReader reader, ColumnMapTree entityDef)
                {
                    var entityType = entityDef.Type; // TODO: The specific type to use when instantiating the entity: should come from discriminator e.g. Agent
                    var collectionType = entityType; // TODO: The root type of the collection where to store and track this entity e.g. Custody

                    // Make sure the dictionary that tracks this type is created already
                    if (!allEntities.ContainsKey(collectionType))
                    {
                        allEntities[collectionType] = new Dictionary<string, DtoBase>();
                    }

                    var entitiesOfType = allEntities[collectionType];

                    var dbId = reader[entityDef.IdIndex];
                    if (dbId == DBNull.Value)
                    {
                        return null;
                    }

                    var id = dbId.ToString();
                    entitiesOfType.TryGetValue(id, out DtoBase entity);
                    bool isPopulated = false;
                    if (entity == null)
                    {
                        entity = Activator.CreateInstance(entityType) as DtoBase;
                        entityDef.IdProperty.SetValue(entity, dbId);

                        entitiesOfType.Add(id, entity);
                        memory.Add((id, entityDef));

                        // new entity
                        isPopulated = false;
                    }
                    else
                    {
                        if (memory.Add((id, entityDef)))
                        {
                            // Entity added before, but from a different part of the join tree
                            isPopulated = false;
                        }
                        else
                        {
                            // Entity added in a previous row
                            isPopulated = true;
                        }
                    }

                    // As an optimization, only populate again if not populated before
                    if (!isPopulated)
                    {
                        foreach (var (propInfo, index) in entityDef.Properties)
                        {
                            var dbValue = reader[index];
                            if (dbValue != DBNull.Value)
                            {
                                // chars still comes from the DB as a string
                                if(propInfo.PropertyType == typeof(char?))
                                {
                                    dbValue = dbValue.ToString()[0]; // gets the char
                                }

                                propInfo.SetValue(entity, dbValue);
                            }

                            entity.EntityMetadata[propInfo.Name] = FieldMetadata.Loaded;
                        }
                    }

                    foreach (var subEntityDef in entityDef.Values)
                    {
                        AddEntity(reader, subEntityDef);
                    }

                    return entity;
                }

                // The result that will be returned at the end
                try
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        foreach (var statement in statements)
                        {
                            var list = results[statement.Query];

                            // Group the column map by the path (which represents the target entity)
                            var entityDefs = ColumnMapTree.Build(statement.ResultType, statement.ColumnMap);

                            // Loop over the result from the database
                            while (await reader.ReadAsync())
                            {
                                var record = AddEntity(reader, entityDefs);
                                list.Add(record);
                            }

                            await reader.NextResultAsync();
                        }
                    }
                }
                finally
                {
                    if (ownsConnection)
                    {
                        _conn.Close();
                        _conn.Dispose();
                    }
                }

                // All simple properties are populated, but not their navigation properties, those are done here
                var typePropertiesCache = new Dictionary<Type, IEnumerable<PropertyInfo>>();
                foreach (var entitiesOfType in allEntities)
                {
                    Type type = null;
                    List<(PropertyInfo NavProperty, PropertyInfo FkProperty)> navProperties = null;

                    foreach (var entity in entitiesOfType.Value.Values.OrderBy(e => e.GetType()))
                    {
                        var entityType = entity.GetType();

                        // Just an optimization
                        if (entityType != type)
                        {
                            type = entityType;
                            navProperties = new List<(PropertyInfo NavProperty, PropertyInfo FkProperty)>();
                            foreach (var prop in type.GetProperties())
                            {
                                var navPropertyAtt = prop.GetCustomAttribute<NavigationPropertyAttribute>();
                                if (navPropertyAtt != null && !prop.PropertyType.IsList())
                                {
                                    var propCollectionType = prop.PropertyType; // TODO: Use the root type

                                    if (allEntities.ContainsKey(propCollectionType))
                                    {
                                        var fkProp = type.GetProperty(navPropertyAtt.ForeignKey);
                                        navProperties.Add((prop, fkProp));
                                    }
                                }
                            }
                        }

                        foreach (var (navProp, fkProp) in navProperties)
                        {
                            var fk = fkProp.GetValue(entity)?.ToString();
                            if (fk != null)
                            {
                                var propCollectionType = navProp.PropertyType; // TODO: Use the root type
                                var entitiesOfPropertyType = allEntities[propCollectionType];

                                entitiesOfPropertyType.TryGetValue(fk, out DtoBase navPropValue);
                                if (navPropValue != null)
                                {
                                    navProp.SetValue(entity, navPropValue);
                                }
                            }

                            entity.EntityMetadata[navProp.Name] = FieldMetadata.Loaded;
                        }
                    }
                }

                List<T> result = new List<T>();

                IList MakeList(Type t, IEnumerable collection = null)
                {
                    var listType = typeof(List<>).MakeGenericType(t);
                    var list = (IList)Activator.CreateInstance(listType);
                    if(collection != null)
                    {
                        foreach (var item in collection)
                        {
                            list.Add(item);
                        }
                    }

                    return list;
                }

                // Here we populate the collection navigation properties after the simple properties have been populated
                foreach (var (query, list) in results)
                {
                    if (query.PrincipalQuery == null)
                    {
                        result = list.Cast<T>().ToList();
                    }
                    else
                    {
                        var principalQuery = query.PrincipalQuery;
                        var principalEntities = results[principalQuery]; // The list of entities that
                        var pathToCollection = query.PathToCollectionPropertyInPrincipal;
                        var pathToCollectionEntity = new ArraySegment<string>(pathToCollection.Array, pathToCollection.Offset, pathToCollection.Count - 1);
                        var collectionPropName = pathToCollection[pathToCollection.Count - 1];

                        var collectionEntities = new HashSet<DtoBase>();
                        foreach (var principalEntity in principalEntities)
                        {
                            // need to go down the path
                            var currentEntity = principalEntity;
                            foreach (var step in pathToCollectionEntity)
                            {
                                var nextObject = currentEntity.GetType().GetProperty(step).GetValue(currentEntity);
                                if (nextObject == null)
                                {
                                    break;
                                }

                                currentEntity = nextObject as DtoBase;
                            }

                            collectionEntities.Add(currentEntity);
                        }

                        var fkName = query.ForeignKeyToPrincipalQuery;
                        var fkProp = query.ResultType.GetProperty(fkName);
                        var groupedCollections = list.GroupBy(e => fkProp.GetValue(e).ToString()).ToDictionary(g => g.Key, g => MakeList(query.ResultType, g));

                        PropertyInfo idProp = null;
                        PropertyInfo collectionProp = null;
                        foreach (var collectionEntity in collectionEntities)
                        {
                            if (idProp == null)
                            {
                                idProp = collectionEntity.GetType().GetProperty("Id");
                            }

                            if (collectionProp == null)
                            {
                                collectionProp = collectionEntity.GetType().GetProperty(collectionPropName);
                            }

                            var id = idProp.GetValue(collectionEntity).ToString();
                            var collection = groupedCollections.ContainsKey(id) ? groupedCollections[id] : MakeList(query.ResultType);
                            
                            collectionProp.SetValue(collectionEntity, collection);
                            collectionEntity.EntityMetadata[collectionPropName] = FieldMetadata.Loaded;
                        }
                    }
                }

                return result;
            }
        }

        public async Task<T> FirstOrDefaultAsync()
        {
            if(string.IsNullOrWhiteSpace(_orderby))
            {
                OrderBy("Id");
            }

            Top(1);
            var result = await ToListAsync();
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Useful for RLS security enforcement, this method takes a list of permission criteria each associated with an index
        /// and returns a mapping from every entity Id in the original query to all the criteria that are satified by this entity
        /// </summary>
        public async Task<IEnumerable<IdIndex<TKey>>> GetIndexToIdMap(IEnumerable<IndexAndCriteria> criteriaIndexes)
        {
            return await GetIndexMapInner<TKey>(criteriaIndexes);
        }

        /// <summary>
        /// Useful for RLS security enforcement, this method takes a list of permission criteria each associated with an index
        /// and returns a mapping from every entity Id (which is specifically and INT) in the original query to all the criteria
        /// that are satified by this entity, this is different from <see cref="GetIndexToIdMap(IEnumerable{IndexAndCriteria})"/>
        /// in that it is useful when the origianl query is dynamically constructed using <see cref="FromSql(string, string, SqlStatementParameter[])"/>
        /// by passing in new data that doesn't have Ids, and hence the indexes of the items in the memory list are used as Ids instead
        /// </summary>
        public async Task<IEnumerable<IdIndex<int>>> GetIndexToIndexMap(IEnumerable<IndexAndCriteria> criteriaIndexes)
        {
            return await GetIndexMapInner<int>(criteriaIndexes);
        }

        /// <summary>
        /// The internal implementation of <see cref="GetIndexToIdMap(IEnumerable{IndexAndCriteria})"/>
        /// and <see cref="GetIndexToIndexMap(IEnumerable{IndexAndCriteria})"/>
        /// </summary>
        private async Task<IEnumerable<IdIndex<TId>>> GetIndexMapInner<TId>(IEnumerable<IndexAndCriteria> criteriaIndexes)
        {
            if (!string.IsNullOrWhiteSpace(_select) || !string.IsNullOrWhiteSpace(_expand))
            {
                throw new InvalidOperationException($"Cannot call {nameof(GetIndexToIdMap)} when select or expand are specified");
            }

            var orderByExp = OrderByExpression.Parse(_orderby);
            FilterExpression filterExp = _filterConditions?.Select(c => FilterExpression.Parse(c))
                .Aggregate((e1, e2) => new FilterConjunction { Left = e1, Right = e2 });

            ValidatePathsAndProperties(null, null, filterExp, orderByExp);

            var flatQuery = new ODataFlatQuery
            {
                ResultType = typeof(T),
                KeyType = typeof(TId),
                Select = SelectExpression.Parse("Id"),
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

                var criteriaQuery = new ODataFlatQuery
                {
                    ResultType = typeof(T),
                    KeyType = typeof(TId),
                    Filter = criteriaExp,
                };

                SqlJoinClause joinClause = criteriaQuery.JoinSql();
                string joinSql = joinClause.ToSql(SourceOverride);
                string whereSql = criteriaQuery.WhereSql(joinClause.JoinTree, ps, _userId, _userTimeZone);


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
                    var result = new List<IdIndex<TId>>();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        var dbId = reader["Id"];
                        var dbIndex = reader["Index"];

                        result.Add(new IdIndex<TId>
                        {

                            Id = (TId)dbId,
                            Index = (int)dbIndex
                        });
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
                    filterPathTree.AddPath(atom.Path);
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
            public Type Type { get; set; }

            public bool IsList { get; set; }

            public static PathTree Build(Type type, IEnumerable<string[]> paths)
            {
                var root = new PathTree { Type = type, IsList = true };

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

                            var isList = prop.PropertyType.IsList();
                            var propType = isList ? prop.PropertyType.GenericTypeArguments[0] : prop.PropertyType;

                            currentTree[step] = new PathTree
                            {
                                Type = propType,
                                IsList = isList
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

                    if (currentTree.IsList)
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
        private class ColumnMapTree : Dictionary<string, ColumnMapTree>
        {
            public Type Type { get; set; }

            public int IdIndex { get; set; }

            public PropertyInfo IdProperty { get; set; }

            public List<(PropertyInfo Property, int Index)> Properties { get; set; } = new List<(PropertyInfo Property, int Index)>();

            public static ColumnMapTree Build(Type type, List<(ArraySegment<string> Path, string Property)> columnMap)
            {
                var root = new ColumnMapTree { Type = type };

                for (var i = 0; i < columnMap.Count; i++)
                {
                    var (path, property) = columnMap[i];
                    var currentTree = root;
                    for (int j = 0; j < path.Count; j++)
                    {
                        var step = path[j];

                        if (!currentTree.ContainsKey(step))
                        {
                            var prop = currentTree.Type.GetProperty(step);
                            if (prop == null)
                            {
                                // Developer mistake
                                throw new InvalidOperationException($"Property {step} was not found on type {currentTree.Type.Name}");
                            }

                            currentTree[step] = new ColumnMapTree
                            {
                                Type = prop.PropertyType,
                            };
                        }

                        currentTree = currentTree[step];
                    }

                    if (property == "Id")
                    {
                        currentTree.IdIndex = i;
                        currentTree.IdProperty = currentTree.Type.GetProperty("Id"); // Useful later for optimization
                    }
                    else
                    {
                        var propInfo = currentTree.Type.GetProperty(property);
                        currentTree.Properties.Add((propInfo, i));
                    }
                }

                return root;
            }
        }
    }

    public class IdIndex<TKey>
    {
        public TKey Id { get; set; }

        public int Index { get; set; }
    }

    public class IndexAndCriteria
    {
        public string Criteria { get; set; }

        public int Index { get; set; }
    }
}
