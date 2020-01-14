using System;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Data.Queries
{
    /// <summary>
    /// Represents a single entry in a <see cref="FilterExpression"/>. A <see cref="FilterExpression"/> is a tree of ANDs (conjunctions), ORs (disjunctions)
    /// and NOTs (negations) with brackets to override the default precedence of these operators.
    /// For example the filter expression "(Order/Total gt 1000) and (Customer/Gender eq 'M')" contains two atoms in a conjunction
    /// </summary>
    public class FilterAtom : FilterExpression
    {
        /// <summary>
        /// The path leading up to the property subject of the <see cref="FilterAtom"/>
        /// </summary>
        public string[] Path { get; set; }

        /// <summary>
        /// The property subject of the <see cref="FilterAtom"/>, this is the first operand of the atom operator <see cref="Op"/>
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// An optional function applied to the path, must be one of the <see cref="Functions"/>
        /// </summary>
        public string Function { get; set; }

        /// <summary>
        /// The binary operator of the atom, e.g. "eq" for equals or "gt" for greather than, the complete list of operators can be found in <see cref="Ops"/>
        /// </summary>
        public string Op { get; set; }

        /// <summary>
        /// The second operand of the atom operator <see cref="Op"/>
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Parses a string representing a single filter atom (no ANDs, ORs or NOTs) into an <see cref="FilterAtom"/>
        /// </summary>
        /// <param name="atom">String representing a simple logical expression (should not contain ANDs, ORs or NOTs)</param>
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
                var (function, path, property) = QueryTools.ExtractFunctionPathAndProperty(pieces[0]);

                // (B) Parse the value (e.g. "'Huntington Rd.'")
                var value = string.Join(" ", pieces.Skip(2));

                // (C) parse the operator (e.g. "eq")
                var op = pieces[1];

                return new FilterAtom
                {
                    Path = path,
                    Property = property,
                    Function = function,
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
