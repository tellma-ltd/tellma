using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tellma.Model.Common;

namespace Tellma.Api.Base
{
    /// <summary>
    /// Contains static extension methods for handling entities.
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Creates a dictionary that maps each entity to its index in the list,
        /// this is a much faster alternative to <see cref="List{T}.IndexOf(T)"/>
        /// if it is expected that it will be performed N times, since it performs 
        /// a linear search resulting in O(N^2) complexity.
        /// </summary>
        public static Dictionary<T, int> ToIndexDictionary<T>(this List<T> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            var result = new Dictionary<T, int>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var entity = list[i];
                result[entity] = i;
            }

            return result;
        }

        /// <summary>
        /// Traverses the <see cref="Entity"/> tree doing the following to each entity: <br/>
        /// 1 - If the entity has an Id property of type integer, it sets it to 0.<br/>
        /// 2 - If the entity has properties of type string, it trims their values or sets them to null if they are just empty spaces.
        /// </summary>
        /// <remarks>
        /// This function cannot handle cyclic entity graphs.
        /// </remarks>
        public static void StructuralPreprocess(this Entity entity)
        {
            if (entity == null)
            {
                // Nothing to do
                return;
            }

            // Inner recursive method that does the trimming on the entire tree
            static void TrimStringPropertiesInner(Entity entity, TypeDescriptor typeDesc)
            {
                // Set Id property to 0
                if (typeDesc.KeyType == KeyType.Int && entity is EntityWithKey entityWKey)
                {
                    if (entityWKey.GetId() == null)
                    {
                        entityWKey.SetId(0);
                    }
                }

                // Trim all string properties
                foreach (var prop in typeDesc.SimpleProperties.Where(p => p.Type == typeof(string)))
                {
                    var originalValue = prop.GetValue(entity)?.ToString();
                    if (string.IsNullOrWhiteSpace(originalValue))
                    {
                        // No empty strings or white spaces allowed
                        prop.SetValue(entity, null);
                    }
                    else
                    {
                        // Trim
                        var trimmedValue = originalValue.Trim();

                        // Removes &zwnj; chracters that sometimes appear when copying values from Tellma UI
                        var result = trimmedValue.Replace("\u200C", "");

                        // Set the value
                        prop.SetValue(entity, result);
                    }
                }

                // Recursively do nav properties
                foreach (var prop in typeDesc.NavigationProperties)
                {
                    if (prop.GetValue(entity) is Entity relatedEntity)
                    {
                        TrimStringPropertiesInner(relatedEntity, prop.TypeDescriptor);
                    }
                }

                // Recursively do the collection properties
                foreach (var prop in typeDesc.CollectionProperties)
                {
                    var collectionTypeDesc = prop.CollectionTypeDescriptor;
                    if (prop.GetValue(entity) is IList collection)
                    {
                        foreach (var obj in collection)
                        {
                            if (obj is Entity relatedEntity)
                            {
                                TrimStringPropertiesInner(relatedEntity, collectionTypeDesc);
                            }
                        }
                    }
                }
            }

            // Trim and return
            var typeDesc = TypeDescriptor.Get(entity.GetType());
            TrimStringPropertiesInner(entity, typeDesc);
        }
    }
}
