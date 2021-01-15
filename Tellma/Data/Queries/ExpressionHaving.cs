using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents a having argument which is a full expression tree that evaluates to a boolean.
    /// For example: "SUM(Value * Direction) > 1000 and Max(Participant.Lookup1.Code) = 'M'".
    /// The having syntax is that which can be parsed into <see cref="QueryexBase"/>, but every column access must be contained within an aggregation.
    /// </summary>
    public class ExpressionHaving
    {
        public QueryexBase Expression { get; }

        public ExpressionHaving(QueryexBase expression)
        {
            Expression = expression;

            Validate(Expression);
        }

        /// <summary>
        /// Parses a string representing a filter argument into a <see cref="ExpressionHaving"/>. 
        /// The filter argument is a full expression tree that evaluated to a boolean.
        /// For example: "(Order.Total > 1000) and (Customer.Gender = 'M')".
        /// </summary>
        public static ExpressionHaving Parse(string filter)
        {
            var expressions = QueryexBase.Parse(filter);

            // Can only contain 0 or 1 atoms
            if (expressions.Skip(1).Any())
            {
                throw new InvalidOperationException("Having parameter must contain a single expression without top level commas.");
            }

            var filterExpression = expressions.First();
            if (filterExpression == null)
            {
                return null;
            }

            return new ExpressionHaving(expressions.First());
        }

        public IEnumerable<QueryexColumnAccess> ColumnAccesses()
        {
            return Expression.ColumnAccesses();
        }

        private static void Validate(QueryexBase exp)
        {                // This is a measure, every column access must be surrounded by an aggregation function
            var exposedColumnAccess = exp.UnaggregatedColumnAccesses().FirstOrDefault();
            if (exposedColumnAccess != null)
            {
                throw new QueryException($"Having parameter contains a column access {exposedColumnAccess} that is not contained within an aggregation.");
            }
        }
    }
}
