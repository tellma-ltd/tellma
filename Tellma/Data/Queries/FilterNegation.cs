//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Tellma.Data.Queries
//{
//    /// <summary>
//    /// Represents a negation of a <see cref="FilterExpression"/> (not(E))
//    /// </summary>
//    public class FilterNegation : FilterExpression
//    {
//        /// <summary>
//        /// The negated <see cref="FilterExpression"/>
//        /// </summary>
//        public FilterExpression Inner { get; set; }

//        public override IEnumerable<FilterAtom> Atoms()
//        {
//            return Inner.Atoms();
//        }

//        /// <summary>
//        /// Returns a <see cref="FilterNegation"/> containing of the inner expression
//        /// </summary>
//        public static FilterNegation Make(FilterExpression inner)
//        {
//            return new FilterNegation { Inner = inner };
//        }
//    }
//}
