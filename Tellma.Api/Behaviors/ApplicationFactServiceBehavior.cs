using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Dto;
using Tellma.Api.Metadata;
using Tellma.Api.Notifications;
using Tellma.Api.Templating;
using Tellma.Model.Application;
using Tellma.Model.Common;
using Tellma.Repository.Admin;
using Tellma.Repository.Application;
using Tellma.Repository.Common;
using Tellma.Utilities.Common;
using Tellma.Utilities.Email;
using Tellma.Utilities.Sms;

namespace Tellma.Api.Behaviors
{
    public class ApplicationFactServiceBehavior : ApplicationServiceBehavior, IFactServiceBehavior
    {
        private static readonly ConcurrentDictionary<int, ApplicationMetadataOverridesProvider> _overridesCache = new();

        private readonly IDefinitionsCache _definitionsCache;
        private readonly ISettingsCache _settingsCache;
        private readonly IPermissionsCache _permissions;
        private readonly IUserSettingsCache _userSettingsCache;
        private readonly TemplateService _templateService;
        private readonly NotificationsQueue _notificationsQueue;
        private readonly ApplicationBehaviorHelper _behaviorHelper;
        private readonly IStringLocalizer<Strings> _localizer;

        protected int? DefinitionId { get; private set; }

        public ApplicationFactServiceBehavior(
            IApplicationRepositoryFactory factory,
            ApplicationVersions versions,
            AdminRepository adminRepo,
            ILogger<ApplicationServiceBehavior> logger,
            IDefinitionsCache definitionsCache,
            ISettingsCache settingsCache,
            IPermissionsCache permissions,
            IUserSettingsCache userSettingsCache,
            TemplateService templateService,
            NotificationsQueue notificationsQueue,
            ApplicationBehaviorHelper behaviorHelper,
            IStringLocalizer<Strings> localizer) : base(factory, versions, adminRepo, logger)
        {
            _definitionsCache = definitionsCache;
            _settingsCache = settingsCache;
            _permissions = permissions;
            _userSettingsCache = userSettingsCache;
            _templateService = templateService;
            _notificationsQueue = notificationsQueue;
            _behaviorHelper = behaviorHelper;
            _localizer = localizer;
        }

        public IQueryFactory QueryFactory<TEntity>() where TEntity : Entity
        {
            if (DefinitionId != null)
            {
                return new FilteredQueryFactory<TEntity>(
                    Repository, $"DefinitionId eq {DefinitionId}");
            }
            else
            {
                return Repository;
            }
        }

        public async Task<SettingsForClient> Settings(CancellationToken cancellation = default)
        {
            return (await _settingsCache.GetSettings(TenantId, SettingsVersion, cancellation)).Data;
        }

        public async Task<DefinitionsForClient> Definitions(CancellationToken cancellation = default)
        {
            return (await _definitionsCache.GetDefinitions(TenantId, DefinitionsVersion, cancellation)).Data;
        }

        public async Task<UserSettingsForClient> UserSettings(CancellationToken cancellation = default)
        {
            return (await _userSettingsCache.GetUserSettings(UserId, TenantId, UserSettingsVersion, cancellation)).Data;
        }

        public async Task<IMetadataOverridesProvider> GetMetadataOverridesProvider(CancellationToken cancellation)
        {
            var settings = await Settings(cancellation);
            var definitions = await Definitions(cancellation);

            var provider = _overridesCache.GetOrAdd(TenantId,
                _ => new ApplicationMetadataOverridesProvider(_localizer, definitions, settings));

            if (provider.Definitions != definitions || provider.Settings != settings)
            {
                _overridesCache.TryRemove(TenantId, out _);
                return await GetMetadataOverridesProvider(cancellation);
            }

            return provider;
        }

        public async Task<AbstractPrintingTemplate> GetPrintingTemplate(int templateId, CancellationToken cancellation)
        {
            var template = await Repository.EntityQuery<PrintingTemplate>()
                .Expand(nameof(PrintingTemplate.Parameters))
                .FilterByIds(new int[] { templateId })
                .FirstOrDefaultAsync(new QueryContext(UserId), cancellation);

            if (template == null)
            {
                return null;
            }

            var parameters = template.Parameters.Select(e => new AbstractParameter(e.Key, e.Control));
            return new AbstractPrintingTemplate(template.Body, template.DownloadName, template.Context, parameters);
        }

        public async Task SetPrintingVariables(Dictionary<string, EvaluationVariable> localVars, Dictionary<string, EvaluationVariable> globalVars, CancellationToken cancellation)
        {
            await _behaviorHelper.SetPrintingVariables(UserId, TenantId, UserSettingsVersion, SettingsVersion, localVars, globalVars, cancellation);
        }

        public async Task SetPrintingFunctions(Dictionary<string, EvaluationFunction> localFuncs, Dictionary<string, EvaluationFunction> globalFuncs, CancellationToken cancellation)
        {
            await _behaviorHelper.SetPrintingFunctions(TenantId, SettingsVersion, localFuncs, globalFuncs, cancellation);
        }

        public void SetDefinitionId(int definitionId) => DefinitionId = definitionId;

        public async Task<IEnumerable<AbstractPermission>> UserPermissions(string view, string action, CancellationToken cancellation)
        {
            // IF anonymous return READ-ALL
            if (IsAnonymous)
            {
                return new List<AbstractPermission> { new AbstractPermission { View = "all", Action = PermissionActions.Read } };
            }
            else
            {
                return await _permissions.PermissionsFromCache(
                    tenantId: TenantId,
                    userId: UserId,
                    version: PermissionsVersion,
                    view: view,
                    action: action,
                    cancellation: cancellation);
            }
        }

        #region Email

