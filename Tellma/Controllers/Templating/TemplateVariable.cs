using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Services.Utilities;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Represents a variable entry in the <see cref="EvaluationContext"/>, this entry is able to do 3 things:
    /// 1 - Evaluate to a single value.
    /// 2 - Compute the list of API calls and SELECTs (in the form of <see cref="IEnumerable{Path}"/>), if any, that are needed for it to evaluate correctly
    /// 3 - If it evaluated to a model entity that is loaded from one or more API calls: Compute the list of possible API calls and associated base paths 
    /// of the returned entity (Computed in the form of <see cref="IEnumerable{Path}"/>).
    /// The difference between variables and functions, is that variables do not require arguments to evaluate, and they are always evaluated once no
    /// matter how many times <see cref="Evaluate()"/> is invoked
    /// </summary>
    public class TemplateVariable
    {
        /// <summary>
        /// Either this, or <see cref="_evalAsync"/> or <see cref="_result"/> are set in the construction of the <see cref="TemplateVariable"/>
        /// </summary>
        private readonly VariableEvaluator _eval;

        /// <summary>
        /// Either this, or <see cref="_eval"/> or <see cref="_result"/> are set in the construction of the <see cref="TemplateVariable"/>
        /// </summary>
        private readonly VariableEvaluatorAsync _evalAsync;

        /// <summary>
        /// Either this, or <see cref="_eval"/> or <see cref="_evalAsync"/> are set in the construction of the <see cref="TemplateVariable"/>
        /// </summary>
        private object _result;

        /// <summary>
        /// Memory to ensure the <see cref="_result"/> is calculated only once
        /// </summary>
        private bool _executedAlready = false;

        private readonly VariableSelectResolver _selectResolver;
        private readonly VariablePathsResolver _pathsResolver;

        public TemplateVariable(
            object value,
            VariableSelectResolver selectResolver = null,
            VariablePathsResolver pathsResolver = null)
        {
            _result = value;
            _executedAlready = true;
            _selectResolver = selectResolver;
            _pathsResolver = pathsResolver;
        }

        public TemplateVariable(
            VariableEvaluator eval,
            VariableSelectResolver selectResolver = null,
            VariablePathsResolver pathsResolver = null)
        {
            _eval = eval ?? throw new ArgumentNullException(nameof(eval));
            _selectResolver = selectResolver;
            _pathsResolver = pathsResolver;
        }

        public TemplateVariable(
            VariableEvaluatorAsync evalAsync,
            VariableSelectResolver selectResolver = null,
            VariablePathsResolver pathsResolver = null)
        {
            _evalAsync = evalAsync ?? throw new ArgumentNullException(nameof(evalAsync));
            _selectResolver = selectResolver;
            _pathsResolver = pathsResolver;
        }

        /// <summary>
        /// Evaluates the variable to a single value. Evaluation only occurs once, subsequent calls return the same cached value
        /// </summary>
        /// <returns>The value of the variable</returns>
        public async Task<object> Evaluate()
        {
            // Variables are evaluted once
            if (!_executedAlready)
            {
                if (_eval != null)
                {
                    _result = _eval();
                }
                else
                {
                    _result = await _evalAsync();
                }

                _executedAlready = true;
            }

            return _result;
        }

        /// <summary>
        /// Compute the list of API calls and SELECTs (in the form of <see cref="IEnumerable{Path}"/>) that are
        /// needed for <see cref="Evaluate()"/> to run correctly.
        /// </summary>
        public IAsyncEnumerable<Path> ResolveSelect()
        {
            if (_selectResolver != null)
            {
                return _selectResolver.Invoke();
            }
            else
            {
                return AsyncUtil.Empty<Path>();
            }
        }

        /// <summary>
        /// If the variable contains a model <see cref="Entity"/>, this function should return the base
        /// <see cref="Path"/>(s) of this entity, which include the <see cref="QueryInfo"/> it will be loaded from
        /// </summary>
        public IAsyncEnumerable<Path> ResolvePaths()
        {
            if (_pathsResolver != null)
            {
                return _pathsResolver.Invoke();
            }
            else
            {
                return AsyncUtil.Empty<Path>();
            }
        }
    }

    public delegate Task<object> VariableEvaluatorAsync();

    public delegate object VariableEvaluator();

    public delegate IAsyncEnumerable<Path> VariableSelectResolver();

    public delegate IAsyncEnumerable<Path> VariablePathsResolver();
}
