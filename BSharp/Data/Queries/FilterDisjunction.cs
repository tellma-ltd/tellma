using System.Collections.Generic;
using System.Linq;

namespace BSharp.Data.Queries
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
    }
}
