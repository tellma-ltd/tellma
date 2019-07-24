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
        public static async Task<List<DtoBase>> LoadStatements<T>(
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
                bool isAggregate = false;
                bool isFirstRow = false;
                var memory = new HashSet<(object Id, ColumnMapTree Tree)>(); // Remembers which (Id, tree) combinations have been added already
                var allIdEntities = new IndexedEntities(); // int ids are cast to string

                var cacheDynamicPropertyName = new Dictionary<(ArraySegment<string>, string, string), string>();
                string DynamicPropertyName(ArraySegment<string> path, string prop, string aggregation)
                {
                    if (!cacheDynamicPropertyName.ContainsKey((path, prop, aggregation)))
                    {
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

                        cacheDynamicPropertyName[(path, prop, aggregation)] = pathAndPropertyString;
                    }

                    return cacheDynamicPropertyName[(path, prop, aggregation)];
                }



                var dynamicProps = new List<DynamicPropInfo>();

                // This method hydrates the properties of entity (whether a normal DTO or a dynamic entity) as per the entityDef
                void HydrateProperties(SqlDataReader reader, DtoBase entity, ColumnMapTree entityDef)
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

                                if (isFirstRow)
                                {
                                    // dynamicProps.Add(new DynamicPropInfo(propInfo.PropertyType, propName));
                                    dynamicProps.Add(new DynamicPropInfo(
                                        propType: propInfo.PropertyType,
                                        name: propName,
                                        declaringType: propInfo.DeclaringType,
                                        path: entityDef.Path,
                                        property: propInfo.Name,
                                        aggregation: null));
                                }
                            }
                            else
                            {
                                propInfo.SetValue(entity, dbValue);
                            }
                        }

                        if (!(entity is DynamicEntity))
                        {
                            entity.EntityMetadata[propInfo.Name] = FieldMetadata.Loaded;
                        }
                    }
                }

                // This method will return one of 3 things: 
                // 1 - Either a propert DTO with ID, if the entity definition contains an Id index
                // 2 - Or a Fact DTO (without Id), if the entity defintion does not contain an Id index
                // 3 - Or a dynamic entity (a dictionary), if the entity defintion does not contain an Id index and this is an aggregate query
                // NOTE: parameter isIdParent ensures that if the parent DTO has an Id, that every child DTO also has an Id
                DtoBase AddEntity(SqlDataReader reader, ColumnMapTree entityDef, DynamicEntity dynamicEntity = null)
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
                            AddEntity(reader, subEntityDef, dynamicEntity);
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
                            var collectionType = entityType.GetRootType();

                            // Make sure the dictionary that tracks this type is created already
                            if (!allIdEntities.TryGetValue(collectionType, out IndexedEntitiesOfType entitiesOfType))
                            {
                                entitiesOfType = allIdEntities[collectionType] = new IndexedEntitiesOfType();
                            }

                            var dbId = reader[entityDef.IdIndex];
                            if (dbId == DBNull.Value)
                            {
                                return null;
                            }

                            var id = dbId;
                            entitiesOfType.TryGetValue(id, out DtoKeyBase keyEntity);

                            if (keyEntity == null)
                            {
                                keyEntity = Activator.CreateInstance(entityType) as DtoKeyBase;
                                keyEntity.SetId(dbId);

                                entitiesOfType.Add(id, keyEntity);
                                memory.Add((id, entityDef));

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

                            entity = keyEntity;
                        }
                        else // NOTE: This must be a level 0, otherwise validation earlier would capture it
                        {
                            // New entity
                            isHydrated = false;

                            // Create the entity
                            entity = Activator.CreateInstance(entityType) as DtoBase;
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
                            AddEntity(reader, subEntityDef);
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
                            isFirstRow = true;
                            isAggregate = statement.IsAggregate;
                            var list = results[statement.Query];

                            // Group the column map by the path (which represents the target entity)
                            var entityDef = ColumnMapTree.Build(statement.ResultType, statement.ColumnMap);

                            // Sanity checking: if this isn't an aggregate query, 
                            if (!isAggregate && entityDef.Children.Any(e => !e.IdExists))
                            {
                                // Developer mistake
                                throw new InvalidOperationException($"The query for '{statement.Sql}' is not an aggregate query but is missing essential Ids");
                            }

                            // Loop over the result from the database
                            while (await reader.ReadAsync())
                            {
                                var record = AddEntity(reader, entityDef);
                                list.Add(record);
                                isFirstRow = false;
                            }

                            if (statement.Query?.PrincipalQuery == null)
                            {
                                // This is the root query, assign the result to the final list
                                result = list;

                                // Set a flag to indicate whether the root query is aggregate (useful later)
                                isAggregateRootQuery = isAggregate;

                                // Assign the dynamicProps to every DynamicEntity
                                if (isAggregateRootQuery)
                                {
                                    result.ForEach(e => ((DynamicEntity)e).Properties = dynamicProps);
                                }
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

                var allEntities = allIdEntities.ToDictionary(e => e.Key, e => e.Value.Values.Cast<DtoBase>().ToList());
                if (result.Any())
                {
                    var resultRootType = result.First().GetType().GetRootType();
                    if (!allEntities.ContainsKey(resultRootType))
                    {
                        // this indicates that the result is a fact table, we add the fact lines here in order to have their weak nav properties hydrated
                        allEntities[resultRootType] = result;
                    }

                    // Here we add the navigation properties to the definition of dynamic entities
                    if (result.First() is DynamicEntity dynamicEntity)
                    {
                        var dimensionProps = dynamicEntity.Properties
                                        .Where(p => p.IsDimension);

                        var spannedTypes = dimensionProps
                                        .Select(p => (p.Path, p.DeclaringType))
                                        .Distinct()
                                        .ToList();

                        // A hash of all the simple dimension properties and their paths
                        var dynamicPropsHash = dimensionProps
                                        .ToDictionary(e => (e.Path, e.Property));

                        // We loop over all the navigation properties in all spanned types, any navigation property whose
                        // foreign key property is in the collection we also add it to the collection
                        foreach (var (path, type) in spannedTypes)
                        {
                            foreach (var propInfo in type.GetProperties())
                            {
                                var fkName = propInfo.GetCustomAttribute<NavigationPropertyAttribute>()?.ForeignKey;
                                if (!string.IsNullOrWhiteSpace(fkName) && dynamicPropsHash.TryGetValue((path, fkName), out DynamicPropInfo fkProp))
                                {
                                    dynamicEntity.Properties.Add(new DynamicPropInfo(
                                        propType: propInfo.PropertyType,
                                        name: DynamicPropertyName(path, propInfo.Name, null),
                                        declaringType: propInfo.DeclaringType,
                                        path: path,
                                        property: propInfo.Name,
                                        aggregation: null,
                                        fk: fkProp));
                                }
                            }
                        }
                    }
                }

                // The method below efficiently retrieves the navigation properties of each entity
                Type cacheType = null;
                List<(IPropInfo NavProp, IPropInfo FkProp)> navAndFkCache = null;
                List<(IPropInfo NavProp, IPropInfo FkProp)> GetNavAndFkProps(DtoBase entity)
                {
                    if (entity.GetType() != cacheType) // In a single root type we might have entities belonging to multiple different deriving types
                    {
                        cacheType = entity.GetType();
                        navAndFkCache = new List<(IPropInfo NavProp, IPropInfo FkProp)>();

                        IEnumerable<IPropInfo> nonListProperties;
                        if (entity is DynamicEntity dynamicEntity)
                        {
                            nonListProperties = dynamicEntity.Properties;
                        }
                        else
                        {
                            nonListProperties = cacheType.GetProperties()
                                .Where(e => !e.PropertyType.IsList())
                                .Select(e => new PropInfo(e));
                        }

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
                    bool isDynamic = rootType == typeof(DynamicEntity);

                    // Loop over all entities and use the method above to hydrate the navigation properties
                    foreach (var entity in entitiesOfType)
                    {
                        foreach (var (navProp, fkProp) in GetNavAndFkProps(entity))
                        {
                            // The nav property can only be loaded if the FK property is loaded first
                            if (isDynamic || entity.EntityMetadata.TryGetValue(fkProp.Name, out FieldMetadata meta) && meta == FieldMetadata.Loaded)
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
                                    var entitiesOfPropertyType = allIdEntities[propCollectionType]; // It must be available since we checked earlier that it exists
                                    entitiesOfPropertyType.TryGetValue(fkValue, out DtoKeyBase navPropValue);
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
                                var prop = currentEntity.GetType().GetProperty(step);
                                currentEntity = prop.GetValue(currentEntity) as DtoBase;

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
                        foreach (var collectionEntity in collectionEntities.Cast<DtoKeyBase>())
                        {
                            // Rule every entity with collections attached to it must be a DtoKeyBase
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
                    var propName = columnInfo.Property;
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

                    if (propName == "Id" && string.IsNullOrWhiteSpace(aggregation) && currentTree.Type.IsSubclassOf(typeof(DtoKeyBase)))
                    {
                        // This IdIndex is only set for DTOable path terminals
                        currentTree.IdIndex = i;
                    }
                    else
                    {
                        var propInfo = currentTree.Type.GetProperty(propName);
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
