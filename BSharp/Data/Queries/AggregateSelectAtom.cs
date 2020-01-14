using System;
using System.Linq;

namespace BSharp.Data.Queries
{
    /// <summary>
    /// Represents a single entry in a comma separated aggregate select argument
    /// For example the aggregate select argument "Invoice/Customer/Name,sum(Amount) desc" contains two atoms separated by a comma
    /// </summary>
    public class AggregateSelectAtom
    {
        /// <summary>
        /// The path component of the atom (split along the slashes)
        /// </summary>
        public string[] Path { get; set; }

        /// <summary>
        /// The property subject of the <see cref="AggregateSelectAtom"/>
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// An optional function applied directly to the path
        /// </summary>
        public string Function { get; set; }

        /// <summary>
        /// An optional aggregation function applied on the <see cref="Property"/> if any,
        /// the aggregation functions are found in <see cref="Aggregations"/>
        /// </summary>
        public string Aggregation { get; set; }

        /// <summary>
        /// An optional order by direction, either "asc", "desc" or null
        /// </summary>
        public string OrderDirection { get; set; }

        /// <summary>
        /// Returns true if the atom does not have an <see cref="Aggregation"/>
        /// </summary>
        public bool IsDimension { get => !IsMeasure; }

        /// <summary>
        /// Returns true if the atom has an <see cref="Aggregation"/>
        /// </summary>
        public bool IsMeasure { get => !string.IsNullOrWhiteSpace(Aggregation); }

        /// <summary>
        /// Parses a string representing a single atom (no commas) into an <see cref="AggregateSelectAtom"/>
        /// </summary>
        /// <param name="atom">String representing a single atom (should not contain commas)</param>
        public static AggregateSelectAtom Parse(string atom)
        {
            // item comes in the general formats:
            // - "Customer/Name asc"
            // - "sum(Amount) desc"
            // - "sum(day(Customer/DateOfBirth)) desc

            if (string.IsNullOrWhiteSpace(atom))
            {
                throw new ArgumentNullException(nameof(atom));
            }

            atom = atom.Trim();

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

                if (atom.EndsWith(")"))
                {
                    atom = atom.Remove(atom.Length - 1).Trim();
                }
            }

            // Extrat path, property and function
            var (function, path, property) = QueryTools.ExtractFunctionPathAndProperty(atom);

            // Return the result
            return new AggregateSelectAtom
            {
                Path = path,
                Property = property,
                Function = function,
                Aggregation = aggregation,
                OrderDirection = orderDirection
            };
        }
    }
}
