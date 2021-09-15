using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Utilities.Common;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Represents a binary operator, e.g. A && B.
    /// </summary>
    public abstract class TemplexBinaryOperator : TemplexBase
    {
        private string _op;

        /// <summary>
        /// The left operand.
        /// </summary>
        public TemplexBase Left { get; private set; }

        /// <summary>
        /// The right operand.
        /// </summary>
        public TemplexBase Right { get; private set; }

        public override async IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            await foreach (var select in Left.ComputeSelect(ctx))
            {
                yield return select;
            }

            await foreach (var select in Right.ComputeSelect(ctx))
            {
                yield return select;
            }
        }

        public override IAsyncEnumerable<Path> ComputePaths(EvaluationContext ctx)
        {
            return AsyncUtil.Empty<Path>();
        }

        public override string ToString()
        {
            return $"({Left} {_op} {Right})";
        }

        /// <summary>
        /// Creates a new <see cref="TemplexBinaryOperator"/> based on the operator.
        /// </summary>
        public static TemplexBinaryOperator Make(string op, TemplexBase left, TemplexBase right)
        {
            TemplexBinaryOperator result = op switch
            {
                // Arithmetic
                "+" => new Addition(),
                "-" => new Subtraction(),
                "*" => new Multiplication(),
                "/" => new Division(),
                "%" => new Modulo(),

                // Logical
                "&&" => new Conjunction(),
                "||" => new Disjunction(),

                // Comparison
                "!=" => new NotEqual(),
                "<>" => new NotEqual(),
                "<=" => new LessThanOrEqual(),
                ">=" => new GreaterThanOrEqual(),
                "<" => new LessThan(),
                ">" => new GreaterThan(),
                "=" => new AreEqual(),

                // Error
                _ => throw new TemplateException($"Unkown binary operator {op}.")
            };

            result.Left = left;
            result.Right = right;
            result._op = op;

            return result;
        }

        /// <summary>
        /// Returns true for operators that can be used.
        /// </summary>
        /// <remarks>
        /// Note: The function does not check if it's a valid operator in the first place.
        /// </remarks>
        public static bool ValidOperator(string op)
        {
            return op.ToLower() switch
            {
                "!" => false,
                _ => true,
            };
        }

        #region Arithmetic

        /// <summary>
        /// Represents an infix arithmetic operator, e.g. A * B.
        /// </summary>
        public abstract class ArithmeticOperator : TemplexBinaryOperator
        {
            protected abstract object Compute(object left, object right, Type commonType);

            public override async Task<object> Evaluate(EvaluationContext ctx)
            {
                object left = await Left.Evaluate(ctx);
                object right = await Right.Evaluate(ctx);

                return EvaluateImpl(left, right, ctx);
            }

            protected virtual object EvaluateImpl(object left, object right, EvaluationContext ctx)
            {
                // Numbers
                var commonType = NumericUtil.CommonNumericType(left, right);
                if (commonType == null)
                {
                    if (!NumericUtil.IsNumericType(left))
                    {
                        throw new TemplateException($"Arithmetic operator could not be applied. Operand ({Left}) does not evaluate to a numeric value.");
                    }
                    else if (!NumericUtil.IsNumericType(right))
                    {
                        throw new TemplateException($"Arithmetic operator could not be applied. Operand ({Right}) does not evaluate to a numeric value.");
                    }
                    else
                    {
                        throw new InvalidOperationException($"Arithmetic operator could not be applied."); // Developer mistake
                    }
                }

                return Compute(left, right, commonType);
            }
        }

        /// <summary>
        /// Represents an infix addition operator, e.g. A + B.
        /// </summary>
        public class Addition : ArithmeticOperator
        {
            protected override object EvaluateImpl(object left, object right, EvaluationContext ctx)
            {
                // String concatenation
                if (left is string || left is char || right is string || right is char)
                {
                    return ToString(left) + ToString(right);
                }
                else
                {
                    return base.EvaluateImpl(left, right, ctx);
                }
            }

            protected override object Compute(object left, object right, Type commonType)
                => NumericUtil.Add(left, right, commonType);
        }

        /// <summary>
        /// Represents an infix subtraction operator, e.g. A - B.
        /// </summary>
        public class Subtraction : ArithmeticOperator
        {
            protected override object Compute(object left, object right, Type commonType)
                => NumericUtil.Subtract(left, right, commonType);
        }

        /// <summary>
        /// Represents an infix multiplication operator, e.g. A * B.
        /// </summary>
        public class Multiplication : ArithmeticOperator
        {
            protected override object Compute(object left, object right, Type commonType)
                => NumericUtil.Multiply(left, right, commonType);
        }

        /// <summary>
        /// Represents an infix division operator, e.g. A / B.
        /// </summary>
        public class Division : ArithmeticOperator
        {
            protected override object Compute(object left, object right, Type commonType)
                => NumericUtil.Divide(left, right, commonType, Right);
        }

        /// <summary>
        /// Represents an infix modulo operator, e.g. A % B.
        /// </summary>
        public class Modulo : ArithmeticOperator
        {
            protected override object Compute(object left, object right, Type commonType)
                => NumericUtil.Modulo(left, right, commonType, Right);
        }

        #endregion

        #region Logical

        /// <summary>
        /// Represents an infix logical operator, e.g. A && B.
        /// </summary>
        public abstract class LogicalOperator : TemplexBinaryOperator
        {
            protected abstract bool Compute(bool left, bool right);

            public override async Task<object> Evaluate(EvaluationContext ctx)
            {
                var left = await Left.Evaluate(ctx) ?? false; // Null is treated as false
                var right = await Right.Evaluate(ctx) ?? false; // Null is treated as false

                if (left is not bool boolLeft)
                {
                    throw new TemplateException($"Logical operator could not be applied. The expression ({Left}) does not evaluate to a boolean value.");
                }
                else if (right is not bool boolRight)
                {
                    throw new TemplateException($"Logical operator could not be applied. The expression ({Right}) does not evaluate to a boolean value.");
                }
                else
                {
                    return Compute(boolLeft, boolRight);
                }
            }
        }

        /// <summary>
        /// Represents an infix AND operator, e.g. A && B.
        /// </summary>
        public class Conjunction : LogicalOperator
        {
            protected override bool Compute(bool left, bool right) => left && right;
        }

        /// <summary>
        /// Represents an infix OR operator, e.g. A || B.
        /// </summary>
        public class Disjunction : LogicalOperator
        {
            protected override bool Compute(bool left, bool right) => left || right;
        }

        #endregion

        #region Comparison

        /// <summary>
        /// Represents an infix comparison operator, e.g. A &gt; B.
        /// </summary>
        public abstract class ComparisonOperator : TemplexBinaryOperator
        {
            protected abstract bool ComputeForComparables(IComparable left, IComparable right);

            protected virtual bool ComputeForObjects(object leftObj, object rightObj)
            {
                if (!NumericUtil.IsNumericType(leftObj))
                {
                    throw new TemplateException($"Comparison operator could not be applied. Operand ({Left}) does not evaluate to a numeric value.");
                }
                else if (!NumericUtil.IsNumericType(rightObj))
                {
                    throw new TemplateException($"Comparison operator could not be applied. Operand ({Right}) does not evaluate to a numeric value.");
                }
                else
                {
                    throw new InvalidOperationException($"Comparison operator could not be applied."); // Developer mistake
                }
            }

            public override async Task<object> Evaluate(EvaluationContext ctx)
            {
                object leftObj = await Left.Evaluate(ctx);
                object rightObj = await Right.Evaluate(ctx);

                var numericType = NumericUtil.CommonNumericType(leftObj, rightObj);
                if (numericType != null)
                {
                    IComparable left = NumericUtil.CastToNumeric(leftObj ?? 0, numericType);
                    IComparable right = NumericUtil.CastToNumeric(rightObj ?? 0, numericType);

                    return ComputeForComparables(left, right);
                }
                else
                {
                    // string "A" and char 'A' should equal each other
                    if (leftObj is string && rightObj is char)
                    {
                        rightObj = rightObj.ToString();
                    }

                    if (leftObj is char && rightObj is string)
                    {
                        leftObj = leftObj.ToString();
                    }

                    return ComputeForObjects(leftObj, rightObj);
                }
            }
        }

        /// <summary>
        /// Represents an infix Not-Equal operator, e.g. A != B.
        /// </summary>
        public class NotEqual : ComparisonOperator
        {
            protected override bool ComputeForComparables(IComparable left, IComparable right)
                => left.CompareTo(right) != 0;

            protected override bool ComputeForObjects(object left, object right)
                => (left != null || right != null) && (left == null || right == null || !left.Equals(right));
        }

        /// <summary>
        /// Represents an infix equivalence operator, e.g. A = B.
        /// </summary>
        public class AreEqual : ComparisonOperator
        {
            protected override bool ComputeForComparables(IComparable left, IComparable right)
                => left.CompareTo(right) == 0;

            protected override bool ComputeForObjects(object left, object right)
                => (left == null && right == null) || (left != null && right != null && left.Equals(right));
        }

        /// <summary>
        /// Represents an less-than-or-equal operator, e.g. A &lt;= B.
        /// </summary>
        public class LessThanOrEqual : ComparisonOperator
        {
            protected override bool ComputeForComparables(IComparable left, IComparable right)
                => left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Represents a greater-than-or-equal operator, e.g. A &gt;= B.
        /// </summary>
        public class GreaterThanOrEqual : ComparisonOperator
        {
            protected override bool ComputeForComparables(IComparable left, IComparable right)
                => left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Represents a less-than operator, e.g. A &lt; B.
        /// </summary>
        public class LessThan : ComparisonOperator
        {
            protected override bool ComputeForComparables(IComparable left, IComparable right)
                => left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Represents a greater-than operator, e.g. A &gt; B.
        /// </summary>
        public class GreaterThan : ComparisonOperator
        {
            protected override bool ComputeForComparables(IComparable left, IComparable right)
                => left.CompareTo(right) > 0;
        }

        #endregion
    }
}
