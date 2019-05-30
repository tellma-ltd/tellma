using System;

namespace BSharp.Services.OData
{
    public class GroupByAtom
    {
        public string[] Path { get; set; }
        public string Property { get; set; }

        public static GroupByAtom Parse(string atom)
        {
            // item comes in the general formats:
            // - "Order/Customer/Name"
            // - "DateOfBirth"

            if (string.IsNullOrWhiteSpace(atom))
            {
                throw new ArgumentNullException(nameof(atom));
            }

            atom = atom.Trim();

            // extract the path and the property
            var (path, property) = ODataTools.ExtractPathAndProperty(atom);
            return new GroupByAtom
            {
                Path = path,
                Property = property
            };
        }
    }
}
