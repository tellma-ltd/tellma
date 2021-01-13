using System;
using System.ComponentModel.DataAnnotations;
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
        private readonly Action<Entity, int?> _indexPropertySetter;
        private readonly Func<Entity, int?> _indexPropertyGetter;

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
        /// The name of the associated index property (if this is a self reference property)
        /// </summary>
        public string IndexPropertyName { get; }

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
        /// True if the property is of type <see cref="Geography"/>
        /// </summary>
        public bool IsGeography { get; }

        /// <summary>
        /// The maximum length of string properties
        /// </summary>
        public int MaxLength { get; }

        /// <summary>
        /// Indicates that a <see cref="DateTime"/> property can store time as well as date (maps to a DateTime column in the database)
        /// </summary>
        public bool IncludesTime { get; set; }

        /// <summary>
        /// Indicates that this property is NOT NULL in the database
        /// </summary>
        public bool IsNotNull { get; set; }

        /// <summary>
        /// For self referencing foreign keys like ParentId (FKs that reference the same entity type),
        /// there should always be an associated index property ParentIndex, to allow for referecing 
        /// another entity in the saved list (may not have an ID yet) while saving such entities in bulk.
        /// This method sets that property to a given integer index
        /// </summary>
        public void SetIndexProperty(Entity entity, int index) => _indexPropertySetter(entity, index);

        /// <summary>
        /// For self referencing foreign keys like ParentId (FKs that reference the same entity type),
        /// there should always be an associated index property ParentIndex, to allow for referecing 
        /// another entity in the saved list (may not have an ID yet) while saving such entities in bulk.
        /// This method sets that property to a given integer index
        /// </summary>
        public int? GetIndexProperty(Entity entity) => _indexPropertyGetter(entity);

        /// <summary>
        /// Syntactic sugar for _indexPropertySetter != null;
        /// </summary>
        public bool IsSelfReferencing => IndexPropertyName != null;

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
            Action<Entity, object> setter,
            Func<Entity, object> getter,
            string indexPropName = null,
            Action<Entity, int?> indexPropSetter = null,
            Func<Entity, int?> indexPropGetter = null)
        {
            PropertyInfo = propInfo ?? throw new ArgumentNullException(nameof(propInfo));
            Type = PropertyInfo.PropertyType;
            Name = PropertyInfo.Name;
            _setter = setter ?? throw new ArgumentNullException(nameof(setter));
            _getter = getter ?? throw new ArgumentNullException(nameof(getter));

            if (indexPropName != null)
            {
                IndexPropertyName = indexPropName;
                _indexPropertySetter = indexPropSetter ?? throw new ArgumentNullException(nameof(indexPropSetter));
                _indexPropertyGetter = indexPropGetter ?? throw new ArgumentNullException(nameof(indexPropGetter));
            }

            IsHierarchyId = Type == typeof(HierarchyId);
            IsGeography = Type == typeof(Geography);

            // Some attributes

            #region MaxLength

            MaxLength = -1;
            var stringLengthAttribute = propInfo.GetCustomAttribute<StringLengthAttribute>(inherit: true);
            if (stringLengthAttribute != null)
            {
                MaxLength = stringLengthAttribute.MaximumLength;
            }

            #endregion

            #region IncludesTime

            IncludesTime = propInfo.GetCustomAttribute<IncludesTimeAttribute>(inherit: true) != null;

            #endregion

            #region Not Null

            IsNotNull = propInfo.GetCustomAttribute<NotNullAttribute>(inherit: true) != null;

            #endregion
        }
    }
}
