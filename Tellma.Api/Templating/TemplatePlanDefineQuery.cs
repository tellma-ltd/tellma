using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Utilities.Common;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Instructs the templating engine to add a <see cref="QueryInfo"/> variable
    /// to the context before evaluating the inner plan.
    /// </summary>
    public class TemplatePlanDefineQuery : TemplatePlan
    {
        /// <summary>
        /// Create a new instance of the <see cref="TemplatePlanDefineQuery"/> class.
        /// </summary>
        /// <param name="varName">The name of the variable to add to the context.</param>
        /// <param name="queryInfo">The <see cref="QueryInfo"/> to assign to the variable.</param>
        /// <param name="inner">The inner plan subject to this <see cref="TemplatePlanDefineQuery"/>.</param>
        public TemplatePlanDefineQuery(string varName, QueryInfo queryInfo, TemplatePlan inner)
        {
            if (inner is null)
            {
                throw new ArgumentNullException(nameof(inner));
            }

            if (string.IsNullOrWhiteSpace(varName))
            {
                throw new ArgumentException($"'{nameof(varName)}' cannot be null or whitespace.", nameof(varName));
            }

            if (queryInfo is null)
            {
                throw new ArgumentNullException(nameof(queryInfo));
            }

            TemplexVariable.ValidateVariableName(varName);

            Inner = inner;
            VariableName = varName;
            QueryInfo = queryInfo;
        }

        /// <summary>
        /// The name of the variable to add to the context.
        /// </summary>
        public TemplatePlan Inner { get; }

        /// <summary>
        /// The inner plan subject to this <see cref="TemplatePlanDefineQuery"/>.
        /// </summary>
        public string VariableName { get; }

        /// <summary>
        /// The <see cref="QueryInfo"/> to assign to the variable.
        /// </summary>
        public QueryInfo QueryInfo { get; }

        public override async IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            var ctxClone = ctx.Clone();
            ctxClone.SetLocalVariable(VariableName, new EvaluationVariable(
                eval: TemplateUtil.VariableThatThrows(varName: VariableName), // This is what makes it a "static" context
                pathsResolver: () => AsyncUtil.Singleton(Path.Empty(QueryInfo))
            ));

            await foreach (var path in Inner.ComputeSelect(ctxClone))
            {
                yield return path;
            }
        }

        public override async Task GenerateOutputs(EvaluationContext ctx)
        {
            if (ctx.TryGetApiResult(QueryInfo, out object result))
            {
                ctx = ctx.Clone();
                ctx.SetLocalVariable(VariableName, new EvaluationVariable(value: result));
            }

            await Inner.GenerateOutputs(ctx);
        }
    }
}
