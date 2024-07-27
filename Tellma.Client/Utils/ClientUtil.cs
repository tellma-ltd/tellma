using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Model.Common;

namespace Tellma.Client
{
    public static class ClientUtil
    {
        /// <summary>
        /// Extension method that retrieves a single entity for save using its Id.
        /// The function uses <see cref="ExpandForSave{TEntityForSave}"/> to calculate
        /// the appropriate expand string and uses it to retrieve the entity with 
        /// <paramref name="id"/> which it then maps using <see cref="ModelUtil.MapToEntityForSave{TEntityForSave, TEntity}(TEntity)"/>.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="id">The id of the entity to get.</param>
        /// <param name="request">The get parameters. <see cref="GetByIdArguments"/> Select and Expand will be overridden.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>The retrieved entity after mapping it to <typeparamref name="TEntityForSave"/>.</returns>
        public static async Task<TEntityForSave> GetByIdForSave<TEntityForSave, TEntity, TKey>(this CrudClientBase<TEntityForSave, TEntity, TKey> client, TKey id, Request<GetByIdArguments> request = null, CancellationToken cancellation = default)
            where TEntityForSave : EntityWithKey<TKey>
            where TEntity : EntityWithKey<TKey>
        {
            request ??= new GetByIdArguments();
            request.Arguments.Expand = client.ExpandForSave;
            request.Arguments.Select = null;

            var result = await client.GetById(id, request, cancellation);

            return ModelUtil.MapToEntityForSave<TEntityForSave, TEntity>(result.Entity);
        }

        public static async Task<EntitiesResult<TEntity>> Save<TEntity, TEntityForSave, TKey>(this CrudClientBase<TEntityForSave, TEntity, TKey> client, TEntityForSave entityForSave, Request<SaveArguments> request = null, CancellationToken cancellation = default) 
            where TEntityForSave : EntityWithKey<TKey>
            where TEntity : EntityWithKey<TKey>
        {
            return await client.Save(new List<TEntityForSave> { entityForSave }, request, cancellation);
        }

        /// <summary>
        /// Returns the expand string that you must use when retreiving an entity
        /// for the intention of modifying and saving it. This expand string 
        /// guarantees that no weak related entities will be deleted upon saving.
        /// </summary>
        /// <typeparam name="TEntityForSave">The type of the entity that will be saved.</typeparam>
        /// <remarks>
        /// Some entities like <see cref="User"/> have a weak collection attached to it like
        /// its roles. If you retrieve the <see cref="User"/> alone, modify it and then save
        /// it, the API will interpret the lack of roles in he submitted <see cref="User"/> 
        /// as you wishing to delete all existing roles on that <see cref="User"/>. 
        /// This is probably not the intended behavior, so you should always include the
        /// weak collections when retrieving an entity for modification and saving.
        /// </remarks>
        public static string ExpandForSave<TEntityForSave>() where TEntityForSave : EntityWithKey
        {
            return ModelUtil.ExpandForSave<TEntityForSave>();
        }

        /// <summary>
        /// Links all entities back using their navigation properties.
        /// </summary>
        internal static void Unflatten<TEntity>(IEnumerable<TEntity> resultEntities, RelatedEntities relatedEntities, CancellationToken cancellation) where TEntity : Entity
        {
            if (resultEntities == null || !resultEntities.Any())
            {
                return;
            }

            relatedEntities ??= new RelatedEntities();

            // Cache related entities in a fast-to-query data structure
            // Mapping: Collection -> Id -> Entity
            var lookup = new Dictionary<string, Dictionary<object, EntityWithKey>>();
            bool TryGetEntity(string collection, object id, out EntityWithKey result)
            {
                // This function populates lookup with entityes of type in a lazy fashion only when requested
                if (!lookup.TryGetValue(collection, out Dictionary<object, EntityWithKey> entitiesOfType))
                {
                    // Id -> Entity
                    entitiesOfType = new Dictionary<object, EntityWithKey>();

                    // Cache related entities in this collection
                    foreach (var entity in relatedEntities.GetEntities(collection))
                    {
                        entitiesOfType.Add(entity.GetId(), entity);
                    }

                    // Cache the main entities if they are from the same collection
                    if (typeof(TEntity).Name == collection)
                    {
                        // If it's a nav entity then we can safely cast it
                        foreach (var entity in resultEntities.Cast<EntityWithKey>())
                        {
                            entitiesOfType.Add(entity.GetId(), entity);
                        }
                    }

                    lookup.Add(collection, entitiesOfType);
                }

                return entitiesOfType.TryGetValue(id, out result);
            }

            // Recursive function
            void UnflattenInner(Entity entity, TypeDescriptor typeDesc)
            {
                if (entity.EntityMetadata.Flattened)
                {
                    // This has already been unflattened before
                    return;
                }

                entity.EntityMetadata.Flattened = true;

                // Recursively go over the nav properties
                foreach (var prop in typeDesc.NavigationProperties)
                {
                    var navDesc = prop.TypeDescriptor;
                    var navCollection = navDesc.Name;
                    var fkValue = prop.ForeignKey.GetValue(entity);

                    if (fkValue != null && TryGetEntity(navCollection, fkValue, out EntityWithKey relatedEntity))
                    {
                        prop.SetValue(entity, relatedEntity);
                        UnflattenInner(relatedEntity, navDesc);
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
                                UnflattenInner(relatedEntity, collectionType);
                            }
                        }
                    }
                }
            }

            // Unflatten every entity in the main list
            var typeDesc = TypeDescriptor.Get<TEntity>();
            foreach (var entity in resultEntities)
            {
                if (entity != null)
                {
                    UnflattenInner(entity, typeDesc);
                    cancellation.ThrowIfCancellationRequested();
                }
            }
        }
    }
}
