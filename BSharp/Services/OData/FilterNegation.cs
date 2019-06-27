using System;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Services.OData
{
    public class FilterNegation : FilterExpression
    {
        public FilterExpression Inner { get; set; }

        public override IEnumerable<FilterAtom> Atoms()
        {
            return Inner.Atoms();
        }
    }
}
