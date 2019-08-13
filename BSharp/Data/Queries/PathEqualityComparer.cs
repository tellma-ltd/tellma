using System;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Data.Queries
{
    /// <summary>
    /// An <see cref="IEqualityComparer{T}"/> implementation for <see cref="ArraySegment{T}"/> paths.
    /// Used in hash sets and dictionaries to equate two paths when the are comprised of identical steps
    /// </summary>
    internal class PathEqualityComparer : IEqualityComparer<ArraySegment<string>>
    {
        public bool Equals(ArraySegment<string> x, ArraySegment<string> y)
        {
            return x == y || (x.Count == y.Count && Enumerable.Range(0, x.Count).All(i => x[i] == y[i]));
        }

        public int GetHashCode(ArraySegment<string> obj)
        {
            return obj.Select(e => e.GetHashCode()).Aggregate(0, (e1, e2) => e1 ^ e2);
        }
    }
}
