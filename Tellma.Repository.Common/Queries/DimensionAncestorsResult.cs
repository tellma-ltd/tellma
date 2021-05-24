using System.Collections.Generic;
using Tellma.Model.Common;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Packages the result of loading the dimension ancestors of a <see cref="SqlDimensionAncestorsStatement"/>.
    /// </summary>
    public class DimensionAncestorsResult
    {
        public DimensionAncestorsResult(int idIndex, int minIndex)
        {
            IdIndex = idIndex;
            MinIndex = minIndex;
            Result = new List<DynamicRow>();
        }

        /// <summary>
        /// The id of the index, clients use this value to identify which tree dimension
        /// this represents if there were multiple of them in the same query.
        /// </summary>
        public int IdIndex { get; }

        /// <summary>
        /// Column index i from <see cref="Result"/> maps to column index i + <see cref="MinIndex"/> in the principal result.
        /// </summary>
        public int MinIndex { get; }

        /// <summary>
        /// The dynamic rows of the dimension ancestors from the DB.
        /// </summary>
        public List<DynamicRow> Result { get; }
    }
}
