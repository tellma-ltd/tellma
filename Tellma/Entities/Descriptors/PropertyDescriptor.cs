using System;
using System.Reflection;

namespace Tellma.Entities.Descriptors
{
    /// <summary>
    /// Describes a mapped property on an entity, this is the base class of all property descriptors
    /// </summary>
    public class PropertyDescriptor
    {
        private readonly Action<Entity, object> _setter;
        private readonly Func<Entity, object> _getter;

        /// <summary>
        /// The type of proprety value
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The <see cref="PropertyInfo"/> of the described property
        /// </summary>
        public PropertyInfo PropertyInfo { get; }

        /// <summary>
        /// The name of the property
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Sets the value of the property on the given entity to the given value
        /// </summary>
        public void SetValue(Entity entity, object value) => _setter(entity, value);

        /// <summary>
        /// Gets the value of the property form the given entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public object GetValue(Entity entity) => _getter(entity);

        /// <summary>
        /// True if the property is of type <see cref="HierarchyId"/>
        /// </summary>
        public bool IsHierarchyId { get; }

        /// <summary>
        /// The maximum length of string properties
        /// </summary>
        public int MaxLength { get; }

        /// <summary>
        /// For <see cref="NavigationPropertyDescriptor"/>s, returns the entity descriptor of the property type.
        /// For <see cref="CollectionPropertyDescriptor"/>s, returns the entity descriptor of the collection's entity type.
        /// For simple properties, throws an exception
        /// </summary>
        public TypeDescriptor GetEntityDescriptor()
        {
            if (this is NavigationPropertyDescriptor navProp)
            {
                return navProp.TypeDescriptor;
            }
            else if (this is CollectionPropertyDescriptor collProp)
            {
                return collProp.CollectionTypeDescriptor;
            }
            else
            {
                // Developer mistake
                throw new InvalidOperationException($"Bug: Simple property {Name} is used like a navigation property");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PropertyDescriptor(
            PropertyInfo propInfo,
            string name,
            Action<Entity, object> setter,
            Func<Entity, object> getter,
            int maxLength = -1)
        {
            PropertyInfo = propInfo ?? throw new ArgumentNullException(nameof(propInfo));
            Type = PropertyInfo.PropertyType;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _setter = setter ?? throw new ArgumentNullException(nameof(setter));
            _getter = getter ?? throw new ArgumentNullException(nameof(getter));

            IsHierarchyId = Type == typeof(HierarchyId);
            MaxLength = maxLength;
        }
    }
}
