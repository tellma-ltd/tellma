using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tellma.Model.Common;

namespace Tellma.Controllers.Utilities
{
    public static class ControllerUtilities
    {
        /// <summary>
        /// Takes a list of <see cref="Entity"/>'s, and for every entity it inspects the navigation properties, if a navigation property
        /// contains an <see cref="Entity"/> with a strong type, it sets that property to null, and moves the strong entity into a separate
        /// "relatedEntities" hash set, this has several advantages: <br/>
        /// 1 - JSON.NET will not have to deal with circular references <br/>
        /// 2 - Every strong entity is mentioned once in the JSON response (smaller response size) <br/>
        /// 3 - It makes it easier for clients to store and track entities in a central workspace <br/>
        /// </summary>
        /// <returns>A dictionary mapping every type name to an <see cref="IEnumerable"/> of related entities of that type (excluding the result entities).</returns>
        public static Dictionary<string, IEnumerable<EntityWithKey>> Flatten<TEntity>(IEnumerable<TEntity> resultEntities, CancellationToken cancellation)
            where TEntity : Entity
        {
            // If the result is empty, nothing to do
            if (resultEntities == null || !resultEntities.Any())
            {
                return new Dictionary<string, IEnumerable<EntityWithKey>>();
            }

            var relatedEntities = new HashSet<EntityWithKey>();
            var resultHash = resultEntities.Cast<Entity>().ToHashSet();

            void FlattenInner(Entity entity, TypeDescriptor typeDesc)
            {
                if (entity.EntityMetadata.Flattened)
                {
                    // This has already been flattened before
                    return;
                }

                // Mark the entity as flattened
                entity.EntityMetadata.Flattened = true;

                // Recursively go over the nav properties
                foreach (var prop in typeDesc.NavigationProperties)
                {
                    if (prop.GetValue(entity) is EntityWithKey relatedEntity)
                    {
                        prop.SetValue(entity, null);

                        if (!resultHash.Contains(relatedEntity))
                        {
                            // Unless it is part of the main result, add it to relatedEntities
                            relatedEntities.Add(relatedEntity);
                        }

                        FlattenInner(relatedEntity, prop.TypeDescriptor);
                    }
                }

                // Recursively go over every entity in the nav collection properties
                foreach (var prop in typeDesc.CollectionProperties)
                {
                    var collectionType = prop.CollectionTypeDescriptor;
                    if (prop.GetValue(entity) is IList collection)
                    {
                        foreach (var obj in collection)
                        {
                            if (obj is Entity relatedEntity)
                            {
                                FlattenInner(relatedEntity, collectionType);
                            }
                        }
                    }
                }
            }

            // Flatten every entity in the main list
            var typeDesc = TypeDescriptor.Get<TEntity>();
            foreach (var entity in resultEntities)
            {
                if (entity != null)
                {
                    FlattenInner(entity, typeDesc);
                    cancellation.ThrowIfCancellationRequested();
                }
            }

            // Return the result
            return relatedEntities
                .GroupBy(e => e.GetType().Name)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());
        }

        /// <summary>
        /// Attempts to intelligently guess the content mime type from the file name
        /// </summary>
        public static string ContentType(string fileName)
        {
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }

        /// <summary>
        /// Retrieves the collection name from the Entity type.
        /// </summary>
        public static string GetCollectionName(Type entityType)
        {
            return entityType.Name;
        }
    }
}
