using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace BSharp.Data.Queries
{
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
            if (!_foreignKeyPropertySet)
            {
                var fkName = _propInfo.GetCustomAttribute<ForeignKeyAttribute>()?.Name;
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
}
