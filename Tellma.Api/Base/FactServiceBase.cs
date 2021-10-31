using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Api.Metadata;
using Tellma.Api.Templating;
using Tellma.Model.Common;
using Tellma.Repository.Common;
using Tellma.Utilities.Common;
using Tellma.Utilities.Email;

namespace Tellma.Api.Base
{
    /// <summary>
    /// Services inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type using Queryex-style arguments.
    /// </summary>
    public abstract class FactServiceBase<TEntity, TEntitiesResult> : ServiceBase, IFactService
        where TEntitiesResult : EntitiesResult<TEntity>
        where TEntity : Entity
    {
        #region Constants 

        /// <summary>
        /// The default maximum page size returned by the <see cref="GetFact(GetArguments)"/>,
        /// it can be overridden by overriding <see cref="MaximumPageSize()"/>.
        /// </summary>
        private const int DefaultMaxPageSize = 10000;

        /// <summary>
        /// The maximum number of rows (data points) that can be returned by <see cref="GetAggregate(GetAggregateArguments)"/>, 
        /// if the result is lager the implementation returns a bad request 400.
        /// </summary>
        private const int MaximumAggregateResultSize = 65536;

        /// <summary>
        /// Queries that have a total count of more than this will not be counted since it
        /// impacts performance. <see cref="int.MaxValue"/> is returned instead.
        /// </summary>
        private const int MaximumCount = 10000; // IMPORTANT: Keep in sync with client side

        #endregion

        #region Lifecycle

        private readonly IStringLocalizer _localizer;
        private readonly TemplateService _templateService;
        private readonly MetadataProvider _metadata;
        private readonly IEmailQueuer _emailQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="FactServiceBase{TEntity}"/> class.
        /// </summary>
        public FactServiceBase(FactServiceDependencies deps) : base(deps.ContextAccessor)
        {
            _localizer = deps.Localizer;
            _templateService = deps.TemplateService;
            _metadata = deps.Metadata;
            _emailQueue = deps.EmailQueue;
        }

        /// <summary>
        /// Sets the definition Id that scopes the service to only a subset of the definitioned entities.
        /// </summary>
        public FactServiceBase<TEntity, TEntitiesResult> SetDefinitionId(int definitionId)
        {
            DefinitionId = definitionId;
            FactBehavior.SetDefinitionId(definitionId);

            return this;
        }

        #endregion

        #region Behavior

        protected override IServiceBehavior Behavior => FactBehavior;

        /// <summary>
        /// When implemented, returns <see cref="IServiceBehavior"/> that is invoked every 
        /// time <see cref="Initialize()"/> is invoked.
        /// </summary>
        protected abstract IFactServiceBehavior FactBehavior { get; }

        #endregion

        #region API

        /// <summary>
        /// Returns a list of entities and optionally their count as per the specifications in <paramref name="args"/>.
        /// </summary>
        public virtual async Task<TEntitiesResult> GetEntities(GetArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Parse the parameters
            var filter = ExpressionFilter.Parse(args.Filter);
            var orderby = ExpressionOrderBy.Parse(args.OrderBy);
            var expand = ExpressionExpand.Parse(args.Expand);
            var select = ParseSelect(args.Select);

            // Prepare the query
            var query = QueryFactory().EntityQuery<TEntity>();

            // Apply read permissions
            var permissionsFilter = await UserPermissionsFilter(PermissionActions.Read, cancellation);
            query = query.Filter(permissionsFilter);

            // Apply search
            query = await Search(query, args, cancellation);

            // Apply filter
            query = query.Filter(filter);

            // Apply orderby
            orderby ??= await DefaultOrderBy(cancellation);
            query = query.OrderBy(orderby);

            // Apply the paging (Protect against DOS attacks by enforcing a maximum page size)
            var top = args.Top;
            var skip = args.Skip;
            top = Math.Min(top, MaximumPageSize());
            query = query.Skip(skip).Top(top);

            // Apply the expand, which has the general format 'Expand=A,B.C,D'
            query = query.Expand(expand);

            // Apply the select, which has the general format 'Select=A,B.C,D'
            query = query.Select(select);

            // Load the data and count in memory
            List<TEntity> data;
            int? count = null;
            if (args.CountEntities)
            {
                var output = await query.ToListAndCountAsync(MaximumCount, QueryContext, cancellation);
                data = output.Entities;
                count = output.Count;
            }
            else
            {
                data = await query.ToListAsync(QueryContext, cancellation);
            }

            // Return
            return await ToEntitiesResult(data, count, cancellation);
        }

        /// <summary>
        /// Returns a list of dynamic rows and optionally their count as per the specifications in <paramref name="args"/>.
        /// </summary>
        public virtual async Task<FactResult> GetFact(FactArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Parse the parameters
            var filter = ExpressionFilter.Parse(args.Filter);
            var orderby = ExpressionOrderBy.Parse(args.OrderBy);
            var select = ExpressionFactSelect.Parse(args.Select);

            // Prepare the query
            var query = QueryFactory().FactQuery<TEntity>();

            // Apply read permissions
            var permissionsFilter = await UserPermissionsFilter(PermissionActions.Read, cancellation);
            query = query.Filter(permissionsFilter);

            // Apply filter
            query = query.Filter(filter);

            // Apply orderby
            orderby ??= await DefaultOrderBy(cancellation);
            query = query.OrderBy(orderby);

            // Apply the paging (Protect against DOS attacks by enforcing a maximum page size)
            var top = args.Top;
            var skip = args.Skip;
            top = Math.Min(top, MaximumPageSize());
            query = query.Skip(skip).Top(top);

            // Apply the select
            query = query.Select(select);

            // Load the data and count in memory
            List<DynamicRow> data;
            int? count = null;
            if (args.CountEntities)
            {
                (data, count) = await query.ToListAndCountAsync(MaximumCount, QueryContext, cancellation);
            }
            else
            {
                data = await query.ToListAsync(QueryContext, cancellation);
            }

            // Return
            return new FactResult(data, count);
        }

        /// <summary>
        /// Returns an aggregated list of dynamic rows and any tree dimension ancestors as per the specifications in <paramref name="args"/>.
        /// </summary>
        public virtual async Task<AggregateResult> GetAggregate(GetAggregateArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Parse the parameters
            var filter = ExpressionFilter.Parse(args.Filter);
            var having = ExpressionHaving.Parse(args.Having);
            var select = ExpressionAggregateSelect.Parse(args.Select);
            var orderby = ExpressionAggregateOrderBy.Parse(args.OrderBy);

            // Prepare the query
            var query = QueryFactory().AggregateQuery<TEntity>();

            // Retrieve and Apply read permissions
            var permissionsFilter = await UserPermissionsFilter(PermissionActions.Read, cancellation);
            query = query.Filter(permissionsFilter); // Important

            // Filter and Having
            query = query.Filter(filter);
            query = query.Having(having);

            // Apply the top parameter
            var top = args.Top == 0 ? int.MaxValue : args.Top; // 0 means get all
            top = Math.Min(top, MaximumAggregateResultSize + 1);
            query = query.Top(top);

            // Apply the select, which has the general format 'Select=A+B.C,Sum(D)'
            query = query.Select(select);

            // Apply the orderby, which has the general format 'A+B.C desc,Sum(D) asc'
            query = query.OrderBy(orderby);

            // Load the data in memory
            var output = await query.ToListAsync(QueryContext, cancellation);
            var data = output.Rows;
            var ancestors = output.Ancestors.Select(e => new DimensionAncestorsResult(e.Result, e.IdIndex, e.MinIndex));

            // Put a limit on the number of data points returned, to prevent DoS attacks
            if (data.Count > MaximumAggregateResultSize)
            {
                var msg = _localizer["Error_NumberOfDataPointsExceedsMaximum0", MaximumAggregateResultSize];
                throw new ServiceException(msg);
            }

            // Return
            return new AggregateResult(data, ancestors);
        }

        /// <summary>
        /// Returns a template-generated text file that is evaluated based on the given <paramref name="templateId"/>.
        /// The text generation will implicitly contain a variable $ that evaluates to the results of the query specified in <paramref name="args"/>.
        /// </summary>
        public async Task<FileResult> PrintEntities(int templateId, PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // (1) Preloaded Query
            var collection = typeof(TEntity).Name;
            var defId = DefinitionId;

            QueryInfo contextQuery;
            if (args.I != null && args.I.Any())
            {
                contextQuery = new QueryEntitiesByIdsInfo(
                    collection: collection,
                    definitionId: defId,
                    ids: args.I);
            }
            else
            {
                contextQuery = new QueryEntitiesInfo(
                    collection: collection,
                    definitionId: defId,
                    filter: args.Filter,
                    orderby: args.OrderBy,
                    top: args.Top,
                    skip: args.Skip);
            }

            // (2) The Template Plan
            var template = await FactBehavior.GetPrintingTemplate(templateId, cancellation);

            var nameP = new TemplatePlanLeaf(template.DownloadName, TemplateLanguage.Text);
            var bodyP = new TemplatePlanLeaf(template.Body, TemplateLanguage.Html);
            var printoutP = new TemplatePlanTuple(nameP, bodyP);

            TemplatePlan plan;
            if (string.IsNullOrWhiteSpace(template.Context))
            {
                plan = printoutP;
            }
            else
            {
                plan = new TemplatePlanDefine("$", template.Context, printoutP);
            }
            plan = new TemplatePlanDefineQuery("$", contextQuery, plan);

            // (3) Functions + Variables
            var globalFunctions = new Dictionary<string, EvaluationFunction>();
            var localFunctions = new Dictionary<string, EvaluationFunction>();
            var globalVariables = new Dictionary<string, EvaluationVariable>();
            var localVariables = new Dictionary<string, EvaluationVariable>
            {
                ["$Source"] = new EvaluationVariable(defId == null ? collection : $"{collection}/{defId}"),
                ["$Filter"] = new EvaluationVariable(args.Filter),
                ["$OrderBy"] = new EvaluationVariable(args.OrderBy),
                ["$Top"] = new EvaluationVariable(args.Top),
                ["$Skip"] = new EvaluationVariable(args.Skip),
                ["$Ids"] = new EvaluationVariable(args.I)
            };

            await FactBehavior.SetPrintingFunctions(localFunctions, globalFunctions, cancellation);
            await FactBehavior.SetPrintingVariables(localVariables, globalVariables, cancellation);

            // (4)  Generate the output
            CultureInfo culture = GetCulture(args.Culture);
            var genArgs = new TemplateArguments(globalFunctions, globalVariables, localFunctions, localVariables, culture);
            await _templateService.GenerateFromPlan(plan, genArgs, cancellation);

            var downloadName = nameP.Outputs[0];
            var body = bodyP.Outputs[0];

            // Change the body to bytes
            var bodyBytes = Encoding.UTF8.GetBytes(body);

            // Use a default download name if none is provided
            if (string.IsNullOrWhiteSpace(downloadName))
            {
                var meta = await GetMetadata(cancellation);
                var titlePlural = meta.PluralDisplay();
                if (args.I != null && args.I.Count > 0)
                {
                    downloadName = $"{titlePlural} ({args.I.Count})";
                }
                else
                {
                    int from = args.Skip + 1;
                    int to = Math.Max(from, args.Skip + args.Top);
                    downloadName = $"{titlePlural} {from}-{to}";
                }
            }

            if (!downloadName.ToLower().EndsWith(".html"))
            {
                downloadName += ".html";
            }

            // Return as a file
            return new FileResult(bodyBytes, downloadName);
        }

        /// <summary>
        /// Returns a template-generated text file that is evaluated based on the given <paramref name="templateId"/>.
        /// The text generation will implicitly contain a variable $ that evaluates to the results of the dynamic query specified in <paramref name="args"/>.
        /// </summary>
        public async Task<FileResult> PrintDynamic(int templateId, PrintDynamicArguments args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // (1) Preloaded Query
            var collection = typeof(TEntity).Name;
            var defId = DefinitionId;

            // (2) Functions + Variables
            var globalFunctions = new Dictionary<string, EvaluationFunction>();
            var localFunctions = new Dictionary<string, EvaluationFunction>();
            var globalVariables = new Dictionary<string, EvaluationVariable>();
            var localVariables = new Dictionary<string, EvaluationVariable>
            {
                ["$Source"] = new EvaluationVariable($"{collection}/{defId}"),
                ["$Type"] = new EvaluationVariable(args.Type),
                ["$Select"] = new EvaluationVariable(args.Select),
                ["$OrderBy"] = new EvaluationVariable(args.OrderBy),
                ["$Filter"] = new EvaluationVariable(args.Filter),
                ["$Having"] = new EvaluationVariable(args.Having),
                ["$Top"] = new EvaluationVariable(args.Top),
                ["$Skip"] = new EvaluationVariable(args.Skip)
            };

            await FactBehavior.SetPrintingFunctions(localFunctions, globalFunctions, cancellation);
            await FactBehavior.SetPrintingVariables(localVariables, globalVariables, cancellation);

            // (2) The templates
            var template = await FactBehavior.GetPrintingTemplate(templateId, cancellation);

            var nameP = new TemplatePlanLeaf(template.DownloadName, TemplateLanguage.Text);
            var bodyP = new TemplatePlanLeaf(template.Body, TemplateLanguage.Html);
            var printoutP = new TemplatePlanTuple(nameP, bodyP);

            TemplatePlan plan;
            if (string.IsNullOrWhiteSpace(template.Context))
            {
                IReadOnlyList<DynamicRow> data;

                if (args.Type == "Fact")
                {
                    var result = await GetFact(new FactArguments
                    {
                        Select = args.Select,
                        Filter = args.Filter,
                        OrderBy = args.OrderBy,
                        Top = args.Top,
                        Skip = args.Skip,
                        CountEntities = false,
                    }, cancellation);

                    data = result?.Data;
                }
                else if (args.Type == "Aggregate")
                {
                    var result = await GetAggregate(new GetAggregateArguments
                    {
                        Select = args.Select,
                        Filter = args.Filter,
                        Having = args.Having,
                        OrderBy = args.OrderBy,
                        Top = args.Top,
                    }, cancellation);

                    data = result?.Data;
                }
                else
                {
                    throw new ServiceException($"Unknown Type '{args.Type}'.");
                }

                localVariables.Add("$", new EvaluationVariable(data));
                plan = printoutP;
            }
            else
            {
                plan = new TemplatePlanDefine("$", template.Context, printoutP);
            }

            // (4) Culture
            CultureInfo culture = GetCulture(args.Culture);

            // Generate the output
            var genArgs = new TemplateArguments(globalFunctions, globalVariables, localFunctions, localVariables, culture);
            await _templateService.GenerateFromPlan(plan, genArgs, cancellation);

            var downloadName = nameP.Outputs[0];
            var body = bodyP.Outputs[0];

            // Change the body to bytes
            var bodyBytes = Encoding.UTF8.GetBytes(body);

            // Use a default download name if none is provided
            if (string.IsNullOrWhiteSpace(downloadName))
            {
                var meta = await GetMetadata(cancellation);
                downloadName = meta.PluralDisplay();
            }

            if (!downloadName.ToLower().EndsWith(".html"))
            {
                downloadName += ".html";
            }

            // Return as a file
            return new FileResult(bodyBytes, downloadName);
        }

        /// <summary>
        /// Returns a template-generated text file that is evaluated based on the given <paramref name="templateId"/>.
        /// The text generation will implicitly contain a variable $ that evaluates to the results of the query specified in <paramref name="args"/>.
        /// </summary>
        public async Task<EmailCommandPreview> EmailCommandPreviewEntities(int templateId, PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // Load the email previews, with the first email pre-loaded
            var template = await FactBehavior.GetEmailTemplate(templateId, cancellation);
            int index = 0;

            var preview = await UnversionedEmailCommandPreview(
                template: template,
                collection: typeof(TEntity).Name,
                defId: DefinitionId,
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

        public async Task<EmailPreview> EmailPreviewEntities(int templateId, int emailIndex, PrintEntitiesArguments<int> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            var template = await FactBehavior.GetEmailTemplate(templateId, cancellation);
            int index = emailIndex;

            var preview = await UnversionedEmailCommandPreview(
                template: template,
                collection: typeof(TEntity).Name,
                defId: DefinitionId,
                fromIndex: index,
                toIndex: index,
                args: args,
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

        public async Task SendByEmail(int templateId, PrintEntitiesArguments<int> args, EmailCommandVersions versions, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            var template = await FactBehavior.GetEmailTemplate(templateId, cancellation);
            int fromIndex = 0;
            int toIndex = int.MaxValue;

            var preview = await UnversionedEmailCommandPreview(
                template: template,
                collection: typeof(TEntity).Name,
                defId: DefinitionId,
                fromIndex: fromIndex,
                toIndex: toIndex,
                args: args,
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

            await _emailQueue.EnqueueEmails(TenantId ?? 0, emailsToSend);
        }

        private static bool MatchVersions(EmailCommandPreview preview, EmailCommandVersions clientVersions, int fromIndex, int toIndex)
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

        protected async Task<EmailCommandPreview> UnversionedEmailCommandPreview(
            AbstractEmailTemplate template,
            string collection,
            int? defId,
            int fromIndex,
            int toIndex,
            PrintEntitiesArguments<int> args,
            CancellationToken cancellation)
        {
            // (1) Functions + Variables
            var globalFunctions = new Dictionary<string, EvaluationFunction>();
            var localFunctions = new Dictionary<string, EvaluationFunction>();
            var globalVariables = new Dictionary<string, EvaluationVariable>();
            var localVariables = new Dictionary<string, EvaluationVariable>
            {
                ["$Source"] = new EvaluationVariable($"{collection}/{defId}"),
                ["$Filter"] = new EvaluationVariable(args.Filter),
                ["$OrderBy"] = new EvaluationVariable(args.OrderBy),
                ["$Top"] = new EvaluationVariable(args.Top),
                ["$Skip"] = new EvaluationVariable(args.Skip),
                ["$Ids"] = new EvaluationVariable(args.I)
            };

            await FactBehavior.SetPrintingFunctions(localFunctions, globalFunctions, cancellation);
            await FactBehavior.SetPrintingVariables(localVariables, globalVariables, cancellation);

            // (3) Culture
            CultureInfo culture = GetCulture(args.Culture);

            // (4) The Template Plan
            var bodyH = new TemplatePlanLeaf(template.BodyTemplate, TemplateLanguage.Html);
            var subjectH = new TemplatePlanLeaf(template.SubjectTemplate);

            // Recipients
            var regularRecipients = template.RegularRecipients.Select(a => new TemplatePlanLeaf(a.Template)).ToList();
            var ccRecipients = template.CcRecipients.Select(a => new TemplatePlanLeaf(a.Template)).ToList();
            var bccRecipients = template.BccRecipients.Select(a => new TemplatePlanLeaf(a.Template)).ToList();

            // Attachments
            var attachmentInfos = new List<(TemplatePlanLeaf body, TemplatePlanLeaf name)>();
            var attachments = new List<TemplatePlan>();
            foreach (var e in template.Attachments)
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
            var bodyAndAttachmentPlans = new List<TemplatePlan> { bodyH };
            bodyAndAttachmentPlans.AddRange(attachments);

            var subjectAndRecipientsPlans = new List<TemplatePlan> { subjectH };
            subjectAndRecipientsPlans.AddRange(regularRecipients);
            subjectAndRecipientsPlans.AddRange(ccRecipients);
            subjectAndRecipientsPlans.AddRange(bccRecipients);

            TemplatePlan emailP;
            if (string.IsNullOrWhiteSpace(template.ListExpression))
            {
                var allPlans = subjectAndRecipientsPlans.Concat(bodyAndAttachmentPlans);
                emailP = new TemplatePlanTuple(allPlans);
            }
            else
            {
                var subjectAndRecipientsP = new TemplatePlanTuple(subjectAndRecipientsPlans);
                var bodyAndAttachmentsP = new TemplatePlanTuple(bodyAndAttachmentPlans);

                emailP = new TemplatePlanRangeForeach(
                    iteratorVarName: "$",
                    listExpression: template.ListExpression,
                    always: subjectAndRecipientsP,
                    rangeOnly: bodyAndAttachmentsP,
                    from: fromIndex,
                    to: toIndex);
            }

            {
                // Preloaded Query
                QueryInfo preloadedQuery;
                if (args.I != null && args.I.Any())
                {
                    preloadedQuery = new QueryEntitiesByIdsInfo(
                        collection: collection,
                        definitionId: defId,
                        ids: args.I);
                }
                else
                {
                    preloadedQuery = new QueryEntitiesInfo(
                        collection: collection,
                        definitionId: defId,
                        filter: args.Filter,
                        orderby: args.OrderBy,
                        top: args.Top,
                        skip: args.Skip);
                }

                emailP = new TemplatePlanDefineQuery("$", preloadedQuery, emailP);
            }

            var genArgs = new TemplateArguments(globalFunctions, globalVariables, localFunctions, localVariables, culture);
            await _templateService.GenerateFromPlan(emailP, genArgs, cancellation);

            var emails = new List<EmailPreview>();
            for (int i = 0; i < subjectH.Outputs.Count; i++)
            {
                var email = new EmailPreview
                {
                    To = GetEmailAddresses(regularRecipients, i),
                    Cc = GetEmailAddresses(ccRecipients, i),
                    Bcc = GetEmailAddresses(bccRecipients, i),
                    Subject = subjectH.Outputs[i]
                };

                if (fromIndex <= i && toIndex >= i) // Within range
                {
                    int rangeIndex = i - fromIndex;
                    email.Body = bodyH.Outputs[rangeIndex];

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

                        email.Attachments ??= new List<AttachmentPreview>(attachmentInfos.Count);
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
                Emails = emails,
            };
        }

        public class TemplatePlanRangeForeach : TemplatePlan
        {
            private TemplexBase _listCandidate;

            public TemplatePlanRangeForeach(
                string iteratorVarName,
                string listExpression,
                TemplatePlan always,
                TemplatePlan rangeOnly,
                int from, int to)
            {
                if (string.IsNullOrWhiteSpace(listExpression))
                {
                    throw new ArgumentException($"'{nameof(listExpression)}' cannot be null or whitespace.", nameof(listExpression));
                }

                IteratorVariableName = iteratorVarName ?? "$";
                ListExpression = listExpression;
                Always = always ?? throw new ArgumentNullException(nameof(always));
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

        private static List<string> GetEmailAddresses(List<TemplatePlanLeaf> plans, int index)
        {
            return plans.Select(e => e.Outputs[index])
                .Where(e => e != null)
                .SelectMany(e => e.Split(';'))
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .NullIfEmpty();
        }

        protected static string GetEmailCommandPreviewVersion(EmailCommandPreview preview)
            => KnuthHash(preview.Emails.SelectMany(email => StringsInPreviewEmail(email)));

        protected static string GetEmailPreviewVersion(EmailPreview email)
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

        #endregion

        #region Helpers

        /// <summary>
        /// An optional definition Id for services that are accessing definitioned resources.
        /// <summary/>
        protected int? DefinitionId { get; private set; }

        /// <summary>
        /// Helper property that returns a <see cref="QueryContext"/> based on <see cref="UserId"/> and <see cref="Today"/>.
        /// </summary>
        protected QueryContext QueryContext => new(UserId, Today);

        /// <summary>
        /// Helper function that returns the <see cref="CultureInfo"/> that corresponds
        /// to the given <paramref name="name"/>, or the current UI culture if name was null.
        /// </summary>
        /// <param name="name">The culture name, for example "en".</param>
        /// <exception cref="ServiceException">If <paramref name="name"/> is not null and invalid.</exception>
        protected static CultureInfo GetCulture(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return CultureInfo.CurrentUICulture;
            }

            try
            {
                return new CultureInfo(name);
            }
            catch (CultureNotFoundException)
            {
                throw new ServiceException($"Value '{name}' could not be interpreted as a valid culture.");
            }
        }

        /// <summary>
        /// Select argument may get huge and unweildly in certain cases, this method offers a chance
        /// for services to optimize queries by understanding special concise "shorthands" in
        /// the select string that get expanded into a proper select expression.
        /// This way clients don't have to send large select strings in the request for common scenarios.
        /// </summary>
        protected virtual ExpressionSelect ParseSelect(string select)
        {
            return ExpressionSelect.Parse(select);
        }

        /// <summary>
        /// Get the <see cref="IQueryFactory"/> that the <see cref="FactServiceBase{TEntity}"/> can use to query the entities.
        /// </summary>
        protected virtual IQueryFactory QueryFactory() => FactBehavior.QueryFactory<TEntity>();

        /// <summary>
        /// Retrieves the user permissions for the current view and the specified action.
        /// </summary>
        protected virtual async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
            => await FactBehavior.UserPermissions(View, action, cancellation);

        /// <summary>
        /// Returns the view to use when checking user permissions.
        /// </summary>
        protected abstract string View { get; }

        /// <summary>
        /// Implementations create the <see cref="TEntitiesResult"/> to return from all the service
        /// methods that return it.
        /// </summary>
        protected abstract Task<TEntitiesResult> ToEntitiesResult(List<TEntity> data, int? count = null, CancellationToken cancellation = default);

        /// <summary>
        /// Retrieves the user permissions for the given action and parses them in the form of an 
        /// <see cref="ExpressionFilter"/>, throws a <see cref="ForbiddenException"/> if none are found.
        /// </summary>    
        /// <exception cref="ForbiddenException">When the user lacks the needed permissions.</exception>
        protected async Task<ExpressionFilter> UserPermissionsFilter(string action, CancellationToken cancellation)
        {
            // Check if the user has any permissions on View at all, else throw forbidden exception
            // If the user has some permissions on View, OR all their criteria together and return as a FilterExpression
            var permissions = await UserPermissions(action, cancellation);
            if (!permissions.Any())
            {
                // Not even authorized to call this API
                throw new ForbiddenException();
            }
            else if (permissions.Any(e => string.IsNullOrWhiteSpace(e.Criteria)))
            {
                // The user can read the entire data set
                return null;
            }
            else
            {
                // The user has access to part of the data set based on a list
                // of filters that will be ORed together in a dynamic query
                return permissions.Select(e => ExpressionFilter.Parse(e.Criteria))
                        .Aggregate((e1, e2) => ExpressionFilter.Disjunction(e1, e2));
            }
        }

        /// <summary>
        /// Applies the search argument to the <see cref="EntityQuery{T}"/>. This is handled differently in every service.
        /// </summary>
        /// <param name="query">The <see cref="EntityQuery{T}"/> to apply the search argument to.</param>
        /// <param name="args">The <see cref="GetArguments"/> containing the relevant search argument.</param>
        /// <returns>The query with the search argument applied to it.</returns>
        protected abstract Task<EntityQuery<TEntity>> Search(EntityQuery<TEntity> query, GetArguments args, CancellationToken cancellation);

        /// <summary>
        /// Returns the default order by to apply on queries when the orderby parameter is null.
        /// </summary>
        protected abstract Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken cancellation);

        /// <summary>
        /// Specifies the maximum page size to be returned by <see cref="GetEntities(GetArguments)"/>. Defaults to <see cref="DefaultMaxPageSize"/>.
        /// </summary>
        protected virtual int MaximumPageSize()
        {
            return DefaultMaxPageSize;
        }

        /// <summary>
        /// Retrieves the metadata of the entity.
        /// </summary>
        protected async Task<TypeMetadata> GetMetadata(CancellationToken cancellation)
        {
            int? tenantId = TenantId;
            int? definitionId = DefinitionId;
            Type type = typeof(TEntity);
            IMetadataOverridesProvider overrides = await FactBehavior.GetMetadataOverridesProvider(cancellation);

            return _metadata.GetMetadata(tenantId, type, definitionId, overrides);
        }

        #endregion

        #region IFactService

        async Task<EntitiesResult<Entity>> IFactService.GetEntities(GetArguments args, CancellationToken cancellation)
        {
            var result = await GetEntities(args, cancellation);
            var genericData = result.Data.Cast<Entity>().ToList();
            var count = result.Count;

            return new EntitiesResult<Entity>(genericData, count);
        }

        #endregion
    }

    /// <summary>
    /// Services inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type using Queryex-style arguments.
    /// </summary>
    public abstract class FactServiceBase<TEntity> : FactServiceBase<TEntity, EntitiesResult<TEntity>>
        where TEntity : Entity
    {
        public FactServiceBase(FactServiceDependencies deps) : base(deps)
        {
        }

        protected override Task<EntitiesResult<TEntity>> ToEntitiesResult(List<TEntity> data, int? count = null, CancellationToken cancellation = default)
        {
            var result = new EntitiesResult<TEntity>(data, count);
            return Task.FromResult(result);
        }
    }

    public interface IFactService
    {
        Task<EntitiesResult<Entity>> GetEntities(GetArguments args, CancellationToken cancellation);

        Task<FactResult> GetFact(FactArguments args, CancellationToken cancellation);

        Task<AggregateResult> GetAggregate(GetAggregateArguments args, CancellationToken cancellation);
    }
}
