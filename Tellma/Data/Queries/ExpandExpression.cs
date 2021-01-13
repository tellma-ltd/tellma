using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents an expand argument which is a comma separated list of paths. For example: "Participant,Lines.Entries"
    /// </summary>
    public class ExpandExpression : IEnumerable<ExpandAtom>
    {
        private readonly List<ExpandAtom> _atoms;

        /// <summary>
        /// Create an instance of <see cref="ExpandExpression"/>
        /// </summary>
        public ExpandExpression()
        {
            _atoms = new List<ExpandAtom>();
        }

        /// <summary>
        /// Create an instance of <see cref="ExpandExpression"/> containing the provided <see cref="ExpandAtom"/>s
        /// </summary>
        /// <param name="atoms"></param>
        public ExpandExpression(IEnumerable<ExpandAtom> atoms)
        {
            _atoms = atoms.ToList() ?? throw new ArgumentNullException(nameof(atoms));
        }

        /// <summary>
        /// Add the given <see cref="ExpandAtom"/> to the atoms comprising this expression
        /// </summary>
        public void Add(ExpandAtom atom)
        {
            _atoms.Add(atom);
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}"/>
        /// </summary>
        public IEnumerator<ExpandAtom> GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}"/>
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        /// <summary>
        /// Parses a string representing an expand argument into an <see cref="ExpandExpression"/>. 
        /// The expand argument is a comma separated list of paths, for example "Participant,Lines.Entries"
        /// </summary>
        public static ExpandExpression Parse(string expand)
        {
            if (string.IsNullOrWhiteSpace(expand))
            {
                return null;
            }

            var atoms = QueryexBase.Parse(expand)
                .Where(exp => exp != null)
                .Select(exp => ExpandAtom.FromExpression(exp));

            return new ExpandExpression(atoms);
        }

        /// <summary>
        /// Returns an empty <see cref="ExpandExpression"/>, containing no atoms, this is the equivalent of a null expand
        /// </summary>
        public static ExpandExpression Empty
        {
            get
            {
                return new ExpandExpression(new List<ExpandAtom>());
            }
        }

        /// <summary>
        /// Returns a <see cref="ExpandExpression"/> containing one atom of an empty path.
        /// Useful for some algorithms that assume the expand argument contains an implicit empty path
        /// </summary>
        public static ExpandExpression RootSingleton
        {
            get
            {
                return new ExpandExpression(new List<ExpandAtom> { new ExpandAtom { Path = new string[0] } });
            }
        }
    }
}
