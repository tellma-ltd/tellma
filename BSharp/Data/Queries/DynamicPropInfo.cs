using System;

namespace BSharp.Data.Queries
{
    /// <summary>
    /// Reflection cannot be used to retrieve the properties of a <see cref="DynamicEntity"/> in
    /// the same manner as other types <see cref="Entities.Entity"/>, since they are
    /// stored as dictionary entries. Therefore several algorithms that rely on reflection
    /// use <see cref="IPropInfo"/> instead to support both types of entities fixed and dynamic
    /// </summary>
    public class DynamicPropInfo : IPropInfo
    {
        public DynamicPropInfo(Type propType, string name, Type declaringType, ArraySegment<string> path, string property, string aggregation, IPropInfo fk = null)
        {
            PropertyType = propType;
            Name = name;
            DeclaringType = declaringType;
            Path = path;
            Property = property;
            Aggregation = aggregation;
            FK = fk;
        }

        public Type PropertyType { get; }

        public string Name { get; }

        public object GetValue(object entity)
        {
            ((DynamicEntity)entity).TryGetValue(Name, out object result);
            return result;
        }

        public void SetValue(object entity, object value)
        {
            // JSON.NET will return dictionary properties that are set to null, regardless of the null handler you specify
            if (value == null)
            {
                ((DynamicEntity)entity).Remove(Name);
            }
            else
            {
                ((DynamicEntity)entity)[Name] = value;
            }
        }

        public IPropInfo ForeignKeyProperty()
        {
            return FK;
        }

        /// <summary>
        /// Even though the properties of a <see cref="DynamicEntity"/> are dynamic, they always have their origins
        /// from a real property on a real <see cref="Entities.Entity"/> that was subject of a group by or an
        /// aggregation, this is the declaring type of that property
        /// </summary>
        public Type DeclaringType { get; }

        /// <summary>
        /// That path from the root type of the query to the property
        /// </summary>
        public ArraySegment<string> Path { get; }

        /// <summary>
        /// The name of the property
        /// </summary>
        public string Property { get; }

        /// <summary>
        /// Any aggregations applied on the property. E.g. "sum" or "count", the full list of aggregations is found in <see cref="Aggregations"/>
        /// </summary>
        public string Aggregation { get; }

        /// <summary>
        /// The <see cref="IPropInfo"/> representing the foreign key of this property if it is a navigation property, or null otherwise
        /// </summary>
        public IPropInfo FK { get; }

        /// <summary>
        /// Equals true if and only if this property was used in a group by, rather than an aggregation
        /// </summary>
        public bool IsDimension { get => string.IsNullOrWhiteSpace(Aggregation); }

        /// <summary>
        /// Equals true if and only if this property was used in an aggregation
        /// </summary>
        public bool IsMeasure { get => !IsDimension; }
    }
}
