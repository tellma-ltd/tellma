using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Tellma.Utilities.Common;

namespace Tellma.Api.Templating
{
    public enum TemplateLanguage
    {
        Text = 0,
        Html = 1
    }

    /// <summary>
    /// Contains a list of templates and all the information needed by <see cref="TemplateService"/> 
    /// to parse them and evaluate them into final blocks of text.
    /// </summary>
    public class TemplateArguments
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateArguments"/> class.
        /// </summary>
        /// <param name="customGlobalFunctions">A dictionary of any custom global functions available during the text generation.</param>
        /// <param name="customGlobalVariables">A dictionary of any custom global variables available during the text generation.</param>
        /// <param name="customLocalFunctions">A dictionary of any custom local functions available during the text generation.</param>
        /// <param name="customLocalVariables">A dictionary of any custom local variables available during the text generation.</param>
        /// <param name="culture">The culture to use when evaluating the template expressions.</param>
        public TemplateArguments(
            IDictionary<string, EvaluationFunction> customGlobalFunctions = null,
            IDictionary<string, EvaluationVariable> customGlobalVariables = null,
            IDictionary<string, EvaluationFunction> customLocalFunctions = null,
            IDictionary<string, EvaluationVariable> customLocalVariables = null,
            CultureInfo culture = null,
            DateTimeOffset? now = null)
        {
            CustomGlobalFunctions = customGlobalFunctions.Clone() ?? new Dictionary<string, EvaluationFunction>();
            CustomGlobalVariables = customGlobalVariables.Clone() ?? new Dictionary<string, EvaluationVariable>();
            CustomLocalFunctions = customLocalFunctions.Clone() ?? new Dictionary<string, EvaluationFunction>();
            CustomLocalVariables = customLocalVariables.Clone() ?? new Dictionary<string, EvaluationVariable>();
            Culture = culture;
            Now = now ?? DateTimeOffset.Now;
        }

        /// <summary>
        /// A dictionary of any custom global functions available during the text generation.
        /// <para/>
        /// Note: Global functions are available at the top level template expressions and any sub template 
        /// expressions (Such as those supplied to the function Filter(list, expr)).
        /// </summary>
        public IDictionary<string, EvaluationFunction> CustomGlobalFunctions { get; }

        /// <summary>
        /// A dictionary of any custom global variables available during the text generation.
        /// <para/>
        /// Note: Global variables are available at the top level template expressions and any sub template 
        /// expressions (Such as those supplied to the function Filter(list, expr)).
        /// </summary>
        public IDictionary<string, EvaluationVariable> CustomGlobalVariables { get; }

        /// <summary>
        /// A dictionary of any custom local functions available during the text generation.
        /// <para/>
        /// Note: Local functions are only available at the top level template expressions. Any sub template 
        /// expressions (Such as those supplied to the function Filter(list, expr)) will not have access to them.
        /// </summary>
        public IDictionary<string, EvaluationFunction> CustomLocalFunctions { get; }

        /// <summary>
        /// A dictionary of any custom local variables available during the text generation.
        /// <para/>
        /// Note: Local variables are only available at the top level template expressions. Any sub template 
        /// expressions (Such as those supplied to the function Filter(list, expr)) will not have access to them.
        /// </summary>
        public IDictionary<string, EvaluationVariable> CustomLocalVariables { get; }

        /// <summary>
        /// The culture to use when evaluating the template expressions.
        /// </summary>
        public CultureInfo Culture { get; }

        /// <summary>
        /// The time that is considered "now". This can be overriden when evaluting a
        /// template that was supposed to be evaluated a while ago.
        /// </summary>
        public DateTimeOffset Now { get; }
    }
}
