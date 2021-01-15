using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents an expand argument which is a comma separated list of paths. Where some paths are optionally
    /// postfixed with "desc" or "asc". For example: "Invoice/Customer,Amount"
    /// </summary>
    public class ExpressionAggregateOrderBy : IEnumerable<QueryexBase>
    {
        private readonly IEnumerable<QueryexBase> _atoms;

        /// <summary>
        /// Create an instance of <see cref="ExpressionAggregateOrderBy"/> containing the provided <see cref="OrderByAtom"/>s
        /// </summary>
        public ExpressionAggregateOrderBy(IEnumerable<QueryexBase> atoms)
        {
            _atoms = atoms ?? throw new ArgumentNullException(nameof(atoms));
        }

        public IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            return _atoms.SelectMany(e => e.ColumnAccesses());
        }

        /// <summary>
        /// Parses a string representing an order by argument into an <see cref="ExpressionAggregateOrderBy"/>. 
        /// The orderby argument is a comma separated list of paths, where some paths are optionally
        /// postfixed with "desc" or "asc". For example "Invoice/Customer,Amount"
        /// </summary>
        public static ExpressionAggregateOrderBy Parse(string orderby)
        {
            if (string.IsNullOrWhiteSpace(orderby))
            {
                return null;
            }

            var expressions = QueryexBase.Parse(orderby, expectDirKeywords: true);
            return new ExpressionAggregateOrderBy(expressions);
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
