using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
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
        
        // Just the names of the standard query functions
        private string QueryByFilter => nameof(QueryByFilter);
        private string QueryById => nameof(QueryById);
        private string QueryAggregate => nameof(QueryAggregate);

        public TemplateService(IServiceProvider serviceProvider)
        {
            _provider = serviceProvider;
            _repo = _provider.GetRequiredService<ApplicationRepository>();
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
            string[] templates,
            Dictionary<string, object> inputVariables,
            QueryInfo preloadedQuery, // Optional, instructs the template service to preload this query in the $ variable
            CultureInfo culture,
            CancellationToken cancellation)
        {
            // Parse the title and body
            TemplateTree[] trees = new TemplateTree[templates.Length];
            for (int i = 0; i < templates.Length; i++)
            {
                trees[i] = TemplateTree.Parse(templates[i]);
            }

            // Prepare the evaluation context
            EvaluationContext ctx = GetRootContext(new TemplateEnvironment
            {
                Culture = culture,
                Cancellation = cancellation,
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

                var select = string.Join(",", trie.GetPaths().Select(p => string.Join("/", p)));

                // Load the query
                if (query is QueryByFilterInfo queryByFilter)
                {
                    // Prepare the GetArguments
                    var args = new GetArguments
                    {
                        Select = select,
                        Filter = queryByFilter.Filter,
                        OrderBy = queryByFilter.OrderBy,
                        // TODO: add query Ids
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

                    switch (query.Collection)
                    {
                        case nameof(Document):
                            {
                                FactServiceBase<Document> controller;
                                if (query.DefinitionId == null)
                                {
                                    controller = _provider.GetRequiredService<DocumentsGenericService>();
                                }
                                else
                                {
                                    controller = _provider.GetRequiredService<DocumentsService>().SetDefinitionId(query.DefinitionId.Value);
                                }

                                var (list, _, _, _) = await controller.GetFact(args, cancellation);

                                queryResults[query] = list;
                                break;
                            }
                        case nameof(DetailsEntry): // TODO
                        case nameof(Relation):
                            {
                                FactServiceBase<Relation> controller;
                                if (query.DefinitionId == null)
                                {
                                    controller = _provider.GetRequiredService<RelationsGenericService>();
                                }
                                else
                                {
                                    controller = _provider.GetRequiredService<RelationsService>().SetDefinitionId(query.DefinitionId.Value);
                                }

                                var (list, _, _, _) = await controller.GetFact(args, cancellation);

                                queryResults[query] = list;
                                break;
                            }
                        default:
                            throw new TemplateException($"Unknown collection '{query.Collection}'");
                    }
                }
                else if (query is QueryByIdInfo queryById)
                {
                    // Prepare the GetArguments
                    var args = new GetByIdArguments
                    {
                        Select = select
                    };

                    switch (query.Collection)
                    {
                        case nameof(Document):
                            {
                                // Definition Id
                                var defId = query.DefinitionId ??
                                    throw new TemplateException("To query documents by Id, the source parameter must contain the definitionId. E.g. 'documents/ManualJournalVoucher'");

                                // Id
                                if (!int.TryParse(queryById.Id, out int id))
                                {
                                    throw new TemplateException("To query documents by Id, the id must be of type integer");
                                }

                                // Load the result
                                FactGetByIdServiceBase<Document, int> controller = _provider.GetRequiredService<DocumentsService>().SetDefinitionId(defId); ;
                                var (entity, _) = await controller.GetById(id, args, cancellation);

                                queryResults[query] = entity;
                                break;
                            }
                        case nameof(DetailsEntry): // TODO
                        case nameof(Relation):
                            {
                                // Definition Id
                                var defId = query.DefinitionId ??
                                    throw new TemplateException("To query documents by Id, the source parameter must contain the definitionId. E.g. 'documents/ManualJournalVoucher'");

                                // Id
                                if (!int.TryParse(queryById.Id, out int id))
                                {
                                    throw new TemplateException("To query documents by Id, the id must be of type integer");
                                }

                                // Load the result
                                FactGetByIdServiceBase<Relation, int> controller = _provider.GetRequiredService<RelationsService>().SetDefinitionId(defId); ;
                                var (entity, _) = await controller.GetById(id, args, cancellation);

                                queryResults[query] = entity;
                                break;
                            }
                        default:
                            throw new TemplateException($"Unknown collection '{query.Collection}'");
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
                    await trees[i].GenerateOutput(builder, ctx);

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
                [nameof(Sum)] = Sum(env),
                [nameof(Filter)] = Filter(env),
                [nameof(Count)] = Count(),
                [nameof(Localize)] = Localize(env),
                [nameof(Format)] = Format(),
                [nameof(If)] = If(),
                [nameof(PreviewWidth)] = PreviewWidth(),
                [nameof(PreviewHeight)] = PreviewHeight()
            };

            // Global Variables
            var globalVariables = new EvaluationContext.VariablesDictionary
            {
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

        private async IAsyncEnumerable<Path> QueryByFilterPaths(ExpressionBase[] args, EvaluationContext ctx)
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

        private async IAsyncEnumerable<Path> QueryByIdPaths(ExpressionBase[] args, EvaluationContext ctx)
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

        private TemplateFunction Filter(TemplateEnvironment env)
        {
            return new TemplateFunction(
                functionAsync: (object[] args, EvaluationContext ctx) => FilterImpl(args, ctx, env),
                additionalSelectResolver: (ExpressionBase[] args, EvaluationContext ctx) => FilterSelect(args, ctx, env),
                pathsResolver: (ExpressionBase[] args, EvaluationContext ctx) => FilterPaths(args, ctx, env));
        }

        private async Task<object> FilterImpl(object[] args, EvaluationContext ctx, TemplateEnvironment env)
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

            var conditionExp = ExpressionBase.Parse(conditionString) ??
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

        private async IAsyncEnumerable<Path> FilterSelect(ExpressionBase[] args, EvaluationContext ctx, TemplateEnvironment env)
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
            if (!(conditionObj is string valueSelectorString))
            {
                throw new TemplateException($"Function '{nameof(Filter)}' expects a 2nd argument selector of type string");
            }

            var conditionExp = ExpressionBase.Parse(valueSelectorString) ??
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

        private IAsyncEnumerable<Path> FilterPaths(ExpressionBase[] args, EvaluationContext ctx, TemplateEnvironment env)
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

        #region Sum

        private TemplateFunction Sum(TemplateEnvironment env)
        {
            return new TemplateFunction(
                functionAsync: (object[] args, EvaluationContext ctx) => SumImpl(args, ctx, env),
                additionalSelectResolver: (ExpressionBase[] args, EvaluationContext ctx) => SumSelect(args, ctx, env));
        }

        private async Task<object> SumImpl(object[] args, EvaluationContext ctx, TemplateEnvironment env)
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

            var valueSelectorExp = ExpressionBase.Parse(valueSelectorString) ??
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

        private async IAsyncEnumerable<Path> SumSelect(ExpressionBase[] args, EvaluationContext ctx, TemplateEnvironment env)
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

            var valueSelectorExp = ExpressionBase.Parse(valueSelectorString) ??
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

        private async IAsyncEnumerable<Path> IfPaths(ExpressionBase[] args, EvaluationContext ctx)
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
        }
    }
}
