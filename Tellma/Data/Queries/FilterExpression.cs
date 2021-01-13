using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tellma.Entities;
using Tellma.Entities.Descriptors;

namespace Tellma.Data.Queries
{
    /// <summary>
    /// Represents a filter argument which is a full expression tree that evaluated to a boolean.
    /// For example: "(Order.Total > 1000) and (Customer.Gender = 'M')".
    /// The filter syntax is that which can be parsed into <see cref="QueryexBase"/>
    /// </summary>
    public class FilterExpression
    {
        public QueryexBase Expression { get; private set; }

        /// <summary>
        /// Parses a string representing a filter argument into a <see cref="FilterExpression"/>. 
        /// The filter argument is a full expression tree that evaluated to a boolean.
        /// For example: "(Order.Total > 1000) and (Customer.Gender = 'M')".
        /// </summary>
        public static FilterExpression Parse(string filter)
        {
            var expressions = QueryexBase.Parse(filter);
            if (expressions.Skip(1).Any())
            {
                throw new InvalidOperationException("Filter parameter must be a single expression without top level commas.");
            }

            var filterExpression = expressions.First();
            if (filterExpression == null)
            {
                return null;
            }

            // Some validation
            if (filterExpression.ContainsAggregations())
            {
                throw new InvalidOperationException("Filter expression cannot contain aggregation functions like Sum or Count.");
            }

            return new FilterExpression
            {
                Expression = expressions.First()
            };
        }

        public static FilterExpression Disjunction(FilterExpression f1, FilterExpression f2)
        {
            throw new NotImplementedException();
            //return new FilterExpression
            //{
            //    Expression = new QxBinaryOperator("or", f1.Expression, f2.Expression)
            //};
        }

        ///// <summary>
        ///// Returns all the <see cref="FilterAtom"/> in this current expression tree
        ///// </summary>
        ///// <returns></returns>
        //public abstract IEnumerable<FilterAtom> Atoms();

        ///// <summary>
        ///// Implementation of <see cref="IEnumerable{T}"/>
        ///// </summary>
        //public IEnumerator<FilterAtom> GetEnumerator()
        //{
        //    return Atoms().GetEnumerator();
        //}

        ///// <summary>
        ///// Implementation of <see cref="IEnumerable{T}"/>
        ///// </summary>
        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return Atoms().GetEnumerator();
        //}
    }

    public static class QxExtensions
    {
        public static bool ContainsAggregations(this QueryexBase @this)
        {
            return @this switch
            {
                QueryexFunction exp => exp.IsAggregation || exp.Arguments.Any(arg => arg.ContainsAggregations()),
                QueryexBinaryOperator exp => exp.Left.ContainsAggregations() || exp.Right.ContainsAggregations(),
                QueryexUnaryOperator exp => exp.Operand.ContainsAggregations(),
                _ => false,
            };
        }
    }
}
