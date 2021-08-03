using System;
using System.Collections.Generic;
using System.Linq;
using Tellma.Repository.Common.Queryex;

namespace Tellma.Repository.Common
{
    /// <summary>
    /// Represents a having argument which is a boolean expression.
    /// <para/>
    /// The syntax is anything that can be compiled by <see cref="QueryexBase"/> into a single boolean expression where every column access is contained within an aggregation.
    /// For example: "SUM(Value * Direction) > 1000 and Max(Participant.Lookup1.Code) = 'M'".
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
        /// The filter argument is a boolean expression. The syntax is anything that can be compiled by
        /// <see cref="QueryexBase"/> into a single boolean expression where every column access is contained within an aggregation.
        /// For example: "SUM(Value * Direction) > 1000 and Max(Participant.Lookup1.Code) = 'M'".
        /// </summary>
        public static ExpressionHaving Parse(string having)
        {
            var expressions = QueryexBase.Parse(having);

            // Can only contain 0 or 1 atoms
            if (expressions.Skip(1).Any())
            {
                throw new InvalidOperationException("Having parameter must contain a single expression without top level commas.");
            }

            var havingExpression = expressions.FirstOrDefault();
            if (havingExpression == null)
            {
                return null;
            }

            return new ExpressionHaving(havingExpression);
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
