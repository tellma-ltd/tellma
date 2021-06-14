using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Model.Application;
using Tellma.Services.Utilities;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Represents a function entry in the <see cref="EvaluationContext"/>, this entry is able to do 3 things:
    /// 1 - Evaluate to a result, given the values of its arguments.
    /// 2 - Compute the list of API calls and SELECTs (in the form of <see cref="IEnumerable{Path}"/>) that are needed for it to evaluate correctly
    /// 3 - If it returns a model entity that is loaded from one or more API calls: Compute the list of possible API calls and associated base paths 
    /// of the returned entity (Computed in the form of <see cref="IEnumerable{Path}"/>).
    /// </summary>
    public class TemplateFunction
    {
        /// <summary>
        /// Either this or <see cref="_functionAsync"/> must be set
        /// </summary>
        private readonly PureFunction _function;
        
        /// <summary>
        /// Either this or <see cref="_function"/> must be set
        /// </summary>
        private readonly PureFunctionAsync _functionAsync;

        private readonly FunctionAdditionalSelectResolver _additionalSelectResolver;
        private readonly FunctionPathsResolver _pathsResolver;

        public TemplateFunction(
            PureFunction function,
            FunctionAdditionalSelectResolver additionalSelectResolver = null,
            FunctionPathsResolver pathsResolver = null)
        {
            _function = function ?? throw new ArgumentNullException(nameof(function));
            _additionalSelectResolver = additionalSelectResolver;
            _pathsResolver = pathsResolver;
        }

        public TemplateFunction(
            PureFunctionAsync functionAsync,
            FunctionAdditionalSelectResolver additionalSelectResolver = null,
            FunctionPathsResolver pathsResolver = null)
        {
            _functionAsync = functionAsync ?? throw new ArgumentNullException(nameof(functionAsync));
            _additionalSelectResolver = additionalSelectResolver;
            _pathsResolver = pathsResolver;
        }

        /// <summary>
        /// Evaluates the function given a bunch of supplied arguments and an <see cref="EvaluationContext"/>.
        /// </summary>
        public Task<object> Evaluate(object[] args, EvaluationContext ctx) => _function != null ? Task.FromResult(_function(args, ctx)) : _functionAsync.Invoke(args, ctx);

        /// <summary>
        /// Compute the list of API calls and SELECTs (in the form of <see cref="IEnumerable{Path}"/>) that are
        /// needed for <see cref="Evaluate(object[], EvaluationContext)"/> to run correctly, aside from the selects of the arguments.
        /// A static <see cref="EvaluationContext"/> is provided in case some of the arguments had to be evaluated
        /// before calling the APIs in order to compute the required paths.
        /// A static <see cref="EvaluationContext"/> is one where any function that accesses an API or any variable that
        /// should be populated by model entities retrieved from an API will throw exceptions if invoked/accessed
        /// </summary>
        /// <param name="args">The list of un-evaluated function arguments</param>
        /// <param name="ctx">The static <see cref="EvaluationContext"/></param>
        public IAsyncEnumerable<Path> ComputeAdditionalSelect(TemplexBase[] args, EvaluationContext ctx)
        {
            if (_additionalSelectResolver != null)
            {
                return _additionalSelectResolver.Invoke(args, ctx);
            }
            else
            {
                return AsyncUtil.Empty<Path>();
            }
        }

        /// <summary>
        /// If the <see cref="TemplateFunction"/> returns an model <see cref="Entity"/>, this function returns
        /// the possible <see cref="Path"/>s leading to the returned <see cref="Entity"/>. The result is computed in
        /// the form of <see cref="IEnumerable{Path}"/>.
        /// A static <see cref="EvaluationContext"/> is provided in case some of the arguments had to be evaluated
        /// before calling the APIs in order to compute the required paths.
        /// A static <see cref="EvaluationContext"/> is one where any function that accesses an API or any variable that
        /// should be populated by model entities retrieved from an API will throw exceptions if invoked/accessed
        /// </summary>
        /// <param name="args">The list of un-evaluated function arguments</param>
        /// <param name="ctx">The static <see cref="EvaluationContext"/></param>
        public IAsyncEnumerable<Path> ComputePaths(TemplexBase[] args, EvaluationContext ctx)
        {
            if (_pathsResolver != null)
            {
                return _pathsResolver.Invoke(args, ctx);
            }
            else
            {
                return AsyncUtil.Empty<Path>();
            }
        }
    }

    public delegate Task<object> PureFunctionAsync(object[] args, EvaluationContext ctx);

    public delegate object PureFunction(object[] args, EvaluationContext ctx);

    public delegate IAsyncEnumerable<Path> FunctionAdditionalSelectResolver(TemplexBase[] args, EvaluationContext ctx);

    public delegate IAsyncEnumerable<Path> FunctionPathsResolver(TemplexBase[] args, EvaluationContext ctx);
}
