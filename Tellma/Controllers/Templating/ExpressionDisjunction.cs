using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Services.Utilities;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Represents an OR expression. E.g. A || B. Always evaluated to a boolean value
    /// </summary>
    public class ExpressionDisjunction : ExpressionBase
    {
        public ExpressionBase Left { get; set; }
        public ExpressionBase Right { get; set; }

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
            var left = await Left.Evaluate(ctx) ?? false; // Null is treated as false
            var right = await Right.Evaluate(ctx) ?? false; // Null is treated as false

            if (!(left is bool boolLeft))
            {
                throw new TemplateException($"Operator '||' could not be applied. The expression ({Left.ToString()}) does not evaluate to a boolean value");
            }
            else if (!(right is bool boolRight))
            {
                throw new TemplateException($"Operator '||' could not be applied. The expression ({Right.ToString()}) does not evaluate to a boolean value");
            }
            else
            {
                return boolLeft || boolRight;
            }
        }

        public override string ToString()
        {
            return $"{Left.ToString()} OR {Right.ToString()}";
        }

        /// <summary>
        /// Creates a new <see cref="ExpressionDisjunction"/>
        /// </summary>
        public static ExpressionDisjunction Make(ExpressionBase left, ExpressionBase right)
        {
            return new ExpressionDisjunction
            {
                Left = left,
                Right = right,
            };
        }
    }
}
