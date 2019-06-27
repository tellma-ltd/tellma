using BSharp.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Services.OData
{
    public class FilterConjunction : FilterExpression
    {
        public FilterExpression Left { get; set; }
        public FilterExpression Right { get; set; }

        public override IEnumerable<FilterAtom> Atoms()
        {
            return Left.Atoms().Union(Right.Atoms());
        }
    }
}
