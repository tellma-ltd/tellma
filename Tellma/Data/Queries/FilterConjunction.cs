using System.Collections.Generic;
using System.Linq;

namespace Tellma.Data.Queries
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

        /// <summary>
        /// Returns a <see cref="FilterConjunction"/> containing of the left and right expressions
        /// </summary>
        public static FilterConjunction Make(FilterExpression left, FilterExpression right)
        {
            if (left is null)
            {
                throw new System.ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new System.ArgumentNullException(nameof(right));
            }

            return new FilterConjunction { Left = left, Right = right };
        }
    }
}
