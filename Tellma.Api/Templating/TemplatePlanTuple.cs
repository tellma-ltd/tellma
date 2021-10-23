using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Instructs the templating engine to evaluate a list of templates together
    /// under a common evaluation context.
    /// </summary>
    public class TemplatePlanTuple : TemplatePlan
    {
        /// <summary>
        /// Create a new instance of the <see cref="TemplatePlanTuple"/> class.
        /// </summary>
        /// <param name="subplans">The list of plans to be evaluated together.</param>
        public TemplatePlanTuple(IEnumerable<TemplatePlan> subplans)
        {
            if (subplans is null)
            {
                throw new ArgumentNullException(nameof(subplans));
            }

            Subplans = subplans.ToList();
        }

        /// <summary>
        /// Create a new instance of the <see cref="TemplatePlanTuple"/> class.
        /// </summary>
        /// <param name="subplans">The list of plans to be evaluated together.</param>
        public TemplatePlanTuple(params TemplatePlan[] subplans) : this(subplans.ToList())
        {
        }

        /// <summary>
        /// The list of plans to be evaluated together.
        /// </summary>
        public List<TemplatePlan> Subplans { get; }

        public override async IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            foreach (var plan in Subplans)
            {
                await foreach (var path in plan.ComputeSelect(ctx))
                {
                    yield return path;
                }
            }
        }

        public override async Task GenerateOutputs(EvaluationContext ctx)
        {
            foreach (var plan in Subplans)
            {
                await plan.GenerateOutputs(ctx);
            }
        }
    }
}
