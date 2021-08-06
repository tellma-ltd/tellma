using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.ImportExport;
using Tellma.Api.Metadata;
using Tellma.Model.Application;
using Tellma.Model.Common;
using Tellma.Repository.Application;
using Tellma.Repository.Common;
using Tellma.Utilities.Blobs;
using Tellma.Utilities.Calendars;
using Tellma.Utilities.Common;

namespace Tellma.Api
{
    public class DocumentsService : CrudServiceBase<DocumentForSave, Document, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer<Strings> _localizer;
        private readonly IBlobService _blobService;
        private readonly IClientProxy _clientProxy;
        private readonly MetadataProvider _metadata;

        /// <summary>
        /// This <see cref="ExpressionSelect"/> replaces any occurrece of the shorthand 
        /// "$Details" in the query select parameter. We cache it here since it is rather big.
        /// </summary>
        private static readonly ExpressionSelect _detailsSelectExpression =
            ExpressionSelect.Parse(string.Join(',', DocDetails.DocumentPaths()));

        // Used across multiple methods
        private List<(string, byte[])> _blobsToSave;
        private List<string> _blobsToDelete;
        private List<InboxStatus> _notificationInfos;

        public DocumentsService(
            ApplicationFactServiceBehavior behavior,
            CrudServiceDependencies deps,
            IBlobService blobService,
            IClientProxy clientProxy) : base(deps)
        {
            _behavior = behavior;
            _localizer = deps.Localizer;
            _blobService = blobService;
            _clientProxy = clientProxy;
            _metadata = deps.Metadata;
        }

        #region Default Tab Entry

        /// <summary>
        /// This is used in preprocessing and validation when a tab entry is null.
        /// </summary>
        private static readonly DocumentLineDefinitionEntryForSave DefaultTabEntryForSave = MakeDefaultTabEntryForSave();

        /// <summary>
        /// This is used in preprocessing and validation when a tab entry is null.
        /// </summary>
        private static readonly DocumentLineDefinitionEntry DefaultTabEntry = MakeDefaultTabEntry();

        /// <summary>
        /// This is used in preprocessing and validation when a tab entry is null.
        /// </summary>
        private static DocumentLineDefinitionEntryForSave MakeDefaultTabEntryForSave() => new()
        {
            PostingDateIsCommon = true,
            MemoIsCommon = true,
            CurrencyIsCommon = true,
            CenterIsCommon = true,

            RelationIsCommon = true,
            ResourceIsCommon = true,
            NotedRelationIsCommon = true,

            QuantityIsCommon = true,
            UnitIsCommon = true,
            Time1IsCommon = true,
            DurationIsCommon = true,
            DurationUnitIsCommon = true,
            Time2IsCommon = true,

            ExternalReferenceIsCommon = true,
            ReferenceSourceIsCommon = true,
            InternalReferenceIsCommon = true,
        };

        /// <summary>
        /// This is used in preprocessing and validation when a tab entry is null.
        /// </summary>
        private static DocumentLineDefinitionEntry MakeDefaultTabEntry()
        {
            var entry = new DocumentLineDefinitionEntry
            {
                PostingDateIsCommon = true,
                MemoIsCommon = true,
                CurrencyIsCommon = true,
                CenterIsCommon = true,

                RelationIsCommon = true,
                ResourceIsCommon = true,
                NotedRelationIsCommon = true,

                QuantityIsCommon = true,
                UnitIsCommon = true,
                Time1IsCommon = true,
                DurationIsCommon = true,
                DurationUnitIsCommon = true,
                Time2IsCommon = true,

                ExternalReferenceIsCommon = true,
                ReferenceSourceIsCommon = true,
                InternalReferenceIsCommon = true,
            };

            foreach (var prop in TypeDescriptor.Get<DocumentLineDefinitionEntry>().Properties)
            {
                entry.EntityMetadata[prop.Name] = FieldMetadata.Loaded;
            }

            return entry;
        }

        /// <summary>
        /// Cached.
        /// </summary>
        private static readonly TypeDescriptor lineDefEntryForSaveDescriptor = TypeDescriptor.Get<DocumentLineDefinitionEntryForSave>();

        /// <summary>
        /// Checks if the supplied DocumentLineDefinitionEntryForSave is equivalent 
        /// to the default one (ignoring Id, LineDefinitionId and EntryIndex properties).
        /// </summary>
        private static bool EqualsDefaultTabEntry(DocumentLineDefinitionEntryForSave tabEntry)
        {
            return lineDefEntryForSaveDescriptor.Properties.All(p =>
            {
                switch (p.Name)
                {
                    case nameof(DocumentLineDefinitionEntryForSave.Id):
                    case nameof(DocumentLineDefinitionEntryForSave.LineDefinitionId):
                    case nameof(DocumentLineDefinitionEntryForSave.EntryIndex):
                        return true; // Those properties don't matter for the comparison
                    default:
                        // Everything else must match
                        var expected = p.GetValue(DefaultTabEntryForSave);
                        var actual = p.GetValue(tabEntry);

                        return (expected == null && actual == null) ||
                            (expected != null && actual != null && expected.Equals(actual));
                }
            });
        }

        #endregion

        #region Include Required Signature

        public DocumentsService SetIncludeRequiredSignatures(bool val)
        {
            IncludeRequiredSignatures = val;
            return this;
        }

        private bool IncludeRequiredSignatures { get; set; }

        #endregion

        protected override string View => $"documents/{DefinitionId}";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        /// <summary>
        /// The current <see cref="DefinitionId"/>, if null throws an exception.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private new int DefinitionId => base.DefinitionId ??
            throw new InvalidOperationException($"DefinitionId was not set in {nameof(DocumentsService)}.");

        /// <summary>
        /// The current TenantId.
        /// </summary>
        private new int TenantId => _behavior.TenantId;

        /// <summary>
        /// Helper method for retrieving the <see cref="DocumentDefinitionForClient"/> 
        /// that corresponds to the current <see cref="DefinitionId"/>.
        /// </summary>
        /// <param name="cancellation">The cancellation instruction.</param>
        private async Task<DocumentDefinitionForClient> Definition(CancellationToken cancellation = default)
        {
            var defs = await _behavior.Definitions(cancellation);
            var docDef = defs.Documents.GetValueOrDefault(DefinitionId) ??
                throw new InvalidOperationException($"Document definition with Id = {DefinitionId} is missing from the cache.");

            return docDef;
        }

        /// <summary>
        /// Helper method for retrieving the <see cref="LineDefinitionForClient"/> 
        /// that corresponds to a certain <paramref name="lineDefId"/>. 
        /// Throws an exception if one is not found.
        /// </summary>
        /// <param name="lineDefId">The id of the line definition to retrieve.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task<LineDefinitionForClient> LineDefinition(int lineDefId, CancellationToken cancellation = default)
        {
            var defs = await _behavior.Definitions(cancellation);
            var lineDef = defs.Lines.GetValueOrDefault(lineDefId) ??
                throw new InvalidOperationException($"Line Definition with Id = {lineDefId} is missing from the cache");

            return lineDef;
        }

        #region State & Workflow

