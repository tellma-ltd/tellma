using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Instructs the templating engine to add a variable to the context before
    /// evaluating the inner plan.
    /// </summary>
    public class TemplatePlanDefine : TemplatePlan
    {
        private TemplexBase _varExpression;

        /// <summary>
        /// Create a new instance of the <see cref="TemplatePlanDefine"/> class.
        /// </summary>
        /// <param name="varName">The name of the variable to add to the context.</param>
        /// <param name="varExpression">The expression to assign to the variable.</param>
        /// <param name="inner">The inner plan subject to this <see cref="TemplatePlanDefine"/>.</param>
        public TemplatePlanDefine(string varName, string varExpression, TemplatePlan inner)
        {
            if (inner is null)
            {
                throw new ArgumentNullException(nameof(inner));
            }

            if (string.IsNullOrWhiteSpace(varName))
            {
                throw new ArgumentException($"'{nameof(varName)}' cannot be null or whitespace.", nameof(varName));
            }

            TemplexVariable.ValidateVariableName(varName);

            if (string.IsNullOrWhiteSpace(varExpression))
            {
                throw new ArgumentException($"'{nameof(varExpression)}' cannot be null or whitespace.", nameof(varExpression));
            }

            Inner = inner;
            VariableName = varName;
            VariableExpression = varExpression;
        }

        /// <summary>
        /// The inner plan subject to this <see cref="TemplatePlanDefine"/>.
        /// </summary>
        public TemplatePlan Inner { get; }

        /// <summary>
        /// The name of the variable to add to the context.
        /// </summary>
        public string VariableName { get;}

        /// <summary>
        /// The expression to assign to the variable.
        /// </summary>
        public string VariableExpression { get; }

        public override async IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            var scopedCtx = GetScopeLocalContext(ctx);
            await foreach (var path in Inner.ComputeSelect(scopedCtx))
            {
                yield return path;
            }
        }

        public override async Task GenerateOutputs(EvaluationContext ctx)
        {
            var scopedCtx = GetScopeLocalContext(ctx);
            await Inner.GenerateOutputs(scopedCtx);
        }

        /// <summary>
        /// Clones the <see cref="EvaluationContext"/> and returns a new one that contains the new variable.
        /// </summary>
        private EvaluationContext GetScopeLocalContext(EvaluationContext ctx)
        {
            _varExpression ??= TemplexBase.Parse(VariableExpression);

            var variable = new EvaluationVariable(
                    evalAsync: () => _varExpression.Evaluate(ctx),
                    selectResolver: () => _varExpression.ComputeSelect(ctx),
                    pathsResolver: () => _varExpression.ComputePaths(ctx));

            var ctxClone = ctx.Clone();
            ctxClone.SetLocalVariable(VariableName, variable);

            return ctxClone;
        }
    }
}
