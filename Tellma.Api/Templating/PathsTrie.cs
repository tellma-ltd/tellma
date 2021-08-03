using System.Collections.Generic;
using System.Linq;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// A trie data structure (https://en.wikipedia.org/wiki/Trie) to efficiently
    /// store and merge a large number of paths (each path is a list of steps).
    /// </summary>
    public class PathsTrie
    {
        /// <summary>
        /// Adds a path to the <see cref="PathsTrie"/>
        /// </summary>
        public void AddPath(IEnumerable<string> path)
        {
            var current = this;
            var currentDic = current._dic;

            foreach (var step in path)
            {
                if (!currentDic.TryGetValue(step, out current))
                {
                    current = new PathsTrie();
                    currentDic[step] = current;
                }

                currentDic = current._dic;
            }
        }

        /// <summary>
        /// Returns a unique list of paths represented by this <see cref="PathsTrie"/>.
        /// </summary>
        public IEnumerable<string[]> GetPaths()
        {
            var acc = new List<List<string>>();
            var basePath = new List<string>();
            GetPathsInner(basePath, acc);

            return acc.Select(l => l.ToArray());
        }

        #region Internal Impl

        private readonly Dictionary<string, PathsTrie> _dic = new Dictionary<string, PathsTrie>();

        private bool IsLeaf => _dic.Keys.Count == 0;

        private IEnumerable<string> Keys => _dic.Keys;

        private void GetPathsInner(List<string> currentPath, List<List<string>> acc)
        {
            foreach (var step in Keys)
            {
                var childPath = new List<string>(currentPath) { step };

                var node = _dic[step];
                if (node.IsLeaf)
                {
                    acc.Add(childPath);
                }
                else
                {
                    node.GetPathsInner(currentPath: childPath, acc);
                }
            }
        }

        #endregion
    }
}
