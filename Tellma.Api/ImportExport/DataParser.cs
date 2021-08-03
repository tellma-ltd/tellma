using Microsoft.Extensions.Localization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Api.Metadata;
using Tellma.Model.Common;

namespace Tellma.Api.ImportExport
{
    /// <summary>
    /// Helper service for parsing raw data into entities for save.<br/>
    /// Raw data is an enumerable of string-arrays that was extracted from a CSV or Excel file.
    /// </summary>
    public class DataParser
    {
        private readonly IApiClientForImport _client;
        private readonly IStringLocalizer<Strings> _localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataParser"/> class.
        /// </summary>
        public DataParser(IApiClientForImport client, IStringLocalizer<Strings> localizer)
        {
            _client = client;
            _localizer = localizer;
        }

        /// <summary>
        /// Takes an <see cref="IEnumerable{T}"/> of string arrays representing the raw data of the imported file.
        /// Then using the specifications in the <see cref="MappingInfo"/> it translates this raw data into an
        /// <see cref="IEnumerable{T}"/> of entities. Any problems will be added in the <see cref="ImportErrors"/> dictionary.
        /// This function is the opposite of <see cref="DataComposer.Compose{TEntityForSave}"/>.
        /// </summary>
        public async Task<IEnumerable<TEntityForSave>> ParseAsync<TEntityForSave>(IEnumerable<string[]> dataWithoutHeader, MappingInfo mapping, ImportErrors errors)
            where TEntityForSave : Entity
        {
            // Load related entities from API
            var relatedEntities = await LoadRelatedEntities(dataWithoutHeader, mapping, errors);
            if (!errors.IsValid)
            {
                return null;
            }

            // Clear all cached entities and lists, and create the root list
            mapping.ClearEntitiesAndLists();
            mapping.List = new List<TEntityForSave>(); // This root list will contain the final result

            // Set some values on the root mapping for for handling self referencing FKs
            mapping.IsRoot = true;
            int matchesIndex = 0;
            foreach (var prop in mapping.SimpleProperties.OfType<ForeignKeyMappingInfo>().Where(p => p.IsSelfReferencing))
            {
                prop.EntityMetadataMatchesIndex = matchesIndex++;
            }

            int rowNumber = 2;
            foreach (var dataRow in dataWithoutHeader) // Foreach row
            {
                bool keepGoing = ParseRow(dataRow, rowNumber, mapping, relatedEntities, errors, matchesIndex); // Recursive function
                if (!keepGoing)
                {
                    // This means the errors collection is full, no need to keep going
                    break;
                }

                rowNumber++;
            }

            // Grab the result from the root mapping
            var result = mapping.List.Cast<TEntityForSave>();

            // Hydrate self referencing FKs if any, these properties are not hydrated in ParseRow()
            // Since they requires all the imported entities to be created and all their other
            // properties already hydrated
            HydrateSelfReferencingForeignKeys(result, mapping, errors);

            // Return the result
            return result;
        }

