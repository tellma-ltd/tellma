using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Simply defines a new local variable, or overrides an existing one in the
    /// <see cref="EvaluationContext"/>, made available to all expressions within its scope
    /// </summary>
    public class StructureDefine : StructureBase
    {
        /// <summary>
        /// This expression evaluates to the value of the new variable
        /// </summary>
        public ExpressionBase Value { get; set; }

        /// <summary>
        /// The name of the new variable
        /// </summary>
        public string VariableName { get; set; }

        public override IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            var scopedCtx = GetScopeLocalContext(ctx);
            return Template.ComputeSelect(scopedCtx);
        }

        public override async Task GenerateOutput(StringBuilder builder, EvaluationContext ctx)
        {
            // Run the template on a scoped context
            var scopedCtx = GetScopeLocalContext(ctx);
            await Template.GenerateOutput(builder, scopedCtx);
        }

        /// <summary>
        /// Clones the <see cref="EvaluationContext"/> and returns a new one that contains the new variable
        /// </summary>
        private EvaluationContext GetScopeLocalContext(EvaluationContext ctx)
        {
            TemplateVariable variable;
            if (Value == null)
            {
                variable = new TemplateVariable(
                    value: null,
                    selectResolver: () => Value.ComputeSelect(ctx),
                    pathsResolver: () => Value.ComputePaths(ctx));
            }
            else
            {
                variable = new TemplateVariable(
                    evalAsync: () => Value.Evaluate(ctx),
                    selectResolver: () => Value.ComputeSelect(ctx),
                    pathsResolver: () => Value.ComputePaths(ctx));
            }

            var clone = ctx.Clone();
            clone.SetLocalVariable(VariableName, variable);

            return clone;
        }
    }
}
