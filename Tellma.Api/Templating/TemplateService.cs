using Microsoft.Extensions.Localization;
using QRCoder;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Utilities.Calendars;
using Tellma.Utilities.Common;

namespace Tellma.Api.Templating
{
    /// <summary>
    /// Provides templating functionality. This service is scoped.
    /// </summary>
    public class TemplateService
    {
        private const string PreloadedQueryVariableName = "$";

        #region Lifecycle

        private readonly IApiClientForTemplating _client;
        private readonly IStringLocalizer _localizer;

        /// <summary>
        /// Create a new instance of the <see cref="TemplateService"/> class.
        /// </summary>
        public TemplateService(IStringLocalizer<Strings> localizer, IApiClientForTemplating client)
        {
            _client = client;
            _localizer = localizer;
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Generates the output string(s) based on the provided <see cref="TemplatePlan"/>, the plan is
        /// hierarchy of nodes that instruct the templating engine on the order, context and dependencies
        /// of evaluating the templates. The result(s) can be retrieved from the plan itself.
        /// <para/>
        /// When evaluating the template plan, the templating engine: <br/>
        /// (1) Parses the templates into abstract expression trees (ASTs). <br/>
        /// (2) Performs analysis on the ASTs to determine the required read API calls and their arguments. <br/>
        /// (3) Invokes those read API calls and loads the data. <br/>
        /// (4) Uses that data together with the ASTs to generate the final string.
        /// </summary>
        /// <param name="plan">The template plan to evaluate.</param>
        /// <param name="args">All the information needed to generate an array of blocks of string text. 
        /// This information is encapsulated in a <see cref="TemplateArguments"/>.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        public async Task GenerateFromPlan(
            TemplatePlan plan,
            TemplateArguments args = null, 
            CancellationToken cancellation = default)
        {
            if (plan == null)
            {
                return;
            }

            // (1) Create static evaluation context
            var customGlobalFunctions = args.CustomGlobalFunctions;
            var customGlobalVariables = args.CustomGlobalVariables;
            var customLocalFunctions = args.CustomLocalFunctions;
            var customLocalVariables = args.CustomLocalVariables;
            var culture = args.Culture ?? CultureInfo.CurrentUICulture;

            var env = new TemplateEnvironment
            {
                Culture = culture,
                Cancellation = cancellation,
                Localizer = _localizer
            };

            // Built-In Global Functions
            var globalFuncs = new EvaluationContext.FunctionsDictionary()
            {
                [nameof(Sum)] = Sum(),
                [nameof(Filter)] = Filter(),
                [nameof(OrderBy)] = OrderBy(),
                [nameof(Count)] = Count(),
                [nameof(Max)] = Max(),
                [nameof(Min)] = Min(),
                [nameof(SelectMany)] = SelectMany(),
                [nameof(StartsWith)] = StartsWith(),
                [nameof(EndsWith)] = EndsWith(),
                [nameof(Format)] = Format(),
                [nameof(FormatDate)] = FormatDate(env),
                [nameof(If)] = If(),
                [nameof(AmountInWords)] = AmountInWords(env),
                [nameof(Barcode)] = Barcode(),
                [nameof(SA_InvoiceQrCode)] = SA_InvoiceQrCode(),
                [nameof(Fact)] = Fact(env),
                [nameof(Aggregate)] = Aggregate(env),
                [nameof(PreviewWidth)] = PreviewWidth(),
                [nameof(PreviewHeight)] = PreviewHeight(),
                [nameof(List)] = List(),

                [nameof(ToInteger)] = ToInteger(),
                [nameof(ToDecimal)] = ToDecimal(),
                [nameof(ToDateTime)] = ToDateTime(),
                [nameof(ToDateTimeOffset)] = ToDateTimeOffset(),
                [nameof(ToBoolean)] = ToBoolean(),
                [nameof(QueryQuote)] = QueryQuote(),
                [nameof(QueryDateTime)] = QueryDateTime(),
                [nameof(QueryDateTimeOffset)] = QueryDateTimeOffset(),
            };

            // Built-In Global Variables
            var globalVars = new EvaluationContext.VariablesDictionary
            {
                ["$Now"] = new EvaluationVariable(DateTimeOffset.Now),
                ["$Lang"] = new EvaluationVariable(env.Culture.Name),
                ["$IsRtl"] = new EvaluationVariable(env.Culture.TextInfo.IsRightToLeft),
            };

            // Custom Global Functions
            foreach (var (name, func) in customGlobalFunctions)
            {
                globalFuncs.Add(name, func);
            }

            // Custom Global Variables
            foreach (var (name, variable) in customGlobalVariables)
            {
                globalVars.Add(name, variable);
            }

            var ctx = EvaluationContext.Create(globalFuncs, globalVars);

            // Custom Local Functions
            foreach (var p in customLocalFunctions)
            {
                ctx.SetLocalFunction(p.Key, p.Value);
            }

            // Custom Local Variables
            foreach (var p in customLocalVariables)
            {
                ctx.SetLocalVariable(p.Key, p.Value);
            }

            // Add the query functions as placeholders that can only determine select and paths
            ctx.SetLocalFunction(FuncNames.Entities, new EvaluationFunction(
                    function: TemplateUtil.FunctionThatThrows(FuncNames.Entities), // This is what makes it a "static" context
                    pathsResolver: (args, ctx) => EntitiesPaths(args, ctx)
                )
            );

            ctx.SetLocalFunction(FuncNames.EntityById, new EvaluationFunction(
                    function: TemplateUtil.FunctionThatThrows(FuncNames.EntityById), // This is what makes it a "static" context
                    pathsResolver: (args, ctx) => EntityByIdPaths(args, ctx)
                )
            );

            ctx.SetLocalFunction(FuncNames.EntitiesByIds, new EvaluationFunction(
                    function: TemplateUtil.FunctionThatThrows(FuncNames.EntitiesByIds), // This is what makes it a "static" context
                    pathsResolver: (args, ctx) => EntitiesByIdsPaths(args, ctx)
                )
            );

            // (2) Analyse the templates: Aggregate the queried SELECT paths into path tries and group them by query info
            var allSelectPaths = new Dictionary<QueryInfo, PathsTrie>();
            await foreach (var path in plan.ComputeSelect(ctx))
            {
                if (!allSelectPaths.TryGetValue(path.QueryInfo, out PathsTrie trie))
                {
                    trie = new PathsTrie();
                    allSelectPaths.Add(path.QueryInfo, trie);
                }

                trie.AddPath(path);
            }

            // (3) Load all the entities/data that the template needs by calling the APIs
            var apiResults = new ConcurrentDictionary<QueryInfo, object>();
            foreach (var (query, trie) in allSelectPaths)
            {
                // Prepare the select Expression for the entity/entities
                var queryPaths = trie.GetPaths();
                if (!queryPaths.Any(e => e.Length > 0))
                {
                    // Query is never accessed in the template
                    continue;
                }

                var select = string.Join(",", queryPaths.Select(p => string.Join(".", p)));

                if (query is QueryEntitiesInfo qe)
                {
                    var entities = await _client.GetEntities(query.Collection, query.DefinitionId, select, qe.Filter, qe.OrderBy, qe.Top, qe.Skip, cancellation);
                    apiResults.TryAdd(query, entities.ToList());
                }
                else if (query is QueryEntitiesByIdsInfo qeis)
                {
                    var entities = await _client.GetEntitiesByIds(query.Collection, query.DefinitionId, select, qeis.Ids, cancellation);
                    apiResults.TryAdd(query, entities.ToList());
                }
                else if (query is QueryEntityByIdInfo qei)
                {
                    var entity = await _client.GetEntityById(query.Collection, query.DefinitionId, select, qei.Id, cancellation);
                    apiResults.TryAdd(query, entity);
                }
                else
                {
                    throw new TemplateException($"Unknown implementation of query type '{query?.GetType()?.Name}'."); // Future proofing
                }
            }

            // (4) Make the evaluation context non-static
            foreach (var (query, result) in apiResults)
            {
                ctx.SetApiResult(query, result);
            }

            ctx.SetLocalFunction(FuncNames.Entities, new EvaluationFunction(function: EntitiesImpl));
            ctx.SetLocalFunction(FuncNames.EntityById, new EvaluationFunction(function: EntityByIdImpl));
            ctx.SetLocalFunction(FuncNames.EntitiesByIds, new EvaluationFunction(function: EntitiesByIdsImpl));

            // (4) Generate the final output(s) using the now non-static context
            using var _ = new CultureScope(culture);
            await plan.GenerateOutputs(ctx);
        }

        //public async Task<object> EvaluateExpression(
        //    TemplexInfo expression,
        //    TemplateArguments args,
        //    CancellationToken cancellation)
        //{
        //    var singleton = new TemplexInfo[] { expression };
        //    var results = await EvaluateExpressions(singleton, args, cancellation);
        //    return results[0];
        //}

        //public async Task<object[]> EvaluateExpressions(
        //    TemplexInfo[] expressions,
        //    TemplateArguments args,
        //    CancellationToken cancellation)
        //    {
        //        expressions ??= Array.Empty<TemplexInfo>();

        //        // (1) Parse the expressions into abstract syntax trees
        //        TemplexBase[] templexes = new TemplexBase[expressions.Length];
        //        for (int i = 0; i < expressions.Length; i++)
        //        {
        //            var templexInfo = expressions[i];
        //            var templex = TemplexBase.Parse(templexInfo.Expression);

        //            templexes[i] = templex;
        //        }

