using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Utilities.Common;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Represents an infix arithmetic operator expression, e.g. 1 + 3.
    /// </summary>
    public class TemplexArithmeticOperator : TemplexBase
    {
        /// <summary>
        /// The left operand.
        /// </summary>
        public TemplexBase Left { get; set; }

        /// <summary>
        /// The right operand.
        /// </summary>
        public TemplexBase Right { get; set; }

        /// <summary>
        /// The infix arithmetic operator. E.g. "+". The full list of supported operators can 
        /// be found in <see cref="TemplexBase.ParseTokenStream(IEnumerable{string})"/>.
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
            object left = await Left.Evaluate(ctx);
            object right = await Right.Evaluate(ctx);

            // String concatenation
            if (Operator == "+" && left is string || left is char || right is string || right is char)
            {
                return ToString(left) + ToString(right);
            }

            // Numbers
            var commonType = NumericUtil.CommonNumericType(left, right);
            if (commonType == null)
            {
                if (!NumericUtil.IsNumericType(left))
                {
                    throw new TemplateException($"Operator '{Operator}' could not be applied. Operand ({Left}) does not evaluate to a numeric value.");
                }
                else if (!NumericUtil.IsNumericType(right))
                {
                    throw new TemplateException($"Operator '{Operator}' could not be applied. Operand ({Right}) does not evaluate to a numeric value.");
                }
                else
                {
                    throw new Exception($"Operator '{Operator}' could not be applied."); // Developer mistake
                }
            }

            return Operator switch
            {
                "/" => NumericUtil.Divide(left, right, commonType, Right),
                "%" => NumericUtil.Modulo(left, right, commonType, Right),
                "*" => NumericUtil.Multiply(left, right, commonType),
                "+" => NumericUtil.Add(left, right, commonType),
                "-" => NumericUtil.Subtract(left, right, commonType),
                _ => throw new InvalidOperationException($"Unknown operator {Operator}."),// Future proofing
            };
        }

        public override string ToString()
        {
            return $"{Left} {Operator} {Right}";
        }

        /// <summary>
        /// Creates a new <see cref="TemplexArithmeticOperator"/>.
        /// </summary>
        public static TemplexArithmeticOperator Make(string op, TemplexBase left, TemplexBase right)
        {
            return new TemplexArithmeticOperator
            {
                Left = left,
                Right = right,
                Operator = op
            };
        }
    }
}
