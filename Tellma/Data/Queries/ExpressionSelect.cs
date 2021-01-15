using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents a select argument which is a comma separated list of column accesses. 
    /// For example: "Line.PostingDate,Value"
    /// </summary>
    public class ExpressionSelect : IEnumerable<QueryexColumnAccess>
    {
        private readonly List<QueryexColumnAccess> _atoms;

        /// <summary>
        /// Create an instance of <see cref="ExpressionSelect"/>
        /// </summary>
        public ExpressionSelect()
        {
            _atoms = new List<QueryexColumnAccess>();
        }

        /// <summary>
        /// Create an instance of <see cref="ExpressionSelect"/> containing the provided <see cref="SelectAtom"/>s
        /// </summary>
        public ExpressionSelect(IEnumerable<QueryexColumnAccess> atoms)
        {
            _atoms = atoms.ToList() ?? throw new ArgumentNullException(nameof(atoms));
        }

        /// <summary>
        /// Add the given <see cref="SelectAtom"/> to the atoms comprising this <see cref="ExpressionSelect"/>
        /// </summary>
        public void Add(QueryexColumnAccess atom)
        {
            _atoms.Add(atom);
        }

        /// <summary>
        /// Add the given <see cref="SelectAtom"/>s to the atoms comprising this <see cref="ExpressionSelect"/>
        /// </summary>
        public void AddAll(IEnumerable<QueryexColumnAccess> atoms)
        {
            foreach (var atom in atoms)
            {
                _atoms.Add(atom);
            }
        }

        /// <summary>
        /// Returns a shallow clone of this current <see cref="ExpressionSelect"/>
        /// </summary>
        public ExpressionSelect Clone()
        {
            return new ExpressionSelect(this);
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}"/>
        /// </summary>
        public IEnumerator<QueryexColumnAccess> GetEnumerator()
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
        /// Parses a string representing a select argument into an <see cref="ExpressionSelect"/>. 
        /// The select argument is a comma separated list of column accesses. 
        /// For example: "Line.PostingDate,Value"
        /// </summary>
        public static ExpressionSelect Parse(string select)
        {
            if (string.IsNullOrWhiteSpace(select))
            {
                return null;
            }

            var expressions = QueryexBase.Parse(select);
            if (!expressions.All(e => e is QueryexColumnAccess))
            {
                throw new QueryException($"Select parameter can only contain column access expressions like this: Id,Name,CreatedBy.Name");
            }

            return new ExpressionSelect(expressions.Cast<QueryexColumnAccess>());
        }
    }
}
