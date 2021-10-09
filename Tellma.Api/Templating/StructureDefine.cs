using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tellma.Utilities.Common;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Simply defines a new local variable, or overrides an existing one in the
    /// <see cref="EvaluationContext"/>, made available to all expressions within its scope.
    /// <para/>
    /// Example: {{ *define total as 1 + 2 }}.
    /// </summary>
    public class StructureDefine : StructureBase
    {
        /// <summary>
        /// This expression evaluates to the value of the new variable.
        /// </summary>
        public TemplexBase Value { get; set; }

        /// <summary>
        /// The name of the new variable.
        /// </summary>
        public string VariableName { get; set; }

        public override IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            if (Template == null)
            {
                return AsyncUtil.Empty<Path>();
            }

            var scopedCtx = GetScopeLocalContext(ctx);
            return Template.ComputeSelect(scopedCtx);
        }

        public override async Task GenerateOutput(StringBuilder builder, EvaluationContext ctx, Func<string, string> encodeFunc = null)
        {
            if (Template == null)
            {
                return;
            }

            // Run the template on a scoped context
            var scopedCtx = GetScopeLocalContext(ctx);
            await Template.GenerateOutput(builder, scopedCtx, encodeFunc);
        }

        /// <summary>
        /// Clones the <see cref="EvaluationContext"/> and returns a new one that contains the new variable.
        /// </summary>
        private EvaluationContext GetScopeLocalContext(EvaluationContext ctx)
        {
            var variable = new EvaluationVariable(
                    evalAsync: () => Value.Evaluate(ctx),
                    selectResolver: () => Value.ComputeSelect(ctx),
                    pathsResolver: () => Value.ComputePaths(ctx));

            var ctxClone = ctx.Clone();
            ctxClone.SetLocalVariable(VariableName, variable);

            return ctxClone;
        }
    }
}
