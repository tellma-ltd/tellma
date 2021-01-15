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
    public class ExpressionAggregateSelect : IEnumerable<QueryexBase>
    {
        private readonly IEnumerable<QueryexBase> _atoms;

        /// <summary>
        /// Create an instance of <see cref="ExpressionAggregateSelect"/>
        /// </summary>
        public ExpressionAggregateSelect()
        {
            _atoms = new List<QueryexBase>();
        }

        /// <summary>
        /// Create an instance of <see cref="ExpressionAggregateSelect"/> containing the provided <see cref="QueryexBase"/>s
        /// </summary>
        public ExpressionAggregateSelect(IEnumerable<QueryexBase> atoms)
        {
            _atoms = atoms.ToList() ?? throw new ArgumentNullException(nameof(atoms));
        }

        public IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            return _atoms.SelectMany(e => e.ColumnAccesses());
        }

        /// <summary>
        /// Parses a string representing an aggregate select argument into an <see cref="ExpressionAggregateSelect"/>. 
        /// The aggregate select argument is a comma separated list of paths that are optionally aggregated with an
        /// aggregation function. Where some of those are optionally postfixed with "desc" or "asc".
        /// For example: "Invoice/Customer/Name,sum(Amount) desc"
        /// </summary>
        public static ExpressionAggregateSelect Parse(string aggregateSelect)
        {
            if (string.IsNullOrWhiteSpace(aggregateSelect))
            {
                return null;
            }

            var expressions = QueryexBase.Parse(aggregateSelect);
            return new ExpressionAggregateSelect(expressions);
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
