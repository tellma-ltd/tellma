using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// The base class of all template components. A markup template is parsed into a tree of <see cref="TemplateBase"/>. 
    /// A markup template is a string value, mostly written in a specific markup language. e.g. HTML. But in between the 
    /// markup are some expressions contained within curly brackets {{ }} that are evaluated and replaced with
    /// the outcome they generate. Some curly brackets contain structural expressions that are not replaced with anything
    /// but simply influence the context and behavior of the markup within their scope between them and {{ *end }}.
    /// </summary>
    public abstract class TemplateBase
    {
        /// <summary>
        /// Returns a list of <see cref="Path"/>s, which represent (1) the API calls (2) and the
        /// Queryex SELECT that need to be passed to each one, in order for
        /// <see cref="GenerateOutput(StringBuilder, EvaluationContext)"/> to execute correctly.
        /// </summary>
        /// <param name="ctx">The static <see cref="EvaluationContext"/>, in case some of the expressions had to be evaluated at this stage,
        /// any db accessing variables or functions will throw an exception if accessed in this <see cref="EvaluationContext"/>.</param>
        /// <returns></returns>
        public abstract IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx);

        /// <summary>
        /// Appends the final output to the provided <see cref="StringBuilder"/>, any function and variables
        /// used in the expressions will be evaluated based on the supplied <see cref="EvaluationContext"/>.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the output to.</param>
        /// <param name="ctx">The evaluation context containing all referenced variables and functions.</param>
        /// <param name="encodeFunc">Optional function that encodes the resulting strings of interpolation expressions before adding them to the final output.</param>
        public abstract Task GenerateOutput(StringBuilder builder, EvaluationContext ctx, Func<string, string> encodeFunc = null);
    }
}
