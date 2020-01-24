using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents a select argument which is a comma separated list of paths. For example: "Invoice/Customer/Name,Amount"
    /// </summary>
    public class SelectExpression : IEnumerable<SelectAtom>
    {
        private List<SelectAtom> _atoms;

        /// <summary>
        /// Create an instance of <see cref="SelectExpression"/>
        /// </summary>
        public SelectExpression()
        {
            _atoms = new List<SelectAtom>();
        }

        /// <summary>
        /// Create an instance of <see cref="SelectExpression"/> containing the provided <see cref="SelectAtom"/>s
        /// </summary>
        public SelectExpression(IEnumerable<SelectAtom> atoms)
        {
            _atoms = atoms.ToList() ?? throw new ArgumentNullException(nameof(atoms));
        }

        /// <summary>
        /// Add the given <see cref="SelectAtom"/> to the atoms comprising this <see cref="SelectExpression"/>
        /// </summary>
        public void Add(SelectAtom atom)
        {
            _atoms.Add(atom);
        }

        /// <summary>
        /// Returns the number of atoms currently contained in this <see cref="SelectExpression"/>
        /// </summary>
        public int Count
        {
            get
            {
                return _atoms.Count;
            }
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}"/>
        /// </summary>
        public IEnumerator<SelectAtom> GetEnumerator()
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
        /// Parses a string representing a select argument into an <see cref="SelectExpression"/>. 
        /// The aggregate select argument is a comma separated list of paths. For example: "Invoice/Customer/Name,Amount"
        /// </summary>
        public static SelectExpression Parse(string select)
        {
            if (string.IsNullOrWhiteSpace(select))
            {
                return null;
            }

            var atoms = select
                .Split(',')
                .Select(e => e?.Trim())
                .Where(e => !string.IsNullOrEmpty(e))
                .Select(s => SelectAtom.Parse(s));

            return new SelectExpression(atoms);
        }
    }
}
