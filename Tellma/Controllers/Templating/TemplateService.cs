﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using QRCoder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Entities;
using Tellma.Services.Utilities;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Scoped service that provides markup templating functionality
    /// </summary>
    public class TemplateService
    {
        private readonly IServiceProvider _provider;
        private readonly ApplicationRepository _repo;
        private readonly IStringLocalizer _localizer;

        // Just the names of the standard query functions
        private string QueryByFilter => nameof(QueryByFilter);
        private string QueryById => nameof(QueryById);
        private string QueryAggregate => nameof(QueryAggregate);

        public TemplateService(IServiceProvider serviceProvider)
        {
            _provider = serviceProvider;
            _repo = _provider.GetRequiredService<ApplicationRepository>();
            _localizer = _provider.GetRequiredService<IStringLocalizer<Strings>>();
        }

        /// <summary>
        /// Parses the templates into abstract expression trees, performs analysis on the trees to determine
        /// the required API calls and their arguments, carries out those API calls and uses the results
        /// together with the abstract expression tree to generate the final results
        /// </summary>
        /// <param name="templates">The array of template strings</param>
        /// <param name="inputVariables">Optional collection of variables to add to the root evaluation context</param>
        /// <param name="preloadedQuery">Optional query to add to the root evaluation context under variable name "$"</param>
        /// <param name="culture">The UI culture according to which the templates are evaluated</param>
        /// <param name="cancellation">Cancellation token for async calls</param>
        /// <returns>An array, equal in size to the supplied templates array, where each output is matched to each input by the array index</returns>
        public async Task<string[]> GenerateMarkup(
            (string template, string language)[] templates,
            Dictionary<string, object> inputVariables,
            QueryInfo preloadedQuery, // Optional, instructs the template service to preload this query in the $ variable
            CultureInfo culture,
            CancellationToken cancellation)
        {
            // Parse the title and body
            TemplateTree[] trees = new TemplateTree[templates.Length];
            for (int i = 0; i < templates.Length; i++)
            {
                trees[i] = TemplateTree.Parse(templates[i].template);
            }

            // Prepare the evaluation context
            EvaluationContext ctx = GetRootContext(new TemplateEnvironment
            {
                Culture = culture,
                Cancellation = cancellation,
                Localizer = _localizer
            });

            // Add to it the input variables
            if (inputVariables != null)
            {
                foreach (var p in inputVariables)
                {
                    ctx.SetLocalVariable(p.Key, new TemplateVariable(p.Value));
                }
            }

            // The context query in the dollar sign
            if (preloadedQuery != null)
            {
                ctx.SetLocalVariable("$", new TemplateVariable(
                    eval: TemplateUtil.VariableThatThrows(varName: "$"),
                    pathsResolver: () => AsyncUtil.Singleton(Path.Empty(preloadedQuery))
                ));
            }

            // Add the query functions as placeholders that can only determine select and paths
            ctx.SetLocalFunction(QueryByFilter, new TemplateFunction(
                    function: TemplateUtil.FunctionThatThrows(QueryByFilter),
                    pathsResolver: (args, ctx) => QueryByFilterPaths(args, ctx)
                )
            );

            ctx.SetLocalFunction(QueryById, new TemplateFunction(
                    function: TemplateUtil.FunctionThatThrows(QueryById),
                    pathsResolver: (args, ctx) => QueryByIdPaths(args, ctx)
                )
            );

            // Step 1: Compute the select paths and group them by query info
            Dictionary<QueryInfo, PathsTrie> allSelectPaths = new Dictionary<QueryInfo, PathsTrie>();
            foreach (var tree in trees.Where(e => e != null))
            {
                await foreach (var path in tree.ComputeSelect(ctx))
                {
                    if (!allSelectPaths.TryGetValue(path.QueryInfo, out PathsTrie trie))
                    {
                        trie = new PathsTrie();
                        allSelectPaths.Add(path.QueryInfo, trie);
                    }

                    trie.AddPath(path);
                }
            }

            // Step 2: Pre-load all the queries that the template needs
            var queryResults = new Dictionary<QueryInfo, object>();
            foreach (var (query, trie) in allSelectPaths)
            {
                // prepare the select Expression
                var queryPaths = trie.GetPaths();
                if (!queryPaths.Any(e => e.Length > 0))
                {
                    // Query is never accessed in the template
                    continue;
                }

                var select = string.Join(",", trie.GetPaths().Select(p => string.Join(".", p)));

                // Load the query
                if (query is QueryByFilterInfo queryByFilter)
                {
                    if (queryByFilter.Ids != null && queryByFilter.Ids.Any())
                    {
                        // If IDs are supplied, ignore all other parameters
                        var args = new GetByIdArguments
                        {
                            Select = select,
                        };

                        try
                        {
                            var service = _provider.FactWithIdServiceByEntityType(query.Collection, query.DefinitionId);
                            var (list, _) = await service.GetByIds(queryByFilter.Ids.ToList(), args, cancellation);
                            queryResults[query] = list;
                        }
                        catch (UnknownCollectionException)
                        {
                            throw new TemplateException($"Unknown collection '{query.Collection}'");
                        }
                    }
                    else
                    {
                        // Prepare the GetArguments
                        var args = new GetArguments
                        {
                            Select = select,
                            Filter = queryByFilter.Filter,
                            OrderBy = queryByFilter.OrderBy,
                            CountEntities = false
                        };

                        if (queryByFilter.Top != null)
                        {
                            args.Top = queryByFilter.Top.Value;
                        }

                        if (queryByFilter.Skip != null)
                        {
                            args.Skip = queryByFilter.Skip.Value;
                        }

                        try
                        {
                            var service = _provider.FactServiceByCollectionName(query.Collection, query.DefinitionId);
                            var (list, _, _, _) = await service.GetFact(args, cancellation);
                            queryResults[query] = list;
                        }
                        catch (UnknownCollectionException)
                        {
                            throw new TemplateException($"Unknown collection '{query.Collection}'");
                        }
                    }
                }
                else if (query is QueryByIdInfo queryById)
                {
                    // Prepare the GetArguments
                    var args = new GetByIdArguments
                    {
                        Select = select
                    };

                    try
                    {
                        // Load the result
                        var service = _provider.FactGetByIdServiceByCollectionName(query.Collection, query.DefinitionId);
                        var (entity, _) = await service.GetById(queryById.Id, args, cancellation);

                        queryResults[query] = entity;
                    }
                    catch (UnknownCollectionException)
                    {
                        throw new TemplateException($"Unknown collection '{query.Collection}'");
                    }
                    catch (RequiredDefinitionIdException)
                    {
                        throw new TemplateException($"To query collection '{query.Collection}' by Id, the source parameter must contain the definition Id. E.g. '{query.Collection}/1'");
                    }
                }
                else
                {
                    throw new TemplateException($"Unknown query type '{query?.GetType()?.Name}'"); // Future proofing
                }
            }

            // Step 3 Replace the placeholder query functions with ones that can now return values
            if (preloadedQuery != null && queryResults.TryGetValue(preloadedQuery, out object value))
            {
                ctx.SetLocalVariable("$", new TemplateVariable(value: value));
            }

            ctx.SetLocalFunction(QueryByFilter, new TemplateFunction(functionAsync: (args, ctx) => QueryByFilterImpl(args, queryResults)));
            ctx.SetLocalFunction(QueryById, new TemplateFunction(functionAsync: (args, ctx) => QueryByIdImpl(args, queryResults)));

            // Step 4 Generate the output
            using CultureScope _ = new CultureScope(culture);

            var outputs = new string[trees.Length];
            for (int i = 0; i < trees.Length; i++)
            {
                if (trees[i] != null)
                {
                    StringBuilder builder = new StringBuilder();
                    Func<string, string> encodeFunc = templates[i].language switch
                    {
                        MimeTypes.Html => HtmlEncoder.Default.Encode,
                        MimeTypes.Text => s => s, // No need to encode anything for a text output
                        _ => s => s,
                    };

                    await trees[i].GenerateOutput(builder, ctx, encodeFunc);
                    outputs[i] = builder.ToString();
                }
            }

            // Return the result
            return outputs;
        }

        /// <summary>
        /// Returns the <see cref="EvaluationContext"/> contianing a library of global variables and functions
        /// </summary>
        private EvaluationContext GetRootContext(TemplateEnvironment env)
        {
            // Global Functions
            var globalFuncs = new EvaluationContext.FunctionsDictionary
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
                [nameof(Localize)] = Localize(env),
                [nameof(Format)] = Format(),
                [nameof(FormatDate)] = FormatDate(env),
                //[nameof(ConvertCalendar)] = ConvertCalendar(),
                [nameof(If)] = If(),
                [nameof(AmountInWords)] = AmountInWords(env),
                [nameof(Barcode)] = Barcode(),
                [nameof(SA_InvoiceQrCode)] = SA_InvoiceQrCode(),
                [nameof(PreviewWidth)] = PreviewWidth(),
                [nameof(PreviewHeight)] = PreviewHeight()
            };

            // Global Variables
            var globalVariables = new EvaluationContext.VariablesDictionary
            {
                ["$ShortCompanyName"] = new TemplateVariable(async () => (await _repo.GetTenantInfoAsync(env.Cancellation)).ShortCompanyName),
                ["$ShortCompanyName2"] = new TemplateVariable(async () => (await _repo.GetTenantInfoAsync(env.Cancellation)).ShortCompanyName2),
                ["$ShortCompanyName3"] = new TemplateVariable(async () => (await _repo.GetTenantInfoAsync(env.Cancellation)).ShortCompanyName3),
                ["$TaxIdentificationNumber"] = new TemplateVariable(async () => (await _repo.GetTenantInfoAsync(env.Cancellation)).TaxIdentificationNumber),
                ["$UserEmail"] = new TemplateVariable(async () => (await _repo.GetUserInfoAsync(env.Cancellation)).Email),
                ["$UserName"] = new TemplateVariable(async () => (await _repo.GetUserInfoAsync(env.Cancellation)).Name),
                ["$UserName2"] = new TemplateVariable(async () => (await _repo.GetUserInfoAsync(env.Cancellation)).Name2),
                ["$UserName3"] = new TemplateVariable(async () => (await _repo.GetUserInfoAsync(env.Cancellation)).Name3),
                ["$Now"] = new TemplateVariable(DateTimeOffset.Now),
                ["$Lang"] = new TemplateVariable(env.Culture.Name),
                ["$IsRtl"] = new TemplateVariable(env.Culture.TextInfo.IsRightToLeft),
            };

            // Return
            return EvaluationContext.Create(globalFuncs, globalVariables);
        }

        #region Queries

        private Task<object> QueryByFilterImpl(object[] args, Dictionary<QueryInfo, object> queryResults)
        {
            var queryInfo = QueryByFilterInfo(args);
            if (!queryResults.TryGetValue(queryInfo, out object result))
            {
                throw new TemplateException("Loading a query with no precalculated select"); // This is a bug
            }

            return Task.FromResult(result);
        }

        private async IAsyncEnumerable<Path> QueryByFilterPaths(TemplexBase[] args, EvaluationContext ctx)
        {
            var sourceObj = args.Length > 0 ? await args[0].Evaluate(ctx) : null;
            var filterObj = args.Length > 1 ? await args[1].Evaluate(ctx) : null;
            var orderbyObj = args.Length > 2 ? await args[2].Evaluate(ctx) : null;
            var topObj = args.Length > 3 ? await args[3].Evaluate(ctx) : null;
            var skipObj = args.Length > 4 ? await args[4].Evaluate(ctx) : null;
            var idsObj = args.Length > 5 ? await args[5].Evaluate(ctx) : null;

            var queryInfo = QueryByFilterInfo(sourceObj, filterObj, orderbyObj, topObj, skipObj, idsObj);
            yield return Path.Empty(queryInfo);
        }

        private QueryByFilterInfo QueryByFilterInfo(params object[] args)
        {
            int argCount = 6;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{QueryByFilter}' expects {argCount} arguments");
            }

            int i = 0;
            var sourceObj = args[i++];
            var filterObj = args[i++];
            var orderbyObj = args[i++];
            var topObj = args[i++];
            var skipObj = args[i++];
            var idsObj = args[i++];

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
                        throw new TemplateException($"Function '{QueryByFilter}' could not interpret the definitionId '{definitionIdString}' as an integer");
                    }
                }
            }
            else
            {
                throw new TemplateException($"Function '{QueryByFilter}' requires a 1st parameter source of type string");
            }

            string filter;
            if (filterObj is null || filterObj is string) // Optional
            {
                filter = filterObj as string;
            }
            else
            {
                throw new TemplateException($"Function '{QueryByFilter}' expects a 2nd parameter filter of type string");
            }

            string orderby;
            if (orderbyObj is null || orderbyObj is string) // Optional
            {
                orderby = orderbyObj as string;
            }
            else
            {
                throw new TemplateException($"Function '{QueryByFilter}' expects a 3rd parameter orderby of type string");
            }

            int? top;
            if (topObj is null || topObj is int) // Optional
            {
                top = topObj as int?;
            }
            else
            {
                throw new TemplateException($"Function '{QueryByFilter}' requires a 4th parameter top of type int");
            }

            int? skip;
            if (skipObj is null || skipObj is int) // Optional
            {
                skip = skipObj as int?;
            }
            else
            {
                throw new TemplateException($"Function '{QueryByFilter}' expects a 5th parameter skip of type int");
            }

            IList ids;
            if (idsObj is null || idsObj is IList) // Optional
            {
                ids = idsObj as IList;
            }
            else
            {
                throw new TemplateException($"Function '{QueryByFilter}' expects a 6th parameter ids of type List");
            }

            return new QueryByFilterInfo(collection, definitionId, filter, orderby, top, skip, ids);
        }

        private Task<object> QueryByIdImpl(object[] args, Dictionary<QueryInfo, object> queryResults)
        {
            var queryInfo = QueryByIdInfo(args);
            if (!queryResults.TryGetValue(queryInfo, out object result))
            {
                throw new TemplateException("Loading a query with no precalculated select"); // This is a bug
            }

            return Task.FromResult(result);
        }

        private async IAsyncEnumerable<Path> QueryByIdPaths(TemplexBase[] args, EvaluationContext ctx)
        {
            var sourceObj = args.Length > 0 ? await args[0].Evaluate(ctx) : null;
            var idObj = args.Length > 1 ? await args[1].Evaluate(ctx) : null;

            var queryInfo = QueryByIdInfo(sourceObj, idObj);
            yield return Path.Empty(queryInfo);
        }

        private QueryByIdInfo QueryByIdInfo(params object[] args)
        {
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{QueryById}' expects {argCount} arguments");
            }

            var sourceObj = args[0];
            var id = args[1];

            // Source
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
                        throw new TemplateException($"Function '{QueryByFilter}' could not interpret the definitionId '{definitionIdString}' as an integer");
                    }
                }
            }
            else
            {
                throw new TemplateException($"Function '{QueryById}' requires a 1st parameter source of type string");
            }

            // Id
            if (id is null) // Required
            {
                throw new TemplateException($"Function '{QueryById}' expects an id argument that isn't null");
            }

            return new QueryByIdInfo(collection, definitionId, id.ToString());
        }

        #endregion

        #region Localize

        private TemplateFunction Localize(TemplateEnvironment env)
        {
            return new TemplateFunction(functionAsync: (object[] args, EvaluationContext _) => LocalizeImpl(args, env));
        }

        private async Task<object> LocalizeImpl(object[] args, TemplateEnvironment env)
        {
            int minArgCount = 2;
            int maxArgCount = 3;
            if (args.Length < minArgCount || args.Length > maxArgCount)
            {
                throw new TemplateException($"Function '{nameof(Localize)}' expects at least {minArgCount} and at most {maxArgCount} arguments");
            }

            object sObj = args[0];
            object sObj2 = args[1];
            object sObj3 = args.Length > 2 ? args[2] : null;

            string s = null;
            if (sObj is null || sObj is string)
            {
                s = sObj as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Localize)}' expects a 1st argument of type string");
            }

            string s2 = null;
            if (sObj2 is null || sObj2 is string)
            {
                s2 = sObj2 as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Localize)}' expects a 2nd argument of type string");
            }

            string s3 = null;
            if (sObj3 is null || sObj3 is string)
            {
                s3 = sObj3 as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Localize)}' expects a 3rd argument of type string");
            }


            var tenantInfo = await _repo.GetTenantInfoAsync(env.Cancellation);
            return tenantInfo.Localize(s, s2, s3);
        }

        #endregion

        #region Filter

        private TemplateFunction Filter()
        {
            return new TemplateFunction(
                functionAsync: (object[] args, EvaluationContext ctx) => FilterImpl(args, ctx),
                additionalSelectResolver: (TemplexBase[] args, EvaluationContext ctx) => FilterSelect(args, ctx),
                pathsResolver: (TemplexBase[] args, EvaluationContext ctx) => FilterPaths(args, ctx));
        }

        private async Task<object> FilterImpl(object[] args, EvaluationContext ctx)
        {
            // Get arguments
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(Filter)}' expects {argCount} arguments");
            }

            if (!(args[0] is IList items))
            {
                throw new TemplateException($"Function '{nameof(Filter)}' expects a 1st argument list of type List");
            }

            if (!(args[1] is string conditionString))
            {
                throw new TemplateException($"Function '{nameof(Filter)}' expects a 2nd argument condition of type string");
            }

            var conditionExp = TemplexBase.Parse(conditionString) ??
                throw new TemplateException($"Function '{nameof(Filter)}' 2nd parameter cannot be an empty string");

            List<object> result = new List<object>();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                // Clone an empty context and set a single local variable
                var scopedCtx = ctx.CloneWithoutLocals();
                scopedCtx.SetLocalVariable("$", new TemplateVariable(value: item));

                var conditionValueObj = await conditionExp.Evaluate(scopedCtx) ?? false;
                if (!(conditionValueObj is bool conditionValue))
                {
                    throw new TemplateException($"Selector '{conditionString}' must evaluate to a boolean value");
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
                throw new TemplateException($"Function '{nameof(Filter)}' expects {argCount} arguments");
            }

            // Get the list expression
            var listExp = args[0];

            // Get and parse the value selector expression
            var conditionParameterExp = args[1];
            var conditionObj = await conditionParameterExp.Evaluate(ctx);
            if (!(conditionObj is string conditionString))
            {
                throw new TemplateException($"Function '{nameof(Filter)}' expects a 2nd argument condition of type string");
            }

            var conditionExp = TemplexBase.Parse(conditionString) ??
                throw new TemplateException($"Function '{nameof(Filter)}' 2nd parameter cannot be an empty string");

            // Remove local variables and functions and add one $ variable
            var scopedCtx = ctx.CloneWithoutLocals();
            scopedCtx.SetLocalVariable("$", new TemplateVariable(
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
                throw new TemplateException($"Function '{nameof(Filter)}' expects {argCount} arguments");
            }

            var listExp = args[0];
            return listExp.ComputePaths(ctx);
        }

        #endregion

        #region OrderBy

        private TemplateFunction OrderBy()
        {
            return new TemplateFunction(
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
                throw new TemplateException($"Function '{nameof(OrderBy)}' expects {argCount} arguments");
            }

            if (!(args[0] is IList items))
            {
                throw new TemplateException($"Function '{nameof(OrderBy)}' expects a 1st argument list of type List");
            }

            if (!(args[1] is string selectorExpString))
            {
                throw new TemplateException($"Function '{nameof(OrderBy)}' expects a 2nd argument selector of type string");
            }

            var selectorExp = TemplexBase.Parse(selectorExpString) ??
                throw new TemplateException($"Function '{nameof(OrderBy)}' 2nd parameter cannot be an empty string");

            // Retrieve the selected value on which the sorting happens
            var selections = new object[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                // Clone an empty context and set a single local variable
                var scopedCtx = ctx.CloneWithoutLocals();
                scopedCtx.SetLocalVariable("$", new TemplateVariable(value: item));

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
                throw new TemplateException($"Function '{nameof(OrderBy)}' expects {argCount} arguments");
            }

            // Get the list expression
            var listExp = args[0];

            // Get and parse the value selector expression
            var selectorParameterExp = args[1];
            var selectorObj = await selectorParameterExp.Evaluate(ctx);
            if (!(selectorObj is string selectorString))
            {
                throw new TemplateException($"Function '{nameof(OrderBy)}' expects a 2nd argument selector of type string");
            }

            var selectorExp = TemplexBase.Parse(selectorString) ??
                throw new TemplateException($"Function '{nameof(OrderBy)}' 2nd parameter cannot be an empty string");

            // Remove local variables and functions and add one $ variable
            var scopedCtx = ctx.CloneWithoutLocals();
            scopedCtx.SetLocalVariable("$", new TemplateVariable(
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
                throw new TemplateException($"Function '{nameof(OrderBy)}' expects {argCount} arguments");
            }

            var listExp = args[0];
            return listExp.ComputePaths(ctx);
        }

        #endregion

        #region SelectMany

        private TemplateFunction SelectMany()
        {
            return new TemplateFunction(
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
                throw new TemplateException($"Function '{nameof(SelectMany)}' expects {argCount} arguments");
            }

            if (!(args[0] is IList items))
            {
                throw new TemplateException($"Function '{nameof(SelectMany)}' expects a 1st argument list of type List");
            }

            if (!(args[1] is string selectorExpString))
            {
                throw new TemplateException($"Function '{nameof(SelectMany)}' expects a 2nd argument selector of type string");
            }

            var selectorExp = TemplexBase.Parse(selectorExpString) ??
                throw new TemplateException($"Function '{nameof(SelectMany)}' 2nd parameter cannot be an empty string");

            List<object> result = new List<object>();
            for (int i = 0; i < items.Count; i++)
            {
                var parentItem = items[i];

                // Clone an empty context and set a single local variable
                var scopedCtx = ctx.CloneWithoutLocals();
                scopedCtx.SetLocalVariable("$", new TemplateVariable(value: parentItem));

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
                    throw new TemplateException($"Selector '{selectorExpString}' must evaluate to a list value");
                }
            }

            return result;
        }

        private async IAsyncEnumerable<Path> SelectManySelect(TemplexBase[] args, EvaluationContext ctx)
        {
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(SelectMany)}' expects {argCount} arguments");
            }

            // Get the list expression
            var listExp = args[0];

            // Get and parse the value selector expression
            var selectorExpExp = args[1];
            var selectorExpObj = await selectorExpExp.Evaluate(ctx);
            if (!(selectorExpObj is string selectorExpString))
            {
                throw new TemplateException($"Function '{nameof(SelectMany)}' expects a 2nd argument selector of type string");
            }

            var selectorExp = TemplexBase.Parse(selectorExpString) ??
                throw new TemplateException($"Function '{nameof(SelectMany)}' 2nd parameter cannot be an empty string");

            // Remove local variables and functions and add one $ variable
            var scopedCtx = ctx.CloneWithoutLocals();
            scopedCtx.SetLocalVariable("$", new TemplateVariable(
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
                throw new TemplateException($"Function '{nameof(SelectMany)}' expects {argCount} arguments");
            }

            var listExp = args[0];

            // Get and parse the value selector expression
            var selectorExpExp = args[1];
            var selectorExpObj = await selectorExpExp.Evaluate(ctx);
            if (!(selectorExpObj is string selectorExpString))
            {
                throw new TemplateException($"Function '{nameof(SelectMany)}' expects a 2nd argument selector of type string");
            }

            var selectorExp = TemplexBase.Parse(selectorExpString) ??
                throw new TemplateException($"Function '{nameof(SelectMany)}' 2nd parameter cannot be an empty string");

            // Remove local variables and functions and add one $ variable
            var scopedCtx = ctx.CloneWithoutLocals();
            scopedCtx.SetLocalVariable("$", new TemplateVariable(
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

        private TemplateFunction Sum()
        {
            return new TemplateFunction(
                functionAsync: SumImpl,
                additionalSelectResolver: SumSelect);
        }

        private async Task<object> SumImpl(object[] args, EvaluationContext ctx)
        {
            // Get arguments
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(Sum)}' expects {argCount} arguments");
            }

            if (!(args[0] is IList items))
            {
                throw new TemplateException($"Function '{nameof(Sum)}' expects a 1st argument list of type List");
            }

            if (!(args[1] is string valueSelectorString))
            {
                throw new TemplateException($"Function '{nameof(Sum)}' expects a 2nd argument selector of type string");
            }

            var valueSelectorExp = TemplexBase.Parse(valueSelectorString) ??
                throw new TemplateException($"Function '{nameof(Sum)}' 2nd parameter cannot be an empty string");

            Type commonType = null;
            object sum = 0;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                // Clone an empty context and set a single local variable
                var scopedCtx = ctx.CloneWithoutLocals();
                scopedCtx.SetLocalVariable("$", new TemplateVariable(value: item));

                var valueObj = await valueSelectorExp.Evaluate(scopedCtx);
                if (!(valueObj is null))
                {
                    commonType ??= NumericUtil.CommonNumericType(sum, valueObj) ??
                        throw new TemplateException($"Selector '{valueSelectorString}' must evaluate to a numeric type");

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
                throw new TemplateException($"Function '{nameof(Sum)}' expects {argCount} arguments");
            }

            // Get the list expression
            var listExp = args[0];

            // Get and parse the value selector expression
            var valueSelectorParameterExp = args[1];
            var valueSelectorObj = await valueSelectorParameterExp.Evaluate(ctx);
            if (!(valueSelectorObj is string valueSelectorString))
            {
                throw new TemplateException($"Function '{nameof(Sum)}' expects a 2nd argument selector of type string");
            }

            var valueSelectorExp = TemplexBase.Parse(valueSelectorString) ??
                throw new TemplateException($"Function '{nameof(Sum)}' 2nd parameter cannot be an empty string");


            // Remove local variables and functions and add one $ variable
            var scopedCtx = ctx.CloneWithoutLocals();
            scopedCtx.SetLocalVariable("$", new TemplateVariable(
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

        private TemplateFunction Max()
        {
            return new TemplateFunction(
                functionAsync: (object[] args, EvaluationContext ctx) => MaxMinImpl(args, ctx, "Max"),
                additionalSelectResolver: MaxMinSelect);
        }

        private TemplateFunction Min()
        {
            return new TemplateFunction(
                functionAsync: (object[] args, EvaluationContext ctx) => MaxMinImpl(args, ctx, "Min"),
                additionalSelectResolver: MaxMinSelect);
        }

        private async Task<object> MaxMinImpl(object[] args, EvaluationContext ctx, string funcName)
        {
            // Get arguments
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{funcName}' expects {argCount} arguments");
            }

            if (!(args[0] is IList items))
            {
                throw new TemplateException($"Function '{funcName}' expects a 1st argument list of type List");
            }

            if (!(args[1] is string valueSelectorString))
            {
                throw new TemplateException($"Function '{funcName}' expects a 2nd argument selector of type string");
            }

            var valueSelectorExp = TemplexBase.Parse(valueSelectorString) ??
                throw new TemplateException($"Function '{funcName}' 2nd parameter cannot be an empty string");

            IComparable result = null;
            foreach (var item in items)
            {
                // Clone an empty context and set a single local variable
                var scopedCtx = ctx.CloneWithoutLocals();
                scopedCtx.SetLocalVariable("$", new TemplateVariable(value: item));

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
                    throw new TemplateException($"Function '{funcName}' expects a list of values that support comparison");
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
                throw new TemplateException($"Function '{nameof(Max)}' expects {argCount} arguments");
            }

            // Get the list expression
            var listExp = args[0];

            // Get and parse the value selector expression
            var valueSelectorParameterExp = args[1];
            var valueSelectorObj = await valueSelectorParameterExp.Evaluate(ctx);
            if (!(valueSelectorObj is string valueSelectorString))
            {
                throw new TemplateException($"Function '{nameof(Max)}' expects a 2nd argument selector of type string");
            }

            var valueSelectorExp = TemplexBase.Parse(valueSelectorString) ??
                throw new TemplateException($"Function '{nameof(Max)}' 2nd parameter cannot be an empty string");


            // Remove local variables and functions and add one $ variable
            var scopedCtx = ctx.CloneWithoutLocals();
            scopedCtx.SetLocalVariable("$", new TemplateVariable(
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

        private TemplateFunction Count()
        {
            return new TemplateFunction((args, _) => CountImpl(args));
        }

        private int CountImpl(object[] args)
        {
            int argCount = 1;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(Count)}' expects {argCount} arguments");
            }

            var listObj = args[0];
            if (listObj is null)
            {
                return 0;
            }

            if (!(listObj is IList list))
            {
                throw new TemplateException($"Function '{nameof(Count)}' expects a 1st argument of type List");
            }

            return list.Count;
        }

        #endregion

        #region Format

        private TemplateFunction Format()
        {
            return new TemplateFunction(FormatImpl);
        }

        private string FormatImpl(object[] args, EvaluationContext ctx)
        {
            int argCount = 2;
            if (args.Length != 2)
            {
                throw new TemplateException($"Function '{nameof(Format)}' expects {argCount} arguments");
            }

            var toFormatObj = args[0];
            if (toFormatObj is null)
            {
                return null; // Null propagation
            }

            if (!(toFormatObj is IFormattable toFormat))
            {
                throw new TemplateException($"Function '{nameof(Format)}' expects a 1st parameter toFormat that can be formatted. E.g. a numerical or datetime value");
            }

            var formatObj = args[1];
            if (!(formatObj is string formatString))
            {
                throw new TemplateException($"Function '{nameof(Format)} expects a 2nd parameter of type string'");
            }

            return toFormat.ToString(formatString, null);
        }

        #endregion

        #region FormatDate

        private TemplateFunction FormatDate(TemplateEnvironment env)
        {
            return new TemplateFunction(function: (args, _) => FormatDateImpl(args, env));
        }

        private string FormatDateImpl(object[] args, TemplateEnvironment env)
        {
            int minArgCount = 2; // date, format
            int maxArgCount = 3; // date, format, calendar
            if (args.Length < minArgCount || args.Length > maxArgCount)
            {
                throw new TemplateException($"Function '{nameof(FormatDate)}' expects at least {minArgCount} and at most {maxArgCount} arguments");
            }

            object dateObj = args[0];
            object formatObj2 = args[1];
            object calendarObj3 = args.Length > 2 ? args[2] : null;

            if (dateObj is null)
            {
                return null; // Null propagation
            }

            if (!(dateObj is DateTime date))
            {
                throw new TemplateException($"Function '{nameof(FormatDate)}' expects a 1st argument of type DateTime");
            }

            string format = null;
            if (formatObj2 is null || formatObj2 is string)
            {
                format = formatObj2 as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(FormatDate)}' expects a 2nd argument of type string");
            }

            string calendar = CalendarUtilities.Gregorian;
            if (calendarObj3 is null || calendarObj3 is string)
            {
                calendar = calendarObj3 as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(FormatDate)}' expects a 3rd argument of type string");
            }

            return CalendarUtilities.FormatDate(date, env.Localizer, format, calendar);
        }

        #endregion

        #region If

        private TemplateFunction If()
        {
            return new TemplateFunction(IfImpl, null, IfPaths);
        }

        private object IfImpl(object[] args, EvaluationContext ctx)
        {
            int argCount = 3;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(If)}' expects {argCount} arguments");
            }

            var conditionObj = args[0] ?? false;
            if (!(conditionObj is bool condition))
            {
                throw new TemplateException($"Function '{nameof(If)}' expects a 1st argument condition of type bool");
            }

            return condition ? args[1] : args[2];
        }

        private async IAsyncEnumerable<Path> IfPaths(TemplexBase[] args, EvaluationContext ctx)
        {
            // Get arguments
            int argCount = 3;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(If)}' expects {argCount} arguments");
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

        private TemplateFunction AmountInWords(TemplateEnvironment env)
        {
            return new TemplateFunction(function: (object[] args, EvaluationContext ctx) => AmountInWordsImpl(args, ctx, env));
        }

        private object AmountInWordsImpl(object[] args, EvaluationContext ctx, TemplateEnvironment env)
        {
            // Validation
            int minArgCount = 1;
            int maxArgCount = 3;
            if (args.Length < minArgCount || args.Length > maxArgCount)
            {
                throw new TemplateException($"Function '{nameof(AmountInWords)}' expects at least {minArgCount} and at most {maxArgCount} arguments");
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
                throw new TemplateException($"{nameof(AmountInWords)} expects a 1st parameter amount of a numeric type");
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
                    throw new TemplateException($"{nameof(AmountInWords)} expects a 3rd parameter decimals of type int");
                }
            }

            // Validation
            if (decimals != null)
            {
                var allowedValues = new List<int> { 0, 2, 3 };
                if (!allowedValues.Contains(decimals.Value))
                {
                    throw new TemplateException($"{nameof(AmountInWords)} 3rd parameter can be one of the following: {string.Join(", ", allowedValues)}");
                }
            }

            // TODO: Add more languages based on env.Culture
            return AmountInWordsEn.ConvertAmount(amount, currencyIso, decimals);
        }

        #endregion

        #region StartsWith

        private TemplateFunction StartsWith()
        {
            return new TemplateFunction(StartsWithImpl);
        }

        private object StartsWithImpl(object[] args, EvaluationContext ctx)
        {
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(StartsWith)}' expects {argCount} arguments");
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
                    throw new TemplateException($"Function '{nameof(StartsWith)}' expects a 2st argument prefix of type string");
                }
            }
            else
            {
                throw new TemplateException($"Function '{nameof(StartsWith)}' expects a 1st argument text of type string");
            }
        }

        #endregion

        #region EndsWith

        private TemplateFunction EndsWith()
        {
            return new TemplateFunction(EndsWithImpl);
        }

        private object EndsWithImpl(object[] args, EvaluationContext ctx)
        {
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function '{nameof(EndsWith)}' expects {argCount} arguments");
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
                    return textString.EndsWith(prefixString);
                }
                else
                {
                    throw new TemplateException($"Function '{nameof(EndsWith)}' expects a 2st argument prefix of type string");
                }
            }
            else
            {
                throw new TemplateException($"Function '{nameof(EndsWith)}' expects a 1st argument text of type string");
            }
        }

        #endregion

        #region Barcode

        private TemplateFunction Barcode()
        {
            return new TemplateFunction(BarcodeImpl);
        }

        private object BarcodeImpl(object[] args, EvaluationContext ctx)
        {
            // Validation
            int minArgCount = 1;
            int maxArgCount = 5;
            if (args.Length < minArgCount || args.Length > maxArgCount)
            {
                throw new TemplateException($"Function '{nameof(AmountInWords)}' expects at least {minArgCount} and at most {maxArgCount} arguments");
            }

            // Amount
            string barcodeValue = args[0]?.ToString();
            if (string.IsNullOrWhiteSpace(barcodeValue))
            {
                return "";
            }

            var barcodeType = BarcodeLib.TYPE.CODE128; // Default
            if (args.Length >= 2)
            {
                // Some of the most widely used 1D barcodes according to https://www.dynamsoft.com/blog/insights/the-comprehensive-guide-to-1d-and-2d-barcodes/
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
                    _ => throw new TemplateException($"Unknown barcode standard '{barcodeTypeString}'"),
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
                    throw new TemplateException($"Function '{nameof(Barcode)}': 3rd argument includeLabel must be a boolean");
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
                    throw new TemplateException($"Function '{nameof(Barcode)}': 4th argument height must be an int");
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
                    throw new TemplateException($"Function '{nameof(Barcode)}': 5th argument barWidth must be an int");
                }
            }

            try
            {
                using var barcodeEncoder = new BarcodeLib.Barcode(barcodeValue, barcodeType)
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

                using var img = barcodeEncoder.Encode(barcodeType, barcodeValue);
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

        #region SA_InvoiceQrCode (TODO: Remove)

        private TemplateFunction SA_InvoiceQrCode()
        {
            return new TemplateFunction(SA_InvoiceQrCodeImpl);
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
            if (!(timestampObj is DateTimeOffset timestamp))
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
                using QRCodeGenerator qrGenerator = new QRCodeGenerator();
                using QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
                using QRCode qrCode = new QRCode(qrCodeData);
                using Bitmap img = qrCode.GetGraphic(pixelsPerModule: 8);
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

        #region PreviewWidth +  PreviewHeight

        private TemplateFunction PreviewWidth()
        {
            return new TemplateFunction(
                function: (object[] args, EvaluationContext ctx) => PreviewWidthImpl(args));
        }

        private TemplateFunction PreviewHeight()
        {
            return new TemplateFunction(
                function: (object[] args, EvaluationContext ctx) => PreviewHeightImpl(args));
        }

        private object PreviewHeightImpl(object[] args)
        {
            int argCount = 2;
            if (args.Length != argCount)
            {
                throw new TemplateException($"Function {nameof(PreviewHeight)} expects {argCount} parameters");
            }

            var pageSizeObj = args[0];
            var orientationObj = args[1];

            if (!(pageSizeObj is string pageSize))
            {
                throw new TemplateException($"Function {nameof(PreviewHeight)} expects 1st parameter pageSize of type string");
            }

            if (!(orientationObj is string orientation))
            {
                throw new TemplateException($"Function {nameof(PreviewHeight)} expects 2nd parameter orientation of type string");
            }

            var orientationLower = orientation.Trim().ToLower();
            if (orientationLower != "portrait" && orientationLower != "landscape")
            {
                throw new TemplateException($"Unknown orientation '{orientation}'");
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
                throw new TemplateException($"Function {nameof(PreviewWidth)} expects {argCount} parameters");
            }

            var pageSizeObj = args[0];
            var orientationObj = args[1];

            if (!(pageSizeObj is string pageSize))
            {
                throw new TemplateException($"Function {nameof(PreviewWidth)} expects 1st parameter pageSize of type string");
            }

            if (!(orientationObj is string orientation))
            {
                throw new TemplateException($"Function {nameof(PreviewWidth)} expects 2nd parameter orientation of type string");
            }

            var orientationLower = orientation.Trim().ToLower();
            if (orientationLower != "portrait" && orientationLower != "landscape")
            {
                throw new TemplateException($"Unknown orientation '{orientation}'");
            }

            bool isPortrait = orientationLower == "portrait";
            var pageWidth = PageLength(pageSize, shortSide: isPortrait);
            return pageWidth;
        }

        private string PageLength(string pageSize, bool shortSide)
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
                _ => throw new TemplateException($"Unknown page size {pageSize}")
            };
        }

        #endregion

        /// <summary>
        /// Additional contextual information used to generate the root <see cref="EvaluationContext"/>
        /// </summary>
        public struct TemplateEnvironment
        {
            public CultureInfo Culture { get; set; }
            public CancellationToken Cancellation { get; set; }
            public IStringLocalizer Localizer { get; set; }
        }
    }
}
