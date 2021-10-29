using Cronos;
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
using Tellma.Model.Common;
using Tellma.Repository.Common;
using Tellma.Utilities.Common;

namespace Tellma.Api
{
    public class NotificationTemplatesService : CrudServiceBase<NotificationTemplateForSave, NotificationTemplate, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer _localizer;
        private readonly TemplateService _templateService;

        public NotificationTemplatesService(
            ApplicationFactServiceBehavior behavior,
            CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
            _templateService = deps.TemplateService;
            _localizer = deps.Localizer;
        }

        protected override string View => $"notification-templates";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        public async Task<EmailCommandPreview> EmailCommandPreviewEntities(NotificationTemplate templateForSave, PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            if (templateForSave == null)
            {
                throw new ServiceException($"Email template was not supplied.");
            }

            await FillNavigationProperties(templateForSave, cancellation);
            var template = ApplicationFactServiceBehavior.MapEmailTemplate(templateForSave);

            int index = 0;

            var preview = await UnversionedEmailCommandPreview(
                template: template,
                collection: templateForSave.Collection,
                defId: templateForSave.DefinitionId,
                fromIndex: index,
                toIndex: index,
                args: args,
                cancellation: cancellation);

            // Add the versions
            preview.Version = GetEmailCommandPreviewVersion(preview);
            if (preview.Emails.Count > 0)
            {
                var email = preview.Emails[0];
                email.Version = GetEmailPreviewVersion(email);
            }

            return preview;
        }

        public async Task<EmailPreview> EmailPreviewEntities(NotificationTemplate templateForSave, int emailIndex, PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            await FillNavigationProperties(templateForSave, cancellation);
            var template = ApplicationFactServiceBehavior.MapEmailTemplate(templateForSave);

            int index = emailIndex;

            var preview = await UnversionedEmailCommandPreview(
                template: template,
                collection: templateForSave.Collection,
                defId: templateForSave.DefinitionId,
                fromIndex: index,
                toIndex: index,
                args: args,
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

        private async Task FillNavigationProperties(NotificationTemplate template, CancellationToken cancellation)
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

            // Fill the Attachments
            if (template?.Attachments != null)
            {
                var printingTemplateIds = template.Attachments.Select(e => e?.PrintingTemplateId ?? 0).Where(e => e != 0);
                var printingTemplates = await _behavior.Repository.PrintingTemplates
                    .Expand($"{nameof(PrintingTemplate.Parameters)}")
                    .FilterByIds(printingTemplateIds)
                    .ToListAsync(QueryContext, cancellation);

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

        protected override Task<EntityQuery<NotificationTemplate>> Search(EntityQuery<NotificationTemplate> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(NotificationTemplate.Name);
                var name2 = nameof(NotificationTemplate.Name2);
                var name3 = nameof(NotificationTemplate.Name3);
                var code = nameof(NotificationTemplate.Code);
                var desc = nameof(NotificationTemplate.Description);
                var desc2 = nameof(NotificationTemplate.Description2);
                var desc3 = nameof(NotificationTemplate.Description3);

                var filterString = $"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}' or {desc} contains '{search}' or {desc2} contains '{search}' or {desc3} contains '{search}'";
                query = query.Filter(filterString);
            }

            return Task.FromResult(query);
        }

        protected override async Task<List<NotificationTemplateForSave>> SavePreprocessAsync(List<NotificationTemplateForSave> entities)
        {
            var settings = await _behavior.Settings();

            // Defaults
            entities.ForEach(entity =>
            {
                entity.Parameters ??= new List<NotificationTemplateParameterForSave>();
                entity.Attachments ??= new List<NotificationTemplateAttachmentForSave>();
                entity.Subscribers ??= new List<NotificationTemplateSubscriberForSave>();

                if (!settings.SmsEnabled)
                {
                    entity.Channel = Channels.Email;
                }

                // Useless fields

                if (entity.Cardinality != Cardinalities.Bulk)
                {
                    entity.ListExpression = null;
                    entity.AddressExpression = null;
                }

                if (entity.Cardinality != Cardinality.Single)
                {
                    entity.MaximumRenotify = null;
                    entity.Subscribers = new List<NotificationTemplateSubscriberForSave>();
                }

                if (entity.Trigger != Triggers.Automatic)
                {
                    entity.Schedule = null;
                    entity.ConditionExpression = null;
                    entity.MaximumRenotify = null;
                }

                if (entity.Trigger != Triggers.Manual)
                {
                    entity.Usage = null;
                    entity.Parameters = new List<NotificationTemplateParameterForSave>();

                    // Collection, DefinitionId etc... are cleaned down below when Usage is null
                }

                if (entity.Channel != Channels.Email)
                {
                    entity.Subject = null;
                    entity.Attachments = new List<NotificationTemplateAttachmentForSave>();

                    entity.Subscribers.ForEach(e => e.Email = null);
                }

                if (entity.Channel != Channels.Sms)
                {
                    entity.Subscribers.ForEach(e => e.Phone = null);
                }

                if (entity.Usage == null)
                {
                    // Collection and DefinitionId only make sense when the usage is specified
                    entity.Collection = null;
                    entity.DefinitionId = null;
                    entity.ReportDefinitionId = null;
                }

                if (entity.Usage == TemplateUsages.FromDetails || entity.Usage == TemplateUsages.FromSearchAndDetails)
                {

                }

                if (entity.Usage == TemplateUsages.FromReport)
                {
                    entity.Collection = null;
                    entity.DefinitionId = null;
                }
                else
                {
                    entity.ReportDefinitionId = null;
                }

                // Defaults

                entity.IsDeployed ??= false;
                entity.Parameters.ForEach(p =>
                {
                    p.IsRequired ??= false;
                    p.ControlOptions = ApplicationUtil.PreprocessControlOptions(p.Control, p.ControlOptions, settings);
                });
            });

            return entities;
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<NotificationTemplateForSave> entities, bool returnIds)
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
                if (entity.Cardinality == Cardinalities.Bulk)
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

                    // AddressExpression
                    if (string.IsNullOrWhiteSpace(entity.AddressExpression))
                    {
                        var path = $"[{index}].{nameof(entity.AddressExpression)}";
                        var msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["NotificationTemplate_AddressExpression"]];
                        ModelState.AddError(path, msg);
                    }
                    else
                    {
                        try
                        {
                            TemplexBase.Parse(entity.AddressExpression);
                        }
                        catch
                        {
                            var path = $"[{index}].{nameof(entity.AddressExpression)}";
                            var msg = _localizer["Error_InvalidTemplateExpression0", entity.AddressExpression];
                            ModelState.AddError(path, msg);
                        }
                    }
                }

