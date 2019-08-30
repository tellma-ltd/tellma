using System;

namespace BSharp.Data.Queries
{
    /// <summary>
    /// Represents a single entry in a comma separated select argument
    /// For example the select argument "Invoice/Customer/Name,Amount" contains two atoms separated by a comma
    /// </summary>
    public class SelectAtom
    {
        /// <summary>
        /// The path component of the atom (split along the slashes)
        /// </summary>
        public string[] Path { get; set; }

        /// <summary>
        /// The property subject of the <see cref="SelectAtom"/>
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// Parses a string representing a single atom (no commas) into an <see cref="AggregateSelectAtom"/>
        /// </summary>
        /// <param name="atom">String representing a single atom (should not contain commas)</param>
        public static SelectAtom Parse(string atom)
        {
            // item comes in the general formats:
            // - "Customer/Name"
            // - "Amount"

            if (string.IsNullOrWhiteSpace(atom))
            {
                throw new ArgumentNullException(nameof(atom));
            }

            atom = atom.Trim();

            var (path, property) = QueryTools.ExtractPathAndProperty(atom);
            return new SelectAtom
            {
                Path = path,
                Property = property
            };
        }
    }
}
