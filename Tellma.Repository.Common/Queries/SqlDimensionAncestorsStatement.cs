using System.Collections.Generic;
using System.Linq;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Represents a single SQL SELECT query that loads the ancestors of a certain tree dimension of an aggregation.
    /// This is useful for clients that wish to arrange the data in a pivot table with tree dimensions.
    /// </summary>
    public class SqlDimensionAncestorsStatement : SqlStatementBase
    {
        public SqlDimensionAncestorsStatement(int idIndex, string sql, IEnumerable<int> targetIndices): base(sql)
        {
            IdIndex = idIndex;
            TargetIndices = targetIndices.ToList();
        }

        /// <summary>
        /// The column index of the Id of the tree entity.
        /// </summary>
        public int IdIndex { get; } // Key

        /// <summary>
        /// The indices in the principal query of all the selected properties of the tree dimension entity.
        /// </summary>
        public IEnumerable<int> TargetIndices { get; }
    }
}
