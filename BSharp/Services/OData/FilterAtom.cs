using System;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Services.OData
{
    public class FilterAtom : FilterExpression
    {
        public string[] Path { get; set; }
        public string Property { get; set; }
        public string Op { get; set; }
        public string Value { get; set; }

        public new static FilterAtom Parse(string atom)
        {
            if (string.IsNullOrWhiteSpace(atom))
            {
                throw new ArgumentNullException(nameof(atom));
            }

            var pieces = atom.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (pieces.Length < 3)
            {
                throw new InvalidOperationException($"One of the atomic expressions ({atom}) does not have the valid form: 'Path op Value'");
            }
            else
            {
                // (A) Parse the member access path (e.g. "Address/Street")
                var (path, property) = ODataTools.ExtractPathAndProperty(pieces[0]);

                // (B) Parse the value (e.g. "'Huntington Rd.'")
                var value = string.Join(" ", pieces.Skip(2));

                // (C) parse the operator (e.g. "eq")
                var op = pieces[1];

                return new FilterAtom
                {
                    Path = path,
                    Property = property,
                    Op = op,
                    Value = value
                };
            }
        }

        public override IEnumerable<FilterAtom> Atoms()
        {
            yield return this;
        }
    }
}
