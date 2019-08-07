using System;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Data.Queries
{
    /// <summary>
    /// Represents a negation of a <see cref="FilterExpression"/> (not(E))
    /// </summary>
    public class FilterNegation : FilterExpression
    {
        /// <summary>
        /// The negated <see cref="FilterExpression"/>
        /// </summary>
        public FilterExpression Inner { get; set; }

        public override IEnumerable<FilterAtom> Atoms()
        {
            return Inner.Atoms();
        }
    }
}