        //        // (2) Create the static context
        //        var ctx = await CreateContext(templexes, args, cancellation);

        //        // (3) Generate the final string using the now non-static context
        //        var culture = GetCulture(args);
        //        using var _ = new CultureScope(culture);

        //        var outputs = new object[templexes.Length];
        //        for (int i = 0; i < templexes.Length; i++)
        //        {
        //            if (templexes[i] != null)
        //            {
        //                var value = await templexes[i].Evaluate(ctx);
        //                outputs[i] = value;
        //            }
        //        }

        //        // Return the result
        //        return outputs;
        //    }

        ///// <summary>
        ///// Generates a list of strings based on a list of templates.
        ///// <para/>
        ///// It does so using the following steps: <br/>
        ///// (1) Parses the templates into abstract expression trees (ASTs). <br/>
        ///// (2) Performs analysis on the ASTs to determine the required read API calls and their arguments. <br/>
        ///// (3) Invokes those read API calls and loads the data. <br/>
        ///// (4) Uses that data together with the ASTs to generate the final string.
        ///// </summary>
        ///// <param name="templates">The array of template strings and the language of each one.</param>
        ///// <param name="args">All the information needed to generate an array of blocks of string text. 
        ///// This information is encapsulated in a <see cref="TemplateArguments"/>.</param>
        ///// <param name="cancellation">The cancellation instruction.</param>
        ///// <returns>An array, equal in size to the supplied <see cref="TemplateArguments.Templates"/> array, 
        ///// where each output is matched to each input by the array index.</returns>
        //public async Task<string[]> GenerateFromTemplates(
        //    TemplateInfo[] templates,
        //    TemplateArguments args,
        //    CancellationToken cancellation)
        //{
        //    templates ??= Array.Empty<TemplateInfo>();

        //    // (1) Parse the templates into abstract syntax trees
        //    TemplateTree[] trees = new TemplateTree[templates.Length];
        //    for (int i = 0; i < templates.Length; i++)
        //    {
        //        var templateInfo = templates[i];
        //        var tree = TemplateTree.Parse(templateInfo.Template);

        //        // If a context is provided, wrap the tree inside a StructureDefine 
        //        if (!string.IsNullOrWhiteSpace(templateInfo.Context))
        //        {
        //            var varAssignment = new StructureDefine
        //            {
        //                VariableName = PreloadedQueryVariableName,
        //                Value = TemplexBase.Parse(templateInfo.Context.Trim()),
        //                Template = tree,
        //            };

        //            tree = new TemplateTree();
        //            tree.Contents.Add(varAssignment);
        //        }

        //        trees[i] = tree;
        //    }

        //    // (2) Create the static context
        //    var ctx = await CreateContext(trees, args, cancellation);

        //    // (3) Generate the final string using the now non-static context
        //    var culture = GetCulture(args);
        //    using var _ = new CultureScope(culture);

        //    var outputs = new string[trees.Length];
        //    for (int i = 0; i < trees.Length; i++)
        //    {
        //        if (trees[i] != null)
        //        {
        //            var builder = new StringBuilder();
        //            Func<string, string> encodeFunc = templates[i].Language switch
        //            {
        //                TemplateLanguage.Html => HtmlEncoder.Default.Encode,
        //                TemplateLanguage.Text => s => s, // No need to encode anything for a text output
        //                _ => s => s,
        //            };

        //            await trees[i].GenerateOutput(builder, ctx, encodeFunc);
        //            outputs[i] = builder.ToString();
        //        }
        //    }

        //    // Return the result
        //    return outputs;
        //}

        /// <summary>
        /// Generates a string based on a template.
        /// <para/>
        /// It does so using the following steps: <br/>
        /// (1) Parses the templates into abstract expression trees (ASTs). <br/>
        /// (2) Performs analysis on the ASTs to determine the required read API calls and their arguments. <br/>
        /// (3) Invokes those read API calls and loads the data. <br/>
        /// (4) Uses that data together with the ASTs to generate the final string.
        /// </summary>
        /// <param name="template">The template to evaluate.</param>
        /// <param name="language">The <see cref="TemplateLanguage"/> of the template to evaluate.</param>
        /// <param name="args">All the information needed to generate an array of blocks of string text. 
        /// This information is encapsulated in a <see cref="TemplateArguments"/>.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>An array, equal in size to the supplied <see cref="TemplateArguments.Templates"/> array, 
        /// where each output is matched to each input by the array index.</returns>
        public async Task<string> GenerateFromTemplate(
            string template,
            TemplateLanguage language = TemplateLanguage.Text,
            TemplateArguments args = null,
            CancellationToken cancellation = default)
        {
            var plan = new TemplatePlanLeaf(template, language);
            await GenerateFromPlan(plan, args, cancellation);
            return plan.Outputs[0];
        }

        //private static CultureInfo GetCulture(TemplateArguments args)
        //{
        //    return args.Culture ?? CultureInfo.CurrentUICulture;
        //}

        //private async Task<EvaluationContext> CreateContext(IEnumerable<TemplateBase> templates, TemplateArguments args, CancellationToken cancellation)
        //{
        //    var customGlobalFunctions = args.CustomGlobalFunctions;
        //    var customGlobalVariables = args.CustomGlobalVariables;
        //    var customLocalFunctions = args.CustomLocalFunctions;
        //    var customLocalVariables = args.CustomLocalVariables;
        //    var preloadedQuery = args.PreloadedQuery;
        //    var culture = GetCulture(args);

        //    var env = new TemplateEnvironment
        //    {
        //        Culture = culture,
        //        Cancellation = cancellation,
        //        Localizer = _localizer
        //    };

        //    // Built-In Global Functions
        //    var globalFuncs = new EvaluationContext.FunctionsDictionary()
        //    {
        //        [nameof(Sum)] = Sum(),
        //        [nameof(Filter)] = Filter(),
        //        [nameof(OrderBy)] = OrderBy(),
        //        [nameof(Count)] = Count(),
        //        [nameof(Max)] = Max(),
        //        [nameof(Min)] = Min(),
        //        [nameof(SelectMany)] = SelectMany(),
        //        [nameof(StartsWith)] = StartsWith(),
        //        [nameof(EndsWith)] = EndsWith(),
        //        [nameof(Format)] = Format(),
        //        [nameof(FormatDate)] = FormatDate(env),
        //        [nameof(If)] = If(),
        //        [nameof(AmountInWords)] = AmountInWords(env),
        //        [nameof(Barcode)] = Barcode(),
        //        [nameof(SA_InvoiceQrCode)] = SA_InvoiceQrCode(),
        //        [nameof(Fact)] = Fact(env),
        //        [nameof(Aggregate)] = Aggregate(env),
        //        [nameof(PreviewWidth)] = PreviewWidth(),
        //        [nameof(PreviewHeight)] = PreviewHeight(),

        //        [nameof(ToInteger)] = ToInteger(),
        //        [nameof(ToDecimal)] = ToDecimal(),
        //        [nameof(ToDateTime)] = ToDateTime(),
        //        [nameof(ToDateTimeOffset)] = ToDateTimeOffset(),
        //        [nameof(ToBoolean)] = ToBoolean(),
        //        [nameof(QueryQuote)] = QueryQuote(),
        //        [nameof(QueryDateTime)] = QueryDateTime(),
        //        [nameof(QueryDateTimeOffset)] = QueryDateTimeOffset(),
        //    };

        //    // Built-In Global Variables
        //    var globalVars = new EvaluationContext.VariablesDictionary
        //    {
        //        ["$Now"] = new EvaluationVariable(DateTimeOffset.Now),
        //        ["$Lang"] = new EvaluationVariable(env.Culture.Name),
        //        ["$IsRtl"] = new EvaluationVariable(env.Culture.TextInfo.IsRightToLeft),
        //    };

        //    // Custom Global Functions
        //    foreach (var (name, func) in customGlobalFunctions)
        //    {
        //        globalFuncs.Add(name, func);
        //    }

        //    // Custom Global Variables
        //    foreach (var (name, variable) in customGlobalVariables)
        //    {
        //        globalVars.Add(name, variable);
        //    }

        //    var ctx = EvaluationContext.Create(globalFuncs, globalVars);

        //    // Custom Local Functions
        //    foreach (var p in customLocalFunctions)
        //    {
        //        ctx.SetLocalFunction(p.Key, p.Value);
        //    }

        //    // Custom Local Variables
        //    foreach (var p in customLocalVariables)
        //    {
        //        ctx.SetLocalVariable(p.Key, p.Value);
        //    }

        //    // Optional Preloaded Query
        //    if (preloadedQuery != null)
        //    {
        //        ctx.SetLocalVariable(PreloadedQueryVariableName, new EvaluationVariable(
        //            eval: TemplateUtil.VariableThatThrows(varName: PreloadedQueryVariableName), // This is what makes it a "static" context
        //            pathsResolver: () => AsyncUtil.Singleton(Path.Empty(preloadedQuery))
        //        ));
        //    }

        //    // Add the query functions as placeholders that can only determine select and paths
        //    ctx.SetLocalFunction(FuncNames.Entities, new EvaluationFunction(
        //            function: TemplateUtil.FunctionThatThrows(FuncNames.Entities), // This is what makes it a "static" context
        //            pathsResolver: (args, ctx) => EntitiesPaths(args, ctx)
        //        )
        //    );

        //    ctx.SetLocalFunction(FuncNames.EntityById, new EvaluationFunction(
        //            function: TemplateUtil.FunctionThatThrows(FuncNames.EntityById), // This is what makes it a "static" context
        //            pathsResolver: (args, ctx) => EntityByIdPaths(args, ctx)
        //        )
        //    );

