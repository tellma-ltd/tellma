using System;
using System.Linq;

namespace BSharp.Data.Queries
{
    /// <summary>
    /// Represents a single entry in a comma separated expand argument
    /// For example the expand argument "Order/Customer,Amount" contains two atoms separated by a comma
    /// </summary>
    public class ExpandAtom
    {
        /// <summary>
        /// The path (split along the slashes)
        /// </summary>
        public string[] Path { get; set; }

        /// <summary>
        /// Parses a string representing a single expand path (no commas) into an <see cref="ExpandAtom"/>
        /// </summary>
        /// <param name="atom">String representing a single path (should not contain commas)</param>
        public static ExpandAtom Parse(string atom)
        {
            // item comes in the general formats:
            // - "Order/Customer"
            // - "Customer"

            if (string.IsNullOrWhiteSpace(atom))
            {
                throw new ArgumentNullException(nameof(atom));
            }

            atom = atom.Trim();

            string[] path = atom.Split('/').Select(e => e.Trim()).ToArray();
            return new ExpandAtom
            {
                Path = path
            };
        }
    }
}
