using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Data.Queries;
using Tellma.Entities;

namespace Tellma.Controllers.ImportExport
{
    /// <summary>
    /// Helper service for importing files
    /// </summary>
    public class DataParser
    {
        private readonly IStringLocalizer<Strings> _localizer;
        private readonly IServiceProvider _sp;

        /// <summary>
        /// Constructor
        /// </summary>
        public DataParser(IServiceProvider sp, IStringLocalizer<Strings> localizer)
        {
            _localizer = localizer;
            _sp = sp;
        }

        /// <summary>
        /// Takes an <see cref="IEnumerable{T}"/> of string arrays representing the raw data of the imported file.
        /// Then using the specifications in the <see cref="MappingInfo"/> it translates this raw data into an
        /// <see cref="IEnumerable{T}"/> of entities. Any problems will be added in the <see cref="ImportErrors"/> dictionary.
        /// This function is the opposite of <see cref="DataComposer.Compose{TEntityForSave}(List{TEntityForSave}, MappingInfo)"/>.
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

            // If there a parent Id property, pop it
            ForeignKeyMappingInfo parentIdProp = mapping.ParentIdProperty();

            int rowNumber = 2;
            foreach (var dataRow in dataWithoutHeader) // Foreach row
            {
                bool keepGoing = ParseRow(dataRow, rowNumber, mapping, relatedEntities, errors, parentIdProp); // Recursive function
                if (!keepGoing)
                {
                    // This means the errors collection is full, no need to keep going
                    break;
                }

                rowNumber++;
            }

            // Grab the result from the root mapping
            var result = mapping.List.Cast<TEntityForSave>();

            // Hydrate the tree property if any, this property is not hydrated in ParseRow()
            // Since it requires all the imported entities to be created and all their other
            // properties already hydrated
            if (parentIdProp != null)
            {
                HydrateParentIds(result, errors, parentIdProp);
            }

            // Return the result
            return result;
        }

        /// <summary>
        /// ParentId property is skipped in the regular parsing and hydration, since it relies
        /// on the complete list of imported entities to be hydrated first.
        /// This method does he needful and hydrates the ParentIds
        /// </summary>
        private void HydrateParentIds<TEntityForSave>(IEnumerable<TEntityForSave> result, ImportErrors errors, ForeignKeyMappingInfo parentIdProp) where TEntityForSave : Entity
        {
            // Prepare the getValue method to retrieve the user codes from the uploaded list
            if (!result.Any())
            {
                return;
            }

            var resultType = result.FirstOrDefault().GetType();
            var resultTypeDesc = Entities.Descriptors.TypeDescriptor.Get(resultType);
            var userKeyPropName = parentIdProp.KeyPropertyMetadata.Descriptor.Name;
            var prop = resultTypeDesc.Property(userKeyPropName);
            if (prop == null)
            {
                throw new InvalidOperationException($"Bug: Expected property {userKeyPropName} on type {resultType.Name}");
            }

            Func<Entity, object> getUserKey = prop.GetValue;

            // Prepare a dictionary mapping every user key value in the uploaded list to the indices of their entities
            var indicesDic = result
                .Where(e => getUserKey(e) != null)
                .Select((entity, index) => (entity, index))
                .GroupBy(pair => getUserKey(pair.entity))
                .ToDictionary(g => g.Key, g => g.Select(e => e.index));

            // Hydrate the entities one by one
            foreach (var entity in result.Where(e => e.EntityMetadata.ParentUserKey != null))
            {
                indicesDic.TryGetValue(entity.EntityMetadata.ParentUserKey, out IEnumerable<int> indices);
                if (indices == null || indices.Count() == 0)
                {
                    // No matches from the imported list, fallback to the db matches
                    var matches = entity.EntityMetadata.ParentMatches;
                    if (matches == null || !matches.Any())
                    {
                        // No matches from the db list => error
                        var typeDisplay = parentIdProp.NavPropertyMetadata.TargetTypeMetadata.SingularDisplay();
                        var propDisplay = parentIdProp.KeyPropertyMetadata.Display();
                        var stringField = entity.EntityMetadata.ParentUserKey.ToString();
                        if (!errors.AddImportError(entity.EntityMetadata.RowNumber, parentIdProp.ColumnNumber, _localizer["Error_No0WasFoundWhere1Equals2", typeDisplay, propDisplay, stringField]))
                        {
                            break;
                        }
                    }
                    else
                    {
                        // More than 1 match in the db list => error                        
                        if (matches.Skip(1).Any()) // given the earlier check, this can only mean more than one
                        {
                            var typeDisplay = parentIdProp.NavPropertyMetadata.TargetTypeMetadata.SingularDisplay();
                            var keyPropDisplay = parentIdProp.KeyPropertyMetadata.Display();
                            var stringField = entity.EntityMetadata.ParentUserKey.ToString();
                            if (!errors.AddImportError(entity.EntityMetadata.RowNumber, parentIdProp.ColumnNumber, _localizer["Error_MoreThanOne0FoundWhere1Equals2", typeDisplay, keyPropDisplay, stringField]))
                            {
                                break;
                            }
                        }
                        else
                        {
                            var single = matches.Single();
                            var id = single.GetId();
                            parentIdProp.Metadata.Descriptor.SetValue(entity, id);
                        }
                    }
                }
                else if (indices.Count() > 1)
                {
                    // More than one match in the uploaded list => error
                    var typeDisplay = parentIdProp.NavPropertyMetadata.TargetTypeMetadata.SingularDisplay();
                    var keyPropDisplay = parentIdProp.KeyPropertyMetadata.Display();
                    var stringField = entity.EntityMetadata.ParentUserKey.ToString();
                    if (!errors.AddImportError(entity.EntityMetadata.RowNumber, parentIdProp.ColumnNumber, _localizer["Error_MoreThanOne0FoundWhere1Equals2", typeDisplay, keyPropDisplay, stringField]))
                    {
                        break;
                    }
                }
                else
                {
                    // Match from the uploaded list, use its index in ParentIndex
                    if (entity is IParentIndex treeEntity)
                    {
                        treeEntity.ParentIndex = indices.Single();
                    }
                    else
                    {
                        var typeDisplay = parentIdProp.NavPropertyMetadata.TargetTypeMetadata.SingularDisplay();
                        throw new InvalidOperationException($"Bug: type {typeDisplay} has a ParentId property but doesn't implement {nameof(IParentIndex)}");
                    }
                }
            }
        }

