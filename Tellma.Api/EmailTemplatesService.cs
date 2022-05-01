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
    public class EmailTemplatesService : CrudServiceBase<EmailTemplateForSave, EmailTemplate, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer _localizer;
        private readonly TemplateService _templateService;

        public EmailTemplatesService(
            ApplicationFactServiceBehavior behavior,
            CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
            _templateService = deps.TemplateService;
            _localizer = deps.Localizer;
        }

        protected override string View => $"email-templates";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        // Studio Preview

        public async Task<EmailCommandPreview> EmailCommandPreviewEntities(EmailTemplate template, PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            if (template == null)
            {
                throw new ServiceException($"Email template was not supplied.");
            }

            await FillNavigationProperties(template, cancellation);

            int index = 0;

            var preloadedQuery = BaseUtil.EntitiesPreloadedQuery(args: args, collection: template.Collection, defId: template.DefinitionId);
            var localVars = BaseUtil.EntitiesLocalVariables(args: args, collection: template.Collection, defId: template.DefinitionId);

            return await _behavior.CreateEmailCommandPreview(
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVars,
                fromIndex: index,
                toIndex: index,
                cultureString: args.Culture,
                cancellation: cancellation);
        }

        public async Task<EmailPreview> EmailPreviewEntities(EmailTemplate template, int emailIndex, PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            await FillNavigationProperties(template, cancellation);

            int index = emailIndex;

            var preloadedQuery = BaseUtil.EntitiesPreloadedQuery(args: args, collection: template.Collection, defId: template.DefinitionId);
            var localVars = BaseUtil.EntitiesLocalVariables(args: args, collection: template.Collection, defId: template.DefinitionId);

            var preview = await _behavior.CreateEmailCommandPreview(
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVars,
                fromIndex: index,
                toIndex: index,
                cultureString: args.Culture,
                cancellation: cancellation);

            // Return the email and the specific index
            if (index < preview.Emails.Count)
            {
                return preview.Emails[index];
            }
            else
            {
                throw new ServiceException($"Index {index} is outside the range.");
            }
        }

        public async Task<EmailCommandPreview> EmailCommandPreviewEntity(int id, EmailTemplate template, PrintEntityByIdArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            if (template == null)
            {
                throw new ServiceException($"Email template was not supplied.");
            }

            await FillNavigationProperties(template, cancellation);

            int index = 0;

            var preloadedQuery = BaseUtil.EntityPreloadedQuery(id, args: args, collection: template.Collection, defId: template.DefinitionId);
            var localVars = BaseUtil.EntityLocalVariables(id, args: args, collection: template.Collection, defId: template.DefinitionId);

            return await _behavior.CreateEmailCommandPreview(
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVars,
                fromIndex: index,
                toIndex: index,
                cultureString: args.Culture,
                cancellation: cancellation);
        }

        public async Task<EmailPreview> EmailPreviewEntity(int id, EmailTemplate template, int emailIndex, PrintEntityByIdArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            await FillNavigationProperties(template, cancellation);

            int index = emailIndex;

            var preloadedQuery = BaseUtil.EntityPreloadedQuery(id, args: args, collection: template.Collection, defId: template.DefinitionId);
            var localVars = BaseUtil.EntityLocalVariables(id, args: args, collection: template.Collection, defId: template.DefinitionId);

            var preview = await _behavior.CreateEmailCommandPreview(
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVars,
                fromIndex: index,
                toIndex: index,
                cultureString: args.Culture,
                cancellation: cancellation);

            // Return the email and the specific index
            if (index < preview.Emails.Count)
            {
                return preview.Emails[index];
            }
            else
            {
                throw new ServiceException($"Index {index} is outside the range.");
            }
        }

        public async Task<EmailCommandPreview> EmailCommandPreview(EmailTemplate template, PrintArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            if (template == null)
            {
                throw new ServiceException($"Email template was not supplied.");
            }

            await FillNavigationProperties(template, cancellation);

            int index = 0;

            var localVariables = BaseUtil.CustomLocalVariables(args, template.Parameters?.Select(e => e.Key));

            return await _behavior.CreateEmailCommandPreview(
                template: template,
                preloadedQuery: null,
                localVariables: localVariables,
                fromIndex: index,
                toIndex: index,
                cultureString: args.Culture,
                cancellation: cancellation);
        }

        public async Task<EmailPreview> EmailPreview(EmailTemplate template, int emailIndex, PrintArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            await FillNavigationProperties(template, cancellation);

            int index = emailIndex;

            var localVariables = BaseUtil.CustomLocalVariables(args, template.Parameters?.Select(e => e.Key));

            var preview = await _behavior.CreateEmailCommandPreview(
                template: template,
                preloadedQuery: null,
                localVariables: localVariables,
                fromIndex: index,
                toIndex: index,
                cultureString: args.Culture,
                cancellation: cancellation);

            // Return the email and the specific index
            if (index < preview.Emails.Count)
            {
                return preview.Emails[index];
            }
            else
            {
                throw new ServiceException($"Index {index} is outside the range.");
            }
        }

        // Standalone

        public async Task<EmailCommandPreview> EmailCommandPreviewByTemplateId(int templateId, PrintArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _behavior.EmailCommandPreview(templateId, args, cancellation);
        }

        public async Task<EmailPreview> EmailPreviewByTemplateId(int templateId, int index, PrintArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _behavior.EmailPreview(templateId, index, args, cancellation);
        }

        public async Task<int> SendByEmail(int templateId, PrintArguments args, EmailCommandVersions versions, CancellationToken cancellation)
        {
            await Initialize(cancellation);
            return await _behavior.SendByEmail(templateId, args, versions, cancellation);
        }

        private async Task FillNavigationProperties(EmailTemplate template, CancellationToken cancellation)
        {
            // Fill the Users
            if (template?.Subscribers != null)
            {
                var userIds = template.Subscribers.Select(e => e?.UserId ?? 0).Where(e => e != 0);
                var users = await _behavior.Repository.Users
                    .FilterByIds(userIds)
                    .ToListAsync(QueryContext(), cancellation);

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

            // Fill the Attachments
            if (template?.Attachments != null)
            {
                var printingTemplateIds = template.Attachments.Select(e => e?.PrintingTemplateId ?? 0).Where(e => e != 0);
                var printingTemplates = await _behavior.Repository.PrintingTemplates
                    .Expand($"{nameof(PrintingTemplate.Parameters)}")
                    .FilterByIds(printingTemplateIds)
                    .ToListAsync(QueryContext(), cancellation);

                var printingTemplatesDic = printingTemplates.ToDictionary(e => e.Id);
                foreach (var att in template.Attachments)
                {
                    if (att?.PrintingTemplateId != null && printingTemplatesDic.TryGetValue(att.PrintingTemplateId.Value, out PrintingTemplate printingTemplate))
                    {
                        att.PrintingTemplate = printingTemplate;
                    }
                    else
                    {
                        att.PrintingTemplate = null;
                    }
                }
            }
        }

        protected override Task<EntityQuery<EmailTemplate>> Search(EntityQuery<EmailTemplate> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(EmailTemplate.Name);
                var name2 = nameof(EmailTemplate.Name2);
                var name3 = nameof(EmailTemplate.Name3);
                var code = nameof(EmailTemplate.Code);
                var desc = nameof(EmailTemplate.Description);
                var desc2 = nameof(EmailTemplate.Description2);
                var desc3 = nameof(EmailTemplate.Description3);

                var filterString = $"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}' or {desc} contains '{search}' or {desc2} contains '{search}' or {desc3} contains '{search}'";
                query = query.Filter(filterString);
            }

            return Task.FromResult(query);
        }

        protected override async Task<List<EmailTemplateForSave>> SavePreprocessAsync(List<EmailTemplateForSave> entities)
        {
            var settings = await _behavior.Settings();

            // Defaults
            entities.ForEach(entity =>
            {
                entity.Parameters ??= new List<EmailTemplateParameterForSave>();
                entity.Attachments ??= new List<EmailTemplateAttachmentForSave>();
                entity.Subscribers ??= new List<EmailTemplateSubscriberForSave>();
                entity.IsDeployed ??= false;
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
                    entity.Subscribers = new List<EmailTemplateSubscriberForSave>();
                }

                if (entity.Trigger != Triggers.Automatic)
                {
                    entity.Schedule = null;
                    entity.ConditionExpression = null;
                }

                if (entity.Trigger != Triggers.Manual)
                {
                    entity.Usage = null;
                    entity.Parameters = new List<EmailTemplateParameterForSave>();

                    // Collection, DefinitionId etc... are cleaned down below when Usage is null
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
                    entity.Parameters = new List<EmailTemplateParameterForSave>();
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

        protected override async Task<List<int>> SaveExecuteAsync(List<EmailTemplateForSave> entities, bool returnIds)
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

                    // EmailAddress
                    if (string.IsNullOrWhiteSpace(entity.EmailAddress))
                    {
                        var path = $"[{index}].{nameof(entity.EmailAddress)}";
                        var msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["NotificationTemplate_EmailAddress"]];
                        ModelState.AddError(path, msg);
                    }
                    else
                    {
                        try
                        {
                            TemplateTree.Parse(entity.EmailAddress);
                        }
                        catch
                        {
                            var path = $"[{index}].{nameof(entity.EmailAddress)}";
                            var msg = _localizer["Error_InvalidTemplate", entity.EmailAddress];
                            ModelState.AddError(path, msg);
                        }
                    }
                }

                if (entity.Trigger == Triggers.Automatic)
                {
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

                    // TODO Check that DefinitionId is compatible with Collection    
                }

                // Subject
                if (string.IsNullOrWhiteSpace(entity.Subject))
                {
                    var path = $"[{index}].{nameof(entity.Subject)}";
                    var msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["NotificationTemplate_Subject"]];
                    ModelState.AddError(path, msg);
                }
                else
                {
                    try
                    {
                        TemplateTree.Parse(entity.Subject);
                    }
                    catch
                    {
                        var path = $"[{index}].{nameof(entity.Subject)}";
                        var msg = _localizer["Error_InvalidTemplate"];
                        ModelState.AddError(path, msg);
                    }
                }

                // Body
                if (!string.IsNullOrWhiteSpace(entity.Body))
                {
                    try
                    {
                        TemplateTree.Parse(entity.Body);
                    }
                    catch
                    {
                        var path = $"[{index}].{nameof(entity.Body)}";
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

                // Attachments
                foreach (var (attachment, attachmentIndex) in entity.Attachments.Indexed())
                {
                    if (!string.IsNullOrWhiteSpace(attachment.ContextOverride))
                    {
                        try
                        {
                            TemplexBase.Parse(attachment.ContextOverride);
                        }
                        catch
                        {
                            var path = $"[{index}].{nameof(entity.Attachments)}[{attachmentIndex}].{nameof(attachment.ContextOverride)}";
                            var msg = _localizer["Error_InvalidTemplateExpression0", attachment.ContextOverride];
                            ModelState.AddError(path, msg);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(attachment.DownloadNameOverride))
                    {
                        try
                        {
                            TemplateTree.Parse(attachment.DownloadNameOverride);
                        }
                        catch
                        {
                            var path = $"[{index}].{nameof(entity.Attachments)}[{attachmentIndex}].{nameof(attachment.DownloadNameOverride)}";
                            var msg = _localizer["Error_InvalidTemplate", attachment.DownloadNameOverride];
                            ModelState.AddError(path, msg);
                        }
                    }
                }
            }

            #endregion

            SaveOutput result = await _behavior.Repository.EmailTemplates__Save(
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
            DeleteOutput result = await _behavior.Repository.EmailTemplates__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken _)
        {
            var result = ExpressionOrderBy.Parse(nameof(EmailTemplate.Code));
            return Task.FromResult(result);
        }
    }
}
