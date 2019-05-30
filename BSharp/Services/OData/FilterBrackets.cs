using System;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Services.OData
{
    public class FilterBrackets : FilterExpression
    {
        public FilterExpression Inner { get; set; }

        public static FilterBrackets ParseStream(IEnumerable<string> tokenStream)
        {
            if (tokenStream == null)
            {
                throw new ArgumentNullException(nameof(tokenStream));
            }

            return new FilterBrackets
            {
                Inner = ParseTokenStream(tokenStream.Skip(1).Take(tokenStream.Count() - 2))
            };
        }

        public override IEnumerable<FilterAtom> Atoms()
        {
            return Inner.Atoms();
        }
    }
}
