using System.Collections.Generic;
using Tellma.Model.Common;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Packages the result of loading an <see cref="SqlDynamicStatement"/>.
    /// </summary>
    public class DynamicResult
    {
        public DynamicResult(List<DynamicRow> rows, IEnumerable<DimensionAncestorsResult> trees, int count)
        {
            Rows = rows;
            Trees = trees;
            Count = count;
        }

        /// <summary>
        /// The dynamic rows returned from the DB.
        /// </summary>
        public List<DynamicRow> Rows { get; }

        /// <summary>
        /// The data comprising the tree dimensions.
        /// </summary>
        public IEnumerable<DimensionAncestorsResult> Trees { get; }

        /// <summary>
        /// The total count of the unfiltered rows if such a count is requested.
        /// </summary>
        public int Count { get; }
    }
}