        public async Task<(Document, Extras)> UpdateAssignment(UpdateAssignmentArguments args)
        {
            await Initialize();

            // Execute and return
            using var trx = TransactionFactory.ReadCommitted();
            (InboxStatusResult result, int documentId) = await _behavior.Repository
                .Documents__UpdateAssignment(
                assignmentId: args.Id,
                comment: args.Comment,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            Document entity = null;
            Extras extras = null;

            if (args.ReturnEntities ?? false)
            {
                var getbyIdArgs = new GetByIdArguments { Select = args.Select, Expand = args.Expand };
                (entity, extras) = await GetById(documentId, getbyIdArgs, cancellation: default);
            }

            _clientProxy.UpdateInboxStatuses(TenantId, result.InboxStatuses);

            trx.Complete();
            return (entity, extras);
        }

        public async Task<(List<Document>, Extras)> Assign(List<int> ids, AssignArguments args)
        {
            await Initialize();

            if (ids == null || !ids.Any())
            {
                throw new ServiceException("No ids were supplied.");
            }

            // Check user permissions
            var action = PermissionActions.Read;
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // C# Validation 
            if (args.AssigneeId == 0)
            {
                throw new ServiceException(_localizer[ErrorMessages.Error_Field0IsRequired, nameof(args.AssigneeId)]);
            }

            // Execute and return
            using var trx = TransactionFactory.ReadCommitted();
            AssignResult result = await _behavior.Repository
                .Documents__Assign(
                ids: ids,
                assigneeId: args.AssigneeId,
                comment: args.Comment,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            List<Document> data = null;
            Extras extras = null;

            if (args.ReturnEntities ?? false)
            {
                (data, extras) = await GetByIds(ids, args, action, cancellation: default);
            }

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, data);

            // Actual Assignment
            var inboxStatuses = result.InboxStatuses;
            var assigneeInfo = result.AssigneeInfo;
            var serial = result.DocumentSerial;

            // Notify relevant parties
            _clientProxy.UpdateInboxStatuses(TenantId, inboxStatuses);

            // If assignee is not the same user, notify them by Email/SMS/Push
            if (UserId != args.AssigneeId)
            {
                var settings = await _behavior.Settings();
                var userSettings = await _behavior.UserSettings();
                var def = await Definition();
                var preferredLang = assigneeInfo.PreferredLanguage ?? settings.PrimaryLanguageId;

                // Things that need to be localized to the preferred language of the notified assignee
                string singularTitle;
                string pluralTitle;
                string senderName;

                var culture = CultureInfo.GetCultureInfo(preferredLang);
                using (var _ = new CultureScope(culture))
                {
                    singularTitle = settings.Localize(def.TitleSingular, def.TitleSingular2, def.TitleSingular3);
                    pluralTitle = settings.Localize(def.TitlePlural, def.TitlePlural2, def.TitlePlural3);
                    senderName = settings.Localize(userSettings.Name, userSettings.Name2, userSettings.Name3);
                }

                var notifyArgs = new NotifyDocumentAssignmentArguments
                {
                    ContactEmail = assigneeInfo.ContactEmail,
                    ContactMobile = assigneeInfo.ContactMobile,
                    PreferredLanguage = preferredLang,
                    ViaEmail = assigneeInfo.EmailNewInboxItem ?? false,
                    ViaSms = assigneeInfo.SmsNewInboxItem ?? false,
                    ViaPush = assigneeInfo.PushNewInboxItem ?? false,
                    DefinitionId = DefinitionId,
                    SingularTitle = singularTitle,
                    PluralTitle = pluralTitle,
                    DocumentCount = ids.Count,
                    DocumentId = ids.First(),
                    FormattedSerial = FormatSerial(serial, def.Prefix, def.CodeWidth),
                    SenderName = senderName,
                    SenderComment = args.Comment
                };

                await _clientProxy.NotifyDocumentsAssignment(TenantId, notifyArgs);
            }

            trx.Complete();
            return (data, extras);
        }

        public async Task<(List<Document>, Extras)> SignLines(List<int> lineIds, SignArguments args)
        {
            await Initialize();
            var returnEntities = args.ReturnEntities ?? false;

            // C# Validation 
            if (string.IsNullOrWhiteSpace(args.RuleType))
            {
                throw new ServiceException(_localizer[ErrorMessages.Error_Field0IsRequired, nameof(args.RuleType)]);
            }

            // Action
            using var trx = TransactionFactory.ReadCommitted();
            SignResult result = await _behavior.Repository.Lines__Sign(
                lineIds,
                args.ToState,
                args.ReasonId,
                args.ReasonDetails,
                args.OnBehalfOfUserId,
                args.RuleType,
                args.RoleId,
                args.SignedAt ?? DateTimeOffset.Now,
                returnIds: returnEntities,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            // Validation
            AddErrorsAndThrowIfInvalid(result.Errors);

            var documentIds = result.DocumentIds;
            if (returnEntities)
            {
                var response = await GetByIds(documentIds.ToList(), args, PermissionActions.Read, cancellation: default);

                trx.Complete();
                return response;
            }
            else
            {
                trx.Complete();
                return default;
            }
        }

        public async Task<(List<Document>, Extras)> UnsignLines(List<int> signatureIds, ActionArguments args)
        {
            await Initialize();
            var returnEntities = args.ReturnEntities ?? false;

            // C# Validation 
            // Goes here

            // Action
            using var trx = TransactionFactory.ReadCommitted();
            SignResult result = await _behavior.Repository.LineSignatures__Delete(
                ids: signatureIds,
                returnIds: returnEntities,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            // Validation
            AddErrorsAndThrowIfInvalid(result.Errors);

            // Load Result
            var documentIds = result.DocumentIds;
            if (returnEntities)
            {
                var response = await GetByIds(documentIds.ToList(), args, PermissionActions.Read, cancellation: default);

                trx.Complete();
                return response;
            }
            else
            {
                trx.Complete();
                return default;
            }
        }

        public async Task<(List<Document>, Extras)> Close(List<int> ids, ActionArguments args)
        {
            return await UpdateDocumentState(ids, args, nameof(Close));
        }

        public async Task<(List<Document>, Extras)> Open(List<int> ids, ActionArguments args)
        {
            return await UpdateDocumentState(ids, args, nameof(Open));
        }

        public async Task<(List<Document>, Extras)> Cancel(List<int> ids, ActionArguments args)
        {
            return await UpdateDocumentState(ids, args, nameof(Cancel));
        }

        public async Task<(List<Document>, Extras)> Uncancel(List<int> ids, ActionArguments args)
        {
            return await UpdateDocumentState(ids, args, nameof(Uncancel));
        }

        private async Task<(List<Document>, Extras)> UpdateDocumentState(List<int> ids, ActionArguments args, string transition)
        {
            await Initialize();

            // Check user permissions
            var action = "State";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // C# Validation 
            // ...

            // Transaction
            using var trx = TransactionFactory.ReadCommitted();

            InboxStatusResult result = transition switch
            {
                nameof(Close) => await _behavior.Repository.Documents__Close(DefinitionId, ids, ModelState.IsError, ModelState.RemainingErrors, UserId),
                nameof(Open) => await _behavior.Repository.Documents__Open(DefinitionId, ids, ModelState.IsError, ModelState.RemainingErrors, UserId),
                nameof(Cancel) => await _behavior.Repository.Documents__Cancel(DefinitionId, ids, ModelState.IsError, ModelState.RemainingErrors, UserId),
                nameof(Uncancel) => await _behavior.Repository.Documents__Uncancel(DefinitionId, ids, ModelState.IsError, ModelState.RemainingErrors, UserId),
                _ => throw new InvalidOperationException($"Unknown transition {transition}"),
            };

            // Validation
            AddErrorsAndThrowIfInvalid(result.Errors);

            // Load Result
            List<Document> data = null;
            Extras extras = null;

            if (args.ReturnEntities ?? false)
            {
                (data, extras) = await GetByIds(ids, args, action, cancellation: default);
            }

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, data);

            // Non-transactional stuff
            var statuses = result.InboxStatuses;
            _clientProxy.UpdateInboxStatuses(TenantId, statuses);

            // Commit and return
            trx.Complete();
            return (data, extras);
        }

        #endregion

        public async Task<(byte[] FileBytes, string FileName)> GetAttachment(int docId, int attachmentId, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // This enforces read permissions
            string att = nameof(Document.Attachments);
            string attFileId = nameof(Attachment.FileId);
            string attFileName = nameof(Attachment.FileName);
            string attFileExt = nameof(Attachment.FileExtension);
            var (doc, _) = await GetById(docId, new GetByIdArguments
            {
                Select = $"{att}.{attFileId},{att}.{attFileName},{att}.{attFileExt}"
            },
            cancellation);

            // Get the blob name
            var attachment = doc?.Attachments?.FirstOrDefault(att => att.Id == attachmentId);
            if (attachment != null && !string.IsNullOrWhiteSpace(attachment.FileId))
            {
                try
                {
                    // Get the bytes
                    string blobName = AttachmentBlobName(attachment.FileId);
                    var fileBytes = await _blobService.LoadBlob(TenantId, blobName, cancellation);

                    // Get the content type
                    var fileName = $"{attachment.FileName ?? "Attachment"}.{attachment.FileExtension}";
                    return (fileBytes, fileName);
                }
                catch (BlobNotFoundException)
                {
                    throw new NotFoundException<int>(attachmentId);
                }
            }
            else
            {
                throw new NotFoundException<int>(attachmentId);
            }
        }

        public override async Task<(Document, Extras)> GetById(int id, GetByIdArguments args, CancellationToken cancellation)
        {
            var (entity, extras) = await base.GetById(id, args, cancellation);

            // TODO it's more accurate to do this from the client side (e.g. if the user views a cached document)
            if (entity.OpenedAt == null)
            {
                if (entity.AssigneeId == UserId)
                {
                    // Mark the entity's OpenedAt both in the DB and in the returned entity
                    var assignedAt = entity.AssignedAt.Value;
                    var openedAt = DateTimeOffset.Now;
                    var infos = await _behavior.Repository.Documents__Preview(entity.Id, assignedAt, openedAt, UserId, cancellation);
                    entity.OpenedAt = openedAt;

                    // Notify the user
                    _clientProxy.UpdateInboxStatuses(TenantId, infos);
                }
            }

            return (entity, extras);
        }

        public async Task<(
            List<LineForSave> lines,
            List<Account> accounts,
            List<Resource> resources,
            List<Relation> relations,
            List<EntryType> entryTypes,
            List<Center> centers,
            List<Currency> currencies,
            List<Unit> units
            )> Generate(int lineDefId, Dictionary<string, string> args, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // TODO: Permissions (?)
            await UserPermissionsFilter(PermissionActions.Update, cancellation: default);
            // ids = await CheckActionPermissionsBefore(actionFilter, ids);

            var lineDef = await LineDefinition(lineDefId, cancellation);

            // Better args will contain only the defined parameter keys and all the defined parameter keys (with possible null values)
            var betterArgs = new Dictionary<string, string>();
            foreach (var param in lineDef.GenerateParameters)
            {
                var value = args.GetValueOrDefault(param.Key);

                // Ensure all required signatures are supplied
                if (param.Visibility == Visibility.Required && string.IsNullOrWhiteSpace(value))
                {
                    var settings = await _behavior.Settings(cancellation);
                    var paramLabel = settings.Localize(param.Label, param.Label2, param.Label3);
                    var msg = _localizer[ErrorMessages.Error_Field0IsRequired, paramLabel];
                    throw new ServiceException(msg);
                }

                betterArgs[param.Key] = value;
            }

            // Call the SP
            return await _behavior.Repository.Lines__Generate(lineDefId, betterArgs, cancellation);
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            var permissions = (await base.UserPermissions(action, cancellation)).ToList();

            // Add a special permission that lets you see the documents that were assigned to you
            permissions.AddRange(DocumentServiceUtil.HardCodedPermissions(action));

            return permissions;
        }

        protected override async Task<EntityQuery<Document>> Search(EntityQuery<Document> query, GetArguments args, CancellationToken cancellation)
        {
            var def = await Definition(cancellation);
            List<(string prefix, int definitionId)> map;
            bool includeInternalRef;
            bool includeExternalRef;
            if (def != null)
            {
                var prefix = def.Prefix;
                map = new List<(string prefix, int definitionId)>
                {
                    (prefix, DefinitionId)
                };
                includeInternalRef = def.InternalReferenceVisibility;
                includeExternalRef = def.ExternalReferenceVisibility;
            }
            else
            {
                map = new List<(string prefix, int definitionId)>();
                includeInternalRef = false;
                includeExternalRef = false;
            }

            return DocumentServiceUtil.SearchImpl(query, args, map, includeInternalRef, includeExternalRef);
        }

        protected override async Task<Extras> GetExtras(IEnumerable<Document> result, CancellationToken cancellation)
        {
            if (IncludeRequiredSignatures)
            {
                // DocumentIds parameter
                var docIds = result.Select(doc => new IdListItem { Id = doc.Id });
                if (!docIds.Any())
                {
                    return await base.GetExtras(result, cancellation);
                }

                var docIdsTable = RepositoryUtilities.DataTable(docIds);
                var docIdsTvp = new SqlParameter("@DocumentIds", docIdsTable)
                {
                    TypeName = $"[dbo].[IdList]",
                    SqlDbType = System.Data.SqlDbType.Structured
                };

                var userId = new SqlParameter("@UserId", UserId);

                var query = _behavior.Repository.EntityQuery<RequiredSignature>()
                    .AdditionalParameters(docIdsTvp, userId)
                    .Expand($"{nameof(RequiredSignature.Role)},{nameof(RequiredSignature.User)},{nameof(RequiredSignature.SignedBy)},{nameof(RequiredSignature.OnBehalfOfUser)},{nameof(RequiredSignature.ProxyRole)}")
                    .OrderBy(nameof(RequiredSignature.LineId));

                var requiredSignatures = await query.ToListAsync(QueryContext, cancellation);

                return new Extras
                {
                    ["RequiredSignatures"] = requiredSignatures
                };
            }
            else
            {
                return await base.GetExtras(result, cancellation);
            }
        }

        protected override async Task<List<DocumentForSave>> SavePreprocessAsync(List<DocumentForSave> docs)
        {
            var docDef = await Definition();

            // Creating new entities forbidden if the definition is archived
            if (docs.Any(e => e?.Id == 0) && docDef.State == DefStates.Archived) // Insert
            {
                var msg = _localizer["Error_DefinitionIsArchived"];
                throw new ServiceException(msg);
            }

            // Get the functional currency Id
            var settings = await _behavior.Settings();
            var functionalId = settings.FunctionalCurrencyId;

            // Get the built in definition Ids
            var defs = await _behavior.Definitions();
            var jvDefId = defs.ManualJournalVouchersDefinitionId;
            var manualLineDefId = defs.ManualLinesDefinitionId;

            bool isJV = DefinitionId == jvDefId;

            // Set default values
            foreach (var (doc, docIndex) in docs.Select((e, i) => (e, i)))
            {
                // Set all IsCommon values that are invisible to FALSE
                if (isJV)
                {
                    // Those are always true in JV
                    doc.PostingDateIsCommon = true;
                    doc.MemoIsCommon = true;
                    doc.CenterIsCommon = false;
                }
                else
                {
                    doc.MemoIsCommon ??= docDef.MemoIsCommonVisibility && (doc.MemoIsCommon ?? false);
                    doc.PostingDateIsCommon ??= docDef.PostingDateIsCommonVisibility && (doc.PostingDateIsCommon ?? false);
                    doc.CenterIsCommon ??= docDef.CenterIsCommonVisibility && (doc.CenterIsCommon ?? false);
                }

                doc.CurrencyIsCommon = docDef.CurrencyVisibility && (doc.CurrencyIsCommon ?? false);
                doc.RelationIsCommon = docDef.RelationVisibility && (doc.RelationIsCommon ?? false);
                doc.ResourceIsCommon = docDef.ResourceVisibility && (doc.ResourceIsCommon ?? false);
                doc.NotedRelationIsCommon = docDef.NotedRelationVisibility && (doc.NotedRelationIsCommon ?? false);
                doc.QuantityIsCommon = docDef.QuantityVisibility && (doc.QuantityIsCommon ?? false);
                doc.UnitIsCommon = docDef.UnitVisibility && (doc.UnitIsCommon ?? false);
                doc.Time1IsCommon = docDef.Time1Visibility && (doc.Time1IsCommon ?? false);
                doc.DurationIsCommon = docDef.DurationVisibility && (doc.DurationIsCommon ?? false);
                doc.DurationUnitIsCommon = docDef.DurationUnitVisibility && (doc.DurationUnitIsCommon ?? false);
                doc.Time2IsCommon = docDef.Time2Visibility && (doc.Time2IsCommon ?? false);
                doc.ExternalReferenceIsCommon = docDef.ExternalReferenceVisibility && (doc.ExternalReferenceIsCommon ?? false);
                doc.ReferenceSourceIsCommon = docDef.ReferenceSourceVisibility && (doc.ReferenceSourceIsCommon ?? false);
                doc.InternalReferenceIsCommon = docDef.InternalReferenceVisibility && (doc.InternalReferenceIsCommon ?? false);

                // In case of a single business unit
                if (settings.SingleBusinessUnitId != null)
                {
                    doc.CenterId = settings.SingleBusinessUnitId;
                }

                // Defaults that make the code simpler later
                doc.Clearance ??= 0; // Public
                doc.Lines ??= new List<LineForSave>();

                doc.Lines.ForEach(line =>
                {
                    // Line defaults
                    line.Entries ??= new List<EntryForSave>();
                    line.Boolean1 ??= false;
                });

                // Remember the indices, comes in handy in the validation later
                doc.EntityMetadata.OriginalIndex = docIndex;

                if (doc.LineDefinitionEntries != null)
                {
                    foreach (var (lineDefEntry, index) in doc.LineDefinitionEntries.Select((e, i) => (e, i)))
                    {
                        if (lineDefEntry != null)
                        {
                            lineDefEntry.EntityMetadata.OriginalIndex = index;
                        }
                    }
                }

                if (doc.Lines != null)
                {
                    foreach (var (line, index) in doc.Lines.Select((e, i) => (e, i)))
                    {
                        if (line != null)
                        {
                            line.EntityMetadata.OriginalIndex = index;

                            if (line.Entries != null)
                            {
                                foreach (var (entry, entryIndex) in line.Entries.Select((e, i) => (e, i)))
                                {
                                    if (entry != null)
                                    {
                                        entry.EntityMetadata.OriginalIndex = entryIndex;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Set common header values on the lines
            docs.ForEach(doc =>
            {
                // All fields that aren't visible and marked as common, set them to null, the UI hides them anyways
                // Those 3 are different than the rest, they can remain visible even when is common = false
                doc.Memo = docDef.MemoVisibility != null ? doc.Memo : null;
                doc.CenterId = docDef.CenterVisibility != null ? doc.CenterId : null;
                doc.PostingDate = docDef.PostingDateVisibility != null ? doc.PostingDate : null;

                doc.CurrencyId = docDef.CurrencyVisibility && doc.CurrencyIsCommon.Value ? doc.CurrencyId : null;

                doc.RelationId = docDef.RelationVisibility && doc.RelationIsCommon.Value ? doc.RelationId : null;
                doc.ResourceId = docDef.ResourceVisibility && doc.ResourceIsCommon.Value ? doc.ResourceId : null;
                doc.NotedRelationId = docDef.NotedRelationVisibility && doc.NotedRelationIsCommon.Value ? doc.NotedRelationId : null;

                doc.Quantity = docDef.QuantityVisibility && doc.QuantityIsCommon.Value ? doc.Quantity : null;
                doc.UnitId = docDef.UnitVisibility && doc.UnitIsCommon.Value ? doc.UnitId : null;
                doc.Time1 = docDef.Time1Visibility && doc.Time1IsCommon.Value ? doc.Time1 : null;
                doc.Duration = docDef.DurationVisibility && doc.DurationIsCommon.Value ? doc.Duration : null;
                doc.DurationUnitId = docDef.DurationUnitVisibility && doc.DurationUnitIsCommon.Value ? doc.DurationUnitId : null;
                doc.Time2 = docDef.Time2Visibility && doc.Time2IsCommon.Value ? doc.Time2 : null;

                doc.ExternalReference = docDef.ExternalReferenceVisibility && doc.ExternalReferenceIsCommon.Value ? doc.ExternalReference : null;
                doc.ReferenceSourceId = docDef.ReferenceSourceVisibility && doc.ReferenceSourceIsCommon.Value ? doc.ReferenceSourceId : null;
                doc.InternalReference = docDef.InternalReferenceVisibility && doc.InternalReferenceIsCommon.Value ? doc.InternalReference : null;

                // Manual lines always inherit memo and posting date from document header (if visible)
                if (docDef.MemoVisibility != null)
                {
                    doc.Lines.ForEach(line =>
                    {
                        if (line.DefinitionId == manualLineDefId)
                        {
                            line.Memo = doc.Memo;
                        }
                    });
                }

                if (docDef.PostingDateVisibility != null)
                {
                    doc.Lines.ForEach(line =>
                    {
                        if (line.DefinitionId == manualLineDefId)
                        {
                            line.PostingDate = doc.PostingDate;
                        }
                    });
                }

                // All fields that are marked as common, copy the common value across to the 
                // lines and entries, we deal with the lines one definitionId at a time
                foreach (var linesGroup in doc.Lines.GroupBy(e => e.DefinitionId.Value))
                {
                    if (!defs.Lines.TryGetValue(linesGroup.Key, out LineDefinitionForClient lineDef))
                    {
                        // Validation takes care of this later on
                        continue;
                    }

                    // Silently remove entries that are out of bounds (they could be a relic from a time when the definition specified more entries)
                    doc.LineDefinitionEntries.RemoveAll(e => e.LineDefinitionId == linesGroup.Key && e.EntryIndex >= lineDef.Entries.Count);

                    var defaultsToForm = lineDef.ViewDefaultsToForm;
                    var tabEntries = new DocumentLineDefinitionEntryForSave[lineDef.Entries.Count];
                    foreach (var tabEntry in doc.LineDefinitionEntries.Where(e => e.LineDefinitionId == linesGroup.Key))
                    {
                        if (tabEntry.EntryIndex < 0)
                        {
                            continue; // Validation takes care of this later
                        }

                        if (tabEntries[tabEntry.EntryIndex.Value] != null)
                        {
                            continue; // Validation takes care of this later
                        }

                        tabEntries[tabEntry.EntryIndex.Value] = tabEntry;
                    }

                    foreach (var line in linesGroup)
                    {
                        // If the number of entries is not the same as the definition specifies, fix that
                        while (line.Entries.Count < lineDef.Entries.Count)
                        {
                            // If less, add the missing entries
                            var entryDef = lineDef.Entries[line.Entries.Count];
                            line.Entries.Add(new EntryForSave { });
                        }

                        // Copy the direction from the definition
                        for (var i = 0; i < line.Entries.Count; i++)
                        {
                            if (line.DefinitionId != manualLineDefId)
                            {
                                line.Entries[i].Direction = lineDef.Entries[i].Direction;
                            }
                        }

                        #region IsCommon Behavior

                        // Copy common values from the headers if they are marked inherits from header
                        // IMPORTANT: Keep in sync with after the SQL preprocess
                        foreach (var colDef in lineDef.Columns)
                        {
                            if (colDef.ColumnName == nameof(Line.Memo))
                            {
                                if (CopyFromDocument(colDef, doc.MemoIsCommon))
                                {
                                    line.Memo = doc.Memo;
                                }
                                else
                                {
                                    var tabEntry = tabEntries.FirstOrDefault() ?? DefaultTabEntryForSave;
                                    if (CopyFromTab(colDef, tabEntry.MemoIsCommon, defaultsToForm))
                                    {
                                        line.Memo = tabEntry.Memo;
                                    }
                                }
                            }
                            else if (colDef.ColumnName == nameof(Line.PostingDate))
                            {
                                if (CopyFromDocument(colDef, doc.PostingDateIsCommon))
                                {
                                    line.PostingDate = doc.PostingDate;
                                }
                                else
                                {
                                    var tabEntry = tabEntries.FirstOrDefault() ?? DefaultTabEntryForSave;
                                    if (CopyFromTab(colDef, tabEntry.PostingDateIsCommon, defaultsToForm))
                                    {
                                        line.PostingDate = tabEntry.PostingDate;
                                    }
                                }
                            }
                            else
                            {
                                if (colDef.EntryIndex >= line.Entries.Count ||
                                    colDef.EntryIndex >= lineDef.Entries.Count ||
                                    colDef.EntryIndex < 0)
                                {
                                    // To avoid index out of bounds exception
                                    continue;
                                }

                                // Copy the common values
                                var entry = line.Entries[colDef.EntryIndex];
                                var tabEntry = tabEntries[colDef.EntryIndex] ?? DefaultTabEntryForSave;

                                switch (colDef.ColumnName)
                                {
                                    case nameof(Entry.CurrencyId):
                                        if (CopyFromDocument(colDef, doc.CurrencyIsCommon))
                                        {
                                            entry.CurrencyId = doc.CurrencyId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.CurrencyIsCommon, defaultsToForm))
                                        {
                                            entry.CurrencyId = tabEntry.CurrencyId;
                                        }
                                        break;

                                    case nameof(Entry.CenterId):
                                        if (CopyFromDocument(colDef, doc.CenterIsCommon))
                                        {
                                            entry.CenterId = doc.CenterId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.CenterIsCommon, defaultsToForm))
                                        {
                                            entry.CenterId = tabEntry.CenterId;
                                        }
                                        break;

                                    case nameof(Entry.RelationId):
                                        if (CopyFromDocument(colDef, doc.RelationIsCommon))
                                        {
                                            entry.RelationId = doc.RelationId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.RelationIsCommon, defaultsToForm))
                                        {
                                            entry.RelationId = tabEntry.RelationId;
                                        }
                                        break;

                                    case nameof(Entry.ResourceId):
                                        if (CopyFromDocument(colDef, doc.ResourceIsCommon))
                                        {
                                            entry.ResourceId = doc.ResourceId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.ResourceIsCommon, defaultsToForm))
                                        {
                                            entry.ResourceId = tabEntry.ResourceId;
                                        }
                                        break;

                                    case nameof(Entry.NotedRelationId):
                                        if (CopyFromDocument(colDef, doc.NotedRelationIsCommon))
                                        {
                                            entry.NotedRelationId = doc.NotedRelationId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.NotedRelationIsCommon, defaultsToForm))
                                        {
                                            entry.NotedRelationId = tabEntry.NotedRelationId;
                                        }
                                        break;

                                    case nameof(Entry.Quantity):
                                        if (CopyFromDocument(colDef, doc.QuantityIsCommon))
                                        {
                                            entry.Quantity = doc.Quantity;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.QuantityIsCommon, defaultsToForm))
                                        {
                                            entry.Quantity = tabEntry.Quantity;
                                        }
                                        break;

                                    case nameof(Entry.UnitId):
                                        if (CopyFromDocument(colDef, doc.UnitIsCommon))
                                        {
                                            entry.UnitId = doc.UnitId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.UnitIsCommon, defaultsToForm))
                                        {
                                            entry.UnitId = tabEntry.UnitId;
                                        }
                                        break;

                                    case nameof(Entry.Time1):
                                        if (CopyFromDocument(colDef, doc.Time1IsCommon))
                                        {
                                            entry.Time1 = doc.Time1;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.Time1IsCommon, defaultsToForm))
                                        {
                                            entry.Time1 = tabEntry.Time1;
                                        }
                                        break;

                                    case nameof(Entry.Duration):
                                        if (CopyFromDocument(colDef, doc.DurationIsCommon))
                                        {
                                            entry.Duration = doc.Duration;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.DurationIsCommon, defaultsToForm))
                                        {
                                            entry.Duration = tabEntry.Duration;
                                        }
                                        break;

                                    case nameof(Entry.DurationUnitId):
                                        if (CopyFromDocument(colDef, doc.DurationUnitIsCommon))
                                        {
                                            entry.DurationUnitId = doc.DurationUnitId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.DurationUnitIsCommon, defaultsToForm))
                                        {
                                            entry.DurationUnitId = tabEntry.DurationUnitId;
                                        }
                                        break;

                                    case nameof(Entry.Time2):
                                        if (CopyFromDocument(colDef, doc.Time2IsCommon))
                                        {
                                            entry.Time2 = doc.Time2;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.Time2IsCommon, defaultsToForm))
                                        {
                                            entry.Time2 = tabEntry.Time2;
                                        }
                                        break;

                                    case nameof(Entry.ExternalReference):
                                        if (CopyFromDocument(colDef, doc.ExternalReferenceIsCommon))
                                        {
                                            entry.ExternalReference = doc.ExternalReference;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.ExternalReferenceIsCommon, defaultsToForm))
                                        {
                                            entry.ExternalReference = tabEntry.ExternalReference;
                                        }
                                        break;

                                    case nameof(Entry.ReferenceSourceId):
                                        if (CopyFromDocument(colDef, doc.ReferenceSourceIsCommon))
                                        {
                                            entry.ReferenceSourceId = doc.ReferenceSourceId;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.ReferenceSourceIsCommon, defaultsToForm))
                                        {
                                            entry.ReferenceSourceId = tabEntry.ReferenceSourceId;
                                        }
                                        break;

                                    case nameof(Entry.InternalReference):
                                        if (CopyFromDocument(colDef, doc.InternalReferenceIsCommon))
                                        {
                                            entry.InternalReference = doc.InternalReference;
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.InternalReferenceIsCommon, defaultsToForm))
                                        {
                                            entry.InternalReference = tabEntry.InternalReference;
                                        }
                                        break;
                                }
                            }
                        }

