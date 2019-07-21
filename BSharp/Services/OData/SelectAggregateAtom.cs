using System;
using System.Linq;

namespace BSharp.Services.OData
{
    public class SelectAggregateAtom
    {
        public string[] Path { get; set; }

        public string Property { get; set; }

        public string Aggregation { get; set; }

        public string OrderDirection { get; set; } // null, asc, desc

        public bool IsDimension { get => !IsMeasure; }
        public bool IsMeasure { get => !string.IsNullOrWhiteSpace(Aggregation); }

        public static SelectAggregateAtom Parse(string atom)
        {
            // item comes in the general formats:
            // - "Customer/Name asc"
            // - "sum(Amount) desc"

            if (string.IsNullOrWhiteSpace(atom))
            {
                throw new ArgumentNullException(nameof(atom));
            }

            atom = atom.Trim();

            // Extract the aggregation function (sum, count etc...) if any
            string[] aggregationKeywords = Aggregations.All;
            string aggregation = aggregationKeywords.FirstOrDefault(ag =>
                atom.ToLower().StartsWith(ag) &&
                atom.Substring(ag.Length).Trim().StartsWith("("));
            if (aggregation != null)
            {
                atom = atom.Substring(aggregation.Length);
                atom = atom.TrimStart();
                atom = atom.Substring(1); // to remove the bracket
            }

            // Extract the order by direction if any
            string[] orderDirKeywords = { " desc", " asc" };
            string orderDirection = orderDirKeywords.FirstOrDefault(ob =>
                atom.ToLower().EndsWith(ob) &&
                atom.Substring(ob.Length).Trim().Length > 0 &&
                !atom.Substring(ob.Length).Trim().EndsWith("/"));
            if (orderDirection != null)
            {
                atom = atom.Remove(atom.Length - orderDirection.Length).Trim();
                orderDirection = orderDirection.Trim();
            }

            if (aggregation != null && atom.EndsWith(")"))
            {
                atom = atom.Remove(atom.Length - 1).Trim();
            }


            // Extrat path and property
            var (path, property) = ODataTools.ExtractPathAndProperty(atom);

            // Return the result
            return new SelectAggregateAtom
            {
                Path = path,
                Property = property,
                Aggregation = aggregation,
                OrderDirection = orderDirection
            };
        }
    }
}
