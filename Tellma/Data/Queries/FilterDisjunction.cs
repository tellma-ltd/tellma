using System.Collections.Generic;
using System.Linq;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents a disjunction of <see cref="FilterExpression"/>s (E1 or E2)
    /// </summary>
    public class FilterDisjunction : FilterExpression
    {
        /// <summary>
        /// The left side of the OR operator
        /// </summary>
        public FilterExpression Left { get; set; }

        /// <summary>
        /// The right side of the OR operator
        /// </summary>
        public FilterExpression Right { get; set; }

        public override IEnumerable<FilterAtom> Atoms()
        {
            return Left.Atoms().Union(Right.Atoms());
        }

        /// <summary>
        /// Returns a <see cref="FilterDisjunction"/> containing of the left and right expressions
        /// </summary>
        public static FilterDisjunction Make(FilterExpression left, FilterExpression right)
        {
            if (left is null)
            {
                throw new System.ArgumentNullException(nameof(left));
            }

            if (right is null)
            {
                throw new System.ArgumentNullException(nameof(right));
            }

            return new FilterDisjunction { Left = left, Right = right };
        }
    }
}
