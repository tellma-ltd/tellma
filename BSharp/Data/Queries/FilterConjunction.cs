using System.Collections.Generic;
using System.Linq;

namespace BSharp.Data.Queries
{
    /// <summary>
    /// Represents a conjunction of <see cref="FilterExpression"/>s (E1 and E2)
    /// </summary>
    public class FilterConjunction : FilterExpression
    {
        /// <summary>
        /// The left side of the AND operator
        /// </summary>
        public FilterExpression Left { get; set; }

        /// <summary>
        /// The right side of the AND operator
        /// </summary>
        public FilterExpression Right { get; set; }

        public override IEnumerable<FilterAtom> Atoms()
        {
            return Left.Atoms().Union(Right.Atoms());
        }
    }
}
