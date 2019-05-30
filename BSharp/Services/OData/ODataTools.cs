using BSharp.Services.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BSharp.Services.OData
{
    public static class ODataTools
    {
        public static string PathString(string[] path, string property = null)
        {
            string result = string.Join(' ', path);
            if (!string.IsNullOrWhiteSpace(property))
            {
                result = $"{result}/{property}";
            }

            return result;
        }

        public static (string[] Path, string Property) ExtractPathAndProperty(string item)
        {
            var steps = item.Split('/').Select(e => e?.Trim());
            string[] path = steps.Take(steps.Count() - 1).ToArray();
            string property = steps.Last();

            return (path, property);
        }

        private static ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> _cacheGetMappedProperties = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();
        public static IEnumerable<PropertyInfo> GetMappedProperties(this Type type)
        {
            if(!_cacheGetMappedProperties.ContainsKey(type))
            {
                // All public properties on the DTO excluding navigation properties and properties adorned with NotMapped
                _cacheGetMappedProperties[type] = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(e => e.GetCustomAttribute<NotMappedAttribute>() == null && !e.PropertyType.IsList() && e.PropertyType.GetProperty("Id") == null);
            }

            return _cacheGetMappedProperties[type];
        }

        public static string IndentLines(string s, int spaces = 4)
        {
            var lines = s.Split(Environment.NewLine);
            StringBuilder bldr = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                string indentedLine = "    " + line;
                if (i == lines.Length - 1)
                {
                    bldr.Append(indentedLine);

                }
                else
                {
                    bldr.AppendLine(indentedLine);
                }
            }

            return bldr.ToString();
        }
    }
}
