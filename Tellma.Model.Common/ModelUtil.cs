using System;
using System.Collections;
using System.Linq;

namespace Tellma.Model.Common
{
    public static class ModelUtil
    {
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
            var desc = TypeDescriptor.Get<TEntity>();
            var descForSave = TypeDescriptor.Get<TEntityForSave>();

            return MapInner(entity, desc, descForSave) as TEntityForSave;
        }

        /// <summary>
        /// Helper function.
        /// </summary>
        private static EntityWithKey MapInner(EntityWithKey entity, TypeDescriptor desc, TypeDescriptor descForSave)
        {
            if (entity == null)
            {
                return null;
            }

            var entityForSave = descForSave.Create() as EntityWithKey;

            if (descForSave.NavigationProperties.Any())
            {
                var navProp = descForSave.NavigationProperties.FirstOrDefault();
                throw new InvalidOperationException($"Navigation properties on source types (such as {navProp.Name} on type {descForSave.Name}) are not supported.");
            }

            foreach (var propForSave in descForSave.SimpleProperties)
            {
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
                var collProp = desc.CollectionProperty(collPropForSave.Name);
                if (collProp == null)
                {
                    throw new InvalidOperationException($"Property {collPropForSave.Name} on source type {descForSave.Name} has no matching property on target type {desc.Name}.");
                }

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
                                var collEntityForSave = MapInner(collEntity, collProp.CollectionTypeDescriptor, collPropForSave.CollectionTypeDescriptor);
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

            return entityForSave;
        }
    }
}
