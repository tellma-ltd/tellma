using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
    public class PrintingTemplatesService : CrudServiceBase<PrintingTemplateForSave, PrintingTemplate, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer _localizer;
        private readonly TemplateService _templateService;

        public PrintingTemplatesService(
            ApplicationFactServiceBehavior behavior,
            CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
            _templateService = deps.TemplateService;
            _localizer = deps.Localizer;
        }

        protected override string View => $"printing-templates";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        public async Task<FileResult> Print(int templateId, PrintArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            var template = await FactBehavior.GetPrintingTemplate(templateId, cancellation);
            var (body, downloadName) = await PrintImpl(template, args, cancellation);

            // Return as a file
            var bodyBytes = Encoding.UTF8.GetBytes(body);
            return new FileResult(bodyBytes, downloadName);
        }

        public async Task<PreviewResult> Preview(PrintingPreviewTemplate template, PrintArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            var parameters = template.Parameters?.Select(e => new AbstractParameter(e.Key, e.Control));
            var abstractTemplate = new AbstractPrintingTemplate(template.Body, template.DownloadName, template.Context, parameters);

            var (body, downloadName) = await PrintImpl(abstractTemplate, args, cancellation);
            return new PreviewResult(body, downloadName);
        }

        private async Task<(string body, string fileName)> PrintImpl(AbstractPrintingTemplate template, PrintArguments args, CancellationToken cancellation)
        {
            // (1) The templates
            var templates = new TemplateInfo[] {
               new TemplateInfo(template.DownloadName, template.Context, TemplateLanguage.Text),
               new TemplateInfo(template.Body, template.Context, TemplateLanguage.Html)
            };

            // (2) Functions + Variables
            var globalFunctions = new Dictionary<string, EvaluationFunction>();
            var localFunctions = new Dictionary<string, EvaluationFunction>();
            var globalVariables = new Dictionary<string, EvaluationVariable>();
            var localVariables = new Dictionary<string, EvaluationVariable>();

            if (args.Custom != null && template.Parameters != null)
            {
                foreach (var parameter in template.Parameters)
                {
                    args.Custom.TryGetValue(parameter.Key, out string value);
                    localVariables.Add(parameter.Key, new EvaluationVariable(value));
                }
            }

            await FactBehavior.SetPrintingFunctions(localFunctions, globalFunctions, cancellation);
            await FactBehavior.SetPrintingVariables(localVariables, globalVariables, cancellation);

            // (3) Culture
            CultureInfo culture = GetCulture(args.Culture);

            // Generate the output
            var genArgs = new TemplateArguments(templates, globalFunctions, globalVariables, localFunctions, localVariables, culture: culture);
            string[] outputs = await _templateService.GenerateFromTemplates(genArgs, cancellation);

            var downloadName = outputs[0];
            var body = outputs[1];

            // Use a default download name if none is provided
            if (string.IsNullOrWhiteSpace(downloadName))
            {
                downloadName = "File.html";
            }

            if (!downloadName.ToLower().EndsWith(".html"))
            {
                downloadName += ".html";
            }

            // Return as a file
            return new(body, downloadName);
        }

        public async Task<PreviewResult> PreviewByFilter(PrintingPreviewTemplate entity, PrintEntitiesArguments<object> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // (1) The templates
            var templates = new TemplateInfo[] {
                new TemplateInfo(entity.DownloadName, entity.Context, TemplateLanguage.Text),
                new TemplateInfo(entity.Body, entity.Context, TemplateLanguage.Html)
            };

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

            await FactBehavior.SetPrintingFunctions(localFunctions, globalFunctions, cancellation);
            await FactBehavior.SetPrintingVariables(localVariables, globalVariables, cancellation);

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
            var templateArgs = new TemplateArguments(
                templates: templates,
                customGlobalFunctions: globalFunctions,
                customGlobalVariables: globalVariables,
                customLocalFunctions: localFunctions,
                customLocalVariables: localVariables,
                preloadedQuery: preloadedQuery,
                culture: culture);

            var outputs = await _templateService.GenerateFromTemplates(templateArgs, cancellation);
            var downloadName = AppendExtension(outputs[0], entity);
            var body = outputs[1];

            // Return as a file
            return new PreviewResult(body, downloadName);
        }

        public async Task<PreviewResult> PreviewById(string id, PrintingPreviewTemplate entity, PrintEntityByIdArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // (1) The templates
            var templates = new TemplateInfo[] {
                new TemplateInfo(entity.DownloadName, entity.Context, TemplateLanguage.Text),
                new TemplateInfo(entity.Body, entity.Context, TemplateLanguage.Html)
            };

            // (2) Functions + Variables
            var globalFunctions = new Dictionary<string, EvaluationFunction>();
            var localFunctions = new Dictionary<string, EvaluationFunction>();
            var globalVariables = new Dictionary<string, EvaluationVariable>();
            var localVariables = new Dictionary<string, EvaluationVariable>
            {
                ["$Source"] = new EvaluationVariable(entity.DefinitionId == null ? entity.Collection : $"{entity.Collection}/{entity.DefinitionId}"),
                ["$Id"] = new EvaluationVariable(id ?? throw new ServiceException("The id argument is required.")),
            };

            await FactBehavior.SetPrintingFunctions(localFunctions, globalFunctions, cancellation);
            await FactBehavior.SetPrintingVariables(localVariables, globalVariables, cancellation);

            // (3) Preloaded Query
            var preloadedQuery = new QueryEntityByIdInfo(entity.Collection, entity.DefinitionId, id);

            // (4) Culture
            var culture = GetCulture(args, await _behavior.Settings(cancellation));

            // Generate output
            var genArgs = new TemplateArguments(
                templates: templates,
                customGlobalFunctions: globalFunctions,
                customGlobalVariables: globalVariables,
                customLocalFunctions: localFunctions,
                customLocalVariables: localVariables,
                preloadedQuery: preloadedQuery,
                culture: culture);

            var outputs = await _templateService.GenerateFromTemplates(genArgs, cancellation);
            var downloadName = AppendExtension(outputs[0], entity);
            var body = outputs[1];

            // Return as a file
            return new PreviewResult(body, downloadName);
        }

        private string AppendExtension(string downloadName, PrintingPreviewTemplate _)
        {
            // Append the file extension if missing
            if (string.IsNullOrWhiteSpace(downloadName))
            {
                downloadName = _localizer["File"];
            }

            var expectedExtension = ".html";
            if (expectedExtension != null && !downloadName.EndsWith(expectedExtension))
            {
                downloadName += expectedExtension;
            }

            return downloadName;
        }

        protected override Task<EntityQuery<PrintingTemplate>> Search(EntityQuery<PrintingTemplate> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(PrintingTemplate.Name);
                var name2 = nameof(PrintingTemplate.Name2);
                var name3 = nameof(PrintingTemplate.Name3);
                var code = nameof(PrintingTemplate.Code);
                var desc = nameof(PrintingTemplate.Description);
                var desc2 = nameof(PrintingTemplate.Description2);
                var desc3 = nameof(PrintingTemplate.Description3);

                var filterString = $"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}' or {desc} contains '{search}' or {desc2} contains '{search}' or {desc3} contains '{search}'";
                query = query.Filter(filterString);
            }

            return Task.FromResult(query);
        }

        protected override async Task<List<PrintingTemplateForSave>> SavePreprocessAsync(List<PrintingTemplateForSave> entities)
        {
            var settings = await _behavior.Settings();

            // Defaults
            entities.ForEach(entity =>
            {
                entity.Roles ??= new List<PrintingTemplateRoleForSave>();
                entity.Parameters ??= new List<PrintingTemplateParameterForSave>();
                entity.Parameters.ForEach(p =>
                {
                    p.IsRequired ??= false;
                    p.ControlOptions = ApplicationUtil.PreprocessControlOptions(p.Control, p.ControlOptions, settings);
                });

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

                if (entity.Usage == TemplateUsages.Standalone)
                {
                    entity.IsDeployed = entity.Roles.Count > 0;
                }
                else
                {
                    entity.Roles = new List<PrintingTemplateRoleForSave>();
                }

                if (entity.Roles.Count == 0)
                {
                    entity.MainMenuIcon = null;
                    entity.MainMenuSection = null;
                    entity.MainMenuSortKey = null;
                }
            });

            return entities;
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<PrintingTemplateForSave> entities, bool returnIds)
        {
            var definitionedCollections = new string[]
            {
                nameof(Document),
                nameof(Resource),
                nameof(Agent),
                nameof(Lookup)
            };

            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                if (entity.Usage == TemplateUsages.FromSearchAndDetails || entity.Usage == TemplateUsages.FromDetails)
                {
                    if (entity.Collection == null)
                    {
                        ModelState.AddError($"[{index}].Collection", _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Template_Collection"]]);
                    }
                    else
                    {
                        if (definitionedCollections.Contains(entity.Collection))
                        {
                            // DefinitionId is required when querying by Id
                            if (entity.Usage == TemplateUsages.FromDetails && entity.DefinitionId == null)
                            {
                                ModelState.AddError($"[{index}].DefinitionId", _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Template_DefinitionId"]]);
                            }
                        }
                        else
                        {
                            entity.DefinitionId = null;
                        }
                    }
                }

                // Label is required
                if (entity.Roles.Any())
                {
                    if (string.IsNullOrWhiteSpace(entity.Name))
                    {
                        string path = $"[{index}].{nameof(entity.Name)}";
                        string msg = _localizer["Error_TitleIsRequiredWhenShowInMainMenu"];

                        ModelState.AddError(path, msg);
                    }
                }

                // TODO Check that DefinitionId is compatible with Collection

                var duplicateKeys = entity
                    .Parameters
                    .Select(e => e.Key)
                    .GroupBy(e => e)
                    .Where(e => e.Count() > 1)
                    .Select(e => e.FirstOrDefault())
                    .ToHashSet();

                // Validate parameters
                foreach (var (parameter, parameterIndex) in entity.Parameters.Select((e, i) => (e, i)))
                {
                    if (!TemplexVariable.IsValidVariableName(parameter.Key))
                    {
                        var path = $"[{index}].{nameof(entity.Parameters)}[{parameterIndex}].{nameof(parameter.Key)}";
                        var msg = "Invalid Key, valid keys contain alphanumeric characters or $ or _ only and do not start with a number.";
                        ModelState.AddError(path, msg);
                    }
                    else if (duplicateKeys.Contains(parameter.Key))
                    {
                        var path = $"[{index}].{nameof(entity.Parameters)}[{parameterIndex}].{nameof(parameter.Key)}";
                        var msg = $"They Key '{parameter.Key}' is used more than once.";
                        ModelState.AddError(path, msg);
                    }
                }
            }

            SaveOutput result = await _behavior.Repository.PrintingTemplates__Save(
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
            DeleteOutput result = await _behavior.Repository.PrintingTemplates__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken _)
        {
            var result = ExpressionOrderBy.Parse(nameof(PrintingTemplate.Name));
            return Task.FromResult(result);
        }

        private static CultureInfo GetCulture(PrintArguments args, SettingsForClient settings)
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
