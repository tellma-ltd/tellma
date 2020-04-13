using Tellma.Entities;
using Tellma.Services.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Tellma.Data.Queries
{
    internal static class EntityLoader
    {
        private static string PrepareSql(string preparatorySql, params SqlStatement[] statements)
        {
            // Prepare the main sql script
            StringBuilder sql = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(preparatorySql))
            {
                sql.AppendLine(preparatorySql);
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
        public static async Task<List<DynamicEntity>> LoadAggregateStatement(
            SqlStatement statement, string preparatorySql, SqlStatementParameters ps, SqlConnection conn, CancellationToken cancellation)
        {
            var result = new List<DynamicEntity>();

            using (var cmd = conn.CreateCommand())
            {
                // Command Text
                cmd.CommandText = PrepareSql(preparatorySql, statement);

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

                // Efficiently calculates the dynamic property name
                var cacheDynamicPropertyName = new Dictionary<(ArraySegment<string>, string, string, string), string>();
                string DynamicPropertyName(ArraySegment<string> path, string prop, string aggregation, string function)
                {
                    if (!cacheDynamicPropertyName.ContainsKey((path, prop, aggregation, function)))
                    {
                        string result = prop;

                        string pathString = string.Join('/', path);
                        if (!string.IsNullOrWhiteSpace(pathString))
                        {
                            result = $"{pathString}/{result}";
                        }

                        if (!string.IsNullOrWhiteSpace(function))
                        {
                            result = $"{result}|{function}";
                        }

                        if (!string.IsNullOrWhiteSpace(aggregation))
                        {
                            result = $"{aggregation}({result})";
                        }

                        cacheDynamicPropertyName[(path, prop, aggregation, function)] = result;
                    }

                    return cacheDynamicPropertyName[(path, prop, aggregation, function)];
                }

                // This recursive method hydrates the dynamic properties from the reader according to the entityDef tree
                void HydrateDynamicProperties(SqlDataReader reader, DynamicEntity entity, ColumnMapTree entityDef)
                {
                    foreach (var (propInfo, index, aggregation, function) in entityDef.Properties)
                    {
                        if (propInfo.PropertyType == typeof(HierarchyId))
                        {
                            continue;
                        }

                        var dbValue = reader[index];
                        if (dbValue != DBNull.Value)
                        {
                            // char still comes from the DB as a string
                            if (propInfo.PropertyType == typeof(char?))
                            {
                                dbValue = dbValue.ToString()[0]; // gets the char
                            }

                            // The propertyNameMap was populated as soon as the ColumnMapTree was created
                            string propName = DynamicPropertyName(entityDef.Path, propInfo.Name, aggregation, function);
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
                    var entityDef = ColumnMapTree.Build(statement.ResultType, statement.ColumnMap, isAggregate: true);

                    // Loop over the result from the result set
                    while (await reader.ReadAsync(cancellation))
                    {
                        var dynamicEntity = new DynamicEntity();
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
        public static async Task<List<Entity>> LoadStatements<T>(
           List<SqlStatement> statements, string preparatorySql, SqlStatementParameters ps, SqlConnection conn, CancellationToken cancellation) where T : Entity
        {
            var results = statements.ToDictionary(e => e.Query, e => new List<Entity>());

            // Prepare the SQL command
            using var cmd = conn.CreateCommand();

            // Command Text
            cmd.CommandText = PrepareSql(preparatorySql, statements.ToArray());

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

            var memory = new HashSet<(object Id, ColumnMapTree Tree)>(); // Remembers which (Id, tree) combinations have been added already
            var allIdEntities = new IndexedEntities(); // This data structure is a dictionary that maps Type -> Id -> Entity, and will contain all loaded entities with Ids

            // This recursive method will return one of 3 things: 
            // 1 - Either a proper EntityWithKey, if the entity definition contains an Id index
            // 2 - Or a Fact Entity (without Id), if the entity defintion does not contain an Id index
            // 3 - Or a DynamicEntity (a dictionary), if the entity defintion does not contain an Id index and this is an aggregate query
            Entity AddEntity(SqlDataReader reader, ColumnMapTree entityDef)
            {
                Entity entity;
                var isHydrated = false;
                var entityType = entityDef.Type; // TODO: The specific type to use when instantiating the entity: should come from discriminator e.g. Agent

                if (entityDef.IdExists)
                {
                    var collectionType = entityType.GetRootType();

                    // Make sure the dictionary that tracks this type is created already
                    if (!allIdEntities.TryGetValue(collectionType, out IndexedEntitiesOfType entitiesOfType))
                    {
                        entitiesOfType = allIdEntities[collectionType] = new IndexedEntitiesOfType();
                    }

                    // Get the Id of this entity
                    var dbId = reader[entityDef.IdIndex];
                    if (dbId == DBNull.Value)
                    {
                        return null;
                    }

                    var id = dbId;
                    entitiesOfType.TryGetValue(id, out EntityWithKey keyEntity);

                    if (keyEntity == null)
                    {
                        // If the entity has not been created already, create it and flag it for hydration
                        keyEntity = Activator.CreateInstance(entityType) as EntityWithKey;
                        keyEntity.SetId(dbId);

                        entitiesOfType.Add(id, keyEntity);
                        memory.Add((id, entityDef));

                        // New entity
                        isHydrated = false;
                    }
                    else
                    {
                        // Otherwise check if it has been added from the same part of the join tree or from a different one
                        if (memory.Add((id, entityDef)))
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
                    entity = Activator.CreateInstance(entityType) as Entity;
                }

                // As an optimization, only hydrate again if not hydrated before
                if (!isHydrated)
                {
                    // Hydrate the simple properties at the current entityDef level
                    foreach (var (propInfo, index, aggregation, function) in entityDef.Properties)
                    {
                        if (propInfo.PropertyType == typeof(HierarchyId))
                        {
                            continue;
                        }

                        var dbValue = reader[index];
                        if (dbValue != DBNull.Value)
                        {
                            // char still comes from the DB as a string
                            if (propInfo.PropertyType == typeof(char?))
                            {
                                dbValue = dbValue.ToString()[0]; // gets the char
                            }

                            propInfo.SetValue(entity, dbValue);
                        }

                        entity.EntityMetadata[propInfo.Name] = FieldMetadata.Loaded;
                    }
                }

                // Recursively call the next levels down
                foreach (var subEntityDef in entityDef.Children)
                {
                    AddEntity(reader, subEntityDef);
                }

                return entity;
            }

            // This collection will be returned at the end
            var result = new List<Entity>();

            try
            {
                using var reader = await cmd.ExecuteReaderAsync(cancellation);

                foreach (var statement in statements)
                {
                    var list = results[statement.Query];

                    // Group the column map by the path (which represents the target entity)
                    var entityDef = ColumnMapTree.Build(statement.ResultType, statement.ColumnMap, isAggregate: false);

                    // Sanity checking: if this isn't an aggregate query, 
                    if (entityDef.Children.Any(e => !e.IdExists))
                    {
                        // Developer mistake
                        throw new InvalidOperationException($"The query for '{statement.Sql}' is not an aggregate query but is missing essential Ids");
                    }

                    // Loop over the result from the result set
                    while (await reader.ReadAsync(cancellation))
                    {
                        Entity record = AddEntity(reader, entityDef);
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

            // Here we prepare a dictionary of all the entities loaded so far
            Dictionary<Type, List<Entity>> allEntities = allIdEntities
                .ToDictionary(e => e.Key, e => e.Value.Values.Cast<Entity>().ToList());

            if (result.Any())
            {
                var resultRootType = result.First().GetType().GetRootType();
                if (!allEntities.ContainsKey(resultRootType))
                {
                    // this indicates that the main result is a fact table, we add the fact lines
                    // here in order to have their weak nav properties hydrated
                    allEntities[resultRootType] = result;
                }
            }

            // The method below efficiently retrieves the navigation properties of each entity
            Type cacheType = null;
            List<(PropertyInfo NavProp, PropertyInfo FkProp)> navAndFkCache = null;
            List<(PropertyInfo NavProp, PropertyInfo FkProp)> GetNavAndFkProps(Entity entity)
            {
                if (entity.GetType() != cacheType) // In a single root type we might have entities belonging to multiple different deriving types
                {
                    cacheType = entity.GetType();
                    navAndFkCache = new List<(PropertyInfo NavProp, PropertyInfo FkProp)>();

                    IEnumerable<PropertyInfo> nonListProperties = cacheType.GetProperties()
                            .Where(e => !e.PropertyType.IsList());

                    navAndFkCache = nonListProperties
                        .Where(p => p.ForeignKeyProperty() != null && allIdEntities.ContainsKey(p.PropertyType.GetRootType()))
                        .Select(p => (p, p.ForeignKeyProperty()))
                        .ToList();
                }

                return navAndFkCache;
            }

            // All simple properties are populated before, but not navigation properties, those are done here
            foreach (var (rootType, entitiesOfType) in allEntities.Where(e => e.Value.Any()))
            {
                // Loop over all entities and use the method above to hydrate the navigation properties
                foreach (var entity in entitiesOfType)
                {
                    foreach (var (navProp, fkProp) in GetNavAndFkProps(entity))
                    {
                        // The nav property can only be loaded if the FK property is loaded first
                        if (fkProp.Name == "Id" || entity.EntityMetadata.TryGetValue(fkProp.Name, out FieldMetadata meta) && meta == FieldMetadata.Loaded)
                        {
                            var fkValue = fkProp.GetValue(entity);
                            if (fkValue == null)
                            {
                                // it is loaded and its value is NULL
                                entity.EntityMetadata[navProp.Name] = FieldMetadata.Loaded;
                            }
                            else
                            {
                                var propCollectionType = navProp.PropertyType.GetRootType();
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
                    var collectionPropName = pathToCollection[pathToCollection.Count - 1];

                    // In the first step, we collect the collection entities in a hashset, by following the paths from the principal query result list
                    var collectionEntities = new HashSet<Entity>();
                    foreach (var principalEntity in principalEntities)
                    {
                        // We go down the path updating currentEntity as we go
                        var currentEntity = principalEntity;
                        foreach (var step in pathToCollectionEntity)
                        {
                            var prop = currentEntity.GetType().GetProperty(step);
                            currentEntity = prop.GetValue(currentEntity) as Entity;

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
                            collectionEntities.Add(currentEntity);
                        }
                    }

                    var fkName = query.ForeignKeyToPrincipalQuery;
                    var fkProp = query.ResultType.GetProperty(fkName);
                    var groupedCollections = list.GroupBy(e => fkProp.GetValue(e)).ToDictionary(g => g.Key, g => MakeList(query.ResultType, g));

                    PropertyInfo collectionProp = null;
                    foreach (var collectionEntity in collectionEntities.Cast<EntityWithKey>())
                    {
                        // Rule every entity with collections attached to it must be a EntityWithKey
                        if (collectionProp == null)
                        {
                            collectionProp = collectionEntity.GetType().GetProperty(collectionPropName);
                        }

                        var id = collectionEntity.GetId();
                        var collection = groupedCollections.ContainsKey(id) ? groupedCollections[id] : MakeList(query.ResultType);

                        collectionEntity.EntityMetadata[collectionPropName] = FieldMetadata.Loaded;
                        collectionProp.SetValue(collectionEntity, collection);
                    }
                }
            }

            // Return the result
            return result;
        }

        public static PropertyInfo ForeignKeyProperty(this PropertyInfo _propInfo)
        {
            var fkName = _propInfo.GetCustomAttribute<ForeignKeyAttribute>()?.Name;
            if (!string.IsNullOrWhiteSpace(fkName))
            {
                return _propInfo.DeclaringType.GetProperty(fkName);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Just a helper method for returning a generic IList
        /// </summary>
        private static IList MakeList(Type t, IEnumerable collection = null)
        {
            var listType = typeof(List<>).MakeGenericType(t);
            var list = (IList)Activator.CreateInstance(listType);
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
        private class ColumnMapTree : Dictionary<string, ColumnMapTree>
        {
            /// <summary>
            /// The type of the Entity representing this level
            /// </summary>
            public Type Type { get; set; }

            /// <summary>
            /// When no IdIndex is set, this level is not Entityable
            /// </summary>
            public int IdIndex { get; set; } = -1;

            /// <summary>
            /// All the properties at this level mentioned by all the paths
            /// </summary>
            public List<(PropertyInfo Property, int Index, string Aggregation, string Function)> Properties { get; set; } = new List<(PropertyInfo, int, string, string)>();

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
            public IEnumerable<ColumnMapTree> Children { get => Values; }

            /// <summary>
            /// Takes a root type and a bunch of paths and constructs the entire <see cref="ColumnMapTree"/> which is useful
            /// for efficiently hydrating Entities and dynamic results from the SQL query result,
            /// a single tree is associated with a single SQL select statement
            /// </summary>
            public static ColumnMapTree Build(Type type, List<SqlStatementColumn> columnMap, bool isAggregate)
            {
                var root = new ColumnMapTree { Type = type, Path = new string[0] };
                for (var i = 0; i < columnMap.Count; i++)
                {
                    var columnInfo = columnMap[i];
                    var path = columnInfo.Path;
                    var propName = columnInfo.Property;
                    var aggregation = string.IsNullOrWhiteSpace(columnInfo.Aggregation) ? null : columnInfo.Aggregation;
                    var function = string.IsNullOrWhiteSpace(columnInfo.Function) ? null : columnInfo.Function;
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
                                Path = new ArraySegment<string>(path.Array, path.Offset, j + 1),
                            };
                        }

                        currentTree = currentTree[step];
                    }

                    // In flat queries, the Id is given special treatment for efficiency, since it used to connect related entities together
                    if (!isAggregate && propName == "Id" && string.IsNullOrWhiteSpace(aggregation) && string.IsNullOrWhiteSpace(function) && currentTree.Type.IsSubclassOf(typeof(EntityWithKey)))
                    {
                        // This IdIndex is only set for Entityable path terminals
                        currentTree.IdIndex = i;
                    }
                    else
                    {
                        var propInfo = currentTree.Type.GetProperty(propName);
                        currentTree.Properties.Add((propInfo, i, aggregation, function));
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
            private static void ValidateIds(ColumnMapTree currentTree, bool parentIdExists = false)
            {
                if (!currentTree.IdExists && parentIdExists)
                {
                    // Developer mistake
                    throw new InvalidOperationException($"The level '{string.Join("/", currentTree.Path)}' of type '{currentTree.Type.Name}' is missing its Id");
                }

                bool currentIdExists = currentTree.IdExists;
                foreach (var childTree in currentTree.Children)
                {
                    ValidateIds(childTree, currentIdExists);
                }
            }
        }
    }
}
