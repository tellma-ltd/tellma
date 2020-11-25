using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Services.Utilities;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Represents a comparison between two expressions that evaluate to an <see cref="IComparable"/>, e.g. 5 < 6.
    /// This expression always evaluates to a boolean value
    /// </summary>
    public class ExpressionComparisonOperator : ExpressionBase
    {
        public ExpressionBase Left { get; set; }
        public ExpressionBase Right { get; set; }

        /// <summary>
        /// The infix comparison operator. E.g. "=". The full list of supported operators can be found in <see cref="ExpressionBase.ParseTokenStream(IEnumerable{string})"/>
        /// </summary>
        public string Operator { get; set; }

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

        public override async Task<object> Evaluate(EvaluationContext ctx)
        {
            object leftObj = await Left.Evaluate(ctx);
            object rightObj = await Right.Evaluate(ctx);

            var numericType = NumericUtil.CommonNumericType(leftObj, rightObj);
            if (numericType != null)
            {
                IComparable left = NumericUtil.CastToNumeric(leftObj ?? 0, numericType);
                IComparable right = NumericUtil.CastToNumeric(rightObj ?? 0, numericType);

                return Operator switch
                {
                    "=" => left.CompareTo(right) == 0,
                    "!=" => left.CompareTo(right) != 0,
                    "<>" => left.CompareTo(right) != 0,
                    "<=" => left.CompareTo(right) <= 0,
                    ">=" => left.CompareTo(right) >= 0,
                    "<" => left.CompareTo(right) < 0,
                    ">" => left.CompareTo(right) > 0,
                    _ => throw new TemplateException($"Unknown comparison operator {Operator}"),// Future proofing
                };
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

                switch (Operator)
                {
                    case "=":
                        return (leftObj == null && rightObj == null) || (leftObj != null && rightObj != null && leftObj.Equals(rightObj));

                    case "!=":
                    case "<>":
                        return (leftObj != null || rightObj != null) && (leftObj == null || rightObj == null || !leftObj.Equals(rightObj));

                    default: // The remaining comparison operators require numeric operands
                        if (!NumericUtil.IsNumericType(leftObj))
                        {
                            throw new TemplateException($"Operator '{Operator}' could not be applied. Operand ({Left}) does not evaluate to a numeric value");
                        }
                        else if (!NumericUtil.IsNumericType(rightObj))
                        {
                            throw new TemplateException($"Operator '{Operator}' could not be applied. Operand ({Right}) does not evaluate to a numeric value");
                        }
                        else
                        {
                            throw new Exception($"Operator '{Operator}' could not be applied"); // Developer mistake
                        }
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="ExpressionComparisonOperator"/>
        /// </summary>
        /// <param name="op"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static ExpressionComparisonOperator Make(string op, ExpressionBase left, ExpressionBase right)
        {
            return new ExpressionComparisonOperator
            {
                Left = left,
                Right = right,
                Operator = op
            };
        }

        public override string ToString()
        {
            return $"{Left} {Operator} {Right}";
        }
    }
}
