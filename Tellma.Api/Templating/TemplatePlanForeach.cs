using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Utilities.Common;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Instructs the templating engine to evaluate the inner plan as many times
    /// as there items in a provided list expression, in each iteration the item
    /// is added to the evaluation context before evaluating the inner plan.
    /// </summary>
    public class TemplatePlanForeach : TemplatePlan
    {
        private TemplexBase _listCandidate;

        /// <summary>
        /// Create a new instance of the <see cref="TemplatePlanForeach"/> class.
        /// </summary>
        /// <param name="iteratorVarName">The name of the variable to add to the context with each iteration.</param>
        /// <param name="listExpression">The list expression to loop over.</param>
        /// <param name="inner">The inner plan subject to this <see cref="TemplatePlanForeach"/>.</param>
        public TemplatePlanForeach(string iteratorVarName, string listExpression, TemplatePlan inner)
        {
            if (inner is null)
            {
                throw new ArgumentNullException(nameof(inner));
            }

            if (string.IsNullOrWhiteSpace(iteratorVarName))
            {
                throw new ArgumentException($"'{nameof(iteratorVarName)}' cannot be null or whitespace.", nameof(iteratorVarName));
            }

            TemplexVariable.ValidateVariableName(iteratorVarName);

            if (string.IsNullOrWhiteSpace(listExpression))
            {
                throw new ArgumentException($"'{nameof(listExpression)}' cannot be null or whitespace.", nameof(listExpression));
            }

            Inner = inner;
            IteratorVariableName = iteratorVarName;
            ListExpression = listExpression;
        }

        /// <summary>
        /// The inner plan subject to this <see cref="TemplatePlanForeach"/>.
        /// </summary>
        public TemplatePlan Inner { get; }

        /// <summary>
        /// The name of the variable to add to the context with each iteration.
        /// </summary>
        public string IteratorVariableName { get; }

        /// <summary>
        /// The list expression to loop over.
        /// </summary>
        public string ListExpression { get; }

        public override async IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            _listCandidate ??= TemplexBase.Parse(ListExpression);

            if (_listCandidate != null)
            {
                // Expression select
                var select = _listCandidate.ComputeSelect(ctx);
                await foreach (var atom in select)
                {
                    yield return atom;
                }

                // Expression paths
                var paths = _listCandidate.ComputePaths(ctx);
                await foreach (var path in paths)
                {
                    yield return path.Append("Id");
                }

                // Inner template select
                var scopedCtx = ctx.Clone();
                scopedCtx.SetLocalVariable(IteratorVariableName, new EvaluationVariable(
                                eval: TemplateUtil.VariableThatThrows(IteratorVariableName),
                                selectResolver: () => select,
                                pathsResolver: () => paths
                                ));

                await foreach (var atom in Inner.ComputeSelect(scopedCtx))
                {
                    yield return atom;
                }
            }
        }

        public override async Task GenerateOutputs(EvaluationContext ctx)
        {
            _listCandidate ??= TemplexBase.Parse(ListExpression);

            if (_listCandidate != null)
            {
                var listObj = (await _listCandidate.Evaluate(ctx)) ?? new List<object>();
                if (listObj is IList list)
                {
                    foreach (var listItem in list)
                    {
                        // Initialize new evaluation context with the new variable in it
                        var scopedCtx = ctx.Clone();
                        scopedCtx.SetLocalVariable(IteratorVariableName, new EvaluationVariable(
                                evalAsync: () => Task.FromResult(listItem),
                                selectResolver: () => AsyncUtil.Empty<Path>(), // It doesn't matter when generating output
                                pathsResolver: () => AsyncUtil.Empty<Path>() // It doesn't matter when generating output
                                ));

                        // Run the template again on that context
                        await Inner.GenerateOutputs(scopedCtx);
                    }
                }
                else
                {
                    throw new TemplateException($"Expression does not evaluate to a list ({_listCandidate}).");
                }
            }
        }
    }
}
