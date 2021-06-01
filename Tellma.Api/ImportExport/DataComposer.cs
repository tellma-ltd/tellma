using System;
using System.Collections;
using System.Collections.Generic;
using Tellma.Model.Common;

namespace Tellma.Api.ImportExport
{
    /// <summary>
    /// Helper service for exporting raw data from a list of a entities.<br/>
    /// Raw data is an enumerable of string-arrays suitable for packaging as a CSV or Excel file.
    /// </summary>
    public class DataComposer
    {
        /// <summary>
        /// Takes a <see cref="List{T}"/> of <paramref name="entities"/> and composes them into raw data using
        /// the specifications in <paramref name="mapping"/>. This raw data can then be exported as a CSV file using a packager.<br/>
        /// This function is the opposite of <see cref="DataParser.ParseAsync{TEntityForSave}"/>.
        /// </summary>
        public IEnumerable<string[]> Compose<TEntityForSave>(List<TEntityForSave> entities, MappingInfo mapping)
            where TEntityForSave : EntityWithKey
        {
            var result = new List<string[]>(entities.Count); // it will be at least that long, so might as well
            int columnCount = mapping.ColumnCount();

            // Recursive function
            int numberOfRows = 0;
            foreach (var entity in entities)
            {
                numberOfRows += ComposeDataRowsFromEntity(entity, mapping, result, numberOfRows, columnCount);
            }

            return result;
        }

        /// <summary>
        /// Composes a single entity into raw data that it adds to the given result list starting from <paramref name="rowIndex"/>.
        /// That resulting raw data may potentially span multiple rows if the entity has a weak collection associated with it.
        /// This function is the opposite of <see cref="DataParser.ParseRow"/>.
        /// </summary>
        /// <param name="result">The result list containing the raw data.</param>
        /// <param name="entity">The <see cref="EntityWithKey"/> to translate into raw data</param>
        /// <param name="mapping">The <see cref="MappingInfo"/> to rely on when mapping the entity to raw data.</param>
        /// <param name="rowIndex">The row index at which to start adding the raw data.</param>
        /// <param name="columnCount">The total number of columns in the result, calculated once and passed to the recursive function for efficiency.</param>
        /// <returns>The number of rows occupied by the composed result.</returns>
        private int ComposeDataRowsFromEntity(EntityWithKey baseEntity, MappingInfo mapping, List<string[]> result, int rowIndex, int columnCount)
        {
            // Get the row, or create it if missing
            string[] dataRow = null;
            while (rowIndex >= result.Count)
            {
                result.Add(dataRow = new string[columnCount]);
            }

            dataRow ??= result[rowIndex];

            // Hydrate the simple props
            foreach (var simpleProp in mapping.SimpleProperties)
            {
                var entity = simpleProp.GetTerminalEntityForRead(baseEntity);
                if (!entity.EntityMetadata.IsLoaded(simpleProp.Metadata.Descriptor.Name))
                {
                    throw new InvalidOperationException($"Bug: Attempt to export unloaded property {simpleProp.Metadata.Descriptor.Name} from type {entity.GetType().Name}.");
                }

                if (simpleProp is ForeignKeyMappingInfo fkProp && fkProp.NotUsingIdAsKey)
                {
                    var navPropertyDesc = fkProp.NavPropertyMetadata.Descriptor;
                    if (!entity.EntityMetadata.IsLoaded(navPropertyDesc.Name))
                    {
                        throw new InvalidOperationException($"Bug: Attempt to export unloaded property {navPropertyDesc.Name} from type {entity.GetType().Name}.");
                    }

                    object navObj = navPropertyDesc.GetValue(entity);
                    if (navObj != null)
                    {
                        if (navObj is EntityWithKey navEntity)
                        {
                            var keyPropertyDesc = fkProp.KeyPropertyMetadata.Descriptor;
                            if (!navEntity.EntityMetadata.IsLoaded(keyPropertyDesc.Name))
                            {
                                throw new InvalidOperationException($"Bug: Attempt to export unloaded property {keyPropertyDesc.Name} from type {navEntity.GetType().Name}.");
                            }

                            var keyValue = keyPropertyDesc.GetValue(navEntity);
                            var keyStringValue = fkProp.KeyPropertyMetadata.Format(keyValue);
                            if (string.IsNullOrWhiteSpace(keyStringValue))
                            {
                                // var stringId = navEntity.GetId()?.ToString() ?? throw new InvalidOperationException($"Bug: Entity with key of type {navEntity.GetType().Name} was loaded without its Id");
                                keyStringValue = $"(undefined)"; // Ensures that if entity is not null, the key value is also not null, otherwise the import might be different
                            }
                            dataRow[fkProp.Index] = keyStringValue;
                        }
                        else
                        {
                            throw new InvalidOperationException($"Bug: Navigation Property {navPropertyDesc.Name} from type {entity.GetType().Name} returned a non-{nameof(EntityWithKey)}.");
                        }
                    }
                }
                else
                {
                    var value = simpleProp.Metadata.Descriptor.GetValue(entity);
                    var stringValue = simpleProp.Metadata.Format(value);

                    dataRow[simpleProp.Index] = stringValue;
                }
            }

            int numberOfRows = 1; // Number of rows occupied by 
            foreach (var nextMapping in mapping.CollectionProperties)
            {
                IEnumerable nextEntities = nextMapping.GetEntitiesForRead(baseEntity);

                // Number of rows occupied by the list (may be larger than the list if the list entities contain collection properties of their own
                int listNumberOfRows = 0;
                foreach (var nextEntity in nextEntities)
                {
                    // recursive call
                    listNumberOfRows += ComposeDataRowsFromEntity(nextEntity as EntityWithKey, nextMapping, result, rowIndex + listNumberOfRows, columnCount);
                }

                // The total number of rows of this entity is the maximum of its collection properties
                numberOfRows = Math.Max(numberOfRows, listNumberOfRows);
            }

            return numberOfRows;
        }
    }
}
