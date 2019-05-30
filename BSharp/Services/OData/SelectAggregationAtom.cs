using System;
using System.Linq;

namespace BSharp.Services.OData
{
    public class SelectAggregationAtom
    {
        public string[] Path { get; set; }
        public string Property { get; set; }
        public string Aggregation { get; set; }

        public static SelectAggregationAtom Parse(string atom)
        {
            // item comes in the general formats:
            // - "Order/Customer/Amount Sum"
            // - "Customer/Name"
            // - "Amount"

            if (string.IsNullOrWhiteSpace(atom))
            {
                throw new ArgumentNullException(nameof(atom));
            }

            atom = atom.Trim();


            // extract the aggregate
            string aggregation = null;
            var splitOverSpace = atom.Split(' ');
            if (splitOverSpace.Count() > 1)
            {
                aggregation = splitOverSpace.Last().Trim();
                var withoutAggregate = splitOverSpace.Take(splitOverSpace.Count() - 1);
                atom = string.Join(' ', withoutAggregate);
            }

            var (path, property) = ODataTools.ExtractPathAndProperty(atom);
            return new SelectAggregationAtom
            {
                Path = path,
                Property = property,
                Aggregation = aggregation
            };
        }
    }
}
