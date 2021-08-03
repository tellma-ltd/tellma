using System;
using System.Collections.Generic;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Evaluation of template component occurs under a certain <see cref="EvaluationContext"/>
    /// that maps all the variables and function names mentioned inside that component to their values.
    /// <para/>
    /// The names in the evaluation context are either global names or local names.<br/>
    /// 1) Global names are universal in all scopes, but have lower precedence, for example 'Localize' and '$Now'.<br/>
    /// 2) Local names are specific to scopes, and their values can change from one part of the template to
    /// another, but they have a higher precedence than the global names.<br/>
    /// </summary>
    public class EvaluationContext
    {
        /// <summary>
        /// Dictionary mapping function names (e.g. 'Sum') to their <see cref="EvaluationFunction"/> implementation.
        /// </summary>
        public class FunctionsDictionary : Dictionary<string, EvaluationFunction>
        {
            public FunctionsDictionary() : base(StringComparer.OrdinalIgnoreCase) { }
            public FunctionsDictionary(FunctionsDictionary dic) : base(dic, StringComparer.OrdinalIgnoreCase) { }
        }

        /// <summary>
        /// Dictionary mapping variable names (e.g. '$Filter') to their <see cref="EvaluationVariable"/> implementation.
        /// </summary>
        public class VariablesDictionary : Dictionary<string, EvaluationVariable>
        {
            public VariablesDictionary() : base(StringComparer.OrdinalIgnoreCase) { }
            public VariablesDictionary(VariablesDictionary dic) : base(dic, StringComparer.OrdinalIgnoreCase) { }
        }

        private readonly FunctionsDictionary _globalFunctions;
        private readonly VariablesDictionary _globalVariables;
        private FunctionsDictionary _localFunctions;
        private VariablesDictionary _localVariables;

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationContext"/> class. 
        /// Making it private forces the creation of evaluation context from the 
        /// <see cref="Create(FunctionsDictionary, VariablesDictionary)"/> method.
        /// </summary>
        /// <param name="globalFunctions">The global functions of the evaluation context.</param>
        /// <param name="globalVariables">The global variables of the evaluation context.</param>
        private EvaluationContext(FunctionsDictionary globalFunctions, VariablesDictionary globalVariables)
        {
            _globalFunctions = globalFunctions;
            _globalVariables = globalVariables;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="EvaluationContext"/> class.
        /// </summary>
        /// <param name="globalFunctions">The global functions of the evaluation context.</param>
        /// <param name="globalVariables">The global variables of the evaluation context.</param>
        /// <returns>A new immutable instance of the evaluation context.</returns>
        public static EvaluationContext Create(FunctionsDictionary globalFunctions,
            VariablesDictionary globalVariables)
        {
            return new EvaluationContext(
                new FunctionsDictionary(globalFunctions),
                new VariablesDictionary(globalVariables)); // Makes them immutable
        }

        /// <summary>
        /// Clones only the global functions and variables of this <see cref="EvaluationContext"/> into a new one.
        /// </summary>
        public EvaluationContext CloneWithoutLocals() => new(_globalFunctions, _globalVariables);

        /// <summary>
        /// Clonses all the functions and variables of the <see cref="EvaluationContext"/> into a new one.
        /// </summary>
        public EvaluationContext Clone() => new (_globalFunctions, _globalVariables)
        {
            _localFunctions = _localFunctions == null ? null : new FunctionsDictionary(_localFunctions),
            _localVariables = _localVariables == null ? null : new VariablesDictionary(_localVariables)
        };

        /// <summary>
        /// Adds a local <see cref="EvaluationFunction"/> to the context under the supplied name, 
        /// overriding any existing <see cref="EvaluationFunction"/> with the same name.
        /// </summary>
        public void SetLocalFunction(string name, EvaluationFunction function)
        {
            _localFunctions ??= new FunctionsDictionary();
            _localFunctions[name] = function;
        }

        /// <summary>
        /// Adds a local <see cref="EvaluationVariable"/> to the context under the supplied name, 
        /// overriding any existing <see cref="EvaluationVariable"/> with the same name.
        /// </summary>
        public void SetLocalVariable(string name, EvaluationVariable variable)
        {
            _localVariables ??= new VariablesDictionary();
            _localVariables[name] = variable;
        }

        /// <summary>
        /// Retrieves the <see cref="EvaluationFunction"/> from the context. Searches through 
        /// the local functions first then falls back to the global ones.
        /// </summary>
        /// <param name="functionName">The name to search for.</param>
        /// <param name="functionEntry">The output result.</param>
        /// <returns>True if the <see cref="EvaluationFunction"/> was found, false otherwise.</returns>
        public bool TryGetFunction(string functionName, out EvaluationFunction functionEntry) =>
            (_localFunctions != null && _localFunctions.TryGetValue(functionName, out functionEntry)) || _globalFunctions.TryGetValue(functionName, out functionEntry);

        /// <summary>
        /// Retrieves the <see cref="EvaluationVariable"/> from the context. Searches through 
        /// the local variables first then falls back to the global ones.
        /// </summary>
        /// <param name="variableName">The name to search for.</param>
        /// <param name="variableEntry">The output result.</param>
        /// <returns>True if the <see cref="EvaluationVariable"/> was found, false otherwise.</returns>
        public bool TryGetVariable(string variableName, out EvaluationVariable variableEntry) =>
            (_localVariables != null && _localVariables.TryGetValue(variableName, out variableEntry)) || _globalVariables.TryGetValue(variableName, out variableEntry);
    }
}
