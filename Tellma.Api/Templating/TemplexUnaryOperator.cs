using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Utilities.Common;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Represents a unary operator, e.g. -A.
    /// </summary>
    public abstract class TemplexUnaryOperator : TemplexBase
    {
        private string _op;

        /// <summary>
        /// The expression that is the operand onto which the operator is applied.
        /// </summary>
        public TemplexBase Operand { get; private set; }

        public override IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            return Operand.ComputeSelect(ctx);
        }

        public override IAsyncEnumerable<Path> ComputePaths(EvaluationContext ctx)
        {
            return AsyncUtil.Empty<Path>();
        }

        public override string ToString()
        {
            return $"({_op}{Operand})";
        }

        /// <summary>
        /// Creates a new <see cref="TemplexUnaryOperator"/> based on the operator.
        /// </summary>
        public static TemplexUnaryOperator Make(string op, TemplexBase operand)
        {
            if (operand is null)
            {
                throw new ArgumentNullException(nameof(operand));
            }

            TemplexUnaryOperator result = op switch
            {
                "!" => new Negation(),
                "-" => new Negative(),
                "+" => new Positive(),
                _ => throw new TemplateException($"Unkown unary operator {op}.")
            };

            result.Operand = operand;
            result._op = op;
            return result;
        }
        
        /// <summary>
        /// Returns true if the operator is a valid unary operator like "-" and "not".
        /// </summary>
        public static bool ValidOperator(string op)
        {
            return op.ToLower() switch
            {
                "-" or "+" or "!" => true,
                _ => false,
            };
        }

        /// <summary>
        /// Represents a logical negation, e.g. !A.
        /// </summary>
        public class Negation : TemplexUnaryOperator
        {
            public override async Task<object> Evaluate(EvaluationContext ctx)
            {
                var operand = await Operand.Evaluate(ctx) ?? false; // Null is treated as false
                if (operand is not bool boolVal)
                {
                    throw new TemplateException($"Negation operator '!' could not be applied. The expression {Operand} does not evaluate to a boolean value.");
                }
                else
                {
                    return !boolVal;
                }
            }
        }

        /// <summary>
        /// Represents a negative sign, e.g. -A.
        /// </summary>
        public class Negative : TemplexUnaryOperator
        {
            public override async Task<object> Evaluate(EvaluationContext ctx)
            {
                var operand = await Operand.Evaluate(ctx);

                const int zero = 0;
                var commonType = NumericUtil.CommonNumericType(zero, operand);
                if (commonType == null)
                {
                    throw new TemplateException($"Negative sign '-' could not be applied. The expression {Operand} does not evaluate to a numeric value.");
                }
                else
                {
                    return NumericUtil.Subtract(zero, operand, commonType);
                }
            }
        }

        /// <summary>
        /// Represents a positive sign, e.g. +A.
        /// </summary>
        public class Positive : TemplexUnaryOperator
        {
            public override async Task<object> Evaluate(EvaluationContext ctx)
            {
                var operand = await Operand.Evaluate(ctx);

                const int zero = 0;
                var commonType = NumericUtil.CommonNumericType(zero, operand);
                if (commonType == null)
                {
                    throw new TemplateException($"Positive sign '+' could not be applied. The expression {Operand} does not evaluate to a numeric value.");
                }
                else
                {
                    return operand;
                }
            }
        }
    }
}
