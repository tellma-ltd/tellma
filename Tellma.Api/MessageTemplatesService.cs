using Cronos;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
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
using Tellma.Utilities.Common;

namespace Tellma.Api
{
    public class MessageTemplatesService : CrudServiceBase<MessageTemplateForSave, MessageTemplate, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer _localizer;
        private readonly TemplateService _templateService;

        public MessageTemplatesService(
            ApplicationFactServiceBehavior behavior,
            CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
            _templateService = deps.TemplateService;
            _localizer = deps.Localizer;
        }

        protected override string View => $"message-templates";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        // Standalone Screen
        public async Task<MessageCommandPreview> MessageCommandPreview(int templateId, PrintArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _behavior.MessageCommandPreview(templateId, args, cancellation);
        }

        public async Task<int> SendByMessage(int templateId, PrintArguments args, string version, CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _behavior.SendByMessage(templateId, args, version, cancellation);
        }

        // Studio Preview
        public async Task<MessageCommandPreview> MessageCommandPreview(MessageTemplate template, PrintArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            if (template == null)
            {
                throw new ServiceException($"Message template was not supplied.");
            }

            await FillNavigationProperties(template, cancellation);
            var localVariables = BaseUtil.CustomLocalVariables(args, template.Parameters?.Select(e => e.Key));

            return await _behavior.CreateMessageCommandPreview(
                template: template,
                preloadedQuery: null,
                localVariables: localVariables,
                cultureString: args.Culture,
                cancellation: cancellation);
        }

        public async Task<MessageCommandPreview> MessageCommandPreviewEntities(MessageTemplate template, PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            if (template == null)
            {
                throw new ServiceException($"Message template was not supplied.");
            }

            await FillNavigationProperties(template, cancellation);

            var preloadedQuery = BaseUtil.EntitiesPreloadedQuery(args: args, collection: template.Collection, defId: template.DefinitionId);
            var localVars = BaseUtil.EntitiesLocalVariables(args: args, collection: template.Collection, defId: template.DefinitionId);

            return await _behavior.CreateMessageCommandPreview(
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVars,
                cultureString: args.Culture,
                cancellation: cancellation);
        }

        public async Task<MessageCommandPreview> MessageCommandPreviewEntity(object id, MessageTemplate template, PrintEntityByIdArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            if (template == null)
            {
                throw new ServiceException($"Message template was not supplied.");
            }

            await FillNavigationProperties(template, cancellation);

            var preloadedQuery = BaseUtil.EntityPreloadedQuery(id, args: args, collection: template.Collection, defId: template.DefinitionId);
            var localVars = BaseUtil.EntityLocalVariables(id, args: args, collection: template.Collection, defId: template.DefinitionId);

            return await _behavior.CreateMessageCommandPreview(
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVars,
                cultureString: args.Culture,
                cancellation: cancellation);
        }

        // Helpers
        private async Task FillNavigationProperties(MessageTemplate template, CancellationToken cancellation)
        {
            // Fill the Users
            if (template?.Subscribers != null)
            {
                var userIds = template.Subscribers.Select(e => e?.UserId ?? 0).Where(e => e != 0);
                var users = await _behavior.Repository.Users
                    .FilterByIds(userIds)
                    .ToListAsync(QueryContext, cancellation);

                var usersDic = users.ToDictionary(e => e.Id);
                foreach (var sub in template.Subscribers)
                {
                    if (sub?.UserId != null && usersDic.TryGetValue(sub.UserId.Value, out User user))
                    {
                        sub.User = user;
                    }
                    else
                    {
                        sub.User = null;
                    }
                }
            }
        }

        protected override Task<EntityQuery<MessageTemplate>> Search(EntityQuery<MessageTemplate> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(MessageTemplate.Name);
                var name2 = nameof(MessageTemplate.Name2);
                var name3 = nameof(MessageTemplate.Name3);
                var code = nameof(MessageTemplate.Code);
                var desc = nameof(MessageTemplate.Description);
                var desc2 = nameof(MessageTemplate.Description2);
                var desc3 = nameof(MessageTemplate.Description3);

                var filterString = $"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}' or {desc} contains '{search}' or {desc2} contains '{search}' or {desc3} contains '{search}'";
                query = query.Filter(filterString);
            }

