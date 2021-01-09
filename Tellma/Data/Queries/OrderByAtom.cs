using System;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents a single entry in a comma separated orderby argument
    /// For example the orderby argument "Invoice/Customer,Amount" contains two atoms separated by a comma
    /// </summary>
    public class OrderByAtom
    {
        /// <summary>
        /// The path component of the atom (split along the slashes)
        /// </summary>
        public string[] Path { get; set; }

        /// <summary>
        /// The property subject to the <see cref="OrderByAtom"/>
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// True if the order by is descending, false otherwise
        /// </summary>
        public bool Desc { get; set; }

        /// <summary>
        /// Parses a string representing a single orderby atom (no commas) into an <see cref="OrderByAtom"/>
        /// </summary>
        /// <param name="atom">String representing a single orderby atom (should not contain commas)</param>
        public static OrderByAtom Parse(string atom)
        {
            // item comes in the general formats:
            // - "Order/Customer/Name desc"
            // - "Customer/DateOfBirth"

            if (string.IsNullOrWhiteSpace(atom))
            {
                throw new ArgumentNullException(nameof(atom));
            }

            atom = atom.Trim();

            // Extract desc
            bool desc = false;
            if (atom.ToLower().EndsWith("desc"))
            {
                desc = true;
                atom = atom[0..^4].Trim();
            }

            // extract the path and the property
            var (path, property) = QueryTools.ExtractPathAndProperty(atom);
            return new OrderByAtom
            {
                Path = path,
                Property = property,
                Desc = desc
            };
        }
    }
}
