using System;
using System.Linq;

namespace BSharp.Services.OData
{
    public class ExpandAtom
    {
        public string[] Path { get; set; }

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