        //    ctx.SetLocalFunction(FuncNames.EntitiesByIds, new EvaluationFunction(
        //            function: TemplateUtil.FunctionThatThrows(FuncNames.EntitiesByIds), // This is what makes it a "static" context
        //            pathsResolver: (args, ctx) => EntitiesByIdsPaths(args, ctx)
        //        )
        //    );

        //    // (3) Analyse the AST: Aggregate the queried SELECT paths into path tries and group them by query info
        //    var allSelectPaths = new Dictionary<QueryInfo, PathsTrie>();
        //    foreach (var tree in templates.Where(e => e != null))
        //    {
        //        await foreach (var path in tree.ComputeSelect(ctx))
        //        {
        //            if (!allSelectPaths.TryGetValue(path.QueryInfo, out PathsTrie trie))
        //            {
        //                trie = new PathsTrie();
        //                allSelectPaths.Add(path.QueryInfo, trie);
        //            }

        //            trie.AddPath(path);
        //        }
        //    }

        //    // (4): Load all the entities/data that the template needs by calling the APIs
        //    var apiResults = new ConcurrentDictionary<QueryInfo, object>();
        //    foreach (var (query, trie) in allSelectPaths)
        //    {
        //        // Prepare the select Expression for the entity/entities
        //        var queryPaths = trie.GetPaths();
        //        if (!queryPaths.Any(e => e.Length > 0))
        //        {
        //            // Query is never accessed in the template
        //            continue;
        //        }

        //        var select = string.Join(",", queryPaths.Select(p => string.Join(".", p)));

        //        if (query is QueryEntitiesInfo qe)
        //        {
        //            var entities = await _client.GetEntities(query.Collection, query.DefinitionId, select, qe.Filter, qe.OrderBy, qe.Top, qe.Skip, cancellation);
        //            apiResults.TryAdd(query, entities.ToList());
        //        }
        //        else if (query is QueryEntitiesByIdsInfo qeis)
        //        {
        //            var entities = await _client.GetEntitiesByIds(query.Collection, query.DefinitionId, select, qeis.Ids, cancellation);
        //            apiResults.TryAdd(query, entities.ToList());
        //        }
        //        else if (query is QueryEntityByIdInfo qei)
        //        {
        //            var entity = await _client.GetEntityById(query.Collection, query.DefinitionId, select, qei.Id, cancellation);
        //            apiResults.TryAdd(query, entity);
        //        }
        //        else
        //        {
        //            throw new TemplateException($"Unknown implementation of query type '{query?.GetType()?.Name}'."); // Future proofing
        //        }
        //    }

        //    // Make the context non-static: replace the placeholder query functions with ones that read data from the loaded entities/data
        //    if (preloadedQuery != null && apiResults.TryGetValue(preloadedQuery, out object value))
        //    {
        //        ctx.SetLocalVariable(PreloadedQueryVariableName, new EvaluationVariable(value: value));
        //    }

        //    ctx.SetLocalFunction(FuncNames.Entities, new EvaluationFunction(function: (args, ctx) => EntitiesImpl(args, apiResults)));
        //    ctx.SetLocalFunction(FuncNames.EntityById, new EvaluationFunction(function: (args, ctx) => EntityByIdImpl(args, apiResults)));
        //    ctx.SetLocalFunction(FuncNames.EntitiesByIds, new EvaluationFunction(function: (args, ctx) => EntitiesByIdsImpl(args, apiResults)));

        //    return ctx;
        //}

        #endregion

        #region Special Queries

        #region Entities

        private static object EntitiesImpl(object[] args, EvaluationContext ctx)
        {
            var queryInfo = QueryEntitiesInfo(args);
            if (!ctx.TryGetApiResult(queryInfo, out object result))
            {
                throw new InvalidOperationException("Loading a query with no precalculated select."); // This is a bug
            }

            return result;
        }

        private static async IAsyncEnumerable<Path> EntitiesPaths(TemplexBase[] args, EvaluationContext ctx)
        {
            int i = 0;

            var sourceObj = args.Length > i ? await args[i++].Evaluate(ctx) : null;
            var filterObj = args.Length > i ? await args[i++].Evaluate(ctx) : null;
            var orderbyObj = args.Length > i ? await args[i++].Evaluate(ctx) : null;
            var topObj = args.Length > i ? await args[i++].Evaluate(ctx) : null;
            var skipObj = args.Length > i ? await args[i++].Evaluate(ctx) : null;

            var queryInfo = QueryEntitiesInfo(sourceObj, filterObj, orderbyObj, topObj, skipObj);
            yield return Path.Empty(queryInfo);
        }

        private static QueryEntitiesInfo QueryEntitiesInfo(params object[] args)
        {
            int argCount = 5;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{FuncNames.Entities}' expects {argCount} arguments.");
            }

            int i = 0;
            var sourceObj = args[i++];
            var filterObj = args[i++];
            var orderbyObj = args[i++];
            var topObj = args[i++];
            var skipObj = args[i++];

            var (collection, definitionId) = DeconstructSource(sourceObj, FuncNames.Entities);

            string filter;
            if (filterObj is null || filterObj is string) // Optional
            {
                filter = filterObj as string;
            }
            else
            {
                throw new TemplateException($"Function '{FuncNames.Entities}' expects a 2nd parameter filter of type string.");
            }

            string orderby;
            if (orderbyObj is null || orderbyObj is string) // Optional
            {
                orderby = orderbyObj as string;
            }
            else
            {
                throw new TemplateException($"Function '{FuncNames.Entities}' expects a 3rd parameter orderby of type string.");
            }

            int? top;
            if (topObj is null || topObj is int) // Optional
            {
                top = topObj as int?;
            }
            else
            {
                throw new TemplateException($"Function '{FuncNames.Entities}' requires a 4th parameter top of type int.");
            }

            int? skip;
            if (skipObj is null || skipObj is int) // Optional
            {
                skip = skipObj as int?;
            }
            else
            {
                throw new TemplateException($"Function '{FuncNames.Entities}' expects a 5th parameter skip of type int.");
            }

            return new QueryEntitiesInfo(
                collection: collection,
                definitionId: definitionId,
                filter: filter,
                orderby: orderby,
                top: top,
                skip: skip);
        }

        #endregion

        #region EntitiesByIds

        private static object EntitiesByIdsImpl(object[] args, EvaluationContext ctx)
        {
            var queryInfo = QueryEntitiesByIdsInfo(args);
            if (!ctx.TryGetApiResult(queryInfo, out object result))
            {
                throw new InvalidOperationException("Loading a query with no precalculated select."); // This is a bug
            }

            return result;
        }

        private static async IAsyncEnumerable<Path> EntitiesByIdsPaths(TemplexBase[] args, EvaluationContext ctx)
        {
            int i = 0;

            var sourceObj = args.Length > i ? await args[i++].Evaluate(ctx) : null;
            var idsObj = args.Length > i ? await args[i++].Evaluate(ctx) : null;

            var queryInfo = QueryEntitiesByIdsInfo(sourceObj, idsObj);
            yield return Path.Empty(queryInfo);
        }

        private static QueryEntitiesByIdsInfo QueryEntitiesByIdsInfo(params object[] args)
        {
            int argCount = 2;
            if (args.Length < argCount)
            {
                throw new TemplateException($"Function '{FuncNames.EntitiesByIds}' expects {argCount} arguments.");
            }

            int i = 0;
            var sourceObj = args[i++];
            var idsObj = args[i++];

            var (collection, definitionId) = DeconstructSource(sourceObj, FuncNames.EntitiesByIds);

            IList ids;
            if (idsObj is null || idsObj is IList) // Optional
            {
                ids = idsObj as IList;
            }
            else
            {
                throw new TemplateException($"Function '{FuncNames.EntitiesByIds}' expects a 2nd parameter ids of type List.");
            }

            return new QueryEntitiesByIdsInfo(
                collection: collection,
                definitionId: definitionId,
                ids: ids);
        }

        #endregion

        #region EntityById

        private static object EntityByIdImpl(object[] args, EvaluationContext ctx)
        {
            var queryInfo = QueryByIdInfo(args);
            if (!ctx.TryGetApiResult(queryInfo, out object result))
            {
                throw new InvalidOperationException("Loading a query with no precalculated select."); // This is a bug
            }

            return result;
        }

        private static async IAsyncEnumerable<Path> EntityByIdPaths(TemplexBase[] args, EvaluationContext ctx)
        {
            int i = 0;

            var sourceObj = args.Length > i ? await args[i++].Evaluate(ctx) : null;
            var idObj = args.Length > i ? await args[i++].Evaluate(ctx) : null;

            var queryInfo = QueryByIdInfo(sourceObj, idObj);
            yield return Path.Empty(queryInfo);
        }

        private static QueryEntityByIdInfo QueryByIdInfo(params object[] args)
        {
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{FuncNames.EntityById}' expects {argCount} arguments.");
            }

            var sourceObj = args[0];
            var id = args[1];

            var (collection, definitionId) = DeconstructSource(sourceObj, FuncNames.EntityById);


            // Id
            if (id is null) // Required
            {
                throw new TemplateException($"Function '{FuncNames.EntityById}' expects an id argument that isn't null.");
            }

