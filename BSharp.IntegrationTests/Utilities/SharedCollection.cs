using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.IntegrationTests.Utilities
{
    // A simple data structure to store a shared collection of items across test methods
    public class SharedCollection
    {
        private Dictionary<string, object> _sharedItems = new Dictionary<string, object>();

        public T GetItem<T>(string name)
        {
            return (T)_sharedItems[name];
        }

        public void SetItem<T>(string name, T item)
        {
            _sharedItems[name] = item;
        }
    }
}
