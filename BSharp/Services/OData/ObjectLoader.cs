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
    public static class ObjectLoader
    {
        /// <summary>
        /// Executes the SQL statement and returns the result in the form of 4 items: 
        /// 1 - The result entities
        /// 2 - All related strong entities in a <see cref="EntitiesMap"/> object
        /// </summary>
        /// <param name="queries">The list of queries and statements should equal the number of result sets returned by sql</param>
        /// <param name="preparatorySql">Any SQL to be included before the main SQL</param>
        /// <param name="ps">The parameters needed by SQL</param>
        /// <param name="conn"></param>
        /// <param name="trx">The transaction if any</param>
        /// <returns>The hydrated DTOs list + any related strong entities</returns>            
        public static async Task<ObjectLoaderResult> LoadStatements<T>(
           List<(IQueryInternal Query, SqlStatement Statement)> queries,
           string preparatorySql,
           SqlStatementParameters ps,
           SqlConnection conn,
           SqlTransaction trx) where T : DtoBase
        {
            var statements = queries.Select(e => e.Statement);
            var results = queries.ToDictionary(e => e.Query, e => new List<DtoBase>());

            // prepare the sql
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

            using (var cmd = conn.CreateCommand())
            {
                if (trx != null)
                {
                    cmd.Transaction = trx;
                }

                // Prepare the SQL command
                cmd.CommandText = sql.ToString();
                foreach (var parameter in ps)
                {
                    cmd.Parameters.Add(parameter);
                }

                bool ownsConnection = conn.State != System.Data.ConnectionState.Open;
                if (ownsConnection)
                {
                    conn.Open();
                }


                // These dictionaries will contain the result
                var memory = new HashSet<(string, ColumnMapTree)>(); // Remembers which (Id, tree) combinations have been added already
                var allIdEntities = new Dictionary<Type, Dictionary<string, DtoBase>>(); // int ids are cast to string
                var relatedEntitiesDic = new Dictionary<Type, List<DtoBase>>();

                // This method will return one of 3 things: 
                // 1 - Either a propert DTO with ID, if the entity definition contains an Id index
                // 2 - Or a Fact DTO (without Id), if the entity defintion does not contain an Id index
                // 3 - Or a dynamic entity (a dictionary), if the entity defintion does not contain an Id index and this is an aggregate query
                // NOTE: parameter isIdParent ensures that if the parent DTO has an Id, that every child DTO also has an Id
                DtoBase AddEntity(SqlDataReader reader, ColumnMapTree entityDef, bool isAggregate, bool addToRelatedEntities, DynamicEntity dynamicEntity = null)
                {
                    if (isAggregate && !entityDef.IdExists)
                    {
                        // Either we have an aggregate query or the result comes from a fact table
                        // either way, it is not to be added to the allEntities collection

                        // This level is not DTOable and needs to be populated in a dynamic entity
                        dynamicEntity = dynamicEntity ?? new DynamicEntity();

                        // Hydrate the properties
                        HydrateProperties(reader, dynamicEntity, entityDef);

                        foreach (var subEntityDef in entityDef.Children)
                        {
                            AddEntity(reader, subEntityDef, isAggregate, addToRelatedEntities: true, dynamicEntity);
                        }

                        return dynamicEntity;

                    }
                    else // Either Id exists or this is not aggregate, we create a proper DTO and add it to the allEntities collection
                    {
                        DtoBase entity;
                        var isHydrated = false;
                        var entityType = entityDef.Type; // TODO: The specific type to use when instantiating the entity: should come from discriminator e.g. Agent

                        if (entityDef.IdExists)
                        {
                            var collectionType = entityType; // TODO: The root type of the collection where to store and track this entity e.g. Custody

                            // Make sure the dictionary that tracks this type is created already
                            if (!allIdEntities.ContainsKey(collectionType))
                            {
                                allIdEntities[collectionType] = new Dictionary<string, DtoBase>();
                            }

                            var entitiesOfType = allIdEntities[collectionType];

                            var dbId = reader[entityDef.IdIndex];
                            if (dbId == DBNull.Value)
                            {
                                return null;
                            }

                            var id = dbId.ToString();
                            entitiesOfType.TryGetValue(id, out entity);

                            if (entity == null)
                            {
                                entity = Activator.CreateInstance(entityType) as DtoBase;
                                entityDef.IdProperty.SetValue(entity, dbId);

                                entitiesOfType.Add(id, entity);
                                memory.Add((id, entityDef));

                                // If instructed, add the entity to related entities
                                if(addToRelatedEntities)
                                {
                                    if (entityType.GetCustomAttribute<StrongDtoAttribute>() != null)
                                    {
                                        // Add the newly created entity to the related entities
                                        if (!relatedEntitiesDic.TryGetValue(collectionType, out List<DtoBase> relatedEntitiesOfType))
                                        {
                                            relatedEntitiesOfType = relatedEntitiesDic[collectionType] = new List<DtoBase>();
                                        }

                                        relatedEntitiesOfType.Add(entity);
                                    }
                                }

                                // New entity
                                isHydrated = false;
                            }
                            else
                            {
                                if (memory.Add((id, entityDef)))
                                {
                                    // Entity added before, but from a different part of the join tree
                                    isHydrated = false;
                                }
                                else
                                {
                                    // Entity added in a previous row
                                    isHydrated = true;
                                }
                            }

                        }
                        else // NOTE: This must be a level 0, otherwise validation earlier would capture it
                        {
                            // This is the root of a query without that doesn't return Id

                            // Create the entity
                            entity = Activator.CreateInstance(entityType) as DtoBase;

                            // New entity
                            isHydrated = false;
                        }

                        // As an optimization, only hydrate again if not hydrated before
                        if (!isHydrated)
                        {
                            // Hydrate the properties
                            HydrateProperties(reader, entity, entityDef);
                        }

                        // Recursively call the next level down
                        foreach (var subEntityDef in entityDef.Children)
                        {
                            AddEntity(reader, subEntityDef, isAggregate, addToRelatedEntities: true);
                        }

                        return entity;
                    }
                }


                // These collections will be returned at the end
                bool isAggregateRootQuery = false;
                var result = new List<DtoBase>();

                // The result that will be returned at the end
                try
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        foreach (var statement in statements)
                        {
                            bool isRootQuery = statement.Query?.PrincipalQuery == null;
                            var list = results[statement.Query];

                            // Group the column map by the path (which represents the target entity)
                            var entityDef = ColumnMapTree.Build(statement.ResultType, statement.ColumnMap);

                            // Sanity checking: if this isn't an aggregate query, 
                            if (!statement.IsAggregate && entityDef.Children.Any(e => !e.IdExists))
                            {
                                // Developer mistake
                                throw new InvalidOperationException($"The query for '{statement.Sql}' is not an aggregate query but is missing essential Ids");
                            }

                            // Loop over the result from the database
                            while (await reader.ReadAsync())
                            {
                                var record = AddEntity(reader, entityDef, statement.IsAggregate, addToRelatedEntities: !isRootQuery);
                                list.Add(record);
                            }

                            if (isRootQuery)
                            {
                                // This is the root query
                                result = list;
                                isAggregateRootQuery = statement.IsAggregate;
                            }

                            await reader.NextResultAsync();
                        }
                    }
                }
                finally
                {
                    if (ownsConnection)
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }


                var allEntities = allIdEntities.ToDictionary(e => e.Key, e => e.Value.Values.ToList());
                if (!isAggregateRootQuery && !allEntities.ContainsKey(typeof(T)))
                {
                    // this indicates that the result is a fact table, we add the fact lines here in order to have their weak nav properties hydrated
                    allEntities[typeof(T)] = result;
                }

                // All simple properties are populated before, but not their navigation properties, those are done here
                Type cacheType = null;
                List<(PropertyInfo, PropertyInfo, bool)> cacheNavProperties = null;

                foreach (var entitiesOfType in allEntities.Values)
                    foreach (var entity in entitiesOfType)
                    {
                        var entityType = entity.GetType();

                        // Just an optimization: prepare the list of nav properties for this type
                        if (entityType != cacheType)
                        {
                            cacheType = entityType;
                            cacheNavProperties = new List<(PropertyInfo, PropertyInfo, bool)>();
                            foreach (var prop in cacheType.GetProperties())
                            {
                                var navPropertyAtt = prop.GetCustomAttribute<NavigationPropertyAttribute>();
                                if (navPropertyAtt != null && !prop.PropertyType.IsList())
                                {
                                    var strongDtoAtt = prop.PropertyType.GetCustomAttribute<StrongDtoAttribute>();
                                    var propCollectionType = strongDtoAtt?.Type ?? prop.PropertyType;

                                    if (allIdEntities.ContainsKey(propCollectionType))
                                    {
                                        // For query, means this property will live in its own
                                        var isStrongPropertyType = strongDtoAtt != null;
                                        var fkProp = cacheType.GetProperty(navPropertyAtt.ForeignKey);
                                        cacheNavProperties.Add((prop, fkProp, isStrongPropertyType));
                                    }
                                }
                            }
                        }

                        foreach (var (navProp, fkProp, isStrongPropertyType) in cacheNavProperties)
                        {
                            // The nav property can only be loaded if the FK property is loaded first
                            if (entity.EntityMetadata.TryGetValue(fkProp.Name, out FieldMetadata meta) && meta == FieldMetadata.Loaded)
                            {
                                var fk = fkProp.GetValue(entity)?.ToString();
                                if (fk == null)
                                {
                                    // it is loaded and its value is NULL
                                    entity.EntityMetadata[navProp.Name] = FieldMetadata.Loaded;
                                }
                                else
                                {
                                    var propCollectionType = navProp.PropertyType; // TODO: Use the root type
                                    var entitiesOfPropertyType = allIdEntities[propCollectionType]; // It must be available since we checked earlier that it exists

                                    if (entitiesOfPropertyType != null)
                                    {
                                        entitiesOfPropertyType.TryGetValue(fk, out DtoBase navPropValue);
                                        if (navPropValue != null)
                                        {
                                            // it is loaded and it has a value
                                            entity.EntityMetadata[navProp.Name] = FieldMetadata.Loaded;
                                            if (!isStrongPropertyType)
                                            {
                                                // If it is for query it will be returned in the "related" entities, not in the DTO itself
                                                navProp.SetValue(entity, navPropValue);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }


                // Here we populate the collection navigation properties after the simple properties have been populated
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
                        var collectionEntities = new HashSet<DtoBase>();
                        foreach (var principalEntity in principalEntities)
                        {
                            // We go down the path updating currentEntity as we go
                            var currentEntity = principalEntity;
                            foreach (var step in pathToCollectionEntity)
                            {
                                object nextObject = null;
                                var prop = currentEntity.GetType().GetProperty(step);
                                if (prop.PropertyType.GetCustomAttribute<StrongDtoAttribute>() != null)
                                {
                                    // this property is for query, retrieve the value from allEntities
                                    var propType = prop.PropertyType; // TODO: Use the collection type
                                    if (allIdEntities.TryGetValue(propType, out Dictionary<string, DtoBase> resultsOfType))
                                    {
                                        var navFkName = prop.GetCustomAttribute<NavigationPropertyAttribute>()?.ForeignKey;
                                        if (navFkName == null)
                                        {
                                            // Developer mistake
                                            throw new InvalidOperationException($"Property {prop.Name} has a strong type, but does not have a foreign key associated with it");
                                        }

                                        nextObject = resultsOfType[navFkName];
                                    }
                                }
                                else
                                {
                                    nextObject = prop.GetValue(currentEntity);
                                }

                                // If there is a null object on the path, call it quits
                                if (nextObject != null)
                                {
                                    currentEntity = nextObject as DtoBase;
                                }
                                else
                                {
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
                        var groupedCollections = list.GroupBy(e => fkProp.GetValue(e).ToString()).ToDictionary(g => g.Key, g => MakeList(query.ResultType, g));

                        PropertyInfo idProp = null;
                        PropertyInfo collectionProp = null;
                        foreach (var collectionEntity in collectionEntities)
                        {
                            if (idProp == null)
                            {
                                idProp = collectionEntity.GetType().GetProperty("Id");
                                if(idProp == null)
                                {
                                    // Programmer mistake
                                    throw new InvalidOperationException($"Type {collectionEntity.GetType().Name} does not have an Id property, and yet it has collection navigation properties");
                                }
                            }

                            if (collectionProp == null)
                            {
                                collectionProp = collectionEntity.GetType().GetProperty(collectionPropName);
                            }

                            var id = idProp.GetValue(collectionEntity).ToString();
                            var collection = groupedCollections.ContainsKey(id) ? groupedCollections[id] : MakeList(query.ResultType);

                            collectionEntity.EntityMetadata[collectionPropName] = FieldMetadata.Loaded;
                            collectionProp.SetValue(collectionEntity, collection);
                        }
                    }
                }

                // Prepare related entities
                var relatedEntities = new EntitiesMap();
                foreach (var (type, entities) in relatedEntitiesDic)
                {
                    relatedEntities[type.Name] = entities;
                }

                // Return the result
                return new ObjectLoaderResult
                {
                    RelatedEntities = relatedEntities,
                    Result = result
                };
            }

        }

        private static string DynamicPropertyName(ArraySegment<string> path, string prop, string aggregation)
        {
            // TODO: cache
            string pathAndPropertyString = prop;

            string pathString = string.Join('/', path);
            if (!string.IsNullOrWhiteSpace(pathString))
            {
                pathAndPropertyString = $"{pathString}/{pathAndPropertyString}";
            }

            if (!string.IsNullOrWhiteSpace(aggregation))
            {
                pathAndPropertyString = $"{aggregation}({pathAndPropertyString})";
            }

            return pathAndPropertyString;
        }

        // This method hydrates the properties of entity (whether a normal DTO or a dynamic entity) as per the entityDef
        private static void HydrateProperties(SqlDataReader reader, DtoBase entity, ColumnMapTree entityDef)
        {
            foreach (var (propInfo, index, aggregation) in entityDef.Properties)
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

                    if (entity is DynamicEntity dynamicEntity)
                    {
                        // The propertyNameMap was populated as soon as the ColumnMapTree was created
                        string propName = DynamicPropertyName(entityDef.Path, propInfo.Name, aggregation);
                        dynamicEntity[propName] = dbValue;
                    }
                    else
                    {
                        propInfo.SetValue(entity, dbValue);
                    }
                }

                entity.EntityMetadata[propInfo.Name] = FieldMetadata.Loaded;
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
        /// way that makes hydrating the DTOs from the statement result more efficient
        /// </summary>
        private class ColumnMapTree : Dictionary<string, ColumnMapTree>
        {
            /// <summary>
            /// The type of the DTO representing this level
            /// </summary>
            public Type Type { get; set; }

            /// <summary>
            /// When no IdIndex is set, this level is not DTOable
            /// </summary>
            public int IdIndex { get; set; } = -1;

            /// <summary>
            /// The PropertyInfo of the Id property is cached here for performance
            /// </summary>
            public PropertyInfo IdProperty { get; set; }

            /// <summary>
            /// All the properties at this level mentioned by all the paths
            /// </summary>
            public List<(PropertyInfo Property, int Index, string Aggregation)> Properties { get; set; } = new List<(PropertyInfo, int, string)>();

            /// <summary>
            /// The segment of the path leading up to this level
            /// </summary>
            public ArraySegment<string> Path { get; set; }

            /// <summary>
            /// Returns true when this level of the tree should hydrate a proper DTO
            /// </summary>
            public bool IdExists { get => IdIndex >= 0; }

            /// <summary>
            /// The children of the current tree level
            /// </summary>
            public IEnumerable<ColumnMapTree> Children { get => Values; }

            /// <summary>
            /// Takes a root type and a bunch of paths and constructs the entire <see cref="ColumnMapTree"/> which is useful
            /// for efficiently hydrating DTOs and dynamic results from the SQL query result,
            /// a single tree is associated with a single SQL select statement
            /// </summary>
            public static ColumnMapTree Build(Type type, List<SqlStatementColumn> columnMap)
            {
                var root = new ColumnMapTree { Type = type, Path = new string[0] };
                for (var i = 0; i < columnMap.Count; i++)
                {
                    var columnInfo = columnMap[i];
                    var path = columnInfo.Path;
                    var property = columnInfo.Property;
                    var aggregation = columnInfo.Aggregation;
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
                                Path = new ArraySegment<string>(path.Array, path.Offset, j + 1)
                            };
                        }

                        currentTree = currentTree[step];
                    }

                    if (property == "Id" && string.IsNullOrWhiteSpace(aggregation))
                    {
                        // This IdIndex is only set for DTOable path terminals
                        currentTree.IdIndex = i;
                        currentTree.IdProperty = currentTree.Type.GetProperty("Id"); // Useful later for optimization
                    }
                    else
                    {
                        var propInfo = currentTree.Type.GetProperty(property);
                        currentTree.Properties.Add((propInfo, i, aggregation));
                    }
                }

                // Ensures that if a parent has Id, that the children do as well, if this fails it exposes a programmer mistake
                // Also ensures that aggregations are forbidden anywhere on a subtree that has Ids
                ValidateIds(root);

                // Return the result
                return root;
            }

            private static void ValidateIds(ColumnMapTree currentTree, bool parentIdExists = false)
            {
                if (!currentTree.IdExists && parentIdExists)
                {
                    // Developer mistake
                    throw new InvalidOperationException($"The level '{string.Join("/", currentTree.Path)}' of type '{currentTree.Type.Name}' is missing its Id");
                }

                if (currentTree.IdExists && currentTree.Properties.Any(e => !string.IsNullOrWhiteSpace(e.Aggregation)))
                {
                    // Developer mistake
                    throw new InvalidOperationException($"The level '{string.Join("/", currentTree.Path)}' of type '{currentTree.Type.Name}' has an Id and also one or more aggregations");
                }

                foreach (var childTree in currentTree.Children)
                {
                    bool currentIdExists = currentTree.IdExists;
                    ValidateIds(childTree, currentIdExists);
                }
            }
        }
    }
}
