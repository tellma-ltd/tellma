using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tellma.Repository.Common.Queryex;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Represents an expand argument which is a comma separated list of column accesses that terminate with nav properties. 
    /// <para/>
    /// For example: "Participant,Lines.Entries"
    /// </summary>
    public class ExpressionExpand : IEnumerable<QueryexColumnAccess>
    {
        private readonly List<QueryexColumnAccess> _atoms;

        /// <summary>
        /// Create an instance of <see cref="ExpressionExpand"/>
        /// </summary>
        public ExpressionExpand()
        {
            _atoms = new List<QueryexColumnAccess>();
        }

        /// <summary>
        /// Create an instance of <see cref="ExpressionExpand"/> containing the provided <see cref="QueryexColumnAccess"/>s.
        /// </summary>
        /// <param name="columnAccesses"></param>
        public ExpressionExpand(IEnumerable<QueryexColumnAccess> columnAccesses)
        {
            _atoms = columnAccesses.ToList() ?? throw new ArgumentNullException(nameof(columnAccesses));
        }

        /// <summary>
        /// Add the given <see cref="ExpandAtom"/> to the atoms comprising this expression.
        /// </summary>
        public void Add(QueryexColumnAccess atom)
        {
            _atoms.Add(atom);
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}"/>.
        /// </summary>
        public IEnumerator<QueryexColumnAccess> GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}"/>.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        /// <summary>
        /// Parses a string representing an expand argument into an <see cref="ExpressionExpand"/>. 
        /// The expand argument is a comma separated list of column accesses.
        /// <para/>
        /// For example "Participant,Lines.Entries"
        /// </summary>
        public static ExpressionExpand Parse(string expand)
        {
            if (string.IsNullOrWhiteSpace(expand))
            {
                return null;
            }

            var expressions = QueryexBase.Parse(expand, expectPathsOnly: true);
            var nonColumnAccess = expressions.FirstOrDefault(e => !(e is QueryexColumnAccess));
            if (nonColumnAccess != null)
            {
                throw new QueryException($"Expand parameter cannot contain an expression like {nonColumnAccess}, only column access expressions are allowed.");
            }

            return new ExpressionExpand(expressions.Cast<QueryexColumnAccess>());
        }

        /// <summary>
        /// Returns an empty <see cref="ExpressionExpand"/>, containing no atoms, this is the equivalent of a null expand
        /// </summary>
        public static ExpressionExpand Empty
        {
            get
            {
                return new ExpressionExpand(new List<QueryexColumnAccess>());
            }
        }

        /// <summary>
        /// Returns a <see cref="ExpressionExpand"/> containing one atom of an empty path.
        /// Useful for some algorithms that assume the expand argument contains an implicit empty path
        /// </summary>
        public static ExpressionExpand RootSingleton
        {
            get
            {
                return new ExpressionExpand(new List<QueryexColumnAccess> { new QueryexColumnAccess(Array.Empty<string>(), null) });
            }
        }
    }
}
