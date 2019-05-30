using System;
using System.Linq;

namespace BSharp.Services.OData
{
    public class SelectAtom
    {
        public string[] Path { get; set; }
        public string Property { get; set; }

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

            var (path, property) = ODataTools.ExtractPathAndProperty(atom);
            return new SelectAtom
            {
                Path = path,
                Property = property
            };
        }
    }
}
