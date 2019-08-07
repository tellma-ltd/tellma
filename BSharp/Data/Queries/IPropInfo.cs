using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BSharp.Data.Queries
{
    /// <summary>
    /// Some algorithms that rely on reflection to hydrate and manipulate entity properties don't use <see cref="PropertyInfo"/> directly, but rely on this interface instead.
    /// This is to support <see cref="DynamicEntity"/> which has a dynamic set of properties that are represented as keys in a Dictionary
    /// </summary>
    public interface IPropInfo
    {
        /// <summary>
        /// The CLR data type of the property
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// The name of the property
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get the value of the property from the given entity
        /// </summary>
        object GetValue(object entity);

        /// <summary>
        /// Set the value of the property on the given entity to a certain value
        /// </summary>
        void SetValue(object entity, object value);

        /// <summary>
        /// Gets the <see cref="IPropInfo"/> corresponding to the foreign key of this property if it was a navigation property, or null otherwise
        /// </summary>
        IPropInfo ForeignKeyProperty();
    }
}
