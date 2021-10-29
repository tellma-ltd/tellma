using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Utilities.Common;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Represents a function entry in the <see cref="EvaluationContext"/>, this entry is able to do 3 things:<br/>
    /// 1 - <see cref="Evaluate"/>: Evaluate to a result, given the values of its arguments.<br/>
    /// 2 - <see cref="ComputeAdditionalSelect"/>: Compute the list of API calls and SELECTs (in the form of <see cref="IEnumerable{Path}"/>) that are needed for it to evaluate correctly.<br/>
    /// 3 - <see cref="ComputePaths"/>: If it returns a model entity that is loaded from one or more API calls: Compute the list of possible API calls and associated base paths 
    /// of the returned entity (Computed in the form of <see cref="IEnumerable{Path}"/>).
    /// </summary>
    public class EvaluationFunction
    {
        /// <summary>
        /// Either this or <see cref="_functionAsync"/> must be set.
        /// </summary>
        private readonly PureFunction _function;
        
        /// <summary>
        /// Either this or <see cref="_function"/> must be set.
        /// </summary>
        private readonly PureFunctionAsync _functionAsync;

        private readonly FunctionAdditionalSelectResolver _additionalSelectResolver;
        private readonly FunctionPathsResolver _pathsResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationFunction"/> class.
        /// </summary>
        public EvaluationFunction(
            PureFunction function,
            FunctionAdditionalSelectResolver additionalSelectResolver = null,
            FunctionPathsResolver pathsResolver = null)
        {
            _function = function ?? throw new ArgumentNullException(nameof(function));
            _additionalSelectResolver = additionalSelectResolver;
            _pathsResolver = pathsResolver;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationFunction"/> class.
        /// </summary>
        public EvaluationFunction(
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
        /// Computes the list of API calls and SELECTs (in the form of <see cref="IEnumerable{Path}"/>) that are
        /// needed for <see cref="Evaluate(object[], EvaluationContext)"/> to run correctly, aside from the selects 
        /// of the arguments. A static <see cref="EvaluationContext"/> is provided in case some of the arguments had 
        /// to be evaluated before calling the APIs in order to compute the required paths.v
        /// A static <see cref="EvaluationContext"/> is one where any function that accesses an API or any variable that
        /// should be populated by model entities retrieved from an API will throw exceptions if invoked/accessed.
        /// </summary>
        /// <param name="args">The list of un-evaluated function arguments.</param>
        /// <param name="ctx">The static <see cref="EvaluationContext"/>.</param>
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
        /// If the <see cref="EvaluationFunction"/> returns an model <see cref="Entity"/>, this function returns
        /// the possible <see cref="Path"/>s leading to the returned <see cref="Entity"/>. The result is computed in
        /// the form of <see cref="IEnumerable{Path}"/>.
        /// A static <see cref="EvaluationContext"/> is provided in case some of the arguments had to be evaluated
        /// before calling the APIs in order to compute the required paths.
        /// A static <see cref="EvaluationContext"/> is one where any function that accesses an API or any variable that
        /// should be populated by model entities retrieved from an API will throw exceptions if invoked/accessed.
        /// </summary>
        /// <param name="args">The list of un-evaluated function arguments.</param>
        /// <param name="ctx">The static <see cref="EvaluationContext"/>.</param>
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