        /// <summary>
        /// Returns a template-generated text file that is evaluated based on the given <paramref name="templateId"/>.
        /// The text generation will implicitly contain a variable $ that evaluates to the results of the query specified in <paramref name="args"/>.
        /// </summary>
        public async Task<EmailCommandPreview> EmailCommandPreviewEntities<TEntity>(int templateId, PrintEntitiesArguments<int> args, CancellationToken cancellation)
            where TEntity : EntityWithKey<int>
        {
            // Load the email previews, with the first email pre-loaded
            var template = await GetEmailTemplate(templateId, cancellation);
            int index = 0;

            var preloadedQuery = BaseUtil.EntitiesPreloadedQuery(args: args, collection: typeof(TEntity).Name, defId: DefinitionId);
            var localVars = BaseUtil.EntitiesLocalVariables(args: args, collection: typeof(TEntity).Name, defId: DefinitionId);

            var preview = await UnversionedEmailCommandPreview(
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVars,
                fromIndex: index,
                toIndex: index,
                cultureString: args.Culture,
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

        public async Task<EmailPreview> EmailPreviewEntities<TEntity>(int templateId, int emailIndex, PrintEntitiesArguments<int> args, CancellationToken cancellation)
            where TEntity : EntityWithKey<int>
        {
            var template = await GetEmailTemplate(templateId, cancellation);
            int index = emailIndex;

            var preloadedQuery = BaseUtil.EntitiesPreloadedQuery(args: args, collection: typeof(TEntity).Name, defId: DefinitionId);
            var localVars = BaseUtil.EntitiesLocalVariables(args: args, collection: typeof(TEntity).Name, defId: DefinitionId);

            var preview = await UnversionedEmailCommandPreview(
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVars,
                fromIndex: index,
                toIndex: index,
                cultureString: args.Culture,
                cancellation: cancellation);

            var clientVersion = args?.Version;
            if (!string.IsNullOrWhiteSpace(clientVersion))
            {
                var serverVersion = GetEmailCommandPreviewVersion(preview);
                if (serverVersion != clientVersion)
                {
                    throw new ServiceException($"The underlying data has changed, please refresh and try again.");
                }
            }

            // Return the email and the specific index
            if (index < preview.Emails.Count)
            {
                var email = preview.Emails[index];
                email.Version = GetEmailPreviewVersion(email);

                return email;
            }
            else
            {
                throw new ServiceException($"Index {index} is outside the range.");
            }
        }

        public async Task SendByEmail<TEntity>(int templateId, PrintEntitiesArguments<int> args, EmailCommandVersions versions, CancellationToken cancellation)
            where TEntity : EntityWithKey<int>
        {
            // (1) Check Permissions
            await CheckEmailPermissions(templateId, cancellation);

            // (2) Generate Emails
            var template = await GetEmailTemplate(templateId, cancellation);
            int fromIndex = 0;
            int toIndex = int.MaxValue;

            var preloadedQuery = BaseUtil.EntitiesPreloadedQuery(args: args, collection: typeof(TEntity).Name, defId: DefinitionId);
            var localVars = BaseUtil.EntitiesLocalVariables(args: args, collection: typeof(TEntity).Name, defId: DefinitionId);

            var preview = await UnversionedEmailCommandPreview(
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVars,
                fromIndex: fromIndex,
                toIndex: toIndex,
                cultureString: args.Culture,
                cancellation: cancellation);

            if (!MatchVersions(preview, versions, fromIndex, toIndex))
            {
                // This provides a mechanism to ensure that what the user previews is exactly what the system will send
                throw new ServiceException($"The underlying data has changed, please refresh and try again.");
            }

            var emailsToSend = preview.Emails.Select(email => new EmailToSend
            {
                To = email.To,
                Subject = email.Subject,
                Body = email.Body,
                Attachments = email.Attachments.Select(e => new EmailAttachmentToSend
                {
                    Name = e.DownloadName,
                    Contents = Encoding.UTF8.GetBytes(e.Body)
                })
            }).ToList();

            var command = new NotificationCommandToSend(templateId)
            {
                Caption = preview.Caption,
                CreatedById = UserId,
            };

            // (3) Send Emails
            await _notificationsQueue.Enqueue(TenantId, emails: emailsToSend, command: command, cancellation: cancellation);
        }

        public async Task<EmailCommandPreview> EmailCommandPreviewEntity<TEntity>(int id, int templateId, PrintEntityByIdArguments args, CancellationToken cancellation)
            where TEntity : EntityWithKey<int>
        {
            // Load the email previews, with the first email pre-loaded
            var template = await GetEmailTemplate(templateId, cancellation);
            int index = 0;

            var preloadedQuery = BaseUtil.EntityPreloadedQuery(id, args: args, collection: typeof(TEntity).Name, defId: DefinitionId);
            var localVars = BaseUtil.EntityLocalVariables(id, args: args, collection: typeof(TEntity).Name, defId: DefinitionId);

            var preview = await UnversionedEmailCommandPreview(
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVars,
                fromIndex: index,
                toIndex: index,
                cultureString: args.Culture,
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

        public async Task<EmailPreview> EmailPreviewEntity<TEntity>(int id, int templateId, int emailIndex, PrintEntityByIdArguments args, CancellationToken cancellation)
            where TEntity : EntityWithKey<int>
        {
            var template = await GetEmailTemplate(templateId, cancellation);
            int index = emailIndex;

            var preloadedQuery = BaseUtil.EntityPreloadedQuery(id, args: args, collection: typeof(TEntity).Name, defId: DefinitionId);
            var localVars = BaseUtil.EntityLocalVariables(id, args: args, collection: typeof(TEntity).Name, defId: DefinitionId);

            var preview = await UnversionedEmailCommandPreview(
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVars,
                fromIndex: index,
                toIndex: index,
                cultureString: args.Culture,
                cancellation: cancellation);

            var clientVersion = args?.Version;
            if (!string.IsNullOrWhiteSpace(clientVersion))
            {
                var serverVersion = GetEmailCommandPreviewVersion(preview);
                if (serverVersion != clientVersion)
                {
                    throw new ServiceException($"The underlying data has changed, please refresh and try again.");
                }
            }

            // Return the email and the specific index
            if (index < preview.Emails.Count)
            {
                var email = preview.Emails[index];
                email.Version = GetEmailPreviewVersion(email);

                return email;
            }
            else
            {
                throw new ServiceException($"Index {index} is outside the range.");
            }
        }

        public async Task SendByEmail<TEntity>(int id, int templateId, PrintEntityByIdArguments args, EmailCommandVersions versions, CancellationToken cancellation)
            where TEntity : EntityWithKey<int>
        {
            // (1) Check Permissions
            await CheckEmailPermissions(templateId, cancellation);

            // (2) Prepare the Email
            var template = await GetEmailTemplate(templateId, cancellation);
            int fromIndex = 0;
            int toIndex = int.MaxValue;

            var preloadedQuery = BaseUtil.EntityPreloadedQuery(id, args: args, collection: typeof(TEntity).Name, defId: DefinitionId);
            var localVars = BaseUtil.EntityLocalVariables(id, args: args, collection: typeof(TEntity).Name, defId: DefinitionId);

            var preview = await UnversionedEmailCommandPreview(
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVars,
                fromIndex: fromIndex,
                toIndex: toIndex,
                cultureString: args.Culture,
                cancellation: cancellation);

            if (!MatchVersions(preview, versions, fromIndex, toIndex))
            {
                throw new ServiceException($"The underlying data has changed, please refresh and try again.");
            }

            var emailsToSend = preview.Emails.Select(email => new EmailToSend
            {
                To = email.To,
                Subject = email.Subject,
                Body = email.Body,
                Attachments = email.Attachments.Select(e => new EmailAttachmentToSend
                {
                    Name = e.DownloadName,
                    Contents = Encoding.UTF8.GetBytes(e.Body)
                })
            }).ToList();

            var command = new NotificationCommandToSend(templateId)
            {
                EntityId = id,
                Caption = preview.Caption,
                CreatedById = UserId,
            };

            // (3) Send the Email
            await _notificationsQueue.Enqueue(TenantId, emails: emailsToSend, command: command, cancellation: cancellation);
        }

        /// <summary>
        /// Retrieves the <see cref="NotificationTemplate"/> from the database.
        /// </summary>
        /// <param name="templateId">The Id of the template to retrieve.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>The <see cref="NotificationTemplate"/> with a matching Id or null if none is found.</returns>
        public async Task<NotificationTemplate> GetEmailTemplate(int templateId, CancellationToken cancellation)
        {
            return await Repository.EntityQuery<NotificationTemplate>()
                .Expand($"{nameof(NotificationTemplate.Parameters)},{nameof(NotificationTemplate.Subscribers)}.{nameof(NotificationTemplateSubscriber.User)},{nameof(NotificationTemplate.Attachments)}.{nameof(NotificationTemplateAttachment.PrintingTemplate)}.{nameof(PrintingTemplate.Parameters)}")
                .Filter($"{nameof(NotificationTemplate.Channel)} eq '{Channels.Email}'")
                .FilterByIds(new int[] { templateId })
                .FirstOrDefaultAsync(new QueryContext(UserId), cancellation);
        }

        protected static bool MatchVersions(EmailCommandPreview preview, EmailCommandVersions clientVersions, int fromIndex, int toIndex)
        {
            if (clientVersions == null)
            {
                // Client did not supply versions
                return true;
            }

            // Overall preview version
            {
                var serverVersion = GetEmailCommandPreviewVersion(preview);
                var clientVersion = clientVersions.Version;
                if (!string.IsNullOrWhiteSpace(clientVersion) && serverVersion != clientVersion)
                {
                    return false;
                }
            }

            // Individual email versions
            if (clientVersions.Emails != null)
            {
                foreach (var clientEmailVersion in clientVersions.Emails.Where(e => !string.IsNullOrWhiteSpace(e.Version)))
                {
                    if (clientEmailVersion == null)
                    {
                        // Client did not supply a version
                        continue;
                    }

                    var clientVersion = clientEmailVersion.Version;
                    if (!string.IsNullOrWhiteSpace(clientVersion))
                    {
                        // Client did not supply a version
                        continue;
                    }

                    var clientIndex = clientEmailVersion.Index;
                    if (clientIndex < 0 || clientIndex >= preview.Emails.Count)
                    {
                        // Client supplied indices outside the range
                        return false;
                    }

                    if (fromIndex <= clientIndex && toIndex >= clientIndex) // Those are the loaded emails
                    {
                        // Check the version of the email
                        var emailPreview = preview.Emails[clientIndex];
                        var serverVersion = GetEmailPreviewVersion(emailPreview);
                        if (serverVersion != clientVersion)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public async Task<EmailCommandPreview> UnversionedEmailCommandPreview(
            NotificationTemplate template,
            QueryInfo preloadedQuery,
            Dictionary<string, EvaluationVariable> localVariables,
            int fromIndex,
            int toIndex,
            string cultureString,
            CancellationToken cancellation)
        {
            // (1) Email address templates
            var addressTemplates = new List<string>();
            if (template.Cardinality == Cardinalities.Single)
            {
                foreach (var sub in template.Subscribers)
                {
                    if (sub.AddressType == AddressTypes.User && !string.IsNullOrWhiteSpace(sub.User?.ContactEmail))
                    {
                        addressTemplates.Add(sub.User?.ContactEmail); // Static
                    }

                    if (sub.AddressType == AddressTypes.Text && !string.IsNullOrWhiteSpace(sub.Email))
                    {
                        addressTemplates.Add(sub.Email); // Template
                    }
                }
            }

            if (template.Cardinality == Cardinalities.Bulk && !string.IsNullOrWhiteSpace(template.AddressExpression))
            {
                addressTemplates.Add($"{{{{ {template.AddressExpression} }}}}"); // Expression
            }

            // (2) Attachment Templates
            var attachmentTemplates = template.Attachments.Select(e =>
            {
                var pt = e.PrintingTemplate;

                // Body
                var body = pt.Body;

                // Context
                var context = e.ContextOverride;
                if (string.IsNullOrWhiteSpace(context))
                {
                    context = pt.Context;
                }

                // DownloadName
                var downloadName = e.DownloadNameOverride;
                if (string.IsNullOrWhiteSpace(downloadName))
                {
                    downloadName = pt.DownloadName;
                }

                // Parameters
                var parameters = pt.Parameters.Select(e => new AbstractParameter(e.Key, e.Control));

                // Result
                return new AbstractPrintingTemplate(body, downloadName, context, parameters);
            });

            // (3) Functions + Variables
            var globalFunctions = new Dictionary<string, EvaluationFunction>();
            var localFunctions = new Dictionary<string, EvaluationFunction>();
            var globalVariables = new Dictionary<string, EvaluationVariable>();
            localVariables ??= new Dictionary<string, EvaluationVariable>();

            await SetPrintingFunctions(localFunctions, globalFunctions, cancellation);
            await SetPrintingVariables(localVariables, globalVariables, cancellation);

            // (4) Culture
            CultureInfo culture = BaseUtil.GetCulture(cultureString);

            // (5) The Template Plan
            // Body, subject and address(es)
            var captionP = new TemplatePlanLeaf(template.Caption);
            var bodyP = new TemplatePlanLeaf(template.Body, TemplateLanguage.Html);
            var subjectP = new TemplatePlanLeaf(template.Subject);
            var addresses = addressTemplates.Select(a => new TemplatePlanLeaf(a)).ToList();

            // Attachments
            var attachmentInfos = new List<(TemplatePlanLeaf body, TemplatePlanLeaf name)>();
            var attachments = new List<TemplatePlan>();
            foreach (var e in attachmentTemplates)
            {
                var attachmentBodyP = new TemplatePlanLeaf(e.Body, TemplateLanguage.Html);
                var attachmentNameP = new TemplatePlanLeaf(e.DownloadName);
                var attachmentBodyAndName = new List<TemplatePlan> { attachmentBodyP, attachmentNameP };

                TemplatePlan attachmentP = new TemplatePlanTuple(attachmentBodyAndName);
                if (!string.IsNullOrWhiteSpace(e.Context))
                {
                    attachmentP = new TemplatePlanDefine("$", e.Context, attachmentP);
                }

                attachmentInfos.Add((attachmentBodyP, attachmentNameP));
                attachments.Add(attachmentP);
            }

            // Put everything together
            var bodyAndAttachmentPlans = new List<TemplatePlan> { bodyP };
            bodyAndAttachmentPlans.AddRange(attachments);

            var subjectAndRecipientsPlans = new List<TemplatePlan> { subjectP };
            subjectAndRecipientsPlans.AddRange(addresses);

            TemplatePlan emailP;
            if (string.IsNullOrWhiteSpace(template.ListExpression))
            {
                var allPlans = subjectAndRecipientsPlans.Concat(bodyAndAttachmentPlans).Concat(new TemplatePlan[] { captionP });
                emailP = new TemplatePlanTuple(allPlans);
            }
            else
            {
                var subjectAndRecipientsP = new TemplatePlanTuple(subjectAndRecipientsPlans);
                var bodyAndAttachmentsP = new TemplatePlanTuple(bodyAndAttachmentPlans);

                // Wrap both Foreach and captionP inside a Define
                emailP = new TemplatePlanDefine("$", template.ListExpression,
                    new TemplatePlanTuple(
                        captionP,
                        new TemplatePlanRangeForeach(
                            iteratorVarName: "$",
                            listExpression: "$", // The list expression
                            everyRow: subjectAndRecipientsP,
                            rangeOnly: bodyAndAttachmentsP,
                            from: fromIndex,
                            to: toIndex)
                        )
                    );
            }

            // Caption

            // Context variable $
            if (preloadedQuery != null)
            {
                emailP = new TemplatePlanDefineQuery("$", preloadedQuery, emailP);
            }

            var genArgs = new TemplateArguments(globalFunctions, globalVariables, localFunctions, localVariables, culture);
            await _templateService.GenerateFromPlan(plan: emailP, args: genArgs, cancellation: cancellation);

            var emails = new List<EmailPreview>();
            for (int i = 0; i < subjectP.Outputs.Count; i++)
            {
                var email = new EmailPreview
                {
                    To = GetEmailAddresses(addresses, i),
                    Subject = subjectP.Outputs[i],
                    Attachments = new()
                };

                if (fromIndex <= i && toIndex >= i) // Within range
                {
                    int rangeIndex = i - fromIndex;
                    email.Body = bodyP.Outputs[rangeIndex];

                    int n = 1;
                    foreach (var (emailAttachmentBody, emailAttachmentName) in attachmentInfos)
                    {
                        var attBody = emailAttachmentBody.Outputs[rangeIndex];
                        var attName = emailAttachmentName.Outputs[rangeIndex];

                        // Handle null name
                        if (string.IsNullOrWhiteSpace(attName))
                        {
                            attName = $"Attachment_{n}.html";
                        }
                        n++;

                        const string extension = ".html";
                        if (!attName.ToLower().EndsWith(extension))
                        {
                            attName += extension;
                        }

                        email.Attachments.Add(new AttachmentPreview
                        {
                            DownloadName = attName,
                            Body = attBody
                        });
                    }
                }

                emails.Add(email);
            }

            return new EmailCommandPreview
            {
                Caption = captionP.Outputs[0],
                Emails = emails,
            };
        }

        private async Task CheckEmailPermissions(int templateId, CancellationToken cancellation)
        {
            var permissions = await _permissions.PermissionsFromCache(TenantId, UserId, PermissionsVersion, $"notification-commands/{templateId}", "Send", cancellation);
            if (!permissions.Any())
            {
                // Not even authorized to send
                throw new ForbiddenException();
            }
        }

        private static List<string> GetEmailAddresses(List<TemplatePlanLeaf> plans, int index)
        {
            return plans.Select(e => e.Outputs[index])
                .Where(e => e != null)
                .SelectMany(e => e.Split(';'))
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .NullIfEmpty();
        }

        public static string GetEmailCommandPreviewVersion(EmailCommandPreview preview)
            => KnuthHash(preview.Emails.SelectMany(email => StringsInPreviewEmail(email)));

        public static string GetEmailPreviewVersion(EmailPreview email)
            => KnuthHash(StringsInEmail(email));

        private static IEnumerable<string> StringsInPreviewEmail(EmailPreview email)
        {
            if (email.To != null)
            {
                foreach (var address in email.To)
                {
                    yield return address;
                }
            }
            if (email.Cc != null)
            {
                foreach (var address in email.Cc)
                {
                    yield return address;
                }
            }
            if (email.Bcc != null)
            {
                foreach (var address in email.Bcc)
                {
                    yield return address;
                }
            }

            yield return email.Subject;
        }

        private static IEnumerable<string> StringsInEmail(EmailPreview email)
        {
            foreach (var str in StringsInPreviewEmail(email))
            {
                yield return str;
            }

            yield return email.Body;

            if (email.Attachments != null)
            {
                foreach (var att in email.Attachments)
                {
                    yield return att.DownloadName;
                    yield return att.Body;
                }
            }
        }

        private static string KnuthHash(IEnumerable<string> values)
        {
            ulong hash = 3074457345618258791ul;
            foreach (var value in values)
            {
                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        hash += value[i];
                        hash *= 3074457345618258799ul;
                    }
                }
            }

            return hash.ToString();
        }

        public class TemplatePlanRangeForeach : TemplatePlan
        {
            private TemplexBase _listCandidate;

            public TemplatePlanRangeForeach(
                string iteratorVarName,
                string listExpression,
                TemplatePlan everyRow,
                TemplatePlan rangeOnly,
                int from, int to)
            {
                if (string.IsNullOrWhiteSpace(listExpression))
                {
                    throw new ArgumentException($"'{nameof(listExpression)}' cannot be null or whitespace.", nameof(listExpression));
                }

                IteratorVariableName = iteratorVarName ?? "$";
                ListExpression = listExpression;
                Always = everyRow ?? throw new ArgumentNullException(nameof(everyRow));
                RangeOnly = rangeOnly ?? throw new ArgumentNullException(nameof(rangeOnly));
                FromIndex = from;
                ToIndex = to;
            }

            public TemplatePlan Always { get; }
            public TemplatePlan RangeOnly { get; }
            public int FromIndex { get; }
            public int ToIndex { get; }
            public string IteratorVariableName { get; }
            public string ListExpression { get; }

            public override async IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
            {
                _listCandidate ??= TemplexBase.Parse(ListExpression);

                if (_listCandidate != null)
                {
                    // Expression select
                    var select = _listCandidate.ComputeSelect(ctx);
                    await foreach (var atom in select)
                    {
                        yield return atom;
                    }

                    // Expression paths
                    var paths = _listCandidate.ComputePaths(ctx);
                    await foreach (var path in paths)
                    {
                        yield return path.Append("Id");
                    }

                    // Inner template select
                    var scopedCtx = ctx.Clone();
                    scopedCtx.SetLocalVariable(IteratorVariableName, new EvaluationVariable(
                                    eval: TemplateUtil.VariableThatThrows(IteratorVariableName),
                                    selectResolver: () => select,
                                    pathsResolver: () => paths
                                    ));

                    await foreach (var atom in Always.ComputeSelect(scopedCtx))
                    {
                        yield return atom;
                    }

                    if (FromIndex <= ToIndex && FromIndex >= 0)
                    {
                        await foreach (var atom in RangeOnly.ComputeSelect(scopedCtx))
                        {
                            yield return atom;
                        }
                    }
                }
            }

            public override async Task GenerateOutputs(EvaluationContext ctx)
            {
                _listCandidate ??= TemplexBase.Parse(ListExpression);

                if (_listCandidate != null)
                {
                    var listObj = (await _listCandidate.Evaluate(ctx)) ?? new List<object>();
                    if (listObj is IList list)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            var listItem = list[i];

                            // Initialize new evaluation context with the new variable in it
                            var scopedCtx = ctx.Clone();
                            scopedCtx.SetLocalVariable(IteratorVariableName, new EvaluationVariable(
                                    evalAsync: () => Task.FromResult(listItem),
                                    selectResolver: () => AsyncUtil.Empty<Path>(), // It doesn't matter when generating output
                                    pathsResolver: () => AsyncUtil.Empty<Path>() // It doesn't matter when generating output
                                    ));

                            // Run the template again on that context
                            await Always.GenerateOutputs(scopedCtx);

                            if (FromIndex <= i && ToIndex >= i)
                            {
                                await RangeOnly.GenerateOutputs(scopedCtx);
                            }
                        }
                    }
                    else
                    {
                        throw new TemplateException($"Expression does not evaluate to a list ({_listCandidate}).");
                    }
                }
            }
        }

        #endregion

        #region Message

        /// <summary>
        /// Returns a template-generated text file that is evaluated based on the given <paramref name="templateId"/>.
        /// </summary>
        public async Task<MessageCommandPreview> MessageCommandPreview(int templateId, PrintArguments args, CancellationToken cancellation)
        {
            // Load the Message previews, with the first Message pre-loaded
            var template = await GetMessageTemplate(templateId, cancellation);
            var localVariables = BaseUtil.CustomLocalVariables(args, template.Parameters?.Select(e => e.Key));

            return await CreateMessageCommandPreview(
                template: template,
                preloadedQuery: null,
                localVariables: localVariables,
                cultureString: args.Culture,
                cancellation: cancellation);
        }

        public async Task<int> SendByMessage(int templateId, PrintArguments args, string version, CancellationToken cancellation)
        {
            // (1) Check Permissions
            await CheckMessagePermissions(templateId, cancellation);

            // (2) Prepare the Message
            var template = await GetMessageTemplate(templateId, cancellation);
            var localVariables = BaseUtil.CustomLocalVariables(args, template.Parameters?.Select(e => e.Key));

            var preview = await CreateMessageCommandPreview(
                template: template,
                preloadedQuery: null,
                localVariables: localVariables,
                cultureString: args.Culture,
                cancellation: cancellation);

            // This provides a mechanism to ensure that what the user previews is exactly what the system will send
            if (!string.IsNullOrWhiteSpace(version) && preview.Version != version)
            {
                throw new ServiceException($"The underlying data has changed, please refresh and try again.");
            }

            // Prepare the messages
            var messagesToSend = preview.Messages.Select(msg => new SmsToSend(
                phoneNumber: msg.PhoneNumber,
                content: msg.Content)
            ).ToList();

            var command = new NotificationCommandToSend(templateId)
            {
                Caption = preview.Caption,
                CreatedById = UserId,
            };

            // (3) Send Messages
            await _notificationsQueue.Enqueue(TenantId, smsMessages: messagesToSend, command: command, cancellation: cancellation);

            return command.MessageCommandId;
        }

        /// <summary>
        /// Returns a template-generated text file that is evaluated based on the given <paramref name="templateId"/>.
        /// The text generation will implicitly contain a variable $ that evaluates to the results of the query specified in <paramref name="args"/>.
        /// </summary>
        public async Task<MessageCommandPreview> MessageCommandPreviewEntities<TEntity>(int templateId, PrintEntitiesArguments<int> args, CancellationToken cancellation)
            where TEntity : EntityWithKey<int>
        {
            // Load the Message previews, with the first Message pre-loaded
            var template = await GetMessageTemplate(templateId, cancellation);

            var preloadedQuery = BaseUtil.EntitiesPreloadedQuery(args: args, collection: typeof(TEntity).Name, defId: DefinitionId);
            var localVars = BaseUtil.EntitiesLocalVariables(args: args, collection: typeof(TEntity).Name, defId: DefinitionId);

            return await CreateMessageCommandPreview(
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVars,
                cultureString: args.Culture,
                cancellation: cancellation);
        }

        public async Task<int> SendByMessage<TEntity>(int templateId, PrintEntitiesArguments<int> args, string version, CancellationToken cancellation)
            where TEntity : EntityWithKey<int>
        {
            // (1) Check Permissions
            await CheckMessagePermissions(templateId, cancellation);

            // (2) Generate Messages
            var template = await GetMessageTemplate(templateId, cancellation);

            var preloadedQuery = BaseUtil.EntitiesPreloadedQuery(args: args, collection: typeof(TEntity).Name, defId: DefinitionId);
            var localVars = BaseUtil.EntitiesLocalVariables(args: args, collection: typeof(TEntity).Name, defId: DefinitionId);

            var preview = await CreateMessageCommandPreview(
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVars,
                cultureString: args.Culture,
                cancellation: cancellation);

            // This provides a mechanism to ensure that what the user previews is exactly what the system will send
            if (!string.IsNullOrWhiteSpace(version) && preview.Version != version)
            {
                throw new ServiceException($"The underlying data has changed, please refresh and try again.");
            }

            // Prepare the messages
            var messagesToSend = preview.Messages.Select(msg => new SmsToSend(
                phoneNumber: msg.PhoneNumber,
                content: msg.Content)
            ).ToList();

            // Prepare the command
            var command = new NotificationCommandToSend(templateId)
            {
                Caption = preview.Caption,
                CreatedById = UserId,
            };

            // (3) Send Messages
            await _notificationsQueue.Enqueue(TenantId, smsMessages: messagesToSend, command: command, cancellation: cancellation);

            return command.MessageCommandId;
        }

        /// <summary>
        /// Returns a template-generated text file that is evaluated based on the given <paramref name="templateId"/>.
        /// The text generation will implicitly contain a variable $ that evaluates to the entity with <paramref name="id"/> and the query specified in <paramref name="args"/>.
        /// </summary>
        public async Task<MessageCommandPreview> MessageCommandPreviewEntity<TEntity>(int id, int templateId, PrintEntityByIdArguments args, CancellationToken cancellation)
            where TEntity : EntityWithKey<int>
        {
            // Load the Message previews, with the first Message pre-loaded
            var template = await GetMessageTemplate(templateId, cancellation);

            var preloadedQuery = BaseUtil.EntityPreloadedQuery(id, args: args, collection: typeof(TEntity).Name, defId: DefinitionId);
            var localVars = BaseUtil.EntityLocalVariables(id, args: args, collection: typeof(TEntity).Name, defId: DefinitionId);

            return await CreateMessageCommandPreview(
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVars,
                cultureString: args.Culture,
                cancellation: cancellation);
        }

        public async Task<int> SendByMessage<TEntity>(int id, int templateId, PrintEntityByIdArguments args, string version, CancellationToken cancellation)
            where TEntity : EntityWithKey<int>
        {
            // (1) Check Permissions
            await CheckMessagePermissions(templateId, cancellation);

            // (2) Prepare the Message
            var template = await GetMessageTemplate(templateId, cancellation);

            var preloadedQuery = BaseUtil.EntityPreloadedQuery(id, args: args, collection: typeof(TEntity).Name, defId: DefinitionId);
            var localVars = BaseUtil.EntityLocalVariables(id, args: args, collection: typeof(TEntity).Name, defId: DefinitionId);

            var preview = await CreateMessageCommandPreview(
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVars,
                cultureString: args.Culture,
                cancellation: cancellation);

            // This provides a mechanism to ensure that what the user previews is exactly what the system will send
            if (!string.IsNullOrWhiteSpace(version) && preview.Version != version)
            {
                throw new ServiceException($"The underlying data has changed, please refresh and try again.");
            }

            // Prepare the messages
            var messagesToSend = preview.Messages.Select(msg => new SmsToSend(
                phoneNumber: msg.PhoneNumber,
                content: msg.Content)
            ).ToList();

            var command = new NotificationCommandToSend(templateId)
            {
                EntityId = id,
                Caption = preview.Caption,
                CreatedById = UserId,
            };

            // (3) Send Messages
            await _notificationsQueue.Enqueue(TenantId, smsMessages: messagesToSend, command: command, cancellation: cancellation);

            return command.MessageCommandId;
        }

        /// <summary>
        /// Retrieves the <see cref="MessageTemplate"/> from the database.
        /// </summary>
        /// <param name="templateId">The Id of the template to retrieve.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>The <see cref="MessageTemplate"/> with a matching Id or null if none is found.</returns>
        public async Task<MessageTemplate> GetMessageTemplate(int templateId, CancellationToken cancellation)
        {
            var output = await Repository.Templates__Load(
                emailTemplateIds: Enumerable.Empty<int>(),
                messageTemplateIds: new List<int> { templateId },
                cancellation: cancellation);

            return output.MessageTemplates.FirstOrDefault();
        }

        public async Task<MessageCommandPreview> CreateMessageCommandPreview(
            MessageTemplate template,
            QueryInfo preloadedQuery,
            Dictionary<string, EvaluationVariable> localVariables,
            string cultureString,
            CancellationToken cancellation)
        {
            return await _behaviorHelper.CreateMessageCommandPreviewUnrestricted(
                tenantId: TenantId,
                userId: UserId,
                settingsVersion: SettingsVersion,
                userSettingsVersion: UserSettingsVersion,
                template: template,
                preloadedQuery: preloadedQuery,
                localVariables: localVariables,
                cultureString: cultureString,
                null,
                isAnonymous: false, // The behavior always evaluates the template under a specific user
                getReadPermissions: async () => await UserPermissions("all", PermissionActions.Read, cancellation),
                cancellation: cancellation);
        }

        private async Task CheckMessagePermissions(int templateId, CancellationToken cancellation)
        {
            var permissions = await _permissions.PermissionsFromCache(TenantId, UserId, PermissionsVersion, $"message-commands/{templateId}", "Send", cancellation);
            if (!permissions.Any())
            {
                // Not even authorized to send
                throw new ForbiddenException();
            }
        }

        #endregion
    }

    public class ApplicationBehaviorHelper
    {
        private readonly TemplateService _templateService;
        private readonly ISettingsCache _settingsCache;
        private readonly IUserSettingsCache _userSettingsCache;
        private readonly ApiClientForTemplatingFactory _clientFactory;

        public ApplicationBehaviorHelper(
            TemplateService templateService,
            ISettingsCache settingsCache,
            IUserSettingsCache userSettingsCache,
            ApiClientForTemplatingFactory clientFactory)
        {
            _templateService = templateService;
            _settingsCache = settingsCache;
            _userSettingsCache = userSettingsCache;
            _clientFactory = clientFactory;
        }

        public async Task<MessageCommandPreview> CreateMessageCommandPreviewUnrestricted(
            int tenantId,
            int userId,
            string settingsVersion,
            string userSettingsVersion,
            MessageTemplate template,
            QueryInfo preloadedQuery,
            Dictionary<string, EvaluationVariable> localVariables,
            string cultureString,
            DateTimeOffset? now,
            bool isAnonymous,
            Func<Task<IEnumerable<AbstractPermission>>> getReadPermissions,
            CancellationToken cancellation)
        {
            // (1) Functions + Variables
            var globalFunctions = new Dictionary<string, EvaluationFunction>();
            var localFunctions = new Dictionary<string, EvaluationFunction>();
            var globalVariables = new Dictionary<string, EvaluationVariable>();
            localVariables ??= new Dictionary<string, EvaluationVariable>();
            IApiClientForTemplating client;

            if (template.Trigger == Triggers.Automatic)
            {
                // Make sure the current user has read-all permission
                if (!isAnonymous)
                {
                    var perms = await getReadPermissions();
                    if (!perms.Any())
                    {
                        throw new ForbiddenException();
                    }
                }

                await SetPrintingFunctionsForAutomatic(tenantId, settingsVersion, localFunctions, globalFunctions, cancellation);
                await SetPrintingVariablesForAutomatic(tenantId, settingsVersion, localVariables, globalVariables, cancellation);
                client = _clientFactory.GetUserless(tenantId);
            }
            else // if (template.Trigger == Triggers.Manual)
            {
                await SetPrintingFunctions(tenantId, settingsVersion, localFunctions, globalFunctions, cancellation);
                await SetPrintingVariables(userId, tenantId, userSettingsVersion, settingsVersion, localVariables, globalVariables, cancellation);
                client = _clientFactory.GetDefault();
            }

            // (2) Culture
            CultureInfo culture = BaseUtil.GetCulture(cultureString);

            // (3) The Template Plan
            var captionP = new TemplatePlanLeaf(template.Caption);
            var phoneP = new TemplatePlanLeaf(template.PhoneNumber);
            var contentP = new TemplatePlanLeaf(template.Content);

            TemplatePlan messageP;
            if (template.Cardinality == Cardinalities.Single || string.IsNullOrWhiteSpace(template.ListExpression))
            {
                messageP = new TemplatePlanTuple(captionP, phoneP, contentP);
            }
            else
            {
                var phoneAndContent = new TemplatePlanTuple(phoneP, contentP);

                // Wrap both Foreach and captionP inside a Define
                messageP = new TemplatePlanTuple(
                    captionP,
                    new TemplatePlanForeach(
                        iteratorVarName: "$",
                        listExpression: template.ListExpression,
                        inner: new TemplatePlanTuple(
                            phoneP,
                            contentP
                        )
                    )
                );
            }

            // Context variable $
            if (preloadedQuery != null)
            {
                messageP = new TemplatePlanDefineQuery("$", preloadedQuery, messageP);
            }

            if (template.Trigger == Triggers.Automatic && !string.IsNullOrWhiteSpace(template.ConditionExpression))
            {
                messageP = new TemplatePlanIf(template.ConditionExpression, messageP);
            }

            var genArgs = new TemplateArguments(globalFunctions, globalVariables, localFunctions, localVariables, culture, now);
            await _templateService.GenerateFromPlan(plan: messageP, args: genArgs, client: client, cancellation: cancellation);

            var messages = new List<MessagePreview>();
            for (int i = 0; i < contentP.Outputs.Count; i++)
            {
                var content = contentP.Outputs[i]?.Trim();
                var phoneNumbers = (phoneP.Outputs[i] ?? "").Split(';')
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Select(e => e.Trim());

                if (template.Cardinality == Cardinalities.Single)
                {
                    // Add the users if any
                    phoneNumbers = phoneNumbers.Concat(template.Subscribers
                    .Select(e => e.User?.ContactMobile)
                    .Where(e => !string.IsNullOrWhiteSpace(e)));
                }

                foreach (var phoneNumber in phoneNumbers)
                {
                    messages.Add(new MessagePreview
                    {
                        PhoneNumber = BaseUtil.ToE164(phoneNumber),
                        Content = content
                    });
                }
            }

            string version = KnuthHash(messages.SelectMany(StringsInMessage));

            return new MessageCommandPreview
            {
                Caption = captionP.Outputs.Count > 0 ? captionP.Outputs[0] : null,
                Messages = messages,
                Version = version
            };
        }

        public async Task SetPrintingVariables(int userId, int tenantId, string userSettingsVersion, string settingsVersion, Dictionary<string, EvaluationVariable> localVars, Dictionary<string, EvaluationVariable> globalVars, CancellationToken cancellation)
        {
            var userSettings = (await _userSettingsCache.GetUserSettings(userId, tenantId, userSettingsVersion, cancellation)).Data;
            globalVars.Add("$UserName", new EvaluationVariable(userSettings.Name));
            globalVars.Add("$UserName2", new EvaluationVariable(userSettings.Name2));
            globalVars.Add("$UserName3", new EvaluationVariable(userSettings.Name3));
            globalVars.Add("$UserEmail", new EvaluationVariable(userSettings.Email));

            await SetPrintingVariablesForAutomatic(tenantId, settingsVersion, localVars, globalVars, cancellation);
        }

        public async Task SetPrintingVariablesForAutomatic(int tenantId, string settingsVersion, Dictionary<string, EvaluationVariable> localVars, Dictionary<string, EvaluationVariable> globalVars, CancellationToken cancellation)
        {
            // var settings = await Settings(cancellation);
            var settings = (await _settingsCache.GetSettings(tenantId, settingsVersion, cancellation)).Data;
            globalVars.Add("$ShortCompanyName", new EvaluationVariable(settings.ShortCompanyName));
            globalVars.Add("$ShortCompanyName2", new EvaluationVariable(settings.ShortCompanyName2));
            globalVars.Add("$ShortCompanyName3", new EvaluationVariable(settings.ShortCompanyName3));
            globalVars.Add("$TaxIdentificationNumber", new EvaluationVariable(settings.TaxIdentificationNumber));
        }

        public async Task SetPrintingFunctions(int tenantId, string settingsVersion, Dictionary<string, EvaluationFunction> localFuncs, Dictionary<string, EvaluationFunction> globalFuncs, CancellationToken cancellation)
        {
            await SetPrintingFunctionsForAutomatic(tenantId, settingsVersion, localFuncs, globalFuncs, cancellation);
        }

        public Task SetPrintingFunctionsForAutomatic(int tenantId, string settingsVersion, Dictionary<string, EvaluationFunction> localFuncs, Dictionary<string, EvaluationFunction> globalFuncs, CancellationToken cancellation)
        {
            globalFuncs.Add(nameof(Localize), Localize(tenantId, settingsVersion, cancellation));
            return Task.CompletedTask;
        }

        private static string KnuthHash(IEnumerable<string> values)
        {
            ulong hash = 3074457345618258791ul;
            foreach (var value in values)
            {
                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        hash += value[i];
                        hash *= 3074457345618258799ul;
                    }
                }
            }

            return hash.ToString();
        }

        private static IEnumerable<string> StringsInMessage(MessagePreview msg)
        {
            yield return msg.PhoneNumber;
            yield return msg.Content;
        }

        #region Localize

        private EvaluationFunction Localize(int tenantId, string settingsVersion, CancellationToken cancellation) => new(functionAsync: (args, ctx) => LocalizeImpl(args, tenantId, settingsVersion, ctx, cancellation));

        private async Task<object> LocalizeImpl(object[] args, int tenantId, string settingsVersion, EvaluationContext ctx, CancellationToken cancellation)
        {
            int minArgCount = 2;
            int maxArgCount = 3;
            if (args.Length < minArgCount || args.Length > maxArgCount)
            {
                throw new TemplateException($"Function '{nameof(Localize)}' expects at least {minArgCount} and at most {maxArgCount} arguments.");
            }

            int i = 0;

            object sObj = args[i++];
            object sObj2 = args[i++];
            object sObj3 = args.Length > i ? args[i++] : null;

            string s = null;
            if (sObj is null || sObj is string)
            {
                s = sObj as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Localize)}' expects a 1st argument of type string.");
            }

            string s2 = null;
            if (sObj2 is null || sObj2 is string)
            {
                s2 = sObj2 as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Localize)}' expects a 2nd argument of type string.");
            }

            string s3 = null;
            if (sObj3 is null || sObj3 is string)
            {
                s3 = sObj3 as string;
            }
            else
            {
                throw new TemplateException($"Function '{nameof(Localize)}' expects a 3rd argument of type string.");
            }

            var settings = (await _settingsCache.GetSettings(tenantId, settingsVersion, cancellation)).Data;
            return settings.Localize(s, s2, s3);
        }

        #endregion
    }
}
