using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BSharp.Services.OData
{
    public interface IPropInfo
    {
        Type PropertyType { get; }

        string Name { get; }

        object GetValue(object entity);

        void SetValue(object entity, object value);

        IPropInfo ForeignKeyProperty();
    }

    /// <summary>
    /// A simple wrapper around <see cref="PropertyInfo"/> which implements <see cref="IPropInfo"/>
    /// </summary>
    public class PropInfo : IPropInfo
    {
        private readonly PropertyInfo _propInfo;
        bool _foreignKeyPropertySet = false;
        IPropInfo _foreignKeyProperty = null;

        public PropInfo(PropertyInfo propInfo)
        {
            _propInfo = propInfo;
        }

        public Type PropertyType => _propInfo.PropertyType;

        public string Name => _propInfo.Name;

        public IPropInfo ForeignKeyProperty()
        {
            if(!_foreignKeyPropertySet)
            {
                var fkName = _propInfo.GetCustomAttribute<NavigationPropertyAttribute>()?.ForeignKey;
                if (!string.IsNullOrWhiteSpace(fkName))
                {
                    var fkProp = _propInfo.DeclaringType.GetProperty(fkName);
                    _foreignKeyProperty = new PropInfo(fkProp);
                }

                _foreignKeyPropertySet = true;
            }

            return _foreignKeyProperty;
        }

        public object GetValue(object entity) => _propInfo.GetValue(entity);

        public void SetValue(object entity, object value)
        {
            _propInfo.SetValue(entity, value);
        }
    }

    /// <summary>
    /// An implementation of <see cref="IPropInfo"/> for the properties of <see cref="DynamicEntity"/>
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
            if(value == null)
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

        public Type DeclaringType { get; }

        public ArraySegment<string> Path { get; }

        public string Property { get; }

        public string Aggregation { get; }

        public IPropInfo FK { get; }

        public bool IsDimension { get => string.IsNullOrWhiteSpace(Aggregation); }

        public bool IsMeasure { get => !IsDimension; }
    }
}
