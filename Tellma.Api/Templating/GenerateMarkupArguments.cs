using System;
using System.Collections.Generic;
using System.Globalization;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Contains a list of template strings and all the information needed by <see cref="TemplateService"/> to parse them and evaluate them into markup text.
    /// </summary>
    public class GenerateMarkupArguments
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateMarkupArguments"/> class.
        /// </summary>
        /// <param name="templates">The array of template strings and the markup language of each one.</param>
        /// <param name="customGlobalFunctions">A dictionary of any custom global functions available during the markup generation.</param>
        /// <param name="customGlobalVariables">A dictionary of any custom global variables available during the markup generation.</param>
        /// <param name="customLocalFunctions">A dictionary of any custom local functions available during the markup generation.</param>
        /// <param name="customLocalVariables">A dictionary of any custom local variables available during the markup generation.</param>
        /// <param name="preloadedQuery">An optional query that will be available in the varilable "$" during expression evaluation.</param>
        /// <param name="culture">The culture to use when evaluating the template expressions.</param>
        public GenerateMarkupArguments(
            (string template, string language)[] templates,
            IDictionary<string, EvaluationFunction> customGlobalFunctions = null,
            IDictionary<string, EvaluationVariable> customGlobalVariables = null,
            IDictionary<string, EvaluationFunction> customLocalFunctions = null,
            IDictionary<string, EvaluationVariable> customLocalVariables = null,
            QueryInfo preloadedQuery = null,
            CultureInfo culture = null)
        {
            Templates = templates ?? throw new ArgumentNullException(nameof(templates));
            CustomGlobalFunctions = customGlobalFunctions ?? new Dictionary<string, EvaluationFunction>();
            CustomGlobalVariables = customGlobalVariables ?? new Dictionary<string, EvaluationVariable>();
            CustomLocalFunctions = customLocalFunctions ?? new Dictionary<string, EvaluationFunction>();
            CustomLocalVariables = customLocalVariables ?? new Dictionary<string, EvaluationVariable>();
            PreloadedQuery = preloadedQuery;
            Culture = culture;
        }

        /// <summary>
        /// The array of template strings and the markup language of each one.
        /// </summary>
        public (string template, string language)[] Templates { get; }

        /// <summary>
        /// A dictionary of any custom global functions available during the markup generation.
        /// <para/>
        /// Note: Global functions are available at the top level template expressions and any sub template 
        /// expressions (Such as those supplied to the function Filter(list, expr)).
        /// </summary>
        public IDictionary<string, EvaluationFunction> CustomGlobalFunctions { get; }

        /// <summary>
        /// A dictionary of any custom global variables available during the markup generation.
        /// <para/>
        /// Note: Global variables are available at the top level template expressions and any sub template 
        /// expressions (Such as those supplied to the function Filter(list, expr)).
        /// </summary>
        public IDictionary<string, EvaluationVariable> CustomGlobalVariables { get; }

        /// <summary>
        /// A dictionary of any custom local functions available during the markup generation.
        /// <para/>
        /// Note: Local functions are only available at the top level template expressions. Any sub template 
        /// expressions (Such as those supplied to the function Filter(list, expr)) will not have access to them.
        /// </summary>
        public IDictionary<string, EvaluationFunction> CustomLocalFunctions { get; }

        /// <summary>
        /// A dictionary of any custom local variables available during the markup generation.
        /// <para/>
        /// Note: Local variables are only available at the top level template expressions. Any sub template 
        /// expressions (Such as those supplied to the function Filter(list, expr)) will not have access to them.
        /// </summary>
        public IDictionary<string, EvaluationVariable> CustomLocalVariables { get; }

        /// <summary>
        /// An optional query that will be available in the varilable "$" so the template designer wouldn't have
        /// to to call the cumberson query syntax. This is used when printing from master or details screens
        /// where the query representing the displayed entity/entities is most likely going to be accessed in
        /// the template.
        /// </summary>
        public QueryInfo PreloadedQuery { get; }

        /// <summary>
        /// The culture to use when evaluating the template expressions.
        /// </summary>
        public CultureInfo Culture { get; }
    }
}
