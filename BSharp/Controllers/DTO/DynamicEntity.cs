using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    /// <summary>
    /// Represents a DTO with variable properties, this is typically used to contain the results of aggregate queries
    /// in which case the properties will be the requested dimensions and measures, it is implemented as a dictionary
    /// which is serialized by JSON.NET as 
    /// </summary>
    public class DynamicEntity : DtoBase, IDictionary<string, object>
    {
        private readonly IDictionary<string, object> _dic = new Dictionary<string, object>();

        public object this[string key] { get => _dic[key]; set => _dic[key] = value; }

        public ICollection<string> Keys => _dic.Keys;

        public ICollection<object> Values => _dic.Values;

        public int Count => _dic.Count;

        public bool IsReadOnly => _dic.IsReadOnly;

        public void Add(string key, object value)
        {
            _dic.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _dic.Add(item);
        }

        public void Clear()
        {
            _dic.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _dic.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _dic.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _dic.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dic.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return _dic.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _dic.Remove(item);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _dic.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dic.GetEnumerator();
        }
    }

}
