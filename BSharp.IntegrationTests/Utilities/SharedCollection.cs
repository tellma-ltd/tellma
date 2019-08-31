using System.Collections.Concurrent;

namespace BSharp.IntegrationTests.Utilities
{
    /// <summary>
    /// A simple data structure to store a shared collection of items across test methods
    /// </summary>
    public class SharedCollection
    {
        private readonly ConcurrentDictionary<string, object> _sharedItems = new ConcurrentDictionary<string, object>();

        public T Get<T>(string name)
        {
            return (T)_sharedItems[name];
        }

        public void Set<T>(string name, T item)
        {
            _sharedItems[name] = item;
        }
    }
}
