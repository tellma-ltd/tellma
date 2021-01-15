using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Entities.Descriptors;
using Tellma.Entities;
using Tellma.Services.Utilities;
using Tellma.Services;

namespace Tellma.Data.Queries
{
    internal static class EntityLoader
    {
        private static string PrepareSql(string preparatorySql, string countSql, params SqlStatement[] statements)
        {
            // Prepare the main sql script
            StringBuilder sql = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(preparatorySql))
            {
                sql.AppendLine(preparatorySql);
                sql.AppendLine(); // Just for aesthetics
            }

            if (!string.IsNullOrEmpty(countSql))
            {
                sql.AppendLine(countSql);
                sql.AppendLine(); // Just for aesthetics
            }

            foreach (var statement in statements)
            {
                sql.AppendLine(statement.Sql);
                sql.AppendLine(); // Just for aesthetics
            }

            return sql.ToString();
        }

        /// <summary>
        /// This methods loads the results of an <see cref="AggregateQuery{T}"/> into a list of <see cref="DynamicEntity"/>
        /// </summary>
        /// <param name="statement">The <see cref="SqlStatement"/> to load</param>
        /// <param name="preparatorySql">Any SQL to be included at the very beginning the main script (cannot contain a SELECT or return a result set</param>
        /// <param name="ps">The parameters needed by SQL</param>
        /// <param name="conn">The SQL Server connection through which to execute the SQL script</param>
        /// <returns>The list of hydrated <see cref="DynamicEntity"/>s</returns>            
        public static async Task<List<DynamicRow>> LoadAggregateStatement(
            SqlStatement statement, SqlStatementVariables vars, SqlStatementParameters ps, SqlConnection conn, CancellationToken cancellation)
        {
            var result = new List<DynamicRow>();

            using (var cmd = conn.CreateCommand())
            {
                // Add any variables in the preparatory SQL
                string preparatorySql = vars.ToSql();

                // Command Text
                cmd.CommandText = PrepareSql(preparatorySql, null, statement);

                // Command Parameters
                foreach (var parameter in ps)
                {
                    cmd.Parameters.Add(parameter);
                }

                // It will always be open, but we add this nonetheless for robustness
                bool ownsConnection = conn.State != System.Data.ConnectionState.Open;
                if (ownsConnection)
                {
                    conn.Open();
                }

                // This recursive method hydrates the dynamic properties from the reader according to the entityDef tree
                static void HydrateDynamicProperties(SqlDataReader reader, DynamicRow entity, ColumnMapTrie entityDef)
                {
                    foreach (var (propDesc, index, aggregation, function) in entityDef.Properties)
                    {
                        if (propDesc.IsHierarchyId || propDesc.IsGeography)
                        {
                            continue;
                        }

                        var dbValue = reader[index];
                        if (dbValue == DBNull.Value)
                        {
                            dbValue = null;
                        }



                        if (dbValue != DBNull.Value)
                        {
                            // char still comes from the DB as a string
                            if (propDesc.Type == typeof(char?))
                            {
                                dbValue = dbValue.ToString()[0]; // gets the char
                            }

                            // The propertyNameMap was populated as soon as the ColumnMapTree was created
                            string propName = DynamicPropertyName(entityDef.Path, propDesc.Name, aggregation, function);
                            entity[propName] = dbValue;
                        }
                    }

                    foreach (var subEntityDef in entityDef.Children)
                    {
                        HydrateDynamicProperties(reader, entity, subEntityDef);
                    }
                }

                // Results are loaded
                try
                {
                    using var reader = await cmd.ExecuteReaderAsync(cancellation);

                    // Group the column map by the path (which represents the target entity)
                    var entityDef = ColumnMapTrie.Build(statement.ResultDescriptor, statement.ColumnMap, isAggregate: true);

                    int columnCount = statement.Col

                    // Loop over the result from the result set
                    while (await reader.ReadAsync(cancellation))
                    {
                        var dynamicEntity = new DynamicRow();
                        HydrateDynamicProperties(reader, dynamicEntity, entityDef);
                        result.Add(dynamicEntity);
                    }
                }
                finally
                {
                    // Otherwise we might get an error when a parameter is reused
                    cmd.Parameters.Clear();

                    // The connection is never owned, but we add this code anyways for robustness
                    if (ownsConnection)
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// This method builds one large SQL script from the supplied <see cref="SqlStatement"/>s and executes it against a SQL server connection in one go,
        /// then it uses the result sets to hydrate and connect the entities together via their navigation properties and then it returns the main list back
        /// </summary>
        /// <param name="queries">The list of <see cref="SqlStatement"/> to load</param>
        /// <param name="preparatorySql">Any SQL to be included at the very beginning the main script (cannot contain a SELECT or return a result set</param>
        /// <param name="ps">The parameters needed by SQL</param>
        /// <param name="conn">The SQL Server connection through which to execute the SQL script</param>
        /// <returns>The list of hydrated entities with all related entities attached by means of navigation properties</returns>            
        public static async Task<(List<Entity> result, int count)> LoadStatements<T>(
           List<SqlStatement> statements,
           string preparatorySql,
           string countSql,
           SqlStatementParameters ps,
           SqlConnection conn,
           IInstrumentationService instrumentation,
           CancellationToken cancellation) where T : Entity
        {
            using var _ = instrumentation.Block("EntityLoader.Load");
            IDisposable block;

            var results = statements.ToDictionary(e => e.Query, e => new List<Entity>());

            block = instrumentation.Block("Command + Connection");

            // Prepare the SQL command
            using var cmd = conn.CreateCommand();

            // Command Text
            cmd.CommandText = PrepareSql(preparatorySql, countSql, statements.ToArray());

            // Command Parameters
            foreach (var parameter in ps)
            {
                cmd.Parameters.Add(parameter);
            }

            // It will always be open, but we add this nonetheless for robustness
            bool ownsConnection = conn.State != System.Data.ConnectionState.Open;
            if (ownsConnection)
            {
                conn.Open();
            }

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
                    foreach (var (propDesc, index, aggregation, function) in entityTrie.Properties)
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

            // This data structure is a dictionary that maps Type -> Id -> Entity, and will contain all loaded entities with Ids
            var allIdEntities = new IndexedEntities();

            // This collection will be returned at the end
            var result = new List<Entity>();
            var count = 0;

            // This will contain all the descriptors of all the loaded entities
            var descriptors = new HashSet<TypeDescriptor>();

            block.Dispose();
            block = instrumentation.Block("Load + Simple Props");

            try
            {
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
                    var entityTrie = ColumnMapTrie.Build(statement.ResultDescriptor, statement.ColumnMap, isAggregate: false);

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
                        result = list;
                    }

                    // Go over to the next result set
                    await reader.NextResultAsync(cancellation);
                }
            }
            finally
            {
                // Otherwise we might get an error when a parameter is reused
                cmd.Parameters.Clear();

                // The connection is never owned, but we add this code anyways for robustness
                if (ownsConnection)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            block.Dispose();
            block = instrumentation.Block("allEntities");

            // Here we prepare a dictionary of all the entities loaded so far
            Dictionary<Type, List<Entity>> allEntities = allIdEntities
                .ToDictionary(e => e.Key, e => e.Value.Values.Cast<Entity>().ToList());

            if (result.Any())
            {
                var resultRootType = result.First().GetType().GetRootType();
                if (!allEntities.ContainsKey(resultRootType))
                {
                    // this indicates that the main result is a fact table (no Ids), we add the fact lines
                    // here in order to have their weak nav properties hydrated
                    allEntities[resultRootType] = result;
                }
            }

            block.Dispose();
            block = instrumentation.Block("Navigation Props");

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
                                var propCollectionType = navProp.Type.GetRootType();
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

            block.Dispose();
            block = instrumentation.Block("Collection Props");

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
                        if(!groupedCollections.TryGetValue(id, out IList collection))
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

            block.Dispose();

            // Return the result
            return (result, count);
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
            public List<(PropertyDescriptor Property, int Index, string Aggregation, string Function)> Properties { get; set; } = new List<(PropertyDescriptor, int, string, string)>();

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
            public static ColumnMapTrie Build(TypeDescriptor rootDescriptor, List<SqlStatementColumn> columnMap, bool isAggregate)
            {
                var root = new ColumnMapTrie { Descriptor = rootDescriptor, Path = new string[0] };
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
                    var aggregation = string.IsNullOrWhiteSpace(columnInfo.Aggregation) ? null : columnInfo.Aggregation;
                    var function = string.IsNullOrWhiteSpace(columnInfo.Modifier) ? null : columnInfo.Modifier;


                    // In flat queries, the Id is given special treatment for efficiency, since it used to connect related entities together
                    if (!isAggregate && propName == "Id" && string.IsNullOrWhiteSpace(aggregation) && string.IsNullOrWhiteSpace(function) && currentTree.Descriptor.HasId)
                    {
                        // This IdIndex is only set for Entityable path terminals
                        currentTree.IdIndex = i;
                    }
                    else
                    {
                        var propDescriptor = currentTree.Descriptor.Property(propName);
                        currentTree.Properties.Add((propDescriptor, i, aggregation, function));
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
                    throw new InvalidOperationException($"The level '{string.Join("/", currentTree.Path)}' of type '{currentTree.Descriptor.Name}' is missing its Id");
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
    }
}
