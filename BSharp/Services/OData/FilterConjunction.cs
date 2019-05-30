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

        public static FilterConjunction ParseStream(IEnumerable<string> tokenStream)
        {
            if (tokenStream == null)
            {
                throw new ArgumentNullException(nameof(tokenStream));
            }

            // find first occurrence of AND outside the brackets, and then parse both sides
            int i = tokenStream.OutsideBrackets().ToList().IndexOf("and");
            var left = tokenStream.Take(i);
            var right = tokenStream.Skip(i + 1);

            return new FilterConjunction
            {
                Left = ParseTokenStream(left),
                Right = ParseTokenStream(right),
            };
        }

        public override IEnumerable<FilterAtom> Atoms()
        {
            return Left.Atoms().Union(Right.Atoms());
        }
    }
}
