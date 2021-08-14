using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Model.Common;
using Tellma.Repository.Common.Queryex;

namespace Tellma.Repository.Common
{
    public class StatementLoader : IStatementLoader
    {
        private const string DIVISION_BY_ZERO_MESSAGE = "The query caused a division by zero.";

        private readonly ILogger _logger;

        public StatementLoader(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Connects to the DB and loads the results of a single dynamic statement described in <paramref name="args"/>.
        /// </summary>
        /// <param name="connString">The connection string of the SQL database from which to load the data.</param>
        /// <param name="args">All the information needed to connect to the database and execute the statement.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>The requested data packaged in a <see cref="DynamicResult"/>.</returns>
        public async Task<DynamicResult> LoadDynamic(string connString, DynamicLoaderArguments args, CancellationToken cancellation = default)
        {
            // Destructure the args
            var countSql = args.CountSql;
            var principalStatement = args.PrincipalStatement;
            var dimAncestorsStatements = args.DimensionAncestorsStatements ?? new List<SqlDimensionAncestorsStatement>();
            var ps = args.Parameters;
            var vars = args.Variables;

            ////////////// Prepare the complete SQL code
            // Add any variables in the preparatory SQL
            string variablesSql = vars.ToSql();

            var statements = new List<string>(1 + dimAncestorsStatements.Count) { principalStatement.Sql };
            statements.AddRange(dimAncestorsStatements.Select(e => e.Sql));

            string sql = PrepareSql(
                variablesSql: variablesSql,
                countSql: countSql,
                statements.ToArray());

            // The result
            DynamicResult result = null;

            try
            {
                using var trx = TransactionFactory.ReadCommitted();
                await ExponentialBackoff(async () =>
                {
                    var rows = new List<DynamicRow>();
                    var trees = new List<DimensionAncestorsResult>();
                    var count = 0;

                    // Connection
                    using var conn = new SqlConnection(connString);

                    // Command Text
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = sql;

                    // Parameters
                    foreach (var parameter in ps)
                    {
                        cmd.Parameters.Add(parameter);
                    }

                    // Execute
                    try // To capture 
                    {
                        await conn.OpenAsync(cancellation);
                        using var reader = await cmd.ExecuteReaderAsync(cancellation);

                        // (1) Load the count if any
                        if (!string.IsNullOrWhiteSpace(countSql))
                        {
                            if (await reader.ReadAsync(cancellation))
                            {
                                count = reader.GetInt32(0);
                            }

                            // Go over to the next result set
                            await reader.NextResultAsync(cancellation);
                        }

                        // (2) Load results of the principal query
                        {
                            int columnCount = principalStatement.ColumnCount;
                            while (await reader.ReadAsync(cancellation))
                            {
                                var row = new DynamicRow(columnCount);
                                for (int index = 0; index < columnCount; index++)
                                {
                                    var dbValue = reader.Value(index);
                                    row.Add(dbValue);
                                }

                                rows.Add(row);
                            }
                        }

                        // (3) Load the tree dimensions
                        foreach (var treeStatement in dimAncestorsStatements)
                        {
                            int columnCount = treeStatement.TargetIndices.Count();

                            int index;
                            int minIndex = treeStatement.TargetIndices.Min();
                            int[] targetIndices = treeStatement.TargetIndices.Select(i => i - minIndex).ToArray();

                            var treeResult = new DimensionAncestorsResult()
                            {
                                IdIndex = treeStatement.IdIndex,
                                MinIndex = minIndex,
                                Result = new List<DynamicRow>()
                            };

                            await reader.NextResultAsync(cancellation);
                            while (await reader.ReadAsync(cancellation))
                            {
                                var row = new DynamicRow(columnCount);
                                for (index = 0; index < targetIndices.Length; index++)
                                {
                                    var dbValue = reader.Value(index);
                                    int targetIndex = targetIndices[index];
                                    row.AddAt(dbValue, targetIndex);
                                }

                                treeResult.Result.Add(row);
                            }

                            trees.Add(treeResult);
                        }
                    }
                    catch (SqlException ex) when (ex.Number is 8134) // Divide by zero
                    {
                        throw new QueryException(DIVISION_BY_ZERO_MESSAGE);
                    }

                    trx.Complete();
                    result = new DynamicResult(rows, trees, count);
                }, cancellation);
            }
            catch (Exception ex)
            {
                // Include the SQL and the parameters
                throw new StatementLoaderException(sql, ps, ex);
            }

            return result;
        }

        /// <summary>
        /// Connects to the DB and loads the results of a list of entity statements described in <paramref name="args"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the root </typeparam>
        /// <param name="connString">The connection string of the SQL database from which to load the data.</param>
        /// <param name="args">All the information needed to connect to the database and execute the statements.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>The requested data packaged in a <see cref="EntityResult"/>.</returns>
        public async Task<EntityResult<TEntity>> LoadEntities<TEntity>(string connString, EntityLoaderArguments args, CancellationToken cancellation = default) where TEntity : Entity
        {
            // Destructure the args
            var countSql = args.CountSql;
            var statements = args.Statements;
            var ps = args.Parameters;
            var vars = args.Variables;

            // Prepare the full SQL
            string variablesSql = vars.ToSql();
            var sql = PrepareSql(
                variablesSql,
                countSql,
                statements.Select(e => e.Sql).ToArray());

            // These will be returned at the end
            EntityResult<TEntity> result = null;

            try
            {
                using var trx = TransactionFactory.ReadCommitted();
                await ExponentialBackoff(async () =>
                {
                    var entities = new List<Entity>();
                    var count = 0;
                    var results = statements.ToDictionary(e => e.Query, e => new List<Entity>());

                    // Connection
                    using var conn = new SqlConnection(connString);

                    // Command
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = sql;

                    // Command Parameters
                    foreach (var parameter in ps)
                    {
                        cmd.Parameters.Add(parameter);
                    }

                    // This data structure is a dictionary that maps Type -> Id -> Entity, and will contain all loaded entities with Ids
                    var allIdEntities = new IndexedEntities();

                    // This will contain all the descriptors of all the loaded entities
                    var descriptors = new HashSet<TypeDescriptor>();

                    try
                    {
                        await conn.OpenAsync(cancellation);
                        using var reader = await cmd.ExecuteReaderAsync(cancellation);

                        // If there is count SQL, load the count
                        if (!string.IsNullOrWhiteSpace(countSql))
                        {
                            if (await reader.ReadAsync(cancellation))
                            {
                                count = reader.GetInt32(0);
                            }

                            // Go over to the next result set
                            await reader.NextResultAsync(cancellation);
                        }

                        // Then load the data
                        foreach (var statement in statements)
                        {
                            var list = results[statement.Query];

                            // Group the column map by the path (which represents the target entity)
                            var entityTrie = ColumnMapTrie.Build(statement.ResultDescriptor, statement.ColumnMap);

                            // Assigns the appropriate EntitiesOfType at every level of the trie (If missing creates a new empty one)
                            entityTrie.InitializeEntitiesOfTypeDictionaries(allIdEntities);

                            // Grab all the entity descriptors loaded by this statement
                            foreach (var descriptor in entityTrie.AllEntityDescriptors())
                            {
                                descriptors.Add(descriptor);
                            }

                            // Sanity checking: if this isn't an aggregate query, 
                            if (entityTrie.Children.Any(e => !e.IdExists))
                            {
                                // Developer mistake
                                throw new InvalidOperationException($"The query for '{statement.Sql}' is not an aggregate query but is missing essential Ids");
                            }

                            // Loop over the result from the result set
                            while (await reader.ReadAsync(cancellation))
                            {
                                Entity record = AddEntity(reader, entityTrie);
                                list.Add(record);
                            }

                            if (statement.Query?.PrincipalQuery == null)
                            {
                                // This is the root query, assign the result to the final list
                                entities = list;
                            }

                            // Go over to the next result set
                            await reader.NextResultAsync(cancellation);
                        }
                    }
                    catch (SqlException ex) when (ex.Number is 8134) // Divide by zero (could be caused by a filter expression)
                    {
                        throw new QueryException(DIVISION_BY_ZERO_MESSAGE);
                    }

                    trx.Complete();

                    // Here we prepare a dictionary of all the entities loaded so far
                    Dictionary<Type, List<Entity>> allEntities = allIdEntities
                        .ToDictionary(e => e.Key, e => e.Value.Values.Cast<Entity>().ToList());

                    if (entities.Any())
                    {
                        var resultRootType = entities.First().GetType();
                        if (!allEntities.ContainsKey(resultRootType))
                        {
                            // this indicates that the main result is a fact table (no Ids), we add the fact lines
                            // here in order to have their weak nav properties hydrated
                            allEntities[resultRootType] = entities;
                        }
                    }

                    // Here we populate the navigation properties, after the simple properties have been populated
                    foreach (var descriptor in descriptors)
                    {
                        var navProps = descriptor.NavigationProperties.Where(p => allIdEntities.ContainsKey(p.Type));

                        var entitiesOfType = allEntities.GetValueOrDefault(descriptor.Type) ?? new List<Entity>();
                        foreach (var entity in entitiesOfType)
                        {
                            foreach (var navProp in navProps)
                            {
                                var fkProp = navProp.ForeignKey;

                                // The nav property can only be loaded if the FK property is loaded first
                                if (entity.EntityMetadata.IsLoaded(fkProp.Name))
                                {
                                    var fkValue = fkProp.GetValue(entity);
                                    if (fkValue == null)
                                    {
                                        // it is loaded and its value is NULL
                                        entity.EntityMetadata[navProp.Name] = FieldMetadata.Loaded;
                                    }
                                    else
                                    {
                                        var propCollectionType = navProp.Type;
                                        var entitiesOfPropertyType = allIdEntities[propCollectionType]; // It must be available since we checked earlier that it is
                                        entitiesOfPropertyType.TryGetValue(fkValue, out EntityWithKey navPropValue);
                                        if (navPropValue != null)
                                        {
                                            // it is loaded and it has a value
                                            entity.EntityMetadata[navProp.Name] = FieldMetadata.Loaded;
                                            navProp.SetValue(entity, navPropValue);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Here we populate the collection navigation properties after the navigation properties have been populated
                    foreach (var (query, list) in results)
                    {
                        if (query.IsAncestorExpand)
                        {
                            // Nothing to do here since this isn't a collection navigation property
                            // This is the Parent property which was already populated in the previous step
                            continue;
                        }
                        else if (query.PrincipalQuery != null)
                        {
                            var principalEntities = results[query.PrincipalQuery]; // The list of entities with collection nav properties
                            var pathToCollection = query.PathToCollectionPropertyInPrincipal;
                            var pathToCollectionEntity = new ArraySegment<string>(pathToCollection.Array, pathToCollection.Offset, pathToCollection.Count - 1);
                            var collectionPropName = pathToCollection[^1]; // Last item

                            // In the first step, we collect the collection entities in a hashset, by following the paths from the principal query result list
                            var collectionEntities = new HashSet<EntityWithKey>();
                            CollectionPropertyDescriptor collectionProp = null;

                            foreach (var principalEntity in principalEntities)
                            {
                                // We go down the path updating currentEntity as we go
                                var currentEntity = principalEntity;
                                var currentDescriptor = query.PrincipalQuery.ResultDescriptor;
                                foreach (var step in pathToCollectionEntity)
                                {
                                    var prop = currentDescriptor.Property(step);
                                    currentEntity = prop.GetValue(currentEntity) as Entity;
                                    currentDescriptor = prop.GetEntityDescriptor();

                                    // Check if the nextObject is null
                                    if (currentEntity == null)
                                    {
                                        // If there is a null object on the path, call it quits
                                        break;
                                    }
                                }

                                // Add the object (if any) to the collection entities
                                if (currentEntity != null)
                                {
                                    collectionEntities.Add((EntityWithKey)currentEntity);
                                }

                                // Grab the collection property
                                if (collectionProp == null)
                                {
                                    collectionProp = currentDescriptor.CollectionProperty(collectionPropName);
                                }

                                cancellation.ThrowIfCancellationRequested();
                            }

                            var fkName = query.ForeignKeyToPrincipalQuery;
                            var fkProp = query.ResultDescriptor.Property(fkName);
                            var groupedCollections = list.GroupBy(e => fkProp.GetValue(e)).ToDictionary(g => g.Key, g => MakeList(query.ResultDescriptor, g));

                            foreach (var collectionEntity in collectionEntities)
                            {
                                var id = collectionEntity.GetId();
                                if (!groupedCollections.TryGetValue(id, out IList collection))
                                {
                                    collection = query.ResultDescriptor.CreateList();
                                }

                                if (collection.Count == 4)
                                {

                                }

                                collectionEntity.EntityMetadata[collectionPropName] = FieldMetadata.Loaded;
                                collectionProp.SetValue(collectionEntity, collection);

                                cancellation.ThrowIfCancellationRequested();
                            }
                        }
                    }

                    result = new EntityResult<TEntity>(entities.Cast<TEntity>().ToList(), count);
                },
                cancellation);
            }
            catch (Exception ex)
            {
                // Include the SQL and the parameters
                throw new StatementLoaderException(sql, ps, ex);
            }

            return result;

            // This recursive method will return one of 2 things: 
            // 1 - Either a proper EntityWithKey, if the entity definition contains an Id index
            // 2 - Or a Fact Entity (without Id), if the entity defintion does not contain an Id index
            static Entity AddEntity(SqlDataReader reader, ColumnMapTrie entityTrie)
            {
                Entity entity;
                var isHydrated = false;

                if (entityTrie.IdExists) // This is an entity with a unique Id
                {
                    // Get the Id of this entity
                    var dbId = reader[entityTrie.IdIndex];
                    if (dbId == DBNull.Value)
                    {
                        return null;
                    }

                    var id = dbId;

                    var entitiesOfType = entityTrie.EntitiesOfType;
                    if (!entitiesOfType.TryGetValue(id, out EntityWithKey keyEntity))
                    {
                        // If the entity has not been created already, create it and flag it for hydration
                        keyEntity = entityTrie.Descriptor.Create() as EntityWithKey;
                        keyEntity.SetId(dbId);

                        entitiesOfType.Add(id, keyEntity);
                        entityTrie.HydratedIds.Add(id);

                        // New entity
                        isHydrated = false;
                    }
                    else
                    {
                        // Otherwise check if it has been added from the same part of the join tree or from a different one
                        if (entityTrie.HydratedIds.Add(id))
                        {
                            // Entity added before, but from a different part of the join tree, flag it for hydration
                            isHydrated = false;
                        }
                        else
                        {
                            // Entity added in a previous row, therefore already hydrated
                            isHydrated = true;
                        }
                    }

                    entity = keyEntity;
                }
                else // Entity has no Id, NOTE: This must be a level 0, otherwise validation earlier would capture it
                {
                    // New entity, flag it for hydration
                    isHydrated = false;

                    // Create the entity
                    entity = entityTrie.Descriptor.Create();
                }

                // As an optimization, only hydrate again if not hydrated before
                if (!isHydrated)
                {
                    // Hydrate the simple properties at the current entityDef level
                    foreach (var (propDesc, index) in entityTrie.Properties)
                    {
                        if (propDesc.IsHierarchyId || propDesc.IsGeography)
                        {
                            continue;
                        }

                        var dbValue = reader[index];
                        if (dbValue != DBNull.Value)
                        {
                            // char still comes from the DB as a string
                            if (propDesc.Type == typeof(char?))
                            {
                                dbValue = dbValue.ToString()[0]; // gets the char
                            }

                            propDesc.SetValue(entity, dbValue);
                        }

                        entity.EntityMetadata[propDesc.Name] = FieldMetadata.Loaded;
                    }
                }

                // Recursively call the next levels down
                foreach (var subEntityTrie in entityTrie.Children)
                {
                    AddEntity(reader, subEntityTrie);
                }

                return entity;
            }
        }

        #region Helper Functions

        private Task ExponentialBackoff(Func<Task> query, CancellationToken cancellation) =>
            RepositoryUtilities.ExponentialBackoff(query, _logger, "", "", cancellation);

        private static string PrepareSql(string variablesSql, string countSql, params string[] statementSqls)
        {
            // Prepare the main sql script
            var sql = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(variablesSql))
            {
                sql.AppendLine(variablesSql);
                sql.AppendLine(); // Just for aesthetics
            }

            if (!string.IsNullOrEmpty(countSql))
            {
                sql.AppendLine(countSql);
                sql.AppendLine(); // Just for aesthetics
            }

            foreach (var statement in statementSqls)
            {
                sql.AppendLine(statement);
                sql.AppendLine(); // Just for aesthetics
            }

            return sql.ToString();
        }

        /// <summary>
        /// Just a helper method for returning a generic IList
        /// </summary>
        private static IList MakeList(TypeDescriptor desc, IEnumerable collection = null)
        {
            var list = desc.CreateList();
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    list.Add(item);
                }
            }

            return list;
        }

        #endregion

        #region ColumnMapTrie

        /// <summary>
        /// A tree structure representing the columns of an SQL select statement, in a
        /// way that makes hydrating the Entities from the statement result more efficient
        /// </summary>
        private class ColumnMapTrie : Dictionary<string, ColumnMapTrie>
        {
            /// <summary>
            /// The type of the Entity representing this level
            /// </summary>
            public TypeDescriptor Descriptor { get; set; }

            /// <summary>
            /// When no IdIndex is set, this level is not Entityable
            /// </summary>
            public int IdIndex { get; set; } = -1;

            /// <summary>
            /// All the properties at this level mentioned by all the paths
            /// </summary>
            public List<(PropertyDescriptor Property, int Index)> Properties { get; set; } = new List<(PropertyDescriptor, int)>();

            /// <summary>
            /// The segment of the path leading up to this level
            /// </summary>
            public ArraySegment<string> Path { get; set; }

            /// <summary>
            /// Returns true when this level of the tree should hydrate a proper EntityWithKey
            /// </summary>
            public bool IdExists { get => IdIndex >= 0; }

            /// <summary>
            /// The children of the current tree level
            /// </summary>
            public IEnumerable<ColumnMapTrie> Children { get => Values; }

            /// <summary>
            /// Takes a root type and a bunch of paths and constructs the entire <see cref="ColumnMapTrie"/> which is useful
            /// for efficiently hydrating Entities and dynamic results from the SQL query result,
            /// a single tree is associated with a single SQL select statement
            /// </summary>
            public static ColumnMapTrie Build(TypeDescriptor rootDescriptor, List<SqlEntityStatementColumn> columnMap)
            {
                var root = new ColumnMapTrie { Descriptor = rootDescriptor, Path = Array.Empty<string>() };
                for (var i = 0; i < columnMap.Count; i++)
                {
                    // Phase (1) Go down the path
                    var columnInfo = columnMap[i];
                    var path = columnInfo.Path;
                    var currentTree = root;

                    for (int j = 0; j < path.Count; j++)
                    {
                        var step = path[j];

                        if (!currentTree.ContainsKey(step))
                        {
                            var propDescriptor = currentTree.Descriptor.Property(step);
                            if (propDescriptor == null)
                            {
                                // Developer mistake
                                throw new InvalidOperationException($"Property {step} was not found on type {currentTree.Descriptor.Name}");
                            }

                            currentTree[step] = new ColumnMapTrie
                            {
                                Descriptor = propDescriptor.GetEntityDescriptor(),
                                Path = new ArraySegment<string>(path.Array, path.Offset, j + 1),
                            };
                        }

                        currentTree = currentTree[step];
                    }

                    // Phase (1) Set the property info
                    var propName = columnInfo.Property;


                    // In flat queries, the Id is given special treatment for efficiency, since it used to connect related entities together
                    if (propName == "Id" && currentTree.Descriptor.HasId)
                    {
                        // This IdIndex is only set for Entityable path terminals
                        currentTree.IdIndex = i;
                    }
                    else
                    {
                        var propDescriptor = currentTree.Descriptor.Property(propName);
                        currentTree.Properties.Add((propDescriptor, i));
                    }
                }

                // Ensures consistency of Ids
                ValidateIds(root);

                // Return the result
                return root;
            }

            /// <summary>
            /// Ensures that if a parent has Id, that the children do as well, if this fails it exposes a programmer mistake
            /// </summary>
            private static void ValidateIds(ColumnMapTrie currentTree, bool parentIdExists = false)
            {
                if (!currentTree.IdExists && parentIdExists)
                {
                    // Developer mistake
                    throw new InvalidOperationException($"The level '{string.Join(".", currentTree.Path)}' of type '{currentTree.Descriptor.Name}' is missing its Id");
                }

                bool currentIdExists = currentTree.IdExists;
                foreach (var childTree in currentTree.Children)
                {
                    ValidateIds(childTree, currentIdExists);
                }
            }

            /// <summary>
            /// Optimization used by the loader to remember which entities where hydrated at this trie level to skip hydrating them again
            /// </summary>
            public HashSet<object> HydratedIds { get; } = new HashSet<object>();

            /// <summary>
            /// The loaded entities of the type of this trie level
            /// </summary>
            public IndexedEntitiesOfType EntitiesOfType { get; set; }

            /// <summary>
            /// To optimize loading entities, this method grabs the appropriate <see cref="EntitiesOfType"/>
            /// for this trie level and stores it in the trie level, it creates it if it can't finds it.
            /// This method ignores levels without an Id
            /// </summary>
            public void InitializeEntitiesOfTypeDictionaries(IndexedEntities allIdEntities)
            {
                if (Descriptor.HasId)
                {
                    var type = Descriptor.Type;
                    if (!allIdEntities.TryGetValue(type, out IndexedEntitiesOfType entities))
                    {
                        entities = allIdEntities[type] = new IndexedEntitiesOfType();
                    }

                    EntitiesOfType = entities;
                }

                foreach (var child in Children)
                {
                    child.InitializeEntitiesOfTypeDictionaries(allIdEntities);
                }
            }

            /// <summary>
            /// Traverses the trees and returns for every loaded type the corresponding list of navigation properties from the descriptor.
            /// Some types may be returned more than once if they are loaded on more than one level of the trie
            /// </summary>
            public IEnumerable<TypeDescriptor> AllEntityDescriptors()
            {
                // For this level
                yield return Descriptor;

                // For children
                foreach (var child in Children)
                {
                    foreach (var descriptor in child.AllEntityDescriptors())
                    {
                        yield return descriptor;
                    }
                }
            }
        }

        #endregion
    }
}
