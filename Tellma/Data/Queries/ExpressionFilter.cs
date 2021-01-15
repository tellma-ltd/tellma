using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents a filter argument which is a full expression tree that evaluates to a boolean.
    /// For example: "(Value * Direction > 1000) and (Participant.Lookup1.Code = 'M')".
    /// The filter syntax is that which can be parsed into <see cref="QueryexBase"/>
    /// </summary>
    public class ExpressionFilter
    {
        public QueryexBase Expression { get; }

        public ExpressionFilter(QueryexBase expression)
        {
            Expression = expression;

            Validate(Expression);
        }

        /// <summary>
        /// Parses a string representing a filter argument into a <see cref="ExpressionFilter"/>. 
        /// The filter argument is a full expression tree that evaluates to a boolean.
        /// For example: "(Value * Direction > 1000) and (Participant.Lookup1.Code = 'M')".
        /// </summary>
        public static ExpressionFilter Parse(string filter)
        {
            var expressions = QueryexBase.Parse(filter);

            // Can only contain 0 or 1 atoms
            if (expressions.Skip(1).Any())
            {
                throw new InvalidOperationException("Filter parameter must contain a single expression without top level commas.");
            }

            var filterExpression = expressions.First();
            if (filterExpression == null)
            {
                return null;
            }

            return new ExpressionFilter(expressions.First());
        }

        public IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            return Expression.ColumnAccesses();
        }

        public static ExpressionFilter Disjunction(ExpressionFilter filter1, ExpressionFilter filter2)
        {
            var disjunction = new QueryexBinaryOperator("or", filter1.Expression, filter2.Expression);
            return new ExpressionFilter(disjunction);
        }

        public static ExpressionFilter Conjunction(ExpressionFilter filter1, ExpressionFilter filter2)
        {
            var conjunction = new QueryexBinaryOperator("and", filter1.Expression, filter2.Expression);
            return new ExpressionFilter(conjunction);
        }

        private static void Validate(QueryexBase exp)
        {
            // Cannot contain aggregations
            if (exp.ContainsAggregations)
            {
                throw new InvalidOperationException("Filter expression cannot contain aggregation functions like Sum or Count.");
            }
        }
    }
}