            return new QueryEntityByIdInfo(
                collection: collection,
                definitionId: definitionId,
                id: id);
        }

        #endregion

        private static (string collection, int? definitionId) DeconstructSource(object sourceObj, string funcName)
        {
            string collection;
            int? definitionId = null;
            if (sourceObj is string source) // Required
            {
                var split = source.Split("/");
                collection = split.First();
                var definitionIdString = split.Length > 1 ? string.Join("/", split.Skip(1)) : null;
                if (definitionIdString != null)
                {
                    if (int.TryParse(definitionIdString, out int definitionIdInt))
                    {
                        definitionId = definitionIdInt;
                    }
                    else
                    {
                        throw new TemplateException($"Function '{funcName}' could not interpret the definitionId '{definitionIdString}' as an integer.");
                    }
                }
            }
            else
            {
                throw new TemplateException($"Function '{funcName}' requires a 1st parameter source of type string.");
            }

            return (collection, definitionId);
        }

        #endregion

        #region Global Functions

        #region Fact

        private EvaluationFunction Fact(TemplateEnvironment env)
        {
            return new EvaluationFunction(
                functionAsync: (args, ctx) => FactImpl(args, ctx, env));
        }

        private async Task<object> FactImpl(object[] args, EvaluationContext ctx, TemplateEnvironment env)
        {
            int argCount = 6;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(Fact)}' expects {argCount} arguments: (source, select, filter, orderby, top, skip).");
            }

            int i = 0;
            var sourceObj = args[i++];
            var selectObj = args[i++];
            var filterObj = args[i++];
            var orderbyObj = args[i++];
            var topObj = args[i++];
            var skipObj = args[i++];

            var (collection, definitionId) = DeconstructSource(sourceObj, nameof(Fact));

            string select;
            if (selectObj is null || selectObj is string) // Optional
            {
                select = selectObj as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Fact)}' expects a 2nd parameter select of type string.");
            }

            string filter;
            if (filterObj is null || filterObj is string) // Optional
            {
                filter = filterObj as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Fact)}' expects a 3rd parameter filter of type string.");
            }

            string orderby;
            if (orderbyObj is null || orderbyObj is string) // Optional
            {
                orderby = orderbyObj as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Fact)}' expects a 4th parameter orderby of type string.");
            }

            int? top;
            if (topObj is null || topObj is int) // Optional
            {
                top = topObj as int?;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Fact)}' requires a 5th parameter top of type int.");
            }

            int? skip;
            if (skipObj is null || skipObj is int) // Optional
            {
                skip = skipObj as int?;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Fact)}' expects a 6th parameter skip of type int.");
            }

            return await _client.GetFact(collection, definitionId, select, filter, orderby, top, skip, env.Cancellation);
        }

        #endregion

        #region Aggregate

        private EvaluationFunction Aggregate(TemplateEnvironment env)
        {
            return new EvaluationFunction(
                functionAsync: (args, ctx) => AggregateImpl(args, ctx, env));
        }

        private async Task<object> AggregateImpl(object[] args, EvaluationContext ctx, TemplateEnvironment env)
        {
            int argCount = 6;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(Aggregate)}' expects {argCount} arguments: (source, select, filter, having, orderby, top).");
            }

            int i = 0;
            var sourceObj = args[i++];
            var selectObj = args[i++];
            var filterObj = args[i++];
            var havingObj = args[i++];
            var orderbyObj = args[i++];
            var topObj = args[i++];

            var (collection, definitionId) = DeconstructSource(sourceObj, nameof(Aggregate));

            string select;
            if (selectObj is null || selectObj is string) // Optional
            {
                select = selectObj as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Aggregate)}' expects a 2nd parameter select of type string.");
            }

            string filter;
            if (filterObj is null || filterObj is string) // Optional
            {
                filter = filterObj as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Aggregate)}' expects a 3rd parameter filter of type string.");
            }

            string having;
            if (havingObj is null || havingObj is string) // Optional
            {
                having = havingObj as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Aggregate)}' expects a 4th parameter having of type string.");
            }

            string orderby;
            if (orderbyObj is null || orderbyObj is string) // Optional
            {
                orderby = orderbyObj as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Aggregate)}' expects a 5th parameter orderby of type string.");
            }

            int? top;
            if (topObj is null || topObj is int) // Optional
            {
                top = topObj as int?;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Aggregate)}' requires a 6th parameter top of type int.");
            }

            return await _client.GetAggregate(collection, definitionId, select, filter, having, orderby, top, env.Cancellation);
        }

        #endregion

        #region Filter

        private EvaluationFunction Filter()
        {
            return new EvaluationFunction(
                functionAsync: FilterImpl,
                additionalSelectResolver: (TemplexBase[] args, EvaluationContext ctx) => FilterSelect(args, ctx),
                pathsResolver: (TemplexBase[] args, EvaluationContext ctx) => FilterPaths(args, ctx));
        }

        private async Task<object> FilterImpl(object[] args, EvaluationContext ctx)
        {
            // Get arguments
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(Filter)}' expects {argCount} arguments: (items, filter).");
            }

            if (args[0] is not IList items)
            {
                throw new TemplateException($"Function '{nameof(Filter)}' expects a 1st argument list of type List.");
            }

            if (args[1] is not string conditionString)
            {
                throw new TemplateException($"Function '{nameof(Filter)}' expects a 2nd argument condition of type string.");
            }

            var conditionExp = TemplexBase.Parse(conditionString) ??
                throw new TemplateException($"Function '{nameof(Filter)}' 2nd parameter cannot be an empty string.");

            var result = new List<object>();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                // Clone an empty context and set a single local variable
                var scopedCtx = ctx.CloneWithoutLocals();
                scopedCtx.SetLocalVariable("$", new EvaluationVariable(value: item));

                var conditionValueObj = await conditionExp.Evaluate(scopedCtx) ?? false;
                if (conditionValueObj is not bool conditionValue)
                {
                    throw new TemplateException($"Selector '{conditionString}' must evaluate to a boolean value.");
                }

                if (conditionValue)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private async IAsyncEnumerable<Path> FilterSelect(TemplexBase[] args, EvaluationContext ctx)
        {
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(Filter)}' expects {argCount} arguments.");
            }

            // Get the list expression
            var listExp = args[0];

            // Get and parse the value selector expression
            var conditionParameterExp = args[1];
            var conditionObj = await conditionParameterExp.Evaluate(ctx);
            if (conditionObj is not string conditionString)
            {
                throw new TemplateException($"Function '{nameof(Filter)}' expects a 2nd argument condition of type string.");
            }

            var conditionExp = TemplexBase.Parse(conditionString) ??
                throw new TemplateException($"Function '{nameof(Filter)}' 2nd parameter cannot be an empty string.");

            // Remove local variables and functions and add one $ variable
            var scopedCtx = ctx.CloneWithoutLocals();
            scopedCtx.SetLocalVariable("$", new EvaluationVariable(
                    eval: TemplateUtil.VariableThatThrows("$"),
                    pathsResolver: () => listExp.ComputePaths(ctx))); // Use the paths of listExp as the paths of the $ variable

            // Return the selects of the inner expression
            await foreach (var path in conditionExp.ComputeSelect(scopedCtx))
            {
                yield return path;
            }
        }

        private IAsyncEnumerable<Path> FilterPaths(TemplexBase[] args, EvaluationContext ctx)
        {
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(Filter)}' expects {argCount} arguments.");
            }

            var listExp = args[0];
            return listExp.ComputePaths(ctx);
        }

        #endregion

        #region OrderBy

        private EvaluationFunction OrderBy()
        {
            return new EvaluationFunction(
                functionAsync: (object[] args, EvaluationContext ctx) => OrderByImpl(args, ctx),
                additionalSelectResolver: (TemplexBase[] args, EvaluationContext ctx) => OrderBySelect(args, ctx),
                pathsResolver: (TemplexBase[] args, EvaluationContext ctx) => OrderByPaths(args, ctx));
        }

        private async Task<object> OrderByImpl(object[] args, EvaluationContext ctx)
        {
            // Get arguments
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(OrderBy)}' expects {argCount} arguments: (list, selector).");
            }

            if (args[0] is not IList items)
            {
                throw new TemplateException($"Function '{nameof(OrderBy)}' expects a 1st argument list of type List.");
            }

            if (args[1] is not string selectorExpString)
            {
                throw new TemplateException($"Function '{nameof(OrderBy)}' expects a 2nd argument selector of type string.");
            }

            var selectorExp = TemplexBase.Parse(selectorExpString) ??
                throw new TemplateException($"Function '{nameof(OrderBy)}' 2nd parameter cannot be an empty string.");

            // Retrieve the selected value on which the sorting happens
            var selections = new object[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                // Clone an empty context and set a single local variable
                var scopedCtx = ctx.CloneWithoutLocals();
                scopedCtx.SetLocalVariable("$", new EvaluationVariable(value: item));

                selections[i] = await selectorExp.Evaluate(scopedCtx);
            }

            // Sort using linq
            var result = items.Cast<object>() // Change to IEnumerable<object>
                .Select((item, index) => (item, index)) // Remember the index
                .OrderBy(e => selections[e.index]) // sort
                .Select(e => e.item) // Forget the index
                .ToList();

            return result;
        }

        private async IAsyncEnumerable<Path> OrderBySelect(TemplexBase[] args, EvaluationContext ctx)
        {
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(OrderBy)}' expects {argCount} arguments.");
            }

            // Get the list expression
            var listExp = args[0];

            // Get and parse the value selector expression
            var selectorParameterExp = args[1];
            var selectorObj = await selectorParameterExp.Evaluate(ctx);
            if (selectorObj is not string selectorString)
            {
                throw new TemplateException($"Function '{nameof(OrderBy)}' expects a 2nd argument selector of type string.");
            }

            var selectorExp = TemplexBase.Parse(selectorString) ??
                throw new TemplateException($"Function '{nameof(OrderBy)}' 2nd parameter cannot be an empty string.");

            // Remove local variables and functions and add one $ variable
            var scopedCtx = ctx.CloneWithoutLocals();
            scopedCtx.SetLocalVariable("$", new EvaluationVariable(
                    eval: TemplateUtil.VariableThatThrows("$"),
                    pathsResolver: () => listExp.ComputePaths(ctx))); // Use the paths of listExp as the paths of the $ variable

            // Return the selects of the inner expression
            await foreach (var path in selectorExp.ComputeSelect(scopedCtx))
            {
                yield return path;
            }
        }

        private IAsyncEnumerable<Path> OrderByPaths(TemplexBase[] args, EvaluationContext ctx)
        {
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(OrderBy)}' expects {argCount} arguments.");
            }

            var listExp = args[0];
            return listExp.ComputePaths(ctx);
        }

        #endregion

        #region SelectMany

        private EvaluationFunction SelectMany()
        {
            return new EvaluationFunction(
                functionAsync: (object[] args, EvaluationContext ctx) => SelectManyImpl(args, ctx),
                additionalSelectResolver: (TemplexBase[] args, EvaluationContext ctx) => SelectManySelect(args, ctx),
                pathsResolver: (TemplexBase[] args, EvaluationContext ctx) => SelectManyPaths(args, ctx));
        }

        private async Task<object> SelectManyImpl(object[] args, EvaluationContext ctx)
        {
            // Get arguments
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(SelectMany)}' expects {argCount} arguments: (list, selector).");
            }

            if (args[0] is not IList items)
            {
                throw new TemplateException($"Function '{nameof(SelectMany)}' expects a 1st argument list of type List.");
            }

            if (args[1] is not string selectorExpString)
            {
                throw new TemplateException($"Function '{nameof(SelectMany)}' expects a 2nd argument selector of type string.");
            }

            var selectorExp = TemplexBase.Parse(selectorExpString) ??
                throw new TemplateException($"Function '{nameof(SelectMany)}' 2nd parameter cannot be an empty string.");

            var result = new List<object>();
            for (int i = 0; i < items.Count; i++)
            {
                var parentItem = items[i];

                // Clone an empty context and set a single local variable
                var scopedCtx = ctx.CloneWithoutLocals();
                scopedCtx.SetLocalVariable("$", new EvaluationVariable(value: parentItem));

                var childListObj = await selectorExp.Evaluate(scopedCtx) ?? false;
                if (childListObj is IList childList)
                {
                    foreach (var childItem in childList)
                    {
                        result.Add(childItem);
                    }
                }
                else if (!(childListObj is null))
                {
                    throw new TemplateException($"Selector '{selectorExpString}' must evaluate to a list value.");
                }
            }

            return result;
        }

        private async IAsyncEnumerable<Path> SelectManySelect(TemplexBase[] args, EvaluationContext ctx)
        {
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(SelectMany)}' expects {argCount} arguments.");
            }

            // Get the list expression
            var listExp = args[0];

            // Get and parse the value selector expression
            var selectorExpExp = args[1];
            var selectorExpObj = await selectorExpExp.Evaluate(ctx);
            if (selectorExpObj is not string selectorExpString)
            {
                throw new TemplateException($"Function '{nameof(SelectMany)}' expects a 2nd argument selector of type string.");
            }

            var selectorExp = TemplexBase.Parse(selectorExpString) ??
                throw new TemplateException($"Function '{nameof(SelectMany)}' 2nd parameter cannot be an empty string.");

            // Remove local variables and functions and add one $ variable
            var scopedCtx = ctx.CloneWithoutLocals();
            scopedCtx.SetLocalVariable("$", new EvaluationVariable(
                    eval: TemplateUtil.VariableThatThrows("$"),
                    pathsResolver: () => listExp.ComputePaths(ctx))); // Use the paths of listExp as the paths of the $ variable

            // Return the selects of the inner expression
            await foreach (var path in selectorExp.ComputeSelect(scopedCtx))
            {
                yield return path;
            }
        }

        private async IAsyncEnumerable<Path> SelectManyPaths(TemplexBase[] args, EvaluationContext ctx)
        {
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(SelectMany)}' expects {argCount} arguments.");
            }

            var listExp = args[0];

            // Get and parse the value selector expression
            var selectorExpExp = args[1];
            var selectorExpObj = await selectorExpExp.Evaluate(ctx);
            if (selectorExpObj is not string selectorExpString)
            {
                throw new TemplateException($"Function '{nameof(SelectMany)}' expects a 2nd argument selector of type string.");
            }

            var selectorExp = TemplexBase.Parse(selectorExpString) ??
                throw new TemplateException($"Function '{nameof(SelectMany)}' 2nd parameter cannot be an empty string.");

            // Remove local variables and functions and add one $ variable
            var scopedCtx = ctx.CloneWithoutLocals();
            scopedCtx.SetLocalVariable("$", new EvaluationVariable(
                    eval: TemplateUtil.VariableThatThrows("$"),
                    pathsResolver: () => listExp.ComputePaths(ctx))); // Use the paths of listExp as the paths of the $ variable

            // Return the paths of the inner expression
            await foreach (var path in selectorExp.ComputePaths(scopedCtx))
            {
                yield return path;
            }
        }

        #endregion

        #region Sum

        private EvaluationFunction Sum()
        {
            return new EvaluationFunction(
                functionAsync: SumImpl,
                additionalSelectResolver: SumSelect);
        }

        private async Task<object> SumImpl(object[] args, EvaluationContext ctx)
        {
            // Get arguments
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(Sum)}' expects {argCount} arguments: (list, selector).");
            }

            if (args[0] is not IList items)
            {
                throw new TemplateException($"Function '{nameof(Sum)}' expects a 1st argument list of type List.");
            }

            if (args[1] is not string valueSelectorString)
            {
                throw new TemplateException($"Function '{nameof(Sum)}' expects a 2nd argument selector of type string.");
            }

            var valueSelectorExp = TemplexBase.Parse(valueSelectorString) ??
                throw new TemplateException($"Function '{nameof(Sum)}' 2nd parameter cannot be an empty string.");

            Type commonType = null;
            object sum = 0;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                // Clone an empty context and set a single local variable
                var scopedCtx = ctx.CloneWithoutLocals();
                scopedCtx.SetLocalVariable("$", new EvaluationVariable(value: item));

                var valueObj = await valueSelectorExp.Evaluate(scopedCtx);
                if (!(valueObj is null))
                {
                    commonType ??= NumericUtil.CommonNumericType(sum, valueObj) ??
                        throw new TemplateException($"Selector '{valueSelectorString}' must evaluate to a numeric type.");

                    sum = NumericUtil.Add(sum, valueObj, commonType);
                }
            }

            return sum;
        }

        private async IAsyncEnumerable<Path> SumSelect(TemplexBase[] args, EvaluationContext ctx)
        {
            // Get arguments
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(Sum)}' expects {argCount} arguments.");
            }

            // Get the list expression
            var listExp = args[0];

            // Get and parse the value selector expression
            var valueSelectorParameterExp = args[1];
            var valueSelectorObj = await valueSelectorParameterExp.Evaluate(ctx);
            if (valueSelectorObj is not string valueSelectorString)
            {
                throw new TemplateException($"Function '{nameof(Sum)}' expects a 2nd argument selector of type string.");
            }

            var valueSelectorExp = TemplexBase.Parse(valueSelectorString) ??
                throw new TemplateException($"Function '{nameof(Sum)}' 2nd parameter cannot be an empty string.");


            // Remove local variables and functions and add one $ variable
            var scopedCtx = ctx.CloneWithoutLocals();
            scopedCtx.SetLocalVariable("$", new EvaluationVariable(
                    eval: TemplateUtil.VariableThatThrows("$"),
                    pathsResolver: () => listExp.ComputePaths(ctx))); // Use the paths of listExp as the paths of the $ variable

            // Return the selects of the inner expression
            await foreach (var path in valueSelectorExp.ComputeSelect(scopedCtx))
            {
                yield return path;
            }
        }

        #endregion

        #region Max + Min

        private EvaluationFunction Max()
        {
            return new EvaluationFunction(
                functionAsync: (object[] args, EvaluationContext ctx) => MaxMinImpl(args, ctx, "Max"),
                additionalSelectResolver: MaxMinSelect);
        }

        private EvaluationFunction Min()
        {
            return new EvaluationFunction(
                functionAsync: (object[] args, EvaluationContext ctx) => MaxMinImpl(args, ctx, "Min"),
                additionalSelectResolver: MaxMinSelect);
        }

        private static async Task<object> MaxMinImpl(object[] args, EvaluationContext ctx, string funcName)
        {
            // Get arguments
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{funcName}' expects {argCount} arguments: (list, selector).");
            }

            if (args[0] is not IList items)
            {
                throw new TemplateException($"Function '{funcName}' expects a 1st argument list of type List.");
            }

            if (args[1] is not string valueSelectorString)
            {
                throw new TemplateException($"Function '{funcName}' expects a 2nd argument selector of type string.");
            }

            var valueSelectorExp = TemplexBase.Parse(valueSelectorString) ??
                throw new TemplateException($"Function '{funcName}' 2nd parameter cannot be an empty string.");

            IComparable result = null;
            foreach (var item in items)
            {
                // Clone an empty context and set a single local variable
                var scopedCtx = ctx.CloneWithoutLocals();
                scopedCtx.SetLocalVariable("$", new EvaluationVariable(value: item));

                var valueObj = await valueSelectorExp.Evaluate(scopedCtx);
                if (valueObj is null)
                {
                    continue; // Null propagation
                }
                else if (valueObj is IComparable value)
                {
                    if (result == null || (funcName == "Max" && value.CompareTo(result) > 0) || (funcName == "Min" && value.CompareTo(result) < 0))
                    {
                        result = value;
                    }
                }
                else
                {
                    throw new TemplateException($"Function '{funcName}' expects a list of values that support comparison.");
                }
            }

            return result;
        }

        private async IAsyncEnumerable<Path> MaxMinSelect(TemplexBase[] args, EvaluationContext ctx)
        {
            // Get arguments
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(Max)}' expects {argCount} arguments.");
            }

            // Get the list expression
            var listExp = args[0];

            // Get and parse the value selector expression
            var valueSelectorParameterExp = args[1];
            var valueSelectorObj = await valueSelectorParameterExp.Evaluate(ctx);
            if (valueSelectorObj is not string valueSelectorString)
            {
                throw new TemplateException($"Function '{nameof(Max)}' expects a 2nd argument selector of type string.");
            }

            var valueSelectorExp = TemplexBase.Parse(valueSelectorString) ??
                throw new TemplateException($"Function '{nameof(Max)}' 2nd parameter cannot be an empty string.");


            // Remove local variables and functions and add one $ variable
            var scopedCtx = ctx.CloneWithoutLocals();
            scopedCtx.SetLocalVariable("$", new EvaluationVariable(
                    eval: TemplateUtil.VariableThatThrows("$"),
                    pathsResolver: () => listExp.ComputePaths(ctx))); // Use the paths of listExp as the paths of the $ variable

            // Return the selects of the inner expression
            await foreach (var path in valueSelectorExp.ComputeSelect(scopedCtx))
            {
                yield return path;
            }
        }

        #endregion

        #region Count

        private EvaluationFunction Count()
        {
            return new EvaluationFunction((args, _) => CountImpl(args));
        }

        private int CountImpl(object[] args)
        {
            int argCount = 1;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(Count)}' expects {argCount} argument: (list).");
            }

            var listObj = args[0];
            if (listObj is null)
            {
                return 0;
            }

            if (listObj is not IList list)
            {
                throw new TemplateException($"Function '{nameof(Count)}' expects a 1st argument of type List.");
            }

            return list.Count;
        }

        #endregion

        #region Format

        private EvaluationFunction Format()
        {
            return new EvaluationFunction(FormatImpl);
        }

        private string FormatImpl(object[] args, EvaluationContext ctx)
        {
            int argCount = 2;
            if (args.Length != 2)
            {
                throw new TemplateException($"Function '{nameof(Format)}' expects {argCount} arguments: (value, format).");
            }

            var toFormatObj = args[0];
            if (toFormatObj is null)
            {
                return null; // Null propagation
            }

            if (toFormatObj is not IFormattable toFormat)
            {
                throw new TemplateException($"Function '{nameof(Format)}' expects a 1st parameter value that can be formatted. E.g. a numerical or datetime value.");
            }

            var formatObj = args[1];
            if (formatObj is not string formatString)
            {
                throw new TemplateException($"Function '{nameof(Format)} expects a 2nd parameter format of type string'.");
            }

            return toFormat.ToString(formatString, null);
        }

        #endregion

        #region FormatDate

        private EvaluationFunction FormatDate(TemplateEnvironment env)
        {
            return new EvaluationFunction(function: (args, _) => FormatDateImpl(args, env));
        }

        private string FormatDateImpl(object[] args, TemplateEnvironment env)
        {
            int minArgCount = 2; // date, format
            int maxArgCount = 3; // date, format, calendar
            if (args.Length < minArgCount || args.Length > maxArgCount)
            {
                throw new TemplateException($"Function '{nameof(FormatDate)}' expects at least {minArgCount} and at most {maxArgCount} arguments: (date, format, calendar).");
            }

            object dateObj = args[0];
            object formatObj2 = args[1];
            object calendarObj3 = args.Length > 2 ? args[2] : null;

            if (dateObj is null)
            {
                return null; // Null propagation
            }

            if (dateObj is not DateTime date)
            {
                throw new TemplateException($"Function '{nameof(FormatDate)}' expects a 1st argument of type DateTime.");
            }

            string format = null;
            if (formatObj2 is null || formatObj2 is string)
            {
                format = formatObj2 as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(FormatDate)}' expects a 2nd argument of type string.");
            }

            string calendar = Calendars.Gregorian;
            if (calendarObj3 is null || calendarObj3 is string)
            {
                calendar = calendarObj3 as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(FormatDate)}' expects a 3rd argument of type string.");
            }

            return CalendarUtilities.FormatDate(date, env.Localizer, format, calendar);
        }

        #endregion

        #region If

        private EvaluationFunction If()
        {
            return new EvaluationFunction(IfImpl, null, IfPaths);
        }

        private object IfImpl(object[] args, EvaluationContext ctx)
        {
            int argCount = 3;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(If)}' expects {argCount} arguments: (condition, value_if_true, value_if_false).");
            }

            var conditionObj = args[0] ?? false;
            if (conditionObj is not bool condition)
            {
                throw new TemplateException($"Function '{nameof(If)}' expects a 1st argument condition of type bool.");
            }

            return condition ? args[1] : args[2];
        }

        private async IAsyncEnumerable<Path> IfPaths(TemplexBase[] args, EvaluationContext ctx)
        {
            // Get arguments
            int argCount = 3;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(If)}' expects {argCount} arguments.");
            }

            // Get the list expression
            var expIfTrue = args[1];
            var expIfFalse = args[2];

            // Return the selects of the inner expression
            await foreach (var path in expIfTrue.ComputePaths(ctx))
            {
                yield return path;
            }

            // Return the selects of the inner expression
            await foreach (var path in expIfFalse.ComputePaths(ctx))
            {
                yield return path;
            }
        }


        #endregion

        #region AmountInWords

        private EvaluationFunction AmountInWords(TemplateEnvironment env)
        {
            return new EvaluationFunction(function: (object[] args, EvaluationContext ctx) => AmountInWordsImpl(args, ctx, env));
        }

        private object AmountInWordsImpl(object[] args, EvaluationContext ctx, TemplateEnvironment env)
        {
            // Validation
            int minArgCount = 1;
            int maxArgCount = 3;
            if (args.Length < minArgCount || args.Length > maxArgCount)
            {
                throw new TemplateException($"Function '{nameof(AmountInWords)}' expects at least {minArgCount} and at most {maxArgCount} arguments: (amount, currency, decimals).");
            }

            // Amount
            object amountObj = args[0];
            decimal amount;
            try
            {
                amount = Convert.ToDecimal(amountObj);
            }
            catch
            {
                throw new TemplateException($"{nameof(AmountInWords)} expects a 1st parameter amount of a numeric type.");
            }

            // Currency ISO
            string currencyIso = null;
            if (args.Length >= 2)
            {
                currencyIso = args[1]?.ToString();
            }

            // Decimals
            int? decimals = null;
            if (args.Length >= 3 && args[2] != null)
            {
                object decimalsObj = args[2];
                try
                {
                    decimals = Convert.ToInt32(decimalsObj);
                }
                catch
                {
                    throw new TemplateException($"{nameof(AmountInWords)} expects a 3rd parameter decimals of type int.");
                }
            }

            // Validation
            if (decimals != null)
            {
                var allowedValues = new List<int> { 0, 2, 3 };
                if (!allowedValues.Contains(decimals.Value))
                {
                    throw new TemplateException($"{nameof(AmountInWords)} 3rd parameter can be one of the following: {string.Join(", ", allowedValues)}.");
                }
            }

            // TODO: Add more languages based on env.Culture
            return AmountInWordsEnglish.ConvertAmount(amount, currencyIso, decimals);
        }

        #endregion

        #region StartsWith

        private EvaluationFunction StartsWith()
        {
            return new EvaluationFunction(StartsWithImpl);
        }

        private object StartsWithImpl(object[] args, EvaluationContext ctx)
        {
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(StartsWith)}' expects {argCount} arguments: (text, prefix).");
            }

            var textObj = args[0];
            if (textObj is null)
            {
                return false; // null does not start with anything
            }
            else if (textObj is string textString)
            {
                var prefixObj = args[1];
                if (prefixObj is null)
                {
                    return true; // Everything starts with null
                }
                else if (prefixObj is string prefixString)
                {
                    return textString.StartsWith(prefixString);
                }
                else
                {
                    throw new TemplateException($"Function '{nameof(StartsWith)}' expects a 2st argument prefix of type string.");
                }
            }
            else
            {
                throw new TemplateException($"Function '{nameof(StartsWith)}' expects a 1st argument text of type string.");
            }
        }

        #endregion

        #region EndsWith

        private EvaluationFunction EndsWith()
        {
            return new EvaluationFunction(EndsWithImpl);
        }

        private object EndsWithImpl(object[] args, EvaluationContext ctx)
        {
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(EndsWith)}' expects {argCount} arguments: (text, postfix).");
            }

            var textObj = args[0];
            if (textObj is null)
            {
                return false; // null does not start with anything
            }
            else if (textObj is string textString)
            {
                var postfixObj = args[1];
                if (postfixObj is null)
                {
                    return true; // Everything starts with null
                }
                else if (postfixObj is string postfixString)
                {
                    return textString.EndsWith(postfixString);
                }
                else
                {
                    throw new TemplateException($"Function '{nameof(EndsWith)}' expects a 2st argument postfix of type string.");
                }
            }
            else
            {
                throw new TemplateException($"Function '{nameof(EndsWith)}' expects a 1st argument text of type string.");
            }
        }

        #endregion

        #region List

        private EvaluationFunction List()
        {
            return new EvaluationFunction(ListImpl);
        }

        private object ListImpl(object[] args, EvaluationContext _)
        {
            return new List<object>(args);
        }

        #endregion

        #region Barcode

        private EvaluationFunction Barcode()
        {
            return new EvaluationFunction(BarcodeImpl);
        }

        private object BarcodeImpl(object[] args, EvaluationContext ctx)
        {
            // Validation
            int minArgCount = 1;
            int maxArgCount = 5;
            if (args.Length < minArgCount || args.Length > maxArgCount)
            {
                throw new TemplateException($"Function '{nameof(AmountInWords)}' expects at least {minArgCount} and at most {maxArgCount} arguments: (value, barcodeType, includeLabel, height, barWidth).");
            }

            // value
            string barcodeValue = args[0]?.ToString();
            if (string.IsNullOrWhiteSpace(barcodeValue))
            {
                return "";
            }

            var barcodeType = BarcodeLib.TYPE.CODE128; // Default
            if (args.Length >= 2)
            {
                // Some of the most widely used 1D barcodes according to
                // // https://www.dynamsoft.com/blog/insights/the-comprehensive-guide-to-1d-and-2d-barcodes/
                string barcodeTypeString = args[1]?.ToString();
                barcodeType = barcodeTypeString switch
                {
                    "UPC-A" => BarcodeLib.TYPE.UPCA,
                    "UPC-E" => BarcodeLib.TYPE.UPCE,
                    "EAN-8" => BarcodeLib.TYPE.EAN8,
                    "EAN-13" => BarcodeLib.TYPE.EAN13,
                    "Industrial 2 of 5" => BarcodeLib.TYPE.Industrial2of5,
                    "Interleaved 2 of 5" => BarcodeLib.TYPE.Interleaved2of5,
                    "Codabar" => BarcodeLib.TYPE.Codabar,
                    "Code 11" => BarcodeLib.TYPE.CODE11,
                    "Code 39" => BarcodeLib.TYPE.CODE39,
                    "Code 93" => BarcodeLib.TYPE.CODE93,
                    "Code 128" => BarcodeLib.TYPE.CODE128,
                    null => barcodeType,
                    _ => throw new TemplateException($"Unknown barcode standard '{barcodeTypeString}'."),
                };
            }

            bool includeLabel = true;
            if (args.Length >= 3)
            {
                var includeLabelObj = args[2] ?? false;
                if (includeLabelObj is bool includeLabelBool)
                {
                    includeLabel = includeLabelBool;
                }
                else if (includeLabelObj is null)
                {
                    // Fine
                }
                else
                {
                    throw new TemplateException($"Function '{nameof(Barcode)}': 3rd argument includeLabel must be a boolean.");
                }
            }

            int? height = null;
            if (args.Length >= 4)
            {
                var heightObj = args[3];
                if (heightObj is int heightInt)
                {
                    height = heightInt;
                }
                else if (heightObj is null)
                {
                    // Fine
                }
                else
                {
                    throw new TemplateException($"Function '{nameof(Barcode)}': 4th argument height must be an int.");
                }
            }


            int? barWidth = null;
            if (args.Length >= 5)
            {
                var barWidthObj = args[4];
                if (barWidthObj is int barWidthInt)
                {
                    barWidth = barWidthInt;
                }
                else if (barWidthObj is null)
                {
                    // Fine
                }
                else
                {
                    throw new TemplateException($"Function '{nameof(Barcode)}': 5th argument barWidth must be an int.");
                }
            }

            try
            {
                var barcodeEncoder = new BarcodeLib.Barcode(barcodeValue, barcodeType)
                {
                    IncludeLabel = includeLabel,
                    StandardizeLabel = true,
                };

                if (height != null)
                {
                    barcodeEncoder.Height = height.Value;
                }

                if (barWidth != null)
                {
                    barcodeEncoder.BarWidth = barWidth;
                }

                var img = barcodeEncoder.Encode(barcodeType, barcodeValue);
                using var memoryStream = new System.IO.MemoryStream();
                img.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                return "data:image/png;base64," + Convert.ToBase64String(memoryStream.ToArray());
            }
            catch (Exception e)
            {
                throw new TemplateException(e.Message);
            }
        }

        #endregion

        #region ToInteger

        private EvaluationFunction ToInteger()
        {
            return new EvaluationFunction(ToIntegerImpl);
        }

        private object ToIntegerImpl(object[] args, EvaluationContext ctx)
        {
            int argCount = 1;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(ToInteger)}' expects a single argument value.");
            }

            object val = args[0];

            if (val is null)
            {
                return null;
            }
            else if (val is int typedVal)
            {
                return typedVal;
            }
            else
            {
                try
                {
                    return Convert.ToInt32(val);
                }
                catch (Exception)
                {
                    throw new TemplateException($"{nameof(ToInteger)}: Could not convert ({val}) to an Integer.");
                }
            }
        }

        #endregion

        #region ToDecimal

        private EvaluationFunction ToDecimal()
        {
            return new EvaluationFunction(ToDecimalImpl);
        }

        private object ToDecimalImpl(object[] args, EvaluationContext ctx)
        {
            int argCount = 1;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(ToDecimal)}' expects a single argument value.");
            }

            object val = args[0];

            if (val is null)
            {
                return null;
            }
            else if (val is decimal typedVal)
            {
                return typedVal;
            }
            else
            {
                try
                {
                    return Convert.ToDecimal(val);
                }
                catch (Exception)
                {
                    throw new TemplateException($"{nameof(ToDecimal)}: Could not convert ({val}) to a Decimal.");
                }
            }
        }

        #endregion

        #region ToDateTime

        private EvaluationFunction ToDateTime()
        {
            return new EvaluationFunction(ToDateTimeImpl);
        }

        private object ToDateTimeImpl(object[] args, EvaluationContext ctx)
        {
            int argCount = 1;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(ToDateTime)}' expects a single argument value.");
            }

            object val = args[0];

            if (val is null)
            {
                return null;
            }
            else if (val is DateTime typedVal)
            {
                return typedVal;
            }
            else
            {
                try
                {
                    return Convert.ToDateTime(val);
                }
                catch (Exception)
                {
                    throw new TemplateException($"{nameof(ToDateTime)}: Could not convert ({val}) to a DateTime.");
                }
            }
        }

        #endregion

        #region ToDateTimeOffset

        private EvaluationFunction ToDateTimeOffset()
        {
            return new EvaluationFunction(ToDateTimeOffsetImpl);
        }

        private object ToDateTimeOffsetImpl(object[] args, EvaluationContext ctx)
        {
            int argCount = 1;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(ToDateTimeOffset)}' expects a single argument value.");
            }

            object val = args[0];

            if (val is null)
            {
                return null;
            }
            else if (val is DateTimeOffset typedVal)
            {
                return typedVal;
            }
            else
            {
                try
                {
                    return DateTimeOffset.Parse(val.ToString());
                }
                catch (Exception)
                {
                    throw new TemplateException($"{nameof(ToDateTimeOffset)}: Could not convert ({val}) to a DateTimeOffset.");
                }
            }
        }

        #endregion

        #region ToBoolean

        private EvaluationFunction ToBoolean()
        {
            return new EvaluationFunction(ToBooleanImpl);
        }

        private object ToBooleanImpl(object[] args, EvaluationContext ctx)
        {
            int argCount = 1;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(ToBoolean)}' expects a single argument value.");
            }

            object val = args[0];

            if (val is null)
            {
                return null;
            }
            else if (val is bool typedVal)
            {
                return typedVal;
            }
            else
            {
                try
                {
                    return Convert.ToBoolean(val);
                }
                catch (Exception)
                {
                    throw new TemplateException($"{nameof(ToBoolean)}: Could not convert ({val}) to a Boolean.");
                }
            }
        }

        #endregion

        #region QueryQuote

        private EvaluationFunction QueryQuote()
        {
            return new EvaluationFunction(QueryQuoteImpl);
        }

        private object QueryQuoteImpl(object[] args, EvaluationContext ctx)
        {
            int argCount = 1;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(QueryQuote)}' expects a single argument value.");
            }

            object val = args[0];

            if (val is null)
            {
                return "";
            }
            else if (val is not string stringVal)
            {
                throw new TemplateException($"Function {nameof(QueryQuote)} expects a parameter of type string.");
            }
            else
            {
                return $"'{stringVal.Replace("'", "''")}'";
            }
        }

        #endregion

        #region QueryDateTime

        private EvaluationFunction QueryDateTime()
        {
            return new EvaluationFunction(QueryDateTimeImpl);
        }

        private object QueryDateTimeImpl(object[] args, EvaluationContext ctx)
        {
            int argCount = 1;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(QueryDateTime)}' expects a single argument value.");
            }

            object val = args[0];

            if (val is null)
            {
                return "";
            }
            else if (val is not DateTime dateVal)
            {
                throw new TemplateException($"Function {nameof(QueryDateTime)} expects a parameter of type DateTime.");
            }
            else
            {
                return $"'{dateVal:yyyy-MM-ddTHH:mm:ss.fff}'";
            }
        }

        #endregion

        #region QueryDateTimeOffset

        private EvaluationFunction QueryDateTimeOffset()
        {
            return new EvaluationFunction(QueryDateTimeOffsetImpl);
        }

        private object QueryDateTimeOffsetImpl(object[] args, EvaluationContext ctx)
        {
            int argCount = 1;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(QueryDateTimeOffset)}' expects a single argument value.");
            }

            object val = args[0];

            if (val is null)
            {
                return "";
            }
            else if (val is not DateTimeOffset dateVal)
            {
                throw new TemplateException($"Function {nameof(QueryDateTimeOffset)} expects a parameter of type DateTimeOffset.");
            }
            else
            {
                return $"'{dateVal:yyyy-MM-ddTHH:mm:ss.fffffffZ}'";
            }
        }

        #endregion

        #region SA_InvoiceQrCode (TODO: Remove)

        private EvaluationFunction SA_InvoiceQrCode()
        {
            return new EvaluationFunction(SA_InvoiceQrCodeImpl);
        }

        private object SA_InvoiceQrCodeImpl(object[] args, EvaluationContext ctx)
        {
            // Validation
            int argCount = 5;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(SA_InvoiceQrCode)}' expects {argCount} parameters.");
            }

            // Seller name
            object sellerNameObj = args[0];
            if (sellerNameObj is string sellerName)
            {
                if (string.IsNullOrWhiteSpace(sellerName))
                {
                    throw new TemplateException($"{nameof(SA_InvoiceQrCode)} expects a 1st argument seller_name that is not null or empty.");
                }
            }
            else
            {
                throw new TemplateException($"{nameof(SA_InvoiceQrCode)} expects a 1st argument seller_name of type string.");
            }

            // Seller VAT registration number
            object vatNumberObj = args[1];
            if (vatNumberObj is string vatNumber)
            {
                if (string.IsNullOrWhiteSpace(vatNumber))
                {
                    throw new TemplateException($"{nameof(SA_InvoiceQrCode)} expects a 2nd argument vat_number that is not null or empty.");
                }
            }
            else
            {
                throw new TemplateException($"{nameof(SA_InvoiceQrCode)} expects a 2nd argument vat_number of type string.");
            }

            // Time stamp of the invoice
            object timestampObj = args[2];
            if (timestampObj is not DateTimeOffset timestamp)
            {
                throw new TemplateException($"{nameof(SA_InvoiceQrCode)} expects a 3rd argument timestamp of type datetimeoffset.");
            }

            // invoice total with VAT
            object totalObj = args[3];
            decimal total;
            try
            {
                total = Convert.ToDecimal(totalObj);
            }
            catch
            {
                throw new TemplateException($"{nameof(SA_InvoiceQrCode)} expects a 4th parameter total of a numeric type.");
            }

            // VAT total
            object vatObj = args[4];
            decimal vat;
            try
            {
                vat = Convert.ToDecimal(vatObj);
            }
            catch
            {
                throw new TemplateException($"{nameof(SA_InvoiceQrCode)} expects a 5th parameter vat of a numeric type.");
            }

            // Assemble the QR code contents
            var qrContentList = new List<byte>();

            // Seller Name
            var sellerNameBytes = Encoding.UTF8.GetBytes(sellerName);
            if (sellerNameBytes.Length > byte.MaxValue)
            {
                throw new TemplateException($"{nameof(SA_InvoiceQrCode)} 1st argument '{sellerName}' encodes to more than {byte.MaxValue} bytes.");
            }

            qrContentList.Add(1);
            qrContentList.Add((byte)sellerNameBytes.Length);
            qrContentList.AddRange(sellerNameBytes);

            // VAT number
            var vatNumberBytes = Encoding.UTF8.GetBytes(vatNumber);
            if (vatNumberBytes.Length > byte.MaxValue)
            {
                throw new TemplateException($"{nameof(SA_InvoiceQrCode)} 2nd argument '{vatNumber}' encodes to more than {byte.MaxValue} bytes.");
            }

            qrContentList.Add(2);
            qrContentList.Add((byte)vatNumberBytes.Length);
            qrContentList.AddRange(vatNumberBytes);

            // Timestamp
            var ksaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time"); // KSA
            var timestampDateTime = TimeZoneInfo.ConvertTimeFromUtc(timestamp.UtcDateTime, ksaTimeZone);
            var timestampString = timestampDateTime.ToString("yyyy-MM-ddTHH:mm:ss");
            var timestampBytes = Encoding.UTF8.GetBytes(timestampString);

            qrContentList.Add(3);
            qrContentList.Add((byte)timestampBytes.Length);
            qrContentList.AddRange(timestampBytes);

            // total
            var totalString = total.ToString("#.##");
            var totalBytes = Encoding.UTF8.GetBytes(totalString);

            qrContentList.Add(4);
            qrContentList.Add((byte)totalBytes.Length);
            qrContentList.AddRange(totalBytes);

            // vat
            var vatString = vat.ToString("#.##");
            var vatBytes = Encoding.UTF8.GetBytes(vatString);

            qrContentList.Add(5);
            qrContentList.Add((byte)vatBytes.Length);
            qrContentList.AddRange(vatBytes);

            var qrContent = Convert.ToBase64String(qrContentList.ToArray());

            try
            {
                using QRCodeGenerator qrGenerator = new();
                using QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
                using QRCode qrCode = new(qrCodeData);
                using System.Drawing.Bitmap img = qrCode.GetGraphic(pixelsPerModule: 8);
                using var memoryStream = new System.IO.MemoryStream();

                img.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                return "data:image/png;base64," + Convert.ToBase64String(memoryStream.ToArray());
            }
            catch (Exception e)
            {
                throw new TemplateException(e.Message);
            }
        }

        #endregion

        #region PreviewWidth + PreviewHeight

        private EvaluationFunction PreviewWidth()
        {
            return new EvaluationFunction(
                function: (object[] args, EvaluationContext ctx) => PreviewWidthImpl(args));
        }

        private EvaluationFunction PreviewHeight()
        {
            return new EvaluationFunction(
                function: (object[] args, EvaluationContext ctx) => PreviewHeightImpl(args));
        }

        private object PreviewHeightImpl(object[] args)
        {
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function {nameof(PreviewHeight)} expects {argCount} parameters: (pageSize, orientation).");
            }

            var pageSizeObj = args[0];
            var orientationObj = args[1];

            if (pageSizeObj is not string pageSize)
            {
                throw new TemplateException($"Function {nameof(PreviewHeight)} expects 1st parameter pageSize of type string.");
            }

            if (orientationObj is not string orientation)
            {
                throw new TemplateException($"Function {nameof(PreviewHeight)} expects 2nd parameter orientation of type string.");
            }

            var orientationLower = orientation.Trim().ToLower();
            if (orientationLower != "portrait" && orientationLower != "landscape")
            {
                throw new TemplateException($"Unknown orientation '{orientation}'.");
            }

            bool isLandscape = orientationLower == "landscape";
            var pageHeight = PageLength(pageSize, shortSide: isLandscape);
            return pageHeight;
        }

        private object PreviewWidthImpl(object[] args)
        {
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function {nameof(PreviewWidth)} expects {argCount} parameters: (pageSize, orientation)");
            }

            var pageSizeObj = args[0];
            var orientationObj = args[1];

            if (pageSizeObj is not string pageSize)
            {
                throw new TemplateException($"Function {nameof(PreviewWidth)} expects 1st parameter pageSize of type string.");
            }

            if (orientationObj is not string orientation)
            {
                throw new TemplateException($"Function {nameof(PreviewWidth)} expects 2nd parameter orientation of type string.");
            }

            var orientationLower = orientation.Trim().ToLower();
            if (orientationLower != "portrait" && orientationLower != "landscape")
            {
                throw new TemplateException($"Unknown orientation '{orientation}'.");
            }

            bool isPortrait = orientationLower == "portrait";
            var pageWidth = PageLength(pageSize, shortSide: isPortrait);
            return pageWidth;
        }

        private static string PageLength(string pageSize, bool shortSide)
        {
            var pageSizeLower = pageSize.Trim().ToLower();
            return pageSizeLower switch
            {
                "a5" => shortSide ? "148mm" : "210mm",
                "a4" => shortSide ? "210mm" : "297mm",
                "a3" => shortSide ? "297mm " : "420mm",
                "b5" => shortSide ? "176mm" : "250mm",
                "b4" => shortSide ? "250mm" : "353mm",
                "jis-b5" => shortSide ? "182mm" : "257mm",
                "jis-b4" => shortSide ? "257mm" : "364mm",
                "letter" => shortSide ? "8.5in" : "11in",
                "legal" => shortSide ? "8.5in" : "14in",
                "ledger" => shortSide ? "11in" : "17in",
                _ => throw new TemplateException($"Unknown page size {pageSize}.")
            };
        }

        #endregion

        #endregion

        #region Helper Classes

        /// <summary>
        /// Additional contextual information used to generate the root <see cref="EvaluationContext"/>.
        /// </summary>
        private struct TemplateEnvironment
        {
            /// <summary>
            /// Localizer for functions that need it.
            /// </summary>
            internal IStringLocalizer Localizer { get; set; }

            /// <summary>
            /// The culture that the templates are being evaluated in.
            /// </summary>
            internal CultureInfo Culture { get; set; }

            /// <summary>
            /// The cancellation instruction.
            /// </summary>
            internal CancellationToken Cancellation { get; set; }
        }

        private static class FuncNames
        {
            // Just the names of the standard query functions
            internal const string Entities = nameof(Entities);
            internal const string EntitiesByIds = nameof(EntitiesByIds);
            internal const string EntityById = nameof(EntityById);
        }

        #endregion
    }
}
