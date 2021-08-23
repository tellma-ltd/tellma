using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.Metadata;
using Tellma.Api.Templating;
using Tellma.Model.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class MarkupTemplatesService : CrudServiceBase<MarkupTemplateForSave, MarkupTemplate, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer _localizer;
        private readonly TemplateService _templateService;

        public MarkupTemplatesService(
            ApplicationFactServiceBehavior behavior,
            CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
            _templateService = deps.TemplateService;
            _localizer = deps.Localizer;
        }

        protected override string View => $"markup-templates";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        public async Task<PreviewResult> Preview(MarkupPreviewTemplate entity, GenerateMarkupArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // (1) The templates
            var templates = new (string, string)[] { (entity.DownloadName, MarkupLanguages.Text), (entity.Body, entity.MarkupLanguage) };

            // (2) Functions + Variables
            var globalFunctions = new Dictionary<string, EvaluationFunction>();
            var localFunctions = new Dictionary<string, EvaluationFunction>();
            var globalVariables = new Dictionary<string, EvaluationVariable>();
            var localVariables = new Dictionary<string, EvaluationVariable>();

            await FactBehavior.SetMarkupFunctions(localFunctions, globalFunctions, cancellation);
            await FactBehavior.SetMarkupVariables(localVariables, globalVariables, cancellation);

            // (3) Culture
            var culture = GetCulture(args, await _behavior.Settings(cancellation));

            // Generate output
            var genArgs = new MarkupArguments(
                templates: templates,
                customGlobalFunctions: globalFunctions,
                customGlobalVariables: globalVariables,
                customLocalFunctions: localFunctions,
                customLocalVariables: localVariables,
                preloadedQuery: null,
                culture: culture);

            var outputs = await _templateService.GenerateMarkup(genArgs, cancellation);
            var downloadName = AppendExtension(outputs[0], entity);
            var body = outputs[1];

            // Return as a file
            return new PreviewResult(body, downloadName);
        }

        public async Task<PreviewResult> PreviewByFilter(MarkupPreviewTemplate entity, GenerateMarkupByFilterArguments<object> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // (1) The templates
            var templates = new (string, string)[] { (entity.DownloadName, MarkupLanguages.Text), (entity.Body, entity.MarkupLanguage) };

            // (2) Functions + Variables
            var globalFunctions = new Dictionary<string, EvaluationFunction>();
            var localFunctions = new Dictionary<string, EvaluationFunction>();
            var globalVariables = new Dictionary<string, EvaluationVariable>();
            var localVariables = new Dictionary<string, EvaluationVariable>
            {
                ["$Source"] = new EvaluationVariable(entity.DefinitionId == null ? entity.Collection : $"{entity.Collection}/{entity.DefinitionId}"),
                ["$Filter"] = new EvaluationVariable(args.Filter),
                ["$OrderBy"] = new EvaluationVariable(args.OrderBy),
                ["$Top"] = new EvaluationVariable(args.Top),
                ["$Skip"] = new EvaluationVariable(args.Skip),
                ["$Ids"] = new EvaluationVariable(args.I)
            };

            await FactBehavior.SetMarkupFunctions(localFunctions, globalFunctions, cancellation);
            await FactBehavior.SetMarkupVariables(localVariables, globalVariables, cancellation);

            // (3) Preloaded Query
            QueryInfo preloadedQuery;
            if (args.I != null && args.I.Any())
            {
                preloadedQuery = new QueryEntitiesByIdsInfo(
                    collection: entity.Collection,
                    definitionId: entity.DefinitionId,
                    ids: args.I);
            }
            else
            {
                preloadedQuery = new QueryEntitiesInfo(
                    collection: entity.Collection,
                    definitionId: entity.DefinitionId,
                    filter: args.Filter,
                    orderby: args.OrderBy,
                    top: args.Top,
                    skip: args.Skip);
            }

            // (4) Culture
            var culture = GetCulture(args, await _behavior.Settings(cancellation));

            // Generate output
            var genArgs = new MarkupArguments(
                templates: templates,
                customGlobalFunctions: globalFunctions,
                customGlobalVariables: globalVariables,
                customLocalFunctions: localFunctions,
                customLocalVariables: localVariables,
                preloadedQuery: preloadedQuery,
                culture: culture);

            var outputs = await _templateService.GenerateMarkup(genArgs, cancellation);
            var downloadName = AppendExtension(outputs[0], entity);
            var body = outputs[1];

            // Return as a file
            return new PreviewResult(body, downloadName);
        }

        public async Task<PreviewResult> PreviewById(string id, MarkupPreviewTemplate entity, GenerateMarkupByIdArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // (1) The templates
            var templates = new (string, string)[] { (entity.DownloadName, MarkupLanguages.Text), (entity.Body, entity.MarkupLanguage) };

            // (2) Functions + Variables
            var globalFunctions = new Dictionary<string, EvaluationFunction>();
            var localFunctions = new Dictionary<string, EvaluationFunction>();
            var globalVariables = new Dictionary<string, EvaluationVariable>();
            var localVariables = new Dictionary<string, EvaluationVariable>
            {
                ["$Source"] = new EvaluationVariable(entity.DefinitionId == null ? entity.Collection : $"{entity.Collection}/{entity.DefinitionId}"),
                ["$Id"] = new EvaluationVariable(id ?? throw new ServiceException("The id argument is required.")),
            };

            await FactBehavior.SetMarkupFunctions(localFunctions, globalFunctions, cancellation);
            await FactBehavior.SetMarkupVariables(localVariables, globalVariables, cancellation);

            // (3) Preloaded Query
            var preloadedQuery = new QueryEntityByIdInfo(entity.Collection, entity.DefinitionId, id);

            // (4) Culture
            var culture = GetCulture(args, await _behavior.Settings(cancellation));

            // Generate output
            var genArgs = new MarkupArguments(
                templates: templates,
                customGlobalFunctions: globalFunctions,
                customGlobalVariables: globalVariables,
                customLocalFunctions: localFunctions,
                customLocalVariables: localVariables,
                preloadedQuery: preloadedQuery,
                culture: culture);

            var outputs = await _templateService.GenerateMarkup(genArgs, cancellation);
            var downloadName = AppendExtension(outputs[0], entity);
            var body = outputs[1];

            // Return as a file
            return new PreviewResult(body, downloadName);
        }

        private string AppendExtension(string downloadName, MarkupPreviewTemplate entity)
        {
            // Append the file extension if missing
            if (string.IsNullOrWhiteSpace(downloadName))
            {
                downloadName = _localizer["File"];
            }

            var expectedExtension = "." + entity.MarkupLanguage switch { MarkupLanguages.Html => "html", _ => null };
            if (expectedExtension != null && !downloadName.EndsWith(expectedExtension))
            {
                downloadName += expectedExtension;
            }

            return downloadName;
        }

        protected override Task<EntityQuery<MarkupTemplate>> Search(EntityQuery<MarkupTemplate> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(MarkupTemplate.Name);
                var name2 = nameof(MarkupTemplate.Name2);
                var name3 = nameof(MarkupTemplate.Name3);
                var code = nameof(MarkupTemplate.Code);
                var desc = nameof(MarkupTemplate.Description);
                var desc2 = nameof(MarkupTemplate.Description2);
                var desc3 = nameof(MarkupTemplate.Description3);

                var filterString = $"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}' or {desc} contains '{search}' or {desc2} contains '{search}' or {desc3} contains '{search}'";
                query = query.Filter(filterString);
            }

            return Task.FromResult(query);
        }

        protected override async Task<List<MarkupTemplateForSave>> SavePreprocessAsync(List<MarkupTemplateForSave> entities)
        {
            var settings = await _behavior.Settings();

            // Defaults
            entities.ForEach(entity =>
            {
                // Set defaults
                entity.SupportsPrimaryLanguage ??= false;
                entity.SupportsSecondaryLanguage ??= false;
                entity.SupportsTernaryLanguage ??= false;

                // Make sure we adhere to company languages
                if (settings.SecondaryLanguageId == null)
                {
                    entity.SupportsSecondaryLanguage = false;
                }

                if (settings.TernaryLanguageId == null)
                {
                    entity.SupportsTernaryLanguage = false;
                }

                // Make sure at least primary language is true
                if (!entity.SupportsSecondaryLanguage.Value && !entity.SupportsTernaryLanguage.Value)
                {
                    entity.SupportsPrimaryLanguage = true;
                }

                // Collection and DefinitionId only make sense when the usage is specified
                if (entity.Usage == null)
                {
                    entity.Collection = null;
                    entity.DefinitionId = null;
                }
            });

            // SQL Preprocessing
            // await _behavior.Repository.MarkupTemplates__Preprocess(entities);

            return entities;
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<MarkupTemplateForSave> entities, bool returnIds)
        {
            var definitionedCollections = new string[]
            {
                nameof(Document),
                nameof(Resource),
                nameof(Relation),
                nameof(Lookup)
            };

            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                if (entity.Usage == MarkupTemplateConst.QueryByFilter || entity.Usage == MarkupTemplateConst.QueryById)
                {
                    if (entity.Collection == null)
                    {
                        ModelState.AddError($"[{index}].Collection", _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["MarkupTemplate_Collection"]]);
                    }
                    else
                    {
                        if (definitionedCollections.Contains(entity.Collection))
                        {
                            // DefinitionId is required when querying by Id
                            if (entity.Usage == MarkupTemplateConst.QueryById && entity.DefinitionId == null)
                            {
                                ModelState.AddError($"[{index}].DefinitionId", _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["MarkupTemplate_DefinitionId"]]);
                            }
                        }
                        else
                        {
                            entity.DefinitionId = null;
                        }
                    }
                }

                // TODO Check that DefinitionId is compatible with Collection
            }

            SaveOutput result = await _behavior.Repository.MarkupTemplates__Save(
                entities: entities,
                returnIds: returnIds,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            return result.Ids;
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            DeleteOutput result = await _behavior.Repository.MarkupTemplates__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken _)
        {
            var result = ExpressionOrderBy.Parse(nameof(MarkupTemplate.Name));
            return Task.FromResult(result);
        }

        private static CultureInfo GetCulture(GenerateMarkupArguments args, SettingsForClient settings)
        {
            var culture = GetCulture(args.Culture);

            // Some validation
            if (args.Culture != null && settings.PrimaryLanguageId != args.Culture && settings.SecondaryLanguageId != args.Culture && settings.TernaryLanguageId != args.Culture)
            {
                throw new ServiceException($"Culture '{args.Culture}' is not supported in this company.");
            }

            return culture;
        }
    }
}
