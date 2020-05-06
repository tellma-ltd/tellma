using System;
using System.Collections.Generic;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Evaluation of template component occurs under a certain <see cref="EvaluationContext"/>
    /// that maps all the variables and function names mentioned inside that component to their values.
    /// The names in the evaluation context are either global names or local names
    /// 1. Global names are universal in all scopes, but have lower precedence, for example 'Localize' and '$Now'
    /// 2. Local names are specific to scopes, and their values can change from one part of the template to
    /// another, but they have a higher precedence than the global names
    /// </summary>
    public class EvaluationContext
    {
        /// <summary>
        /// Dictionary mapping function names (e.g. 'Sum') to their <see cref="TemplateFunction"/> implementation
        /// </summary>
        public class FunctionsDictionary : Dictionary<string, TemplateFunction>
        {
            public FunctionsDictionary() : base(StringComparer.OrdinalIgnoreCase) { }
            public FunctionsDictionary(FunctionsDictionary dic) : base(dic, StringComparer.OrdinalIgnoreCase) { }
        }

        /// <summary>
        /// Dictionary mapping variable names (e.g. '$Filter') to their <see cref="TemplateVariable"/> implementation
        /// </summary>
        public class VariablesDictionary : Dictionary<string, TemplateVariable>
        {
            public VariablesDictionary() : base(StringComparer.OrdinalIgnoreCase) { }
            public VariablesDictionary(VariablesDictionary dic) : base(dic, StringComparer.OrdinalIgnoreCase) { }
        }

        private readonly FunctionsDictionary _globalFunctions;
        private readonly VariablesDictionary _globalVariables;
        private FunctionsDictionary _localFunctions;
        private VariablesDictionary _localVariables;

        private EvaluationContext( // Private ctor forces the creation of evaluation context from Create method
            FunctionsDictionary globalFunctions,
            VariablesDictionary globalVariables) // Globals are only created once
        {
            _globalFunctions = globalFunctions;
            _globalVariables = globalVariables;
        }

        public static EvaluationContext Create(FunctionsDictionary globalFunctions,
            VariablesDictionary globalVariables)
        {
            return new EvaluationContext(
                new FunctionsDictionary(globalFunctions),
                new VariablesDictionary(globalVariables)); // Makes them immutable
        }

        /// <summary>
        /// Clones strictly the global functions and variables of this <see cref="EvaluationContext"/> into a new one
        /// </summary>
        public EvaluationContext CloneWithoutLocals() => new EvaluationContext(_globalFunctions, _globalVariables);

        /// <summary>
        /// Clonses all the functions and variables of the <see cref="EvaluationContext"/> into a new one
        /// </summary>
        /// <returns></returns>
        public EvaluationContext Clone() => new EvaluationContext(_globalFunctions, _globalVariables)
        {
            _localFunctions = _localFunctions == null ? null : new FunctionsDictionary(_localFunctions),
            _localVariables = _localVariables == null ? null : new VariablesDictionary(_localVariables)
        };

        /// <summary>
        /// Adds a local <see cref="TemplateFunction"/> to the context under the supplied name, 
        /// overriding any existing <see cref="TemplateFunction"/> with the same name
        /// </summary>
        public void SetLocalFunction(string name, TemplateFunction function)
        {
            if (_localFunctions == null)
            {
                _localFunctions = new FunctionsDictionary();
            }

            _localFunctions[name] = function;
        }

        /// <summary>
        /// Adds a local <see cref="TemplateVariable"/> to the context under the supplied name, 
        /// overriding any existing <see cref="TemplateVariable"/> with the same name
        /// </summary>
        public void SetLocalVariable(string name, TemplateVariable variable)
        {
            if (_localVariables == null)
            {
                _localVariables = new VariablesDictionary();
            }

            _localVariables[name] = variable;
        }

        /// <summary>
        /// Retrieves the <see cref="TemplateVariable"/> from the context. Searches through the local variables first then falls back to the global
        /// </summary>
        /// <param name="variableName">The name to search for</param>
        /// <param name="variableEntry">The output result</param>
        /// <returns>True if the <see cref="TemplateVariable"/> was found, false otherwise</returns>
        public bool TryGetVariable(string variableName, out TemplateVariable variableEntry) =>
            (_localVariables != null && _localVariables.TryGetValue(variableName, out variableEntry)) || _globalVariables.TryGetValue(variableName, out variableEntry);

        /// <summary>
        /// Retrieves the <see cref="TemplateFunction"/> from the context. Searches through the local functions first then falls back to the global
        /// </summary>
        /// <param name="functionName">The name to search for</param>
        /// <param name="functionEntry">The output result</param>
        /// <returns>True if the <see cref="TemplateFunction"/> was found, false otherwise</returns>
        public bool TryGetFunction(string functionName, out TemplateFunction functionEntry) =>
            (_localFunctions != null && _localFunctions.TryGetValue(functionName, out functionEntry)) || _globalFunctions.TryGetValue(functionName, out functionEntry);
    }
}
