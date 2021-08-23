using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tellma.Model.Common
{
    public static class TypeExtensions
    {
        /// <summary>
        /// This is similar to <see cref="Type.GetProperties"/> but returns base
        /// class properties before child class properties.
        /// <para/>
        /// Credit: https://bit.ly/2UGAkKj
        /// </summary>
        public static PropertyInfo[] GetPropertiesBaseFirst(this Type type, BindingFlags bindingAttr)
        {
            var orderList = new List<Type>();
            var iteratingType = type;
            do
            {
                orderList.Insert(0, iteratingType);
                iteratingType = iteratingType.BaseType;
            }
            while (iteratingType != null);

            var props = type.GetProperties(bindingAttr)
                .OrderBy(x => orderList.IndexOf(x.DeclaringType))
                .ToArray();

            return props;
        }

        /// <summary>
        /// Determines whether this type is a <see cref="List{T}"/>.
        /// </summary>
        public static bool IsList(this Type @this)
        {
            return @this.IsGenericType && @this.GetGenericTypeDefinition() == typeof(List<>);
        }
    }
}