                if (entity.Trigger == Triggers.Automatic)
                {
                    // Schedule
                    if (string.IsNullOrWhiteSpace(entity.Schedule))
                    {
                        var path = $"[{index}].{nameof(entity.AddressExpression)}";
                        var msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["NotificationTemplate_AddressExpression"]];
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
                    else if (entity.Usage == TemplateUsages.FromReport)
                    {
                        if (entity.ReportDefinitionId == null)
                        {
                            var path = $"[{index}].{nameof(entity.ReportDefinitionId)}";
                            var msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Template_ReportDefinitionId"]];
                            ModelState.AddError(path, msg);
                        }
                    }

                    // TODO Check that DefinitionId is compatible with Collection    
                }

                if (entity.Channel == Channels.Email)
                {
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
                }

                // Body
                if (!string.IsNullOrWhiteSpace(entity.Body))
                {
                    if (entity.Channel == Channels.Sms)
                    {
                        const int maxSmsExpressionLength = 1024;
                        if (entity.Body.Length > maxSmsExpressionLength)
                        {
                            var path = $"[{index}].{nameof(entity.Body)}";
                            var msg = _localizer[ErrorMessages.Error_Field0LengthMaximumOf1, _localizer["Template_Body"], maxSmsExpressionLength];
                            ModelState.AddError(path, msg);
                        }
                    }

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
                            var msg = _localizer["Error_InvalidTemplate0", attachment.DownloadNameOverride];
                            ModelState.AddError(path, msg);
                        }
                    }
                }

                // Subscribers
                foreach (var (subscriber, subscriberIndex) in entity.Subscribers.Indexed())
                {
                    if (subscriber.AddressType == AddressTypes.User)
                    {
                        if (subscriber.UserId == null)
                        {
                            var path = $"[{index}].{nameof(entity.Subscribers)}[{subscriberIndex}].{nameof(subscriber.UserId)}";
                            var msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["NotificationTemplate_User"]];
                            ModelState.AddError(path, msg);
                        }
                    }

                    if (subscriber.AddressType == AddressTypes.Text)
                    {
                        if (entity.Channel == Channels.Email)
                        {
                            if (string.IsNullOrWhiteSpace(subscriber.Email))
                            {
                                var path = $"[{index}].{nameof(entity.Subscribers)}[{subscriberIndex}].{nameof(subscriber.Email)}";
                                var msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["NotificationTemplate_Email"]];
                                ModelState.AddError(path, msg);
                            }
                            else
                            {
                                try
                                {
                                    TemplateTree.Parse(subscriber.Email);
                                }
                                catch
                                {
                                    var path = $"[{index}].{nameof(entity.Subscribers)}[{subscriberIndex}].{nameof(subscriber.Email)}";
                                    var msg = _localizer["Error_InvalidTemplate"];
                                    ModelState.AddError(path, msg);
                                }
                            }
                        }

                        if (entity.Channel == Channels.Sms)
                        {
                            if (string.IsNullOrWhiteSpace(subscriber.Phone))
                            {
                                var path = $"[{index}].{nameof(entity.Subscribers)}[{subscriberIndex}].{nameof(subscriber.Phone)}";
                                var msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["NotificationTemplate_Phone"]];
                                ModelState.AddError(path, msg);
                            }
                            else
                            {
                                try
                                {
                                    TemplateTree.Parse(subscriber.Phone);
                                }
                                catch
                                {
                                    var path = $"[{index}].{nameof(entity.Subscribers)}[{subscriberIndex}].{nameof(subscriber.Phone)}";
                                    var msg = _localizer["Error_InvalidTemplate"];
                                    ModelState.AddError(path, msg);
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            SaveOutput result = await _behavior.Repository.NotificationTemplates__Save(
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
            DeleteOutput result = await _behavior.Repository.NotificationTemplates__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken _)
        {
            var result = ExpressionOrderBy.Parse(nameof(NotificationTemplate.Name));
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
