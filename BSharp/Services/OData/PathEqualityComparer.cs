using System;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Services.OData
{
    public class PathEqualityComparer : IEqualityComparer<ArraySegment<string>>
    {
        public bool Equals(ArraySegment<string> x, ArraySegment<string> y)
        {
            return x.Count == y.Count && Enumerable.Range(0, x.Count).All(i => x[i] == y[i]);
        }

        public int GetHashCode(ArraySegment<string> obj)
        {
            return obj.Select(e => e.GetHashCode()).Aggregate(0, (e1, e2) => e1 ^ e2);
        }
    }
}
