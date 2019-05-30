using System;

namespace BSharp.Services.OData
{
    public class OrderByAtom
    {
        public string[] Path { get; set; }
        public string Property { get; set; }
        public bool Desc { get; set; }

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
                atom = atom.Substring(0, atom.Length - 4).Trim();
            }

            // extract the path and the property
            var (path, property) = ODataTools.ExtractPathAndProperty(atom);
            return new OrderByAtom
            {
                Path = path,
                Property = property,
                Desc = desc
            };
        }
    }
}
