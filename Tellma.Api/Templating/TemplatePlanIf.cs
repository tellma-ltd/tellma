using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// The leaf node of a plan, contains a single Templex expression that is evaluated
    /// once or multiple times into the <see cref="Outputs"/> collection of objects.
    /// </summary>
    public class TemplatePlanIf : TemplatePlan
    {
        private TemplexBase _conditionCandidate;

        /// <summary>
        /// Create a new instance of the <see cref="TemplatePlanIf"/> class.
        /// </summary>
        public TemplatePlanIf(string conditionExpression, TemplatePlan inner)
        {
            if (inner is null)
            {
                throw new ArgumentNullException(nameof(inner));
            }

            if (string.IsNullOrWhiteSpace(conditionExpression))
            {
                throw new ArgumentException($"'{nameof(conditionExpression)}' cannot be null or whitespace.", nameof(conditionExpression));
            }

            Inner = inner;
            ConditionExpression = conditionExpression;
        }

        /// <summary>
        /// The inner plan subject to this <see cref="TemplatePlanIf"/>.
        /// </summary>
        public TemplatePlan Inner { get; }

        /// <summary>
        /// The expression to evaluate.
        /// </summary>
        public string ConditionExpression { get; }

        public override async IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            _conditionCandidate ??= TemplexBase.Parse(ConditionExpression);

            if (_conditionCandidate != null && Inner != null)
            {
                await foreach (var select in _conditionCandidate.ComputeSelect(ctx))
                {
                    yield return select;
                }

                await foreach (var select in Inner.ComputeSelect(ctx))
                {
                    yield return select;
                }
            }
        }

        public override async Task GenerateOutputs(EvaluationContext ctx)
        {
            _conditionCandidate ??= TemplexBase.Parse(ConditionExpression);

            if (_conditionCandidate != null && Inner != null)
            {
                var conditionObj = (await _conditionCandidate.Evaluate(ctx)) ?? false;
                if (conditionObj is bool condition)
                {
                    if (condition)
                    {
                        await Inner.GenerateOutputs(ctx);
                    }
                }
                else
                {
                    throw new TemplateException($"If expression could not be applied. Expression ({_conditionCandidate}) does not evaluate to a true or false.");
                }
            }
        }
    }
}