            return Task.FromResult(query);
        }

        protected override async Task<List<MessageTemplateForSave>> SavePreprocessAsync(List<MessageTemplateForSave> entities)
        {
            var settings = await _behavior.Settings();

            // Defaults
            entities.ForEach(entity =>
            {
                // Defaults

                entity.PreventRenotify ??= false;
                entity.IsDeployed ??= false;
                entity.Parameters ??= new List<MessageTemplateParameterForSave>();
                entity.Subscribers ??= new List<MessageTemplateSubscriberForSave>();
                entity.Parameters.ForEach(p =>
                {
                    p.IsRequired ??= false;
                    p.ControlOptions = ApplicationUtil.PreprocessControlOptions(p.Control, p.ControlOptions, settings);
                });

                // Useless fields

                if (entity.Cardinality != Cardinalities.Multiple)
                {
                    entity.ListExpression = null;
                }

                if (entity.Cardinality != Cardinality.Single)
                {
                    entity.Subscribers = new List<MessageTemplateSubscriberForSave>();
                }

                if (entity.Trigger != Triggers.Automatic)
                {
                    entity.Schedule = null;
                    entity.ConditionExpression = null;
                    entity.PreventRenotify = false; // Default
                }

                if (entity.Trigger != Triggers.Manual)
                {
                    entity.Usage = null;
                    entity.Parameters = new List<MessageTemplateParameterForSave>();
                }

                if (entity.Usage != TemplateUsages.FromSearchAndDetails && entity.Usage != TemplateUsages.FromDetails)
                {
                    // Collection and DefinitionId only make sense in certain usages
                    entity.Collection = null;
                    entity.DefinitionId = null;
                }
                
                if (entity.Usage != TemplateUsages.Standalone)
                {
                    // Parameters are only supported in standalone
                    entity.Parameters = new List<MessageTemplateParameterForSave>();

                    entity.MainMenuIcon = null;
                    entity.MainMenuSection = null;
                    entity.MainMenuSortKey = null;
                }

                if (!entity.PreventRenotify.Value)
                {
                    entity.Version = null;
                }

                if (entity.Usage != TemplateUsages.Standalone || !entity.IsDeployed.Value)
                {
                    entity.MainMenuIcon = null;
                    entity.MainMenuSection = null;
                    entity.MainMenuSortKey = null;
                }
            });

            return entities;
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<MessageTemplateForSave> entities, bool returnIds)
        {
            #region Validation

            var definitionedCollections = new string[]
            {
                nameof(Document),
                nameof(Resource),
                nameof(Agent),
                nameof(Lookup)
            };

            foreach (var (entity, index) in entities.Indexed())
            {
                if (entity.Cardinality == Cardinalities.Multiple)
                {
                    // ListExpression
                    if (string.IsNullOrWhiteSpace(entity.ListExpression))
                    {
                        var path = $"[{index}].{nameof(entity.ListExpression)}";
                        var msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["NotificationTemplate_ListExpression"]];
                        ModelState.AddError(path, msg);
                    }
                    else
                    {
                        try
                        {
                            TemplexBase.Parse(entity.ListExpression);
                        }
                        catch
                        {
                            var path = $"[{index}].{nameof(entity.ListExpression)}";
                            var msg = _localizer["Error_InvalidTemplateExpression0", entity.ListExpression];
                            ModelState.AddError(path, msg);
                        }
                    }
                }

                if (entity.Trigger == Triggers.Automatic)
                {
                    // Automatic notifications can only be created by a Read-All user.
                    var permissions = await FactBehavior.UserPermissions(view: "all", action: "Read", cancellation: default);
                    if (!permissions.Any())
                    {
                        var path = $"[{index}].{nameof(entity.Trigger)}";
                        var msg = _localizer["Error_AutomaticTriggerOnlyForReadAllUsers"];
                        ModelState.AddError(path, msg);
                    }

                    // Schedule
                    if (string.IsNullOrWhiteSpace(entity.Schedule))
                    {
                        var path = $"[{index}].{nameof(entity.Schedule)}";
                        var msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["NotificationTemplate_Schedule"]];
                        ModelState.AddError(path, msg);
                    }
                    else
                    {
                        string faultyCron = null;
                        try
                        {
                            var crons = entity.Schedule.Split(';')
                                .Where(e => !string.IsNullOrWhiteSpace(e))
                                .Select(e => e.Trim());

                            foreach (var cron in crons)
                            {
                                faultyCron = cron;
                                CronExpression.Parse(cron);
                            }
                        }
                        catch
                        {
                            var path = $"[{index}].{nameof(entity.Schedule)}";
                            var msg = _localizer["Error_InvalidCronExpression0", faultyCron];
                            ModelState.AddError(path, msg);
                        }
                    }

                    if (string.IsNullOrWhiteSpace(entity.ConditionExpression))
                    {
                        // Doesn't matter
                    }
                    else
                    {
                        try
                        {
                            TemplexBase.Parse(entity.ConditionExpression);
                        }
                        catch
                        {
                            var path = $"[{index}].{nameof(entity.ConditionExpression)}";
                            var msg = _localizer["Error_InvalidTemplateExpression0", entity.ConditionExpression];
                            ModelState.AddError(path, msg);
                        }
                    }
                }

                if (entity.Trigger == Triggers.Manual)
                {
                    // Usage
                    if (string.IsNullOrWhiteSpace(entity.Usage))
                    {
                        var path = $"[{index}].{nameof(entity.Usage)}";
                        var msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Template_Usage"]];
                        ModelState.AddError(path, msg);
                    }
                    else if (entity.Usage == TemplateUsages.FromSearchAndDetails || entity.Usage == TemplateUsages.FromDetails)
                    {
                        if (string.IsNullOrWhiteSpace(entity.Collection))
                        {
                            var path = $"[{index}].{nameof(entity.Collection)}";
                            var msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Template_Collection"]];
                            ModelState.AddError(path, msg);
                        }
                        else
                        {
                            if (definitionedCollections.Contains(entity.Collection))
                            {
                                // DefinitionId is required when querying by Id
                                if (entity.Usage == TemplateUsages.FromDetails && entity.DefinitionId == null)
                                {
                                    var path = $"[{index}].{nameof(entity.DefinitionId)}";
                                    var msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Template_DefinitionId"]];
                                    ModelState.AddError(path, msg);
                                }
                            }
                            else
                            {
                                entity.DefinitionId = null;
                            }
                        }
                    }
                    else if (entity.Usage == TemplateUsages.Standalone)
                    {
                        // Nothing to check here
                    }

                    // TODO Check that DefinitionId is compatible with Collection    
                }


                // PhoneNumber
                if (!string.IsNullOrWhiteSpace(entity.PhoneNumber))
                {
                    try
                    {
                        TemplateTree.Parse(entity.PhoneNumber);
                    }
                    catch
                    {
                        var path = $"[{index}].{nameof(entity.PhoneNumber)}";
                        var msg = _localizer["Error_InvalidTemplate"];
                        ModelState.AddError(path, msg);
                    }
                }

                // Body
                if (!string.IsNullOrWhiteSpace(entity.Content))
                {
                    const int maxSmsExpressionLength = 2048;
                    if (entity.Content.Length > maxSmsExpressionLength)
                    {
                        var path = $"[{index}].{nameof(entity.Content)}";
                        var msg = _localizer[ErrorMessages.Error_Field0LengthMaximumOf1, _localizer["NotificationTemplate_Content"], maxSmsExpressionLength];
                        ModelState.AddError(path, msg);
                    }

                    try
                    {
                        TemplateTree.Parse(entity.Content);
                    }
                    catch
                    {
                        var path = $"[{index}].{nameof(entity.Content)}";
                        var msg = _localizer["Error_InvalidTemplate"];
                        ModelState.AddError(path, msg);
                    }
                }

                // Caption
                try
                {
                    TemplateTree.Parse(entity.Caption);
                }
                catch
                {
                    var path = $"[{index}].{nameof(entity.Caption)}";
                    var msg = _localizer["Error_InvalidTemplate"];
                    ModelState.AddError(path, msg);
                }

                var duplicateKeys = entity
                    .Parameters
                    .Select(e => e.Key)
                    .GroupBy(e => e)
                    .Where(e => e.Count() > 1)
                    .Select(e => e.FirstOrDefault())
                    .ToHashSet();

                // Parameters
                foreach (var (parameter, parameterIndex) in entity.Parameters.Indexed())
                {
                    if (!TemplexVariable.IsValidVariableName(parameter.Key))
                    {
                        var path = $"[{index}].{nameof(entity.Parameters)}[{parameterIndex}].{nameof(parameter.Key)}";
                        var msg = "Invalid Key. Valid keys contain only alphanumeric characters, dollar symbols, and underscores and do not start with a number.";
                        ModelState.AddError(path, msg);
                    }
                    else if (duplicateKeys.Contains(parameter.Key))
                    {
                        var path = $"[{index}].{nameof(entity.Parameters)}[{parameterIndex}].{nameof(parameter.Key)}";
                        var msg = $"The Key '{parameter.Key}' is used more than once.";
                        ModelState.AddError(path, msg);
                    }
                }
            }

            #endregion

            SaveOutput result = await _behavior.Repository.MessageTemplates__Save(
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
            DeleteOutput result = await _behavior.Repository.MessageTemplates__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken _)
        {
            var result = ExpressionOrderBy.Parse(nameof(MessageTemplate.Code));
            return Task.FromResult(result);
        }
    }
}
