using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Model.Common
{
    public static class ModelUtil
    {
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
            static IEnumerable<string> CollectionAtoms(TypeDescriptor desc, HashSet<Type> processedAlready)
            {
                if (processedAlready.Add(desc.Type))
                {
                    foreach (var collProp in desc.CollectionProperties)
                    {
                        // For every collection navigation property
                        // 1 - Either return its name if it has no collection properties of its own
                        // 2 - Or return its name appended to the same of each one of its collection properties.
                        var collDesc = collProp.CollectionTypeDescriptor;
                        var collAtoms = CollectionAtoms(collDesc, processedAlready).ToList();
                        if (collAtoms.Count > 0)
                        {
                            foreach (var expand in collAtoms)
                            {
                                yield return $"{collProp.Name}.{expand}";
                            }
                        }
                        else
                        {
                            yield return collProp.Name;
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException($"The type {typeof(TEntityForSave).Name} cannot be used with {nameof(ExpandForSave)} since it causes infinite recursion.");
                }
            }

            var types = new HashSet<Type>();
            var desc = TypeDescriptor.Get<TEntityForSave>();
            return string.Join(',', CollectionAtoms(desc, types));
        }

        /// <summary>
        /// Maps an entity to its "ForSave" version, for example maps <see cref="User"/> to
        /// a <see cref="UserForSave"/> copying all the properties and weak collections across.
        /// </summary>
        /// <typeparam name="TEntityForSave">The "ForSave" type to map <paramref name="entity"/> to.</typeparam>
        /// <typeparam name="TEntity">The type of <paramref name="entity"/>.</typeparam>
        /// <param name="entity">The entity to map</param>
        /// <returns></returns>
        public static TEntityForSave MapToEntityForSave<TEntityForSave, TEntity>(TEntity entity)
            where TEntityForSave : EntityWithKey
            where TEntity : EntityWithKey
        {
            var descForSave = TypeDescriptor.Get<TEntityForSave>();
            var entityForSave = descForSave.Create() as TEntityForSave;

            return MapToEntityForSave(entity, entityForSave, null);
        }

        /// <summary>
        /// Maps an entity to its "ForSave" version, for example maps <see cref="User"/> to
        /// a <see cref="UserForSave"/> copying all the properties and weak collections across.
        /// </summary>
        /// <typeparam name="TEntityForSave">The "ForSave" type to map <paramref name="entity"/> to.</typeparam>
        /// <typeparam name="TEntity">The type of <paramref name="entity"/>.</typeparam>
        /// <param name="entity">The entity to map</param>
        /// <returns></returns>
        public static TEntityForSave MapToEntityForSave<TEntityForSave, TEntity>(TEntity entity, TEntityForSave entityForSave, HashSet<string> propsToSkip)
            where TEntityForSave : EntityWithKey
            where TEntity : EntityWithKey
        {
            var desc = TypeDescriptor.Get<TEntity>();
            var descForSave = TypeDescriptor.Get<TEntityForSave>();

            MapInner(entity, entityForSave, desc, descForSave, propsToSkip);

            return entityForSave;
        }

        /// <summary>
        /// Helper function.
        /// </summary>
        private static void MapInner(EntityWithKey entity, EntityWithKey entityForSave, TypeDescriptor desc, TypeDescriptor descForSave, HashSet<string> propsToSkip)
        {
            if (entity == null)
            {
                return;
            }

            if (descForSave.NavigationProperties.Any())
            {
                var navProp = descForSave.NavigationProperties.FirstOrDefault();
                throw new InvalidOperationException($"Navigation properties on source types (such as {navProp.Name} on type {descForSave.Name}) are not supported.");
            }

            propsToSkip ??= new HashSet<string>();

            foreach (var propForSave in descForSave.SimpleProperties)
            {
                if (propsToSkip.Contains(propForSave.Name))
                {
                    continue;
                }

                var prop = desc.Property(propForSave.Name);
                if (prop == null)
                {
                    throw new InvalidOperationException($"Property {propForSave.Name} on source type {descForSave.Name} has no matching property on target type {desc.Name}.");
                }
                else if (propForSave.Type != prop.Type)
                {
                    throw new InvalidOperationException($"Property {propForSave.Name} on source type {descForSave.Name} has a matching property on target type {desc.Name} but with a different type.");
                }

                var value = prop.GetValue(entity);
                propForSave.SetValue(entityForSave, value);
            }

            foreach (var collPropForSave in descForSave.CollectionProperties)
            {
                if (propsToSkip.Contains(collPropForSave.Name))
                {
                    continue;
                }

                var collProp = desc.CollectionProperty(collPropForSave.Name) ?? throw new InvalidOperationException($"Property {collPropForSave.Name} on source type {descForSave.Name} has no matching property on target type {desc.Name}.");
                
                var value = collProp.GetValue(entity);
                if (value != null)
                {
                    if (value is IList list)
                    {
                        var listForSave = collPropForSave.CollectionTypeDescriptor.CreateList();

                        foreach (var obj in list)
                        {
                            if (obj == null)
                            {
                                listForSave.Add(null);
                            }
                            else if (obj is EntityWithKey collEntity)
                            {
                                var collEntityForSave = collPropForSave.CollectionTypeDescriptor.Create() as EntityWithKey;
                                MapInner(collEntity, collEntityForSave, collProp.CollectionTypeDescriptor, collPropForSave.CollectionTypeDescriptor, null);
                                listForSave.Add(collEntityForSave);
                            }
                            else
                            {
                                throw new InvalidOperationException($"Collection {collPropForSave.Name} on {descForSave.Name} contains an entity that does not inherit from {nameof(EntityWithKey)}.");
                            }
                        }

                        collPropForSave.SetValue(entityForSave, listForSave);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Property {collPropForSave.Name} on source type {descForSave.Name} has a matching property on target type {desc.Name} that is not a list.");
                    }
                }
            }
        }
    }
}
