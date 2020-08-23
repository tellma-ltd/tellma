using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Omits the template within its scope unless the <see cref="ConditionCandidate"/> evaluates to true
    /// </summary>
    public class StructureIf : StructureBase
    {
        /// <summary>
        /// The condition to be evaluated to determine whether to output the scoped template
        /// </summary>
        public ExpressionBase ConditionCandidate { get; set; }

        public override async IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            if (ConditionCandidate != null)
            {
                await foreach (var select in ConditionCandidate.ComputeSelect(ctx))
                {
                    yield return select;
                }

                await foreach (var select in Template.ComputeSelect(ctx))
                {
                    yield return select;
                }
            }
        }

        public override async Task GenerateOutput(StringBuilder builder, EvaluationContext ctx)
        {
            if (ConditionCandidate != null)
            {
                var conditionObj = (await ConditionCandidate.Evaluate(ctx)) ?? false;
                if (conditionObj is bool condition)
                {
                    if (condition)
                    {
                        await Template.GenerateOutput(builder, ctx);
                    }
                }
                else
                {
                    throw new TemplateException($"If expression could not be applied. Expression ({ConditionCandidate}) does not evaluate to a true or false");
                }
            }
        }
    }
}