        /// <summary>
        /// Inspects the data and mapping and loads all related entities from the API, that are referenced by custom use keys like Code and Name.
        /// The purpose is to use the Ids of these entities in the constructed Entity objects that are being imported
        /// </summary>
        private async Task<RelatedEntities> LoadRelatedEntities(IEnumerable<string[]> dataWithoutHeader, MappingInfo mapping, ImportErrors errors)
        {
            if (errors is null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            // Each set of foreign keys will result in an API query to retrieve the corresponding Ids for FK hydration
            // So we group foreign keys by the target type, the target definition Id and the key property (e.g. Code or Name)
            var queryInfos = new List<(Type, int?, PropertyMetadata, HashSet<object>)>();
            foreach (var g in mapping.GetForeignKeys().Where(p => p.NotUsingIdAsKey)
                .GroupBy(fk => (fk.TargetType, fk.TargetDefId, fk.KeyPropertyMetadata)))
            {
                var (navType, navDefId, keyPropMetadata) = g.Key;
                HashSet<object> keysSet = new HashSet<object>();

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
                                throw new InvalidOperationException("Bug: Only int and string IDs are supported");
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
                foreach (var (navType, navDefId, keyPropMeta, keysSet) in queryInfos)
                {
                    IFactWithIdService service = _sp.FactWithIdServiceByEntityType(navType, navDefId);

                    var keyPropDesc = keyPropMeta.Descriptor;
                    var keyPropName = keyPropDesc.Name;
                    var args = new SelectExpandArguments { Select = keyPropName };

                    var (data, _) = await service.GetByPropertyValues(keyPropName, keysSet, args, cancellation: default);

                    var grouped = data.GroupBy(e => keyPropDesc.GetValue(e)).ToDictionary(g => g.Key, g => (IEnumerable<EntityWithKey>)g);
                    result.Add((navType, navDefId, keyPropName), grouped);
                }
            }

            return result;
        }

        /// <summary>
        /// Recursive function that parses a single data row (string array) into 0 or more entities that are each
        /// added in the correct list in the <see cref="MappingInfo"/> tree. Any errors will be added to the <see cref="ImportErrors"/>.
        /// Entities referenced by user keys should be loaded in advance and passed to this function as a <see cref="RelatedEntities"/> dictionary.   
        /// This function is the opposite of <see cref="DataComposer.ComposeDataRowsFromEntity(List{string[]}, EntityWithKey, MappingInfo, int, int)"/>
        /// </summary>
        /// <param name="dataRow">An array of strings representing a single row in a CSV or XSLX file</param>
        /// <param name="rowNumber">The number of the current row, for the purpose of error reporting</param>
        /// <param name="mapping">The <see cref="MappingInfo"/> to rely on for constructing the <see cref="Entity"/> objects</param>
        /// <param name="entities">All related entities that are referenced by user keys in the raw data</param>
        /// <param name="errors">Any validation errors are added to this collection</param>
        /// <returns>False if the <see cref="ImportErrors"/> dictionary has been maxed out, true otherwise.</returns>
        private bool ParseRow(string[] dataRow, int rowNumber, MappingInfo mapping, RelatedEntities entities, ImportErrors errors, ForeignKeyMappingInfo parentIdProp = null)
        {
            bool entityCreated = false;
            foreach (var prop in mapping.SimpleProperties)
            {
                var stringField = dataRow[prop.Index];
                if (!string.IsNullOrEmpty(stringField))
                {
                    if (!entityCreated)
                    {
                        mapping.Entity = mapping.Metadata.Descriptor.Create();
                        mapping.Entity.EntityMetadata.RowNumber = rowNumber; // for validation reporting
                        mapping.List.Add(mapping.Entity);
                        entityCreated = true;
                    }

                    // If it's a # placeholder to trigger entity creation => continue
                    if (prop.Ignore)
                    {
                        // This is just a # placeholder to trigger entity creation
                        continue;
                    }

                    // Hydrate the property
                    if (prop is ForeignKeyMappingInfo fkProp && fkProp.NotUsingIdAsKey)
                    {
                        // Get the user key value (usually the code or the name)
                        object userKeyValue = fkProp.KeyType switch
                        {
                            KeyType.String => stringField,
                            KeyType.Int => int.Parse(stringField),
                            _ => null
                        };

                        // Get the entity from the dictionary
                        var dic = entities[(fkProp.TargetType, fkProp.TargetDefId, fkProp.KeyPropertyMetadata.Descriptor.Name)];
                        dic.TryGetValue(userKeyValue, out IEnumerable<EntityWithKey> matches);

                        // ParentId requires special handling, we simply store the user key value and
                        // the matches from the database in a the entity metadata.
                        // Later, after all the entities have been hydrated, we check the hydrated
                        // list of entities for parent matches, if there are they take precedent over the
                        // matches from the database entities, this allows users to specify one of the
                        // other items in the imported sheet as parent
                        if (fkProp == parentIdProp)
                        {
                            mapping.Entity.EntityMetadata.ParentUserKey = userKeyValue;
                            mapping.Entity.EntityMetadata.ParentMatches = matches;
                            continue;
                        }

                        if (matches == null || !matches.Any())
                        {
                            var typeDisplay = fkProp.NavPropertyMetadata.TargetTypeMetadata.SingularDisplay();
                            var propDisplay = fkProp.KeyPropertyMetadata.Display();
                            var errorName = fkProp.KeyType == KeyType.String && stringField.Any(e => char.IsLetter(e)) ? "Error_No0WasFoundWhere1Equals2EnsureCaseMatch" : "Error_No0WasFoundWhere1Equals2";
                            if (!errors.AddImportError(rowNumber, prop.ColumnNumber, _localizer[errorName, typeDisplay, propDisplay, stringField]))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (matches.Skip(1).Any()) // More than one match
                            {
                                var typeDisplay = fkProp.NavPropertyMetadata.TargetTypeMetadata.SingularDisplay();
                                var keyPropDisplay = fkProp.KeyPropertyMetadata.Display();
                                if (!errors.AddImportError(rowNumber, prop.ColumnNumber, _localizer["Error_MoreThanOne0FoundWhere1Equals2", typeDisplay, keyPropDisplay, stringField]))
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                var single = matches.Single();
                                var id = single.GetId();
                                prop.Metadata.Descriptor.SetValue(mapping.Entity, id);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            object parsedField = prop.Metadata.Parse(stringField);
                            prop.Metadata.Descriptor.SetValue(mapping.Entity, parsedField);
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
                            string msg = $"Unhandled parsing exception: {ex.Message}";
                            if (!errors.AddImportError(rowNumber, prop.ColumnNumber, msg))
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
                    nextMapping.List = nextMapping.GetOrCreateList(mapping.Entity);
                }

                bool keepGoing = ParseRow(dataRow, rowNumber, nextMapping, entities, errors);
                if (!keepGoing)
                {
                    return false;
                }
            }

            return true;
        }

        #region Helper Classes 

        /// <summary>
        /// Data structure for storing and efficiently retrieving preloaded related entities which are referenced by foreign keys in the imported list
        /// </summary>
        private class RelatedEntities : Dictionary<(Type Type, int? DefId, string PropName), Dictionary<object, IEnumerable<EntityWithKey>>>
        {
        }

        #endregion
    }
}