        /// <summary>
        /// Inspects the data and mapping and loads all related entities from the API, that are referenced by custom use keys like Code and Name.
        /// The purpose is to use the Ids of these entities in the constructed Entity objects that are being imported.
        /// </summary>
        private async Task<RelatedEntities> LoadRelatedEntities(IEnumerable<string[]> dataWithoutHeader, MappingInfo mapping, ImportErrors errors)
        {
            if (errors is null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            // Each set of foreign keys will result in an API query to retrieve the corresponding Ids for FK hydration
            // So we group foreign keys by the target type, the target definition Id and the key property (e.g. Code or Name)
            var queryInfos = new List<(Type navType, int? navDefId, PropertyMetadata keyPropMeta, HashSet<object> keysSet)>();
            foreach (var g in mapping.GetForeignKeys().Where(p => p.NotUsingIdAsKey)
                .GroupBy(fk => (fk.TargetType, fk.TargetDefId, fk.KeyPropertyMetadata)))
            {
                var (navType, navDefId, keyPropMetadata) = g.Key;
                var keysSet = new HashSet<object>();

                foreach (var fkMapping in g)
                {
                    int rowNumber = 2;

                    foreach (var row in dataWithoutHeader)
                    {
                        string stringKey = row[fkMapping.Index];
                        if (string.IsNullOrEmpty(stringKey))
                        {
                            continue;
                        }

                        switch (fkMapping.KeyType)
                        {
                            case KeyType.String:
                                keysSet.Add(stringKey);

                                break;
                            case KeyType.Int:
                                if (int.TryParse(stringKey, out int intKey))
                                {
                                    keysSet.Add(intKey);
                                }
                                else if (!errors.AddImportError(rowNumber, fkMapping.ColumnNumber, _localizer[$"Error_TheValue0IsNotAValidInteger", stringKey]))
                                {
                                    // This means the validation errors are at maximum capacity, pointless to keep going.
                                    return null;
                                }

                                break;

                            default:
                                throw new InvalidOperationException("Bug: Only int and string Ids are supported.");
                        }

                        rowNumber++;
                    }
                }

                if (keysSet.Any())
                {
                    // Actual API calls are delayed till the end in case there are any errors
                    queryInfos.Add((navType, navDefId, keyPropMetadata, keysSet));
                }
            }

            if (!errors.IsValid)
            {
                return null;
            }

            var result = new RelatedEntities();
            if (queryInfos.Any())
            {
                // Load all related entities in parallel
                await Task.WhenAll(queryInfos.Select(async queryInfo =>
                {
                    var (navType, navDefId, keyPropMeta, keysSet) = queryInfo; // Deconstruct the queryInfo

                    var keyPropDesc = keyPropMeta.Descriptor;
                    var keyPropName = keyPropDesc.Name;

                    var data = await _client.GetEntitiesByPropertyValues(
                    collection: navType.Name,
                            definitionId: navDefId,
                            propName: keyPropName,
                            values: keysSet,
                            cancellation: default);


                    var grouped = data.GroupBy(e => keyPropDesc.GetValue(e)).ToDictionary(g => g.Key, g => (IEnumerable<EntityWithKey>)g);
                    result.TryAdd((navType, navDefId, keyPropName), grouped);
                }));
            }

            return result;
        }

        /// <summary>
        /// Recursive function that parses a single data row (string array) into 0 or more entities that are each
        /// added in the correct list in the <paramref name="mapping"/> tree. Any errors will be added to <paramref name="errors"/>.
        /// Entities referenced by user keys should be loaded in advance and passed to this function as a <see cref="RelatedEntities"/> dictionary.   
        /// This function is the opposite of <see cref="DataComposer.ComposeDataRowsFromEntity"/>.
        /// </summary>
        /// <param name="dataRow">An array of strings representing a single row in a CSV or XLSX file.</param>
        /// <param name="rowNumber">The number of the current row, for the purpose of error reporting.</param>
        /// <param name="mapping">The <see cref="MappingInfo"/> to rely on for constructing the <see cref="Entity"/> objects.</param>
        /// <param name="entities">All related entities that are referenced by user keys in the raw data.</param>
        /// <param name="errors">Any validation errors are added to this collection.</param>
        /// <returns>False if the <see cref="ImportErrors"/> dictionary has been maxed out, true otherwise.</returns>
        private bool ParseRow(string[] dataRow, int rowNumber, MappingInfo mapping, RelatedEntities entities, ImportErrors errors, int selfRefPropertiesCount = 0)
        {
            bool entityCreated = false;
            foreach (var prop in mapping.SimpleProperties)
            {
                var stringField = dataRow[prop.Index];
                if (!string.IsNullOrEmpty(stringField))
                {
                    if (!entityCreated)
                    {
                        mapping.Entity = mapping.CreateBaseEntity(rowNumber);
                        mapping.List.Add(mapping.Entity);
                        entityCreated = true;
                    }

                    var entity = prop.GetTerminalEntityForSave(mapping.Entity);

                    // Hydrate the property
                    if (prop is ForeignKeyMappingInfo fkProp && fkProp.NotUsingIdAsKey)
                    {
                        // Get the user key value (usually the code or the name)
                        object userKeyValue = fkProp.KeyType switch
                        {
                            KeyType.String => stringField?.Trim(),
                            KeyType.Int => int.Parse(stringField),
                            _ => null
                        };

                        // Get the entity from the dictionary
                        var dic = entities[(fkProp.TargetType, fkProp.TargetDefId, fkProp.KeyPropertyMetadata.Descriptor.Name)];
                        dic.TryGetValue(userKeyValue, out IEnumerable<EntityWithKey> matches);

                        // Self Referencing FKs require special handling, we simply store the user key value and
                        // the matches from the database in a the entity metadata.
                        // Later, after all the entities have been hydrated, we check the hydrated
                        // list of entities for matches, if there are they take precedent over the
                        // matches from the database entities, this allows users to refer to one of the
                        // other items in the imported sheet in the self referencing FK
                        if (IsSelfReferencing(fkProp, mapping))
                        {
                            var matchPairsArray = entity.EntityMetadata.MatchPairs ??= new (object userKey, IEnumerable<EntityWithKey> matches)[selfRefPropertiesCount];
                            matchPairsArray[fkProp.EntityMetadataMatchesIndex] = (userKeyValue, matches);
                            continue;
                        }
                        else if (matches == null || !matches.Any()) // No matches at all -> Problem
                        {
                            var typeDisplay = fkProp.NavPropertyMetadata.TargetTypeMetadata.SingularDisplay();
                            var propDisplay = fkProp.KeyPropertyMetadata.Display();
                            var errorName = fkProp.KeyType == KeyType.String && stringField.Any(e => char.IsLetter(e)) ? "Error_No0WasFoundWhere1Equals2EnsureCaseMatch" : "Error_No0WasFoundWhere1Equals2";
                            if (!errors.AddImportError(rowNumber, prop.ColumnNumber, _localizer[errorName, typeDisplay, propDisplay, stringField]))
                            {
                                return false;
                            }
                        }
                        else if (matches.Skip(1).Any()) // More than one match -> Problem
                        {
                            var typeDisplay = fkProp.NavPropertyMetadata.TargetTypeMetadata.SingularDisplay();
                            var keyPropDisplay = fkProp.KeyPropertyMetadata.Display();
                            if (!errors.AddImportError(rowNumber, prop.ColumnNumber, _localizer["Error_MoreThanOne0FoundWhere1Equals2", typeDisplay, keyPropDisplay, stringField]))
                            {
                                return false;
                            }
                        }
                        else // Exactly one match -> Perfect
                        {
                            var single = matches.Single();
                            var id = single.GetId();
                            // prop.SetValue(mapping.Entity, id);
                            prop.MetadataForSave.Descriptor.SetValue(entity, id);
                        }
                    }
                    else
                    {
                        try
                        {
                            object parsedField = prop.MetadataForSave.Parse(stringField);
                            prop.MetadataForSave.Descriptor.SetValue(entity, parsedField);
                            // prop.SetValue(mapping.Entity, parsedField);
                        }
                        catch (ParseException ex)
                        {
                            if (!errors.AddImportError(rowNumber, prop.ColumnNumber, ex.Message))
                            {
                                // Too many errors, call it quits
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!errors.AddImportError(rowNumber, prop.ColumnNumber, $"Unhandled parsing exception: {ex.Message}."))
                            {
                                // Too many errors, call it quits
                                return false;
                            }
                        }
                    }
                }
            }

            foreach (var nextMapping in mapping.CollectionProperties)
            {
                if (entityCreated)
                {
                    nextMapping.List = nextMapping.GetOrCreateListForSave(mapping.Entity);
                }

                bool keepGoing = ParseRow(dataRow, rowNumber, nextMapping, entities, errors);
                if (!keepGoing)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Self Referencing FKs are skipped in the regular parsing and hydration, since they rely
        /// on the complete list of imported entities to be hydrated first.<br/>
        /// This method does the needful and hydrates all self referencing FKs.
        /// </summary>
        private void HydrateSelfReferencingForeignKeys<TEntityForSave>(IEnumerable<TEntityForSave> result, MappingInfo mapping, ImportErrors errors) where TEntityForSave : Entity
        {
            // Prepare the getValue method to retrieve the user codes from the uploaded list
            if (!result.Any())
            {
                return;
            }

            var resultType = result.FirstOrDefault().GetType();
            var resultTypeDesc = TypeDescriptor.Get(resultType);

            foreach (var selfRefProp in mapping.SimpleProperties.OfType<ForeignKeyMappingInfo>().Where(fkProp => IsSelfReferencing(fkProp, mapping)))
            {
                // Get the user key descriptor
                var userKeyPropName = selfRefProp.KeyPropertyMetadata.Descriptor.Name;
                var userKeyProp = resultTypeDesc.Property(userKeyPropName);
                if (userKeyProp == null)
                {
                    throw new InvalidOperationException($"Bug: Expected property {userKeyPropName} on type {resultType.Name}.");
                }

                // Prepare a dictionary mapping every user key value in the uploaded list to the indices of their entities
                var indicesDic = result
                    .Select((entity, index) => (entity, index)) // This should come before the call to "Where"
                    .Where(e => userKeyProp.GetValue(e.entity) != null)
                    .GroupBy(pair => userKeyProp.GetValue(pair.entity))
                    .ToDictionary(g => g.Key, g => g.Select(e => e.index));

                // Hydrate the entities one by one
                foreach (var entityBase in result)
                {
                    var entity = selfRefProp.GetTerminalEntityForSave(entityBase);
                    if (entity.EntityMetadata.TryGetMatchPairs(selfRefProp.EntityMetadataMatchesIndex, out (object, IEnumerable<EntityWithKey>) matchesPair))
                    {
                        var (userKey, matches) = matchesPair;

                        indicesDic.TryGetValue(userKey, out IEnumerable<int> indices);
                        if (indices == null || !indices.Any())
                        {
                            // No matches from the imported list, fallback to the db matches
                            if (matches == null || !matches.Any())
                            {
                                // No matches from the db list => error
                                var typeDisplay = selfRefProp.NavPropertyMetadata.TargetTypeMetadata.SingularDisplay();
                                var propDisplay = selfRefProp.KeyPropertyMetadata.Display();
                                var stringField = userKey.ToString();
                                if (!errors.AddImportError(entity.EntityMetadata.RowNumber, selfRefProp.ColumnNumber, _localizer["Error_No0WasFoundWhere1Equals2", typeDisplay, propDisplay, stringField]))
                                {
                                    break;
                                }
                            }
                            else
                            {
                                // More than 1 match in the db list => error                        
                                if (matches.Skip(1).Any())
                                {
                                    var typeDisplay = selfRefProp.NavPropertyMetadata.TargetTypeMetadata.SingularDisplay();
                                    var keyPropDisplay = selfRefProp.KeyPropertyMetadata.Display();
                                    var stringField = userKey.ToString();
                                    if (!errors.AddImportError(entity.EntityMetadata.RowNumber, selfRefProp.ColumnNumber, _localizer["Error_MoreThanOne0FoundWhere1Equals2", typeDisplay, keyPropDisplay, stringField]))
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    var single = matches.Single();
                                    var id = single.GetId();
                                    selfRefProp.MetadataForSave.Descriptor.SetValue(entity, id);
                                    // selfRefProp.SetValue(entity, id);
                                }
                            }
                        }
                        else if (indices.Count() > 1)
                        {
                            // More than one match in the uploaded list => error
                            var typeDisplay = selfRefProp.NavPropertyMetadata.TargetTypeMetadata.SingularDisplay();
                            var keyPropDisplay = selfRefProp.KeyPropertyMetadata.Display();
                            var stringField = userKey.ToString();
                            if (!errors.AddImportError(entity.EntityMetadata.RowNumber, selfRefProp.ColumnNumber, _localizer["Error_MoreThanOne0FoundWhere1Equals2", typeDisplay, keyPropDisplay, stringField]))
                            {
                                break;
                            }
                        }
                        else
                        {
                            // Single match from the uploaded list, set it in the index
                            selfRefProp.SetIndexProperty(entity, indices.Single());
                        }
                    }
                }
            }
        }

        #region Helpers

        /// <summary>
        /// A property is self referencing if it lives in the root entity type being uploaded AND it is 
        /// adoned with <see cref="SelfReferencingAttribute"/> AND it  targets a null definition Id or 
        /// a definition Id that matches the one of the uploaded list.
        /// </summary>
        private static bool IsSelfReferencing(ForeignKeyMappingInfo fkProp, MappingInfo mapping)
        {
            // A property is self referencing if it lives in the root entity type being uploaded AND it is adoned
            // with SelfReferencingAttribute, and it has a matching definition Id as the target type it references
            return mapping.IsRoot && fkProp.IsSelfReferencing &&
                (fkProp.NavPropertyMetadata.TargetTypeMetadata.DefinitionId == null || fkProp.NavPropertyMetadata.TargetTypeMetadata.DefinitionId == mapping.MetadataForSave.DefinitionId);
        } 

        /// <summary>
        /// Data structure for storing and efficiently retrieving preloaded related entities which are 
        /// referenced by foreign keys in the imported list.
        /// </summary>
        private class RelatedEntities : ConcurrentDictionary<(Type Type, int? DefId, string PropName), Dictionary<object, IEnumerable<EntityWithKey>>>
        {
        }

        #endregion
    }
}