                        #endregion
                    }
                }
            });

            // SQL server preprocessing
            await _behavior.Repository.Documents__Preprocess(DefinitionId, docs);

            var tabEntryDesc = TypeDescriptor.Get<DocumentLineDefinitionEntryForSave>();

            // C# Processing after SQL
            docs.ForEach(doc =>
            {
                // Lines preprocessing
                doc.Lines.ForEach(line =>
                {
                    line.Entries.ForEach(entry =>
                    {
                        // If currency is functional, make sure that Value = MonetaryValue
                        if (entry.CurrencyId == settings.FunctionalCurrencyId)
                        {
                            if (line.DefinitionId == manualLineDefId)
                            {
                                // Manual lines, the value is always entered by the user
                                entry.MonetaryValue = entry.Value;
                            }
                            else
                            {
                                // Smart lines, the monetary value is always entered by the user
                                entry.Value = entry.MonetaryValue;
                            }
                        }

                        // Always round the value according to the functional currency decimals
                        if (entry.Value != null)
                        {
                            entry.Value = Math.Round(entry.Value.Value, settings.FunctionalCurrencyDecimals);
                        }
                    });
                });

                ////// handle subtle exchange rate rounding bugs. IF...
                // (1) All non-manual entries have the same non-functional currency
                // (2) Monetary Value of non-manual entries is balanced
                // (3) Value of non-manual is not balanced
                // => Take the difference and distribute it evenly on the entries
                if (doc.Lines.Count > 0)
                {
                    var smartEntries = doc.Lines.Where(line => line.DefinitionId != manualLineDefId).SelectMany(line => line.Entries);
                    if (smartEntries.Any())
                    {
                        var currencyId = smartEntries.First().CurrencyId;
                        if (currencyId != functionalId &&
                            smartEntries.All(entry => entry.CurrencyId == currencyId) &&
                            smartEntries.Sum(entry => entry.Direction.Value * (entry.MonetaryValue ?? 0)) == 0)
                        {
                            var valueDifference = smartEntries.Sum(entry => entry.Direction.Value * (entry.MonetaryValue ?? 0));
                            if (valueDifference != 0)
                            {
                                //// This variable will equal the smallest amount that can be represented
                                //// in the functional currency. E.g. for USD it's 0.01
                                //decimal adjustment = 1.0m;
                                //for (byte i = 0; i < settings.FunctionalCurrencyDecimals; i++)
                                //{
                                //    adjustment *= 0.1m;
                                //}

                                //// TODO: 
                                //smartEntries.First().Value += adjustment;
                            }
                        }
                    }
                }
            });

            #region IsCommon Double Check

            // Ensure IsCommon is still honored
            // IMPORTANT: Keep in sync with part before the SQL preprocess
            docs.ForEach(doc =>
            {
                // All fields that are marked as common, copy the common value across to the 
                // lines and entries, we deal with the lines one definitionId at a time
                foreach (var linesGroup in doc.Lines.GroupBy(e => e.DefinitionId.Value))
                {
                    if (!defs.Lines.TryGetValue(linesGroup.Key, out LineDefinitionForClient lineDef))
                    {
                        // Validation takes care of this later on
                        continue;
                    }

                    var defaultsToForm = lineDef.ViewDefaultsToForm;
                    var tabEntries = new DocumentLineDefinitionEntryForSave[lineDef.Entries.Count];
                    foreach (var tabEntry in doc.LineDefinitionEntries.Where(e => e.LineDefinitionId == linesGroup.Key))
                    {
                        if (tabEntry.EntryIndex < 0)
                        {
                            continue; // Validation takes care of this later
                        }

                        if (tabEntries[tabEntry.EntryIndex.Value] != null)
                        {
                            continue; // Validation takes care of this later
                        }

                        tabEntries[tabEntry.EntryIndex.Value] = tabEntry;
                    }

                    foreach (var line in linesGroup)
                    {
                        // Copy common values from the headers if they are marked inherits from header
                        foreach (var colDef in lineDef.Columns)
                        {
                            if (colDef.ColumnName == nameof(Line.Memo))
                            {
                                if (CopyFromDocument(colDef, doc.MemoIsCommon))
                                {
                                    if (line.Memo != doc.Memo)
                                    {
                                        throw new InvalidOperationException($"[Bug] {nameof(doc.MemoIsCommon)}=true, but {nameof(line.Memo)} of line of type {lineDef.TitleSingular} was changed in preprocess from '{doc.Memo}' to '{line.Memo}'");
                                    }
                                }
                                else
                                {
                                    var tabEntry = tabEntries.FirstOrDefault() ?? DefaultTabEntryForSave;
                                    if (CopyFromTab(colDef, tabEntry.MemoIsCommon, defaultsToForm))
                                    {
                                        if (line.Memo != tabEntry.Memo)
                                        {
                                            throw new InvalidOperationException($"[Bug] {nameof(tabEntry.MemoIsCommon)}=true, but {nameof(line.Memo)} of line of type {lineDef.TitleSingular} was changed in preprocess from '{tabEntry.Memo}' to '{line.Memo}'");
                                        }
                                    }
                                }
                            }
                            else if (colDef.ColumnName == nameof(Line.PostingDate))
                            {
                                if (CopyFromDocument(colDef, doc.PostingDateIsCommon))
                                {
                                    line.PostingDate = doc.PostingDate;
                                    if (line.PostingDate != doc.PostingDate)
                                    {
                                        throw new InvalidOperationException($"[Bug] {nameof(doc.PostingDateIsCommon)}=true, but {nameof(line.PostingDate)} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.PostingDate:yyyy-MM-dd} to '{line.PostingDate:yyyy-MM-dd}'");
                                    }
                                }
                                else
                                {
                                    var tabEntry = tabEntries.FirstOrDefault() ?? DefaultTabEntryForSave;
                                    if (CopyFromTab(colDef, tabEntry.PostingDateIsCommon, defaultsToForm))
                                    {
                                        if (line.PostingDate != tabEntry.PostingDate)
                                        {
                                            throw new InvalidOperationException($"[Bug] {nameof(tabEntry.PostingDateIsCommon)}=true, but {nameof(line.PostingDate)} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.PostingDate:yyyy-MM-dd} to '{line.PostingDate:yyyy-MM-dd}'");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (colDef.EntryIndex >= line.Entries.Count ||
                                    colDef.EntryIndex >= lineDef.Entries.Count ||
                                    colDef.EntryIndex < 0)
                                {
                                    // To avoid index out of bounds exception
                                    continue;
                                }

                                // Copy the common values
                                var entry = line.Entries[colDef.EntryIndex];
                                var tabEntry = tabEntries[colDef.EntryIndex] ?? DefaultTabEntryForSave;

                                switch (colDef.ColumnName)
                                {
                                    case nameof(Entry.CurrencyId):
                                        if (CopyFromDocument(colDef, doc.CurrencyIsCommon))
                                        {
                                            if (entry.CurrencyId != doc.CurrencyId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.CurrencyId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.CurrencyId} to {entry.CurrencyId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.CurrencyIsCommon, defaultsToForm))
                                        {
                                            if (entry.CurrencyId != tabEntry.CurrencyId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.CurrencyId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.CurrencyId} to {entry.CurrencyId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.RelationId):
                                        if (CopyFromDocument(colDef, doc.RelationIsCommon))
                                        {
                                            if (entry.RelationId != doc.RelationId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.RelationId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.RelationId} to {entry.RelationId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.RelationIsCommon, defaultsToForm))
                                        {
                                            if (entry.RelationId != tabEntry.RelationId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.RelationId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.RelationId} to {entry.RelationId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.ResourceId):
                                        if (CopyFromDocument(colDef, doc.ResourceIsCommon))
                                        {
                                            if (entry.ResourceId != doc.ResourceId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.ResourceId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.ResourceId} to {entry.ResourceId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.ResourceIsCommon, defaultsToForm))
                                        {
                                            if (entry.ResourceId != tabEntry.ResourceId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.ResourceId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.ResourceId} to {entry.ResourceId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.NotedRelationId):
                                        if (CopyFromDocument(colDef, doc.NotedRelationIsCommon))
                                        {
                                            if (entry.NotedRelationId != doc.NotedRelationId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.NotedRelationId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.NotedRelationId} to {entry.NotedRelationId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.NotedRelationIsCommon, defaultsToForm))
                                        {
                                            if (entry.NotedRelationId != tabEntry.NotedRelationId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.NotedRelationId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.NotedRelationId} to {entry.NotedRelationId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.Quantity):
                                        if (CopyFromDocument(colDef, doc.QuantityIsCommon))
                                        {
                                            if (entry.Quantity != doc.Quantity)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.Quantity)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.Quantity} to {entry.Quantity}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.QuantityIsCommon, defaultsToForm))
                                        {
                                            if (entry.Quantity != tabEntry.Quantity)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.Quantity)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.Quantity} to {entry.Quantity}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.UnitId):
                                        if (CopyFromDocument(colDef, doc.UnitIsCommon))
                                        {
                                            if (entry.UnitId != doc.UnitId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.UnitId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.UnitId} to {entry.UnitId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.UnitIsCommon, defaultsToForm))
                                        {
                                            if (entry.UnitId != tabEntry.UnitId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.UnitId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.UnitId} to {entry.UnitId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.CenterId):
                                        if (CopyFromDocument(colDef, doc.CenterIsCommon))
                                        {
                                            if (entry.CenterId != doc.CenterId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.CenterId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.CenterId} to {entry.CenterId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.CenterIsCommon, defaultsToForm))
                                        {
                                            if (entry.CenterId != tabEntry.CenterId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.CenterId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.CenterId} to {entry.CenterId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.Time1):
                                        if (CopyFromDocument(colDef, doc.Time1IsCommon))
                                        {
                                            if (entry.Time1 != doc.Time1)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.Time1)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.Time1} to {entry.Time1}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.Time1IsCommon, defaultsToForm))
                                        {
                                            if (entry.Time1 != tabEntry.Time1)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.Time1)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.Time1} to {entry.Time1}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.Duration):
                                        if (CopyFromDocument(colDef, doc.DurationIsCommon))
                                        {
                                            if (entry.Duration != doc.Duration)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.Duration)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.Duration} to {entry.Duration}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.DurationIsCommon, defaultsToForm))
                                        {
                                            if (entry.Duration != tabEntry.Duration)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.Duration)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.Duration} to {entry.Duration}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.DurationUnitId):
                                        if (CopyFromDocument(colDef, doc.DurationUnitIsCommon))
                                        {
                                            if (entry.DurationUnitId != doc.DurationUnitId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.DurationUnitId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.DurationUnitId} to {entry.DurationUnitId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.DurationUnitIsCommon, defaultsToForm))
                                        {
                                            if (entry.DurationUnitId != tabEntry.DurationUnitId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.DurationUnitId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.DurationUnitId} to {entry.DurationUnitId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.Time2):
                                        if (CopyFromDocument(colDef, doc.Time2IsCommon))
                                        {
                                            if (entry.Time2 != doc.Time2)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.Time2)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.Time2} to {entry.Time2}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.Time2IsCommon, defaultsToForm))
                                        {
                                            if (entry.Time2 != tabEntry.Time2)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.Time2)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.Time2} to {entry.Time2}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.ExternalReference):
                                        if (CopyFromDocument(colDef, doc.ExternalReferenceIsCommon))
                                        {
                                            if (entry.ExternalReference != doc.ExternalReference)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.ExternalReference)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.ExternalReference} to {entry.ExternalReference}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.ExternalReferenceIsCommon, defaultsToForm))
                                        {
                                            if (entry.ExternalReference != tabEntry.ExternalReference)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.ExternalReference)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.ExternalReference} to {entry.ExternalReference}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.ReferenceSourceId):
                                        if (CopyFromDocument(colDef, doc.ReferenceSourceIsCommon))
                                        {
                                            if (entry.ReferenceSourceId != doc.ReferenceSourceId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.ReferenceSourceId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.ReferenceSourceId} to {entry.ReferenceSourceId}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.ReferenceSourceIsCommon, defaultsToForm))
                                        {
                                            if (entry.ReferenceSourceId != tabEntry.ReferenceSourceId)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.ReferenceSourceId)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.ReferenceSourceId} to {entry.ReferenceSourceId}");
                                            }
                                        }
                                        break;

                                    case nameof(Entry.InternalReference):
                                        if (CopyFromDocument(colDef, doc.InternalReferenceIsCommon))
                                        {
                                            if (entry.InternalReference != doc.InternalReference)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.InternalReference)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {doc.InternalReference} to {entry.InternalReference}");
                                            }
                                        }
                                        else if (CopyFromTab(colDef, tabEntry.InternalReferenceIsCommon, defaultsToForm))
                                        {
                                            if (entry.InternalReference != tabEntry.InternalReference)
                                            {
                                                throw new InvalidOperationException($"[Bug] IsCommon = true, but {nameof(entry.InternalReference)} of EntryIndex = {colDef.EntryIndex} of line of type {lineDef.TitleSingular} was changed in preprocess from {tabEntry.InternalReference} to {entry.InternalReference}");
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            });

            #endregion

            return docs;
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<DocumentForSave> docs, bool returnIds)
        {
            #region Validation

            // Find lines with duplicate Ids
            var duplicateLineIds = docs.SelectMany(doc => doc.Lines) // All lines
                .Where(line => line.Id != 0).GroupBy(line => line.Id).Where(g => g.Count() > 1) // Duplicate Ids
                .SelectMany(g => g).ToDictionary(line => line, line => line.Id); // to dictionary

            // Find entries with duplicate Ids
            var duplicateEntryIds = docs.SelectMany(doc => doc.Lines).SelectMany(line => line.Entries)
                .Where(entry => entry.Id != 0).GroupBy(entry => entry.Id).Where(g => g.Count() > 1)
                .SelectMany(g => g).ToDictionary(entry => entry, entry => entry.Id);

            // Find documents with duplicate Serial numbers
            var duplicateSerialNumbers = docs.Where(doc => doc.SerialNumber > 0)
                .GroupBy(doc => doc.SerialNumber).Where(g => g.Count() > 1).SelectMany(g => g)
                .ToDictionary(doc => doc, doc => doc.SerialNumber.Value);

            var settings = await _behavior.Settings();
            var defs = await _behavior.Definitions();
            var docDef = await Definition();
            var manualLineDefId = defs.ManualLinesDefinitionId;
            var meta = await GetMetadataForSave(cancellation: default);

            ///////// Document Validation
            for (int docIndex = 0; docIndex < docs.Count; docIndex++)
            {
                var doc = docs[docIndex];

                if (!docDef.IsOriginalDocument)
                {
                    // If not an original document, the serial number is required
                    if (doc.SerialNumber == null || doc.SerialNumber == 0)
                    {
                        ModelState.AddError($"[{docIndex}].{nameof(doc.SerialNumber)}",
                            _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Document_SerialNumber"]]);
                    }
                    else if (duplicateSerialNumbers.ContainsKey(doc))
                    {
                        var serial = duplicateSerialNumbers[doc];
                        ModelState.AddError($"[{docIndex}].{nameof(doc.SerialNumber)}",
                            _localizer["Error_TheSerialNumber0IsDuplicated", FormatSerial(serial, docDef.Prefix, docDef.CodeWidth)]);
                    }
                }

                if (doc.PostingDateIsCommon.Value && doc.PostingDate != null)
                {
                    // Date cannot be in the future
                    if (doc.PostingDate > DateTime.Today.AddDays(1))
                    {
                        ModelState.AddError($"[{docIndex}].{nameof(doc.PostingDate)}",
                            _localizer["Error_DateCannotBeInTheFuture"]);
                    }

                    // Date cannot be before archive date
                    if (doc.PostingDate <= settings.ArchiveDate && docDef.DocumentType >= 2)
                    {
                        var calendar = Calendar ?? settings.PrimaryCalendar;
                        var archiveDate = CalendarUtilities.FormatDate(settings.ArchiveDate, _localizer, settings.DateFormat, calendar);
                        ModelState.AddError($"[{docIndex}].{nameof(doc.PostingDate)}",
                            _localizer["Error_DateCannotBeBeforeArchiveDate1", archiveDate]);
                    }
                }

                ////////// LineDefinitionEntries Validation
                if (doc.LineDefinitionEntries != null)
                {
                    // Remove duplicates
                    var duplicateTabEntries = doc.LineDefinitionEntries
                        .GroupBy(e => (e.LineDefinitionId, e.EntryIndex))
                        .Where(g => g.Count() > 1)
                        .SelectMany(g => g)
                        .ToHashSet();

                    // Make sure EntryIndex is not below 0
                    for (int tabEntryIndex = 0; tabEntryIndex < doc.LineDefinitionEntries.Count; tabEntryIndex++)
                    {
                        var tabEntry = doc.LineDefinitionEntries[tabEntryIndex];
                        if (tabEntry.EntryIndex < 0)
                        {
                            var path = $"[{docIndex}].{nameof(doc.LineDefinitionEntries)}[{tabEntryIndex}].{nameof(tabEntry.EntryIndex)}";
                            var msg = "Entry index cannot be negative";
                            ModelState.AddError(path, msg);
                        }

                        if (duplicateTabEntries.Contains(tabEntry))
                        {
                            var path = $"[{docIndex}].{nameof(doc.LineDefinitionEntries)}[{tabEntryIndex}].{nameof(tabEntry.EntryIndex)}";
                            var msg = $"Entry index {tabEntry.EntryIndex} is duplicated for the same LineDefinitionId '{tabEntry.LineDefinitionId}'";
                            ModelState.AddError(path, msg);
                        }
                    }
                }

                // All fields that are marked as common, copy the common value across to the 
                // lines and entries, we deal with the lines one definitionId at a time
                foreach (var linesGroup in doc.Lines.GroupBy(e => e.DefinitionId.Value))
                {
                    // Retrieve the line definition
                    if (!defs.Lines.TryGetValue(linesGroup.Key, out LineDefinitionForClient lineDef)) // We checked earlier if this is null
                    {
                        foreach (var line in linesGroup)
                        {
                            ModelState.AddError(LinePath(docIndex, line.EntityMetadata.OriginalIndex, nameof(Line.DefinitionId)),
                                _localizer["Error_UnknownLineDefinitionId0", line.DefinitionId]);
                        }

                        continue; // No point to keep going
                    }

                    // Collect the line definition entries that belong to this line definition in a neat array

                    var defaultsToForm = lineDef.ViewDefaultsToForm;
                    var tabEntries = new DocumentLineDefinitionEntryForSave[lineDef.Entries.Count];
                    foreach (var tabEntry in doc.LineDefinitionEntries.Where(e => e.LineDefinitionId == linesGroup.Key))
                    {
                        if (tabEntry.EntryIndex >= 0 && tabEntry.EntryIndex < lineDef.Entries.Count)
                        {
                            tabEntries[tabEntry.EntryIndex.Value] = tabEntry;
                        }
                    }

                    foreach (var line in linesGroup)
                    {
                        int lineIndex = line.EntityMetadata.OriginalIndex;

                        // Prevent duplicate line Ids
                        if (duplicateLineIds.ContainsKey(line))
                        {
                            // This error indicates a bug
                            var id = duplicateLineIds[line];
                            ModelState.AddError(LinePath(docIndex, lineIndex, nameof(Line.Id)),
                                _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                        }

                        if (!doc.PostingDateIsCommon.Value && line.PostingDate != null)
                        {
                            // Date cannot be in the future
                            if (line.PostingDate > DateTime.Today.AddDays(1))
                            {
                                ModelState.AddError(LinePath(docIndex, lineIndex, nameof(Line.PostingDate)),
                                    _localizer["Error_DateCannotBeInTheFuture"]);
                            }

                            // Date cannot be before archive date
                            if (line.PostingDate <= settings.ArchiveDate && docDef.DocumentType >= 2)
                            {
                                var calendar = Calendar ?? settings.PrimaryCalendar;
                                var archiveDate = CalendarUtilities.FormatDate(settings.ArchiveDate, _localizer, settings.DateFormat, calendar);
                                ModelState.AddError(LinePath(docIndex, lineIndex, nameof(Line.PostingDate)),
                                    _localizer["Error_DateCannotBeBeforeArchiveDate1", archiveDate]);
                            }
                        }

                        foreach (var entry in line.Entries)
                        {
                            var entryIndex = entry.EntityMetadata.OriginalIndex;

                            // Prevent duplicate entry Ids
                            if (duplicateEntryIds.ContainsKey(entry))
                            {
                                var id = duplicateEntryIds[entry];
                                ModelState.AddError(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.Id)),
                                    _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                            }

                            // Value must be positive
                            if (entry.Value < 0)
                            {
                                string fieldLabel = null;
                                if (line.DefinitionId == manualLineDefId)
                                {
                                    fieldLabel = entry.Direction == -1 ? _localizer["Credit"] : _localizer["Debit"];
                                }
                                else
                                {
                                    var columnDef = lineDef.Columns.FirstOrDefault(e => e.EntryIndex == entryIndex && e.ColumnName == nameof(Entry.Value));
                                    if (columnDef != null)
                                    {
                                        fieldLabel = settings.Localize(columnDef.Label, columnDef.Label2, columnDef.Label3);
                                    }
                                }

                                if (fieldLabel != null)
                                {
                                    ModelState.AddError(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.Value)),
                                        _localizer["Error_TheField0CannotBeNegative", fieldLabel]);
                                }
                            }

                            // MonetaryValue must be positive
                            if (entry.MonetaryValue < 0)
                            {
                                string fieldLabel = null;
                                if (line.DefinitionId == manualLineDefId)
                                {
                                    fieldLabel = _localizer["Entry_MonetaryValue"];
                                }
                                else
                                {
                                    var columnDef = lineDef.Columns.FirstOrDefault(e => e.EntryIndex == entryIndex && e.ColumnName == nameof(Entry.MonetaryValue));
                                    if (columnDef != null)
                                    {
                                        fieldLabel = settings.Localize(columnDef.Label, columnDef.Label2, columnDef.Label3);
                                    }
                                }

                                if (fieldLabel != null)
                                {
                                    ModelState.AddError(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.MonetaryValue)),
                                        _localizer["Error_TheField0CannotBeNegative", fieldLabel]);
                                }
                            }

                            // Quantity must be positive
                            if (entry.Quantity < 0)
                            {
                                if (line.DefinitionId == manualLineDefId)
                                {
                                    var fieldLabel = _localizer["Entry_Quantity"];
                                    var msg = _localizer["Error_TheField0CannotBeNegative", fieldLabel];
                                    var path = EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.Quantity));

                                    ModelState.AddError(path, msg);
                                }
                                else
                                {
                                    var colDef = lineDef.Columns.FirstOrDefault(e => e.EntryIndex == entryIndex && e.ColumnName == nameof(Entry.Quantity));
                                    if (colDef != null)
                                    {
                                        if (CopyFromDocument(colDef, doc.QuantityIsCommon))
                                        {
                                            var fieldLabel = settings.Localize(docDef.QuantityLabel, docDef.QuantityLabel2, docDef.QuantityLabel3) ?? _localizer["Entry_Quantity"];
                                            var msg = _localizer["Error_TheField0CannotBeNegative", fieldLabel];
                                            var path = DocumentPath(docIndex, nameof(Document.Quantity));

                                            ModelState.AddError(path, msg);
                                        }
                                        else
                                        {
                                            var tabEntry = (entryIndex < tabEntries.Length ? tabEntries[entryIndex] : null) ?? DefaultTabEntryForSave;

                                            var fieldLabel = settings.Localize(colDef.Label, colDef.Label2, colDef.Label3) ?? _localizer["Entry_Quantity"];
                                            var msg = _localizer["Error_TheField0CannotBeNegative", fieldLabel];

                                            if (CopyFromTab(colDef, tabEntry.QuantityIsCommon, defaultsToForm))
                                            {
                                                if (tabEntry != DefaultTabEntryForSave) // The default one has no index
                                                {
                                                    var index = tabEntry.EntityMetadata.OriginalIndex;
                                                    var path = LineDefinitionEntryPath(docIndex, index, nameof(Document.Quantity));

                                                    ModelState.AddError(path, msg);
                                                }
                                            }
                                            else
                                            {
                                                var path = EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.Quantity));

                                                ModelState.AddError(path, msg);
                                            }
                                        }
                                    }
                                }
                            }

                            // CenterId is required in Entries table
                            if (entry.CenterId == null)
                            {
                                if (line.DefinitionId == manualLineDefId)
                                {
                                    var fieldLabel = _localizer["Entry_Center"];
                                    var msg = _localizer[ErrorMessages.Error_Field0IsRequired, fieldLabel];
                                    var path = EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.CenterId));

                                    ModelState.AddError(path, msg);
                                }
                                else
                                {
                                    var colDef = lineDef.Columns.FirstOrDefault(e => e.EntryIndex == entryIndex && e.ColumnName == nameof(Entry.CenterId));
                                    if (colDef != null)
                                    {
                                        if (CopyFromDocument(colDef, doc.CenterIsCommon))
                                        {
                                            var fieldLabel = settings.Localize(docDef.CenterLabel, docDef.CenterLabel2, docDef.CenterLabel3) ?? _localizer["Document_Center"];
                                            var msg = _localizer[ErrorMessages.Error_Field0IsRequired, fieldLabel];
                                            var path = DocumentPath(docIndex, nameof(Document.CenterId));

                                            ModelState.AddError(path, msg);
                                        }
                                        else
                                        {
                                            var tabEntry = (entryIndex < tabEntries.Length ? tabEntries[entryIndex] : null) ?? DefaultTabEntryForSave;

                                            var fieldLabel = settings.Localize(colDef.Label, colDef.Label2, colDef.Label3) ?? _localizer["Entry_Center"];
                                            var msg = _localizer[ErrorMessages.Error_Field0IsRequired, fieldLabel];

                                            if (CopyFromTab(colDef, tabEntry.CenterIsCommon, defaultsToForm))
                                            {
                                                if (tabEntry != DefaultTabEntryForSave) // The default one has no index
                                                {
                                                    var index = tabEntry.EntityMetadata.OriginalIndex;
                                                    var path = LineDefinitionEntryPath(docIndex, index, nameof(Document.CenterId));

                                                    ModelState.AddError(path, msg);
                                                }
                                            }
                                            else
                                            {
                                                var path = EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.CenterId));

                                                ModelState.AddError(path, msg);
                                            }
                                        }
                                    }
                                }
                            }

                            // CurrencyId is required in Entries table
                            if (entry.CurrencyId == null)
                            {
                                if (line.DefinitionId == manualLineDefId)
                                {
                                    var fieldLabel = _localizer["Entry_Currency"];
                                    var msg = _localizer[ErrorMessages.Error_Field0IsRequired, fieldLabel];
                                    var path = EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.CurrencyId));

                                    ModelState.AddError(path, msg);
                                }
                                else
                                {
                                    var colDef = lineDef.Columns.FirstOrDefault(e => e.EntryIndex == entryIndex && e.ColumnName == nameof(Entry.CurrencyId));
                                    if (colDef != null)
                                    {
                                        if (CopyFromDocument(colDef, doc.CurrencyIsCommon))
                                        {
                                            var fieldLabel = settings.Localize(docDef.CurrencyLabel, docDef.CurrencyLabel2, docDef.CurrencyLabel3) ?? _localizer["Entry_Currency"];
                                            var msg = _localizer[ErrorMessages.Error_Field0IsRequired, fieldLabel];
                                            var path = DocumentPath(docIndex, nameof(Document.CurrencyId));

                                            ModelState.AddError(path, msg);
                                        }
                                        else
                                        {
                                            var tabEntry = (entryIndex < tabEntries.Length ? tabEntries[entryIndex] : null) ?? DefaultTabEntryForSave;

                                            var fieldLabel = settings.Localize(colDef.Label, colDef.Label2, colDef.Label3) ?? _localizer["Entry_Currency"];
                                            var msg = _localizer[ErrorMessages.Error_Field0IsRequired, fieldLabel];

                                            if (CopyFromTab(colDef, tabEntry.CurrencyIsCommon, defaultsToForm))
                                            {
                                                if (tabEntry != DefaultTabEntryForSave) // The default one has no index
                                                {
                                                    var index = tabEntry.EntityMetadata.OriginalIndex;
                                                    var path = LineDefinitionEntryPath(docIndex, index, nameof(Document.CurrencyId));

                                                    ModelState.AddError(path, msg);
                                                }
                                            }
                                            else
                                            {
                                                var path = EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.CurrencyId));

                                                ModelState.AddError(path, msg);
                                            }
                                        }
                                    }
                                }
                            }

                            // If the currency is functional, value must equal monetary value
                            if (entry.CurrencyId == settings.FunctionalCurrencyId && entry.Value != entry.MonetaryValue)
                            {
                                var currencyName = settings
                                    .Localize(settings.FunctionalCurrencyName,
                                                settings.FunctionalCurrencyName2,
                                                settings.FunctionalCurrencyName3);

                                // TODO: Use the proper field name from definition, instead of "Amount"
                                ModelState.AddError(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.MonetaryValue)),
                                    _localizer["TheAmount0DoesNotMatchTheValue1EvenThoughBothIn2", entry.MonetaryValue ?? 0, entry.Value ?? 0, currencyName]);
                            }
                        }
                    }
                }

                ///////// Attachment Validation
                if (doc.Attachments != null)
                {
                    for (var attIndex = 0; attIndex < doc.Attachments.Count; attIndex++)
                    {
                        var att = doc.Attachments[attIndex];

                        if (att.Id != 0 && att.File != null)
                        {
                            ModelState.AddError($"[{docIndex}].{nameof(doc.Attachments)}[{attIndex}].File",
                                _localizer["Error_OnlyNewAttachmentsCanIncludeFileBytes"]);
                        }

                        if (att.Id == 0 && att.File == null)
                        {
                            ModelState.AddError($"[{docIndex}].{nameof(doc.Attachments)}[{attIndex}].File",
                                _localizer["Error_NewAttachmentsMustIncludeFileBytes"]);
                        }

                        if (att.File != null && att.File.Length == 0)
                        {
                            ModelState.AddError($"[{docIndex}].{nameof(doc.Attachments)}[{attIndex}].File",
                                _localizer["Error_AttachmentCannotBeAnEmptyFile"]);
                        }
                    }
                }
            }

            // Just in case an entry's CurrencyId or CenterId is still null and it was not 
            // captured by model validation because perhaps the field is not visible, we 
            // throw a 400 (otherwise SQL will throw an error upon save since they are NOT NULL)
            if (ModelState.IsValid)
            {
                docs.ForEach(doc =>
                {
                    doc?.Lines?.ForEach(line =>
                    {
                        line?.Entries?.ForEach(entry =>
                        {
                            if (entry?.CurrencyId == null)
                            {
                                var docIndex = doc.EntityMetadata.OriginalIndex;
                                var lineIndex = line.EntityMetadata.OriginalIndex;
                                var entryIndex = entry.EntityMetadata.OriginalIndex;

                                var entryPath = EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.CurrencyId));
                                throw new ServiceException($"[Bug] the CurrencyId for {entryPath} was never set.");
                            }

                            if (entry?.CenterId == null)
                            {
                                var docIndex = doc.EntityMetadata.OriginalIndex;
                                var lineIndex = line.EntityMetadata.OriginalIndex;
                                var entryIndex = entry.EntityMetadata.OriginalIndex;

                                var entryPath = EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.CenterId));
                                throw new ServiceException($"[Bug] the CenterId for {entryPath} was never set.");
                            }
                        });
                    });
                });
            }

            // Finally: Remove empty tab entries
            // Should technically be in preprocess, but it would mess up the validation then
            if (ModelState.IsValid)
            {
                docs.ForEach(doc => doc.LineDefinitionEntries?.RemoveAll(EqualsDefaultTabEntry));
            }

            #endregion

            _blobsToSave = BaseUtil.ExtractAttachments(docs, e => e.Attachments, AttachmentBlobName).ToList();

            // Save the documents
            var (result, notificationInfos, fileIdsToDelete) = await _behavior.Repository.Documents__Save(
                    DefinitionId,
                    documents: docs,
                    returnIds: returnIds,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            // Validation
            AddErrorsAndThrowIfInvalid(result.Errors);

            _notificationInfos = notificationInfos;
            _blobsToDelete = fileIdsToDelete.Select(AttachmentBlobName).ToList();

            // Return the new Ids
            return result.Ids;
        }

        protected override async Task NonTransactionalSideEffectsForSave(List<DocumentForSave> entities, List<Document> data)
        {
            // Notify affected users
            _clientProxy.UpdateInboxStatuses(TenantId, _notificationInfos);

            // Delete the file Ids retrieved earlier if any
            if (_blobsToDelete.Any())
            {
                await _blobService.DeleteBlobsAsync(TenantId, _blobsToDelete);
            }

            // Save new blobs if any
            if (_blobsToSave.Any())
            {
                await _blobService.SaveBlobsAsync(TenantId, _blobsToSave);
            }
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            var (result, fileIdsToDelete) = await _behavior.Repository.Documents__Delete(
                definitionId: DefinitionId,
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            // Non-transactional side effects:
            // (1) Inbox notifications
            _clientProxy.UpdateInboxStatuses(TenantId, result.InboxStatuses);

            // (2) Delete the file Ids retrieved earlier if any
            if (fileIdsToDelete.Any())
            {
                var blobsToDelete = fileIdsToDelete.Select(AttachmentBlobName);
                await _blobService.DeleteBlobsAsync(TenantId, blobsToDelete);
            }
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken _)
        {
            var result = ExpressionOrderBy.Parse($"{nameof(Document.SerialNumber)} desc");
            return Task.FromResult(result);
        }

        private static string AttachmentBlobName(string guid)
        {
            return $"Attachments/{guid}";
        }

        protected override ExpressionSelect ParseSelect(string select)
        {
            if (select == null)
            {
                return null;
            }

            // We provide a shorthand notation for common and huge select
            // strings, this one is usually requested from the document details
            // screen and it contains hundreds of atoms
            string shorthand = "$Details";
            ExpressionSelect result;
            if (select.Contains(shorthand))
            {
                // Use the built in expansion for shorthand
                result = _detailsSelectExpression.Clone();
                select = select.Replace(shorthand, "");

                // Add any additional atoms
                ExpressionSelect remainderExpression = base.ParseSelect(select);
                if (remainderExpression != null)
                {
                    result.AddAll(remainderExpression);
                }
            }
            else
            {
                result = base.ParseSelect(select);
            }

            return result;
        }

        protected override IEnumerable<string> AdditionalSelectForExport()
        {
            // Those are needed to get and set the entity values, even though they are not exported or imported themselves
            yield return $"{nameof(Document.Lines)}.{nameof(Line.DefinitionId)}";
            yield return $"{nameof(Document.LineDefinitionEntries)}.{nameof(DocumentLineDefinitionEntry.EntryIndex)}";
            yield return $"{nameof(Document.LineDefinitionEntries)}.{nameof(DocumentLineDefinitionEntry.LineDefinitionId)}";
        }

        protected override async Task<MappingInfo> GetDefaultMapping(TypeMetadata docMetaForSave, TypeMetadata docMeta, CancellationToken cancellation)
        {
            var defs = await _behavior.Definitions(cancellation);

            // Step #1 - Add the header properties of the document
            var def = await Definition(cancellation);
            var docProps = GetDefaultSimplePropertyMappings(docMetaForSave, docMeta, 0);
            int nextAvailableIndex = docProps.Count;

            // Step #2 - Add the tabs
            var lineDefs = defs.Lines;
            var settings = await _behavior.Settings(cancellation);

            // Some metadata objects to help us later
            var lineMeta = _metadata.GetMetadata(TenantId, typeof(Line));
            var lineMetaForSave = _metadata.GetMetadata(TenantId, typeof(LineForSave));
            var entryMeta = _metadata.GetMetadata(TenantId, typeof(Entry));
            var entryMetaForSave = _metadata.GetMetadata(TenantId, typeof(EntryForSave));
            var tabMeta = _metadata.GetMetadata(TenantId, typeof(DocumentLineDefinitionEntry));
            var tabMetaForSave = _metadata.GetMetadata(TenantId, typeof(DocumentLineDefinitionEntryForSave));

            var linesCollectionPropertyMeta = docMeta.CollectionProperty(nameof(Document.Lines));
            var linesCollectionPropertyMetaForSave = docMetaForSave.CollectionProperty(nameof(Document.Lines));
            var entriesCollectionPropertyMeta = lineMeta.CollectionProperty(nameof(Line.Entries));
            var entriesCollectionPropertyMetaForSave = lineMetaForSave.CollectionProperty(nameof(Line.Entries));

            string selectPrefixForTabHeaderProperties = nameof(Document.LineDefinitionEntries);
            string selectPrefixForSmartEntries = nameof(Line.Entries);
            string selectForManualLines = $"{nameof(Document.Lines)}.{nameof(Line.Entries)}";

            var tabFkNames = tabMeta.NavigationProperties.ToDictionary(e => e.ForeignKey.Descriptor.Name);
            var entryFkNames = entryMeta.NavigationProperties.ToDictionary(e => e.ForeignKey.Descriptor.Name);
            var lineFkNames = lineMeta.NavigationProperties.ToDictionary(e => e.ForeignKey.Descriptor.Name);

            var tabMappings = new List<MappingInfo>();
            foreach (var lineDefId in def.LineDefinitions.Select(e => e.LineDefinitionId).Where(id => lineDefs.ContainsKey(id)))
            {
                var lineDef = lineDefs[lineDefId];
                string TabDisplay() => settings.Localize(lineDef.TitlePlural, lineDef.TitlePlural2, lineDef.TitlePlural3);

                // Special handling for manual lines
                if (lineDefId == defs.ManualLinesDefinitionId)
                {
                    var adjustmentSimpleProps = GetDefaultSimplePropertyMappings(entryMetaForSave, entryMeta, nextAvailableIndex);
                    nextAvailableIndex += adjustmentSimpleProps.Count;
                    tabMappings.Add(new MappingInfo(entryMetaForSave, entryMeta, adjustmentSimpleProps, new List<MappingInfo>(), entriesCollectionPropertyMetaForSave, entriesCollectionPropertyMeta)
                    {
                        // Like onNewManualEntry
                        CreateEntity = () => new Entry { Id = 0, Direction = 1 },
                        GetEntitiesForRead = (Entity entity) =>
                        {
                            var doc = entity as Document;
                            return doc.Lines.Where(e => e.DefinitionId == lineDefId).SelectMany(e => e.Entries);
                        },
                        GetOrCreateListForSave = (Entity entity) =>
                        {
                            var doc = entity as DocumentForSave;
                            if (doc.EntityMetadata.ManualLine is not LineForSave manualLine)
                            {
                                manualLine = doc.Lines.FirstOrDefault(e => e.DefinitionId == lineDefId);
                                if (manualLine == null)
                                {
                                    manualLine = new LineForSave
                                    {
                                        DefinitionId = lineDefId,
                                        Entries = new List<EntryForSave>()
                                    };

                                    doc.Lines.Add(manualLine);
                                }

                                doc.EntityMetadata.ManualLine = manualLine;
                            }

                            return manualLine.Entries;
                        },
                        Display = DefinitionId == defs.ManualJournalVouchersDefinitionId ? () => _localizer["Entries"] : TabDisplay,
                        Select = selectForManualLines,
                    }); ;

                    continue;
                }

                // First: add the tab headers
                if (!lineDef.ViewDefaultsToForm)
                {
                    foreach (var column in lineDef.Columns.Where(e => e.InheritsFromHeader > InheritsFrom.None))
                    {
                        if (column.ReadOnlyState <= 0)
                        {
                            continue; // Those are readonly and most likely auto-computed
                        }

                        var colName = column.ColumnName;
                        var entryIndex = IsLineColumn(colName) ? 0 : column.EntryIndex;

                        var propMeta = tabMeta.Property(colName);
                        var propMetaForSave = tabMetaForSave.Property(colName);

                        Entity GetOrCreateEntityForSave(Entity entity)
                        {
                            var doc = entity as DocumentForSave;
                            var tabEntry = doc.LineDefinitionEntries.FirstOrDefault(e => e.LineDefinitionId == lineDefId && e.EntryIndex == entryIndex);
                            if (tabEntry == null)
                            {
                                tabEntry = MakeDefaultTabEntryForSave();
                                tabEntry.EntryIndex = entryIndex;
                                tabEntry.LineDefinitionId = lineDefId;
                                tabEntry.EntityMetadata.BaseEntity = doc;
                                doc.LineDefinitionEntries.Add(tabEntry);
                            }

                            return tabEntry;
                        }

                        Entity GetEntityForRead(Entity entity)
                        {
                            var doc = entity as Document;
                            var tabEntry = doc.LineDefinitionEntries.FirstOrDefault(e => e.LineDefinitionId == lineDefId && e.EntryIndex == entryIndex) ?? DefaultTabEntry;
                            return tabEntry;
                        }

                        string Display() => $"{TabDisplay()}: {settings.Localize(column.Label, column.Label2, column.Label3)}";
                        int index = nextAvailableIndex++;

                        if (tabFkNames.TryGetValue(colName, out NavigationPropertyMetadata navPropMetadata))
                        {
                            // Foreign Key
                            var keyPropMetadata = navPropMetadata.TargetTypeMetadata.SuggestedUserKeyProperty;
                            docProps.Add(new ForeignKeyMappingInfo(propMeta, propMetaForSave, navPropMetadata, keyPropMetadata)
                            {
                                Display = Display,
                                Index = index,
                                SelectPrefix = selectPrefixForTabHeaderProperties,
                                GetTerminalEntityForSave = GetOrCreateEntityForSave,
                                GetTerminalEntityForRead = GetEntityForRead,
                            });
                        }
                        else
                        {
                            // Scalar Property
                            docProps.Add(new PropertyMappingInfo(propMeta, propMetaForSave)
                            {
                                Display = Display,
                                Index = index,
                                SelectPrefix = selectPrefixForTabHeaderProperties,
                                GetTerminalEntityForSave = GetOrCreateEntityForSave,
                                GetTerminalEntityForRead = GetEntityForRead,
                            });
                        }

                        // Property Is Common
                        var isCommonPropName = (colName.EndsWith("Id") ? colName[0..^2] : colName) + "IsCommon";
                        var propIsCommonMeta = tabMeta.Property(isCommonPropName); // What about UnitId
                        var propIsCommonMetaForSave = tabMetaForSave.Property(isCommonPropName);
                        docProps.Add(new PropertyMappingInfo(propIsCommonMeta, propIsCommonMetaForSave)
                        {
                            Index = nextAvailableIndex++,
                            Display = () => $"{TabDisplay()}: {_localizer["Field0IsCommon", settings.Localize(column.Label, column.Label2, column.Label3)]}",
                            SelectPrefix = selectPrefixForTabHeaderProperties,
                            GetTerminalEntityForSave = GetOrCreateEntityForSave,
                            GetTerminalEntityForRead = GetEntityForRead,
                        });
                    }
                }

                // Second: add the line properties
                var pivotedLineProps = new List<PropertyMappingInfo>(); // Flattens out the entry properties with the line properties, just like the UI screen
                foreach (var column in lineDef.Columns)
                {
                    if (column.ReadOnlyState <= 0)
                    {
                        continue; // Those are readonly and most likely auto-computed
                    }

                    var colName = column.ColumnName;
                    string Display() => settings.Localize(column.Label, column.Label2, column.Label3);
                    int index = nextAvailableIndex++;
                    if (IsLineColumn(colName))
                    {
                        // Line Properties
                        var propMeta = lineMeta.Property(colName);
                        var propMetaForSave = lineMetaForSave.Property(colName);
                        if (lineFkNames.TryGetValue(colName, out NavigationPropertyMetadata navPropMetadata))
                        {
                            var keyPropMetadata = navPropMetadata.TargetTypeMetadata.SuggestedUserKeyProperty;
                            pivotedLineProps.Add(new ForeignKeyMappingInfo(propMeta, propMetaForSave, navPropMetadata, keyPropMetadata)
                            {
                                Index = index,
                                Display = Display,
                            });
                        }
                        else
                        {
                            pivotedLineProps.Add(new PropertyMappingInfo(propMeta, propMetaForSave)
                            {
                                Index = index,
                                Display = Display
                            });
                        }
                    }
                    else
                    {
                        // Entry Properties
                        var propMeta = entryMeta.Property(colName);
                        var propMetaForSave = entryMetaForSave.Property(colName);

                        Entity GetEntityForRead(Entity entity)
                        {
                            var line = entity as Line;
                            if (line.Entries.Count > column.EntryIndex)
                            {
                                return line.Entries[column.EntryIndex];
                            }
                            else
                            {
                                return null; // Should return a default entry
                            }
                        }

                        Entity GetOrCreateEntityForSave(Entity entity)
                        {
                            var line = entity as LineForSave;
                            if (line.Entries.Count > column.EntryIndex)
                            {
                                return line.Entries[column.EntryIndex];
                            }
                            else
                            {
                                return null; // Should return a default entry
                            }
                        }

                        if (entryFkNames.TryGetValue(colName, out NavigationPropertyMetadata navPropMetadata))
                        {
                            var keyPropMetadata = navPropMetadata.TargetTypeMetadata.SuggestedUserKeyProperty;
                            pivotedLineProps.Add(new ForeignKeyMappingInfo(propMeta, propMetaForSave, navPropMetadata, keyPropMetadata)
                            {
                                Index = index,
                                Display = Display,
                                SelectPrefix = selectPrefixForSmartEntries,
                                GetTerminalEntityForRead = GetEntityForRead,
                                GetTerminalEntityForSave = GetOrCreateEntityForSave
                            });
                        }
                        else
                        {
                            pivotedLineProps.Add(new PropertyMappingInfo(propMeta, propMetaForSave)
                            {
                                Index = index,
                                Display = Display,
                                SelectPrefix = selectPrefixForSmartEntries,
                                GetTerminalEntityForRead = GetEntityForRead,
                                GetTerminalEntityForSave = GetOrCreateEntityForSave
                            });
                        }
                    }
                }

                tabMappings.Add(new MappingInfo(lineMetaForSave, lineMeta, pivotedLineProps, new List<MappingInfo>(), linesCollectionPropertyMetaForSave, linesCollectionPropertyMeta)
                {
                    CreateEntity = () =>
                    {
                        var line = new LineForSave
                        {
                            DefinitionId = lineDefId,
                            Boolean1 = false,
                            Entries = new List<EntryForSave>(),
                        };

                        foreach (var entry in lineDef.Entries)
                        {
                            line.Entries.Add(new Entry
                            {
                                Id = 0,
                                Direction = entry.Direction,
                                Value = 0,

                                EntityMetadata = new EntityMetadata { BaseEntity = line }
                            });
                        }

                        return line;
                    },
                    GetEntitiesForRead = (Entity entity) =>
                    {
                        var doc = entity as Document;
                        return doc.Lines.Where(e => e.DefinitionId == lineDefId);
                    },
                    Display = TabDisplay
                });
            }

            return new MappingInfo(docMetaForSave, docMeta, docProps, tabMappings, null, null)
            {
                CreateEntity = () => new DocumentForSave
                {
                    PostingDateIsCommon = true,
                    MemoIsCommon = true,
                    CurrencyIsCommon = true,
                    CenterIsCommon = true,

                    RelationIsCommon = true,
                    ResourceIsCommon = true,
                    NotedRelationIsCommon = true,

                    QuantityIsCommon = true,
                    UnitIsCommon = true,
                    Time1IsCommon = true,
                    DurationIsCommon = true,
                    DurationUnitIsCommon = true,
                    Time2IsCommon = true,

                    ExternalReferenceIsCommon = true,
                    ReferenceSourceIsCommon = true,
                    InternalReferenceIsCommon = true,

                    LineDefinitionEntries = new List<DocumentLineDefinitionEntryForSave>(),
                    Lines = new List<LineForSave>(),
                },
            };
        }

        #region Helper Functions

        private static bool CopyFromDocument(LineDefinitionColumnForClient colDef, bool? docIsCommon)
        {
            return colDef.InheritsFromHeader >= InheritsFrom.DocumentHeader && (docIsCommon ?? false);
        }

        private static bool CopyFromTab(LineDefinitionColumnForClient colDef, bool? tabIsCommon, bool isForm)
        {
            return !isForm && colDef.InheritsFromHeader >= InheritsFrom.TabHeader && (tabIsCommon ?? false);
        }

        private static string FormatSerial(int serial, string prefix, int codeWidth)
        {
            var result = serial.ToString();
            if (result.Length < codeWidth)
            {
                result = "00000000000000000".Substring(0, codeWidth - result.Length) + result;
            }

            if (!string.IsNullOrWhiteSpace(prefix))
            {
                result = prefix + result;
            }

            return result;
        }

        private static string DocumentPath(int docIndex, string propName)
        {
            return $"[{docIndex}].{propName}";
        }

        private static string LineDefinitionEntryPath(int docIndex, int index, string propName)
        {
            return $"[{docIndex}].{nameof(Document.LineDefinitionEntries)}[{index}].{propName}";
        }

        private static string EntryPath(int docIndex, int lineIndex, int entryIndex, string propName)
        {
            return $"[{docIndex}].{nameof(Document.Lines)}[{lineIndex}].{nameof(Line.Entries)}[{entryIndex}].{propName}";
        }

        private static string LinePath(int docIndex, int lineIndex, string propName)
        {
            return $"[{docIndex}].{nameof(Document.Lines)}[{lineIndex}].{propName}";
        }

        private static bool IsLineColumn(string colName)
        {
            return colName switch
            {
                nameof(Line.Memo) or
                nameof(Line.PostingDate) or
                nameof(Line.Boolean1) or
                nameof(Line.Decimal1) or
                nameof(Line.Text1) => true,
                _ => false,
            };
        }

        #endregion
    }

    public class DocumentsGenericService : FactWithIdServiceBase<Document, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IPermissionsCache _permissionsCache;

        public DocumentsGenericService(
            ApplicationFactServiceBehavior behavior,
            FactServiceDependencies deps,
            IPermissionsCache permissionsCache) : base(deps)
        {
            _behavior = behavior;
            _permissionsCache = permissionsCache;
        }

        protected override string View => throw new NotImplementedException(); // We override user permissions

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            // Get all permissions pertaining to documents
            string prefix = "documents/";
            var permissions = (await _permissionsCache
                .GenericPermissionsFromCache(
                    tenantId: _behavior.TenantId,
                    userId: UserId,
                    version: _behavior.PermissionsVersion,
                    viewPrefix: prefix,
                    action: action,
                    cancellation: cancellation)).ToList();

            // Massage the permissions by adding definitionId = {definitionId} as an extra clause 
            // (since the controller will not filter the results per any specific definition Id)
            foreach (var permission in permissions.Where(e => e.View != "all"))
            {
                string definitionIdString = permission.View.Remove(0, prefix.Length);
                if (!int.TryParse(definitionIdString, out int definitionId))
                {
                    throw new ServiceException($"Could not parse definition Id '{definitionIdString}' to a valid integer.");
                }

                string definitionPredicate = $"{nameof(Document.DefinitionId)} eq {definitionId}";
                if (!string.IsNullOrWhiteSpace(permission.Criteria))
                {
                    permission.Criteria = $"{definitionPredicate} and ({permission.Criteria})";
                }
                else
                {
                    permission.Criteria = definitionPredicate;
                }
            }

            // Add a special permission that lets you see the documents that were assigned to you
            permissions.AddRange(DocumentServiceUtil.HardCodedPermissions(action));

            // Return the massaged permissions
            return permissions;
        }

        protected override async Task<EntityQuery<Document>> Search(EntityQuery<Document> query, GetArguments args, CancellationToken cancellation)
        {
            // Get a map from all serial prefixes to definitionIds
            var defs = await _behavior.Definitions(cancellation);
            var prefixMap = defs.Documents.Select(e => (e.Value.Prefix, e.Key)); // Select all (Serial Prefix, DefinitionId)

            return DocumentServiceUtil.SearchImpl(query, args, prefixMap, false, false);
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken _)
        {
            var result = ExpressionOrderBy.Parse($"{nameof(Document.PostingDate)} desc");
            return Task.FromResult(result);
        }
    }

    internal class DocumentServiceUtil
    {
        /// <summary>
        /// This is needed in both the generic and specific services, so we move it out here.
        /// </summary>
        internal static EntityQuery<Document> SearchImpl(EntityQuery<Document> query, GetArguments args, IEnumerable<(string Prefix, int DefinitionId)> prefixMap, bool includeInternalRef, bool includeExternalRef)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                // IF: the search starts with the serial prefix (case insensitive) then search the serial numbers exclusively 
                var searchLower = search.Trim().ToLower();
                var (prefix, definitionId) = prefixMap.FirstOrDefault(e =>
                    !string.IsNullOrWhiteSpace(e.Prefix) &&
                    searchLower.StartsWith(e.Prefix.ToLower()) &&
                    searchLower.Length > e.Prefix.Length);

                if (definitionId != 0 && int.TryParse(searchLower.Remove(0, prefix.Length), out int serial))
                {
                    var serialNumberProp = nameof(Document.SerialNumber);
                    var definitionIdProp = nameof(Document.DefinitionId);

                    // Prepare the filter string
                    var filterString = $"{serialNumberProp} eq {serial} and {definitionIdProp} eq {definitionId}";

                    // Apply the filter
                    query = query.Filter(filterString);
                }

                // ELSE: search the memo, posting date, etc normally
                else
                {
                    search = search.Replace("'", "''"); // escape quotes by repeating them


                    // Prepare the filter string
                    var filterString = $"{nameof(Document.Memo)} contains '{search}'";

                    // If the search is a number, include documents with that serial number
                    if (int.TryParse(search.Trim(), out int searchNumber))
                    {
                        filterString = $"{filterString} or {nameof(Document.SerialNumber)} eq {searchNumber}";
                    }

                    // If the search is a date, include documents with that date
                    if (DateTime.TryParse(search.Trim(), out DateTime searchDate))
                    {
                        filterString = $"{filterString} or {nameof(Document.PostingDate)} eq '{searchDate:yyyy-MM-dd}'";
                    }

                    if (includeInternalRef)
                    {
                        filterString = $"{filterString} or {nameof(Document.InternalReference)} contains '{search}'";
                    }

                    if (includeExternalRef)
                    {
                        filterString = $"{filterString} or {nameof(Document.ExternalReference)} contains '{search}'";
                    }

                    // Apply the filter
                    query = query.Filter(ExpressionFilter.Parse(filterString));
                }
            }

            return query;
        }

        /// <summary>
        /// The permission to read a document that is in your inbox is universal. 
        /// Otherwise you would think your inbox is empty when it is not.
        /// </summary>
        internal static IEnumerable<AbstractPermission> HardCodedPermissions(string action)
        {
            if (action == PermissionActions.Read)
            {
                // If someone assigns the document to you, you can read it
                // and forward it to someone else, until it either gets
                // forwarded again (the second condition so that you can 
                // refresh the document immediately after forwarding)
                yield return new AbstractPermission
                {
                    View = "documents", // Not important
                    Action = PermissionActions.Read,
                    Criteria = "AssigneeId eq me OR AssignedById eq me"
                };
            }
        }
    }
}
