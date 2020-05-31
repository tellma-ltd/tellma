using System;
using System.Reflection;

namespace Tellma.Entities.Descriptors
{
    /// <summary>
    /// Describes a navigation property that points to another <see cref="Entity"/>
    /// </summary>
    public class NavigationPropertyDescriptor : PropertyDescriptor
    {
        private TypeDescriptor _typeDescriptor; // Caching
        private readonly Func<TypeDescriptor> _getTypeDescriptor;

        /// <summary>
        /// True if the property is called "Parent" on a hierarchal entity, as per convention
        /// </summary>
        public bool IsParent { get; }

        /// <summary>
        /// The <see cref="PropertyDescriptor"/> of the foreign key associated with this <see cref="NavigationPropertyDescriptor"/>
        /// </summary>
        public PropertyDescriptor ForeignKey { get; }

        /// <summary>
        /// The <see cref="Descriptors.TypeDescriptor"/> of the type of the value of this property
        /// </summary>
        public TypeDescriptor TypeDescriptor => _typeDescriptor ??= _getTypeDescriptor();

        /// <summary>
        /// Constructor
        /// </summary>
        public NavigationPropertyDescriptor(
            PropertyInfo propInfo,
            string name,
            Action<Entity, object> setter,
            Func<Entity, object> getter,
            bool isParent,
            PropertyDescriptor foreignKey,
            Func<TypeDescriptor> getTypeDescriptor) : base(propInfo, name, setter, getter)
        {
            _getTypeDescriptor = getTypeDescriptor;
            IsParent = isParent;
            ForeignKey = foreignKey;
        }
    }
}
