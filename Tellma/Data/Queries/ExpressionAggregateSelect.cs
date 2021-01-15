using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents an aggregate select argument which is a comma separated list of non-boolean expressions.
    /// The syntax is anything that can be compiled by <see cref="QueryexBase"/>.
    /// Valid <see cref="ExpressionAggregateSelect"/> expressions are two types:
    /// - Dimensions: Do not contain any aggregation functions.
    /// - Measures: Contain at least one aggregation function in the expression tree.
    /// If an expression is a measures: all column accesses in that expression must be contained within an aggregation function.
    /// For example: Account.Id,Account.Name,Sum(Value * Direction)
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
        /// Parses a string representing of an aggregate select argument into an <see cref="ExpressionAggregateSelect"/>. 
        /// The aggregate select argument is a comma separated list of non-boolean expressions. 
        /// The syntax is anything that can be compiled by <see cref="QueryexBase"/>.
        /// Valid <see cref="ExpressionAggregateSelect"/> expressions are two types:
        /// - Dimensions: Do not contain any aggregation functions.
        /// - Measures: Contain at least one aggregation function in the expression tree.
        /// If an expression is a measures: all column accesses in that expression must be contained within an aggregation function.
        /// For example: Account.Id,Account.Name,Sum(Value * Direction)
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
