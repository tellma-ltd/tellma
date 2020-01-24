using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents an aggregate select argument which is a comma separated list of paths that
    /// are optionally aggregated with an aggregation function. Where some of those are optionally
    /// postfixed with "desc" or "asc". For example: "Invoice/Customer/Name,sum(Amount) desc"
    /// </summary>
    public class AggregateSelectExpression : IEnumerable<AggregateSelectAtom>
    {
        private readonly List<AggregateSelectAtom> _atoms;

        /// <summary>
        /// Create an instance of <see cref="AggregateSelectExpression"/>
        /// </summary>
        public AggregateSelectExpression()
        {
            _atoms = new List<AggregateSelectAtom>();
        }

        /// <summary>
        /// Create an instance of <see cref="AggregateSelectExpression"/> containing the provided <see cref="AggregateSelectAtom"/>s
        /// </summary>
        public AggregateSelectExpression(IEnumerable<AggregateSelectAtom> atoms)
        {
            _atoms = atoms.ToList() ?? throw new ArgumentNullException(nameof(atoms));
        }

        /// <summary>
        /// Add the given <see cref="AggregateSelectAtom"/> to the atoms comprising this <see cref="AggregateSelectExpression"/>
        /// </summary>
        public void Add(AggregateSelectAtom atom)
        {
            _atoms.Add(atom);
        }

        /// <summary>
        /// Returns the number of atoms currently contained in this <see cref="AggregateSelectExpression"/>
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
        public IEnumerator<AggregateSelectAtom> GetEnumerator()
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
        /// Parses a string representing an aggregate select argument into an <see cref="AggregateSelectExpression"/>. 
        /// The aggregate select argument is a comma separated list of paths that are optionally aggregated with an
        /// aggregation function. Where some of those are optionally postfixed with "desc" or "asc".
        /// For example: "Invoice/Customer/Name,sum(Amount) desc"
        /// </summary>
        public static AggregateSelectExpression Parse(string select)
        {
            if (string.IsNullOrWhiteSpace(select))
            {
                return null;
            }

            var atoms = select
                .Split(',')
                .Select(e => e?.Trim())
                .Where(e => !string.IsNullOrEmpty(e))
                .Select(s => AggregateSelectAtom.Parse(s));

            return new AggregateSelectExpression(atoms);
        }
    }
}
