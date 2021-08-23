using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tellma.Repository.Common.Queryex;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Represents a select argument which is a comma separated list of non-boolean non-aggregated expressions.
    /// <para/>
    /// The syntax is anything that can be compiled by <see cref="QueryexBase"/>.
    /// For example: Account.Id,Account.Name,Value * Direction
    /// </summary>
    public class ExpressionFactSelect : IEnumerable<QueryexBase>
    {
        private readonly IEnumerable<QueryexBase> _atoms;

        /// <summary>
        /// Create an instance of <see cref="ExpressionFactSelect"/> containing the provided <see cref="QueryexBase"/>s.
        /// </summary>
        public ExpressionFactSelect(IEnumerable<QueryexBase> atoms)
        {
            _atoms = atoms.ToList() ?? throw new ArgumentNullException(nameof(atoms));
        }

        public IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            return _atoms.SelectMany(e => e.ColumnAccesses());
        }

        /// <summary>
        /// Parses a string representing of an aggregate select argument into an <see cref="ExpressionFactSelect"/>. 
        /// The fact select argument is a comma separated list of non-boolean non-aggregated expressions. 
        /// The syntax is anything that can be compiled by <see cref="QueryexBase"/>.
        /// For example: Account.Id,Account.Name,Value * Direction
        /// </summary>
        public static ExpressionFactSelect Parse(string factSelect)
        {
            if (string.IsNullOrWhiteSpace(factSelect))
            {
                return null;
            }

            var expressions = QueryexBase.Parse(factSelect);
            var aggregation = expressions.SelectMany(e => e.Aggregations()).FirstOrDefault();
            if (aggregation != null)
            {
                throw new QueryException($"Select parameter cannot contain aggregation functions like {aggregation.Name}.");
            }

            return new ExpressionFactSelect(expressions);
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable"/>
        /// </summary>
        public IEnumerator<QueryexBase> GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable"/>
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _atoms.GetEnumerator();
        }
    }
}
