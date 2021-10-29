using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Base class for all template plans which are hierarchical objects that
    /// instruct the templating engine on the order, context and dependencies
    /// of evaluating a collection of templates.
    /// </summary>
    public abstract class TemplatePlan
    {
        /// <summary>
        /// Returns a list of <see cref="Path"/>s of all the templates in the <see cref="TemplatePlan"/> tree, 
        /// those paths represent (1) the API calls (2) and the Queryex SELECT that need to be passed to each one,
        /// in order for <see cref="GenerateOutputs(EvaluationContext)"/> to execute correctly.
        /// </summary>
        /// <param name="ctx">The static <see cref="EvaluationContext"/>, in case some of the expressions had to be evaluated at this stage,
        /// any db accessing variables or functions will throw an exception if accessed in this <see cref="EvaluationContext"/>.</param>
        /// <returns></returns>
        public abstract IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx);

        /// <summary>
        /// Evaluates the templates in the plan and adds the final output(s) to the plan tree nodes
        /// themselves, any functions and variables used in the expressions will be evaluated based
        /// on the supplied <see cref="EvaluationContext"/>, nodes in the plan tree may be modify
        /// the context for lower levels of the plan tree.
        /// </summary>
        /// <param name="ctx">The <see cref="EvaluationContext"/> containing all referenced variables and functions.</param>
        public abstract Task GenerateOutputs(EvaluationContext ctx);
    }
}
