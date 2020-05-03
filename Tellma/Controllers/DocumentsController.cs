using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.BlobStorage;
using Tellma.Services.ClientInfo;
using Tellma.Services.MultiTenancy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using System.Data.SqlClient;
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS + "{definitionId}")]
    [ApplicationController]
    public class DocumentsController : CrudControllerBase<DocumentForSave, Document, int>
    {
        public const string BASE_ADDRESS = "documents/";

        private readonly DocumentsService _service;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(DocumentsService service, ILogger<DocumentsController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{docId}/attachments/{attachmentId}")]
        public async Task<ActionResult> GetAttachment(int docId, int attachmentId, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var (fileBytes, fileName) = await _service.GetAttachment(docId, attachmentId, cancellation);
                var contentType = ContentType(fileName);
                return File(fileContents: fileBytes, contentType: contentType, fileName);
            }, _logger);
        }

        private string ContentType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }

        [HttpPut("assign")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Assign([FromBody] List<int> ids, [FromQuery] AssignArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Assign(ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

                if (args.ReturnEntities ?? false)
                {
                    return Ok(response);
                }
                else
                {
                    return Ok();
                }
            }
            , _logger);
        }

        [HttpPut("sign-lines")]
        public async Task<ActionResult<EntitiesResponse<Document>>> SignLines([FromBody] List<int> lineIds, [FromQuery] SignArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.SignLines(lineIds, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

                if (args.ReturnEntities ?? false)
                {
                    return Ok(response);
                }
                else
                {
                    return Ok();
                }
            }
            , _logger);
        }

        [HttpPut("unsign-lines")]
        public async Task<ActionResult<EntitiesResponse<Document>>> UnsignLines([FromBody] List<int> signatureIds, [FromQuery] ActionArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.UnsignLines(signatureIds, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

                if (args.ReturnEntities ?? false)
                {
                    return Ok(response);
                }
                else
                {
                    return Ok();
                }
            }
            , _logger);
        }

        [HttpPut("post")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Post([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Post(ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

                if (args.ReturnEntities ?? false)
                {
                    return Ok(response);
                }
                else
                {
                    return Ok();
                }
            }, _logger);
        }

        [HttpPut("unpost")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Unpost([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Unpost(ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

                if (args.ReturnEntities ?? false)
                {
                    return Ok(response);
                }
                else
                {
                    return Ok();
                }
            }, _logger);
        }

        [HttpPut("cancel")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Cancel([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Cancel(ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

                if (args.ReturnEntities ?? false)
                {
                    return Ok(response);
                }
                else
                {
                    return Ok();
                }
            }, _logger);
        }

        [HttpPut("uncancel")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Uncancel([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Uncancel(ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);

                if (args.ReturnEntities ?? false)
                {
                    return Ok(response);
                }
                else
                {
                    return Ok();
                }
            }, _logger);
        }

        protected override CrudServiceBase<DocumentForSave, Document, int> GetCrudService()
        {
            return _service;
        }

        protected override Extras TransformExtras(Extras extras, CancellationToken cancellation)
        {
            if (extras != null && extras.TryGetValue("RequiredSignatures", out object requiredSignaturesObj)) {
                var requiredSignatures = requiredSignaturesObj as List<RequiredSignature>;

                var relatedEntities = FlattenAndTrim(requiredSignatures, cancellation);
                requiredSignatures.ForEach(rs => rs.EntityMetadata = null); // Smaller response size

                extras["RequiredSignaturesRelatedEntities"] = relatedEntities;
            }

            return extras;
        }
    }

    public class DocumentsService : CrudServiceBase<DocumentForSave, Document, int>
    {
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;
        private readonly ITenantIdAccessor _tenantIdAccessor;
        private readonly IBlobService _blobService;
        private readonly IDefinitionsCache _definitionsCache;
        private readonly ISettingsCache _settingsCache;
        private readonly IClientInfoAccessor _clientInfo;
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ITenantInfoAccessor _tenantInfoAccessor;
        private readonly IHubContext<ServerNotificationsHub, INotifiedClient> _hubContext;
        private readonly IHttpContextAccessor _contextAccessor;

        private string ManualJournalVouchers => "manual-journal-vouchers";
        private string ManualLine => "ManualLine";
        private string Lines => "Lines";
        private string Entries => "Entries";

        public DocumentsService(IStringLocalizer<Strings> localizer,
            ApplicationRepository repo, ITenantIdAccessor tenantIdAccessor, IBlobService blobService,
            IDefinitionsCache definitionsCache, ISettingsCache settingsCache, IClientInfoAccessor clientInfo,
            IModelMetadataProvider modelMetadataProvider, ITenantInfoAccessor tenantInfoAccessor,
            IHubContext<ServerNotificationsHub, INotifiedClient> hubContext, IHttpContextAccessor contextAccessor) : base(localizer)
        {
            _localizer = localizer;
            _repo = repo;
            _tenantIdAccessor = tenantIdAccessor;
            _tenantIdAccessor = tenantIdAccessor;
            _blobService = blobService;
            _definitionsCache = definitionsCache;
            _settingsCache = settingsCache;
            _clientInfo = clientInfo;
            _modelMetadataProvider = modelMetadataProvider;
            _tenantInfoAccessor = tenantInfoAccessor;
            _hubContext = hubContext;
            _contextAccessor = contextAccessor;
        }


        #region Context Params

        private bool? _includeRequiredSignaturesOverride;
        private string _definitionIdOverride;
        private int TenantId => _tenantIdAccessor.GetTenantId(); // Syntactic sugar

        private string DefinitionId => _definitionIdOverride ??
            _contextAccessor.HttpContext?.Request?.RouteValues?.GetValueOrDefault("definitionId")?.ToString() ??
            throw new BadRequestException($"Bug: DefinitoinId could not be determined in {nameof(DocumentsService)}");
        private bool IncludeRequiredSignatures =>
            _includeRequiredSignaturesOverride ?? GetQueryParameter("includeRequiredSignatures")?.ToLower() == "true";

        private string GetQueryParameter(string name)
        {
            var query = _contextAccessor.HttpContext?.Request?.Query;
            if (query != null && query.TryGetValue(name, out StringValues value))
            {
                return value.FirstOrDefault();
            }

            return null;
        }

        private string View => $"{DocumentsController.BASE_ADDRESS}{DefinitionId}";

        private DocumentDefinitionForClient Definition() => _definitionsCache.GetCurrentDefinitionsIfCached()?.Data?.Documents?
            .GetValueOrDefault(DefinitionId) ?? throw new InvalidOperationException($"Definition for '{DefinitionId}' was missing from the cache");

        #endregion

        #region State & Workflow

        public async Task<(List<Document>, Extras)> Assign(List<int> ids, [FromQuery] AssignArguments args)
        {
            // User permissions
            // TODO: Check the user can read the document
            await CheckActionPermissions("Read", ids);

            // C# Validation 
            // Goes here

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();

            // Validate
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var errors = await _repo.Documents_Validate__Assign(
                ids,
                args.AssigneeId,
                args.Comment,
                top: remainingErrorCount
                );

            ControllerUtilities.AddLocalizedErrors(ModelState, errors, _localizer);
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            // Actual Assignment
            var notificationInfos = await _repo.Documents__Assign(ids, args.AssigneeId, args.Comment, recordInHistory: true);

            // Notify relevant parties
            await _hubContext.NotifyInboxAsync(TenantId, notificationInfos);

            // Return result
            if (args.ReturnEntities ?? false)
            {
                var response = await GetByIds(ids, args, cancellation: default);

                trx.Complete();
                return response;
            }
            else
            {
                trx.Complete();
                return default;
            }

        }

        public async Task<(List<Document>, Extras)> SignLines(List<int> lineIds, SignArguments args)
        {
            var returnEntities = args.ReturnEntities ?? false;

            // C# Validation 
            // Goes here

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();

            // Validate
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var errors = await _repo.Lines_Validate__Sign(
                lineIds,
                args.OnBehalfOfUserId,
                args.RuleType,
                args.RoleId,
                args.ToState,
                top: remainingErrorCount
                );

            ControllerUtilities.AddLocalizedErrors(ModelState, errors, _localizer);
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            // Sign
            var documentIds = await _repo.Lines__SignAndRefresh(
                lineIds,
                args.ToState,
                args.ReasonId,
                args.ReasonDetails,
                args.OnBehalfOfUserId,
                args.RuleType,
                args.RoleId,
                args.SignedAt ?? DateTimeOffset.Now,
                returnIds: returnEntities);

            if (returnEntities)
            {
                var response = await GetByIds(documentIds.ToList(), args, cancellation: default);

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
            var returnEntities = args.ReturnEntities ?? false;

            // C# Validation 
            // Goes here

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();

            // Validate
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var errors = await _repo.LineSignatures_Validate__Delete(signatureIds, top: remainingErrorCount);
            ControllerUtilities.AddLocalizedErrors(ModelState, errors, _localizer);

            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            // Unsign
            var documentIds = await _repo.LineSignatures__DeleteAndRefresh(signatureIds, returnIds: returnEntities);
            if (returnEntities)
            {
                var response = await GetByIds(documentIds.ToList(), args, cancellation: default);

                trx.Complete();
                return response;
            }
            else
            {
                trx.Complete();
                return default;
            }
        }

        public async Task<(List<Document>, Extras)> Post(List<int> ids, [FromQuery] ActionArguments args)
        {
            return await UpdateDocumentState(ids, args, nameof(Post));
        }

        public async Task<(List<Document>, Extras)> Unpost(List<int> ids, [FromQuery] ActionArguments args)
        {
            return await UpdateDocumentState(ids, args, nameof(Unpost));
        }

        public async Task<(List<Document>, Extras)> Cancel(List<int> ids, [FromQuery] ActionArguments args)
        {
            return await UpdateDocumentState(ids, args, nameof(Cancel));
        }

        public async Task<(List<Document>, Extras)> Uncancel(List<int> ids, [FromQuery] ActionArguments args)
        {
            return await UpdateDocumentState(ids, args, nameof(Uncancel));
        }

        private async Task<(List<Document>, Extras)> UpdateDocumentState([FromBody] List<int> ids, [FromQuery] ActionArguments args, string transition)
        {
            // Check user permissions
            await CheckActionPermissions("State", ids);

            // C# Validation 
            // TODO

            // Transaction
            using var trx = ControllerUtilities.CreateTransaction();

            // Validate
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var errors = transition switch
            {
                nameof(Post) => await _repo.Documents_Validate__Post(DefinitionId, ids, top: remainingErrorCount),
                nameof(Unpost) => await _repo.Documents_Validate__Unpost(DefinitionId, ids, top: remainingErrorCount),
                nameof(Cancel) => await _repo.Documents_Validate__Cancel(DefinitionId, ids, top: remainingErrorCount),
                nameof(Uncancel) => await _repo.Documents_Validate__Uncancel(DefinitionId, ids, top: remainingErrorCount),
                _ => throw new BadRequestException($"Unknown transition {transition}"),
            };

            ControllerUtilities.AddLocalizedErrors(ModelState, errors, _localizer);
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            var notificationInfos = transition switch
            {
                nameof(Post) => await _repo.Documents__Post(ids),
                nameof(Unpost) => await _repo.Documents__Unpost(ids),
                nameof(Cancel) => await _repo.Documents__Cancel(ids),
                nameof(Uncancel) => await _repo.Documents__Uncancel(ids),
                _ => throw new BadRequestException($"Unknown transition {transition}"),
            };

            await _hubContext.NotifyInboxAsync(TenantId, notificationInfos);

            if (args.ReturnEntities ?? false)
            {
                var response = await GetByIds(ids, args, cancellation: default);

                trx.Complete();
                return response;
            }
            else
            {
                trx.Complete();
                return default;
            }
        }

        #endregion

        public async Task<(byte[] FileBytes, string FileName)> GetAttachment(int docId, int attachmentId, CancellationToken cancellation)
        {
            // This enforces read permissions
            string attachments = nameof(Document.Attachments);
            var (doc, _) = await GetById(docId, new GetByIdArguments
            {
                Select = $"{attachments}/{nameof(Attachment.FileId)},{attachments}/{nameof(Attachment.FileName)},{attachments}/{nameof(Attachment.FileExtension)}"
            }, 
            cancellation);

            // Get the blob name
            var attachment = doc?.Attachments?.FirstOrDefault(att => att.Id == attachmentId);
            if (attachment != null && !string.IsNullOrWhiteSpace(attachment.FileId))
            {
                // Get the bytes
                string blobName = BlobName(attachment.FileId);
                var fileBytes = await _blobService.LoadBlob(blobName, cancellation);

                // Get the content type
                var fileName = $"{attachment.FileName ?? "Attachment"}.{attachment.FileExtension}";
                return (fileBytes, fileName);
            }
            else
            {
                throw new NotFoundException<int>(attachmentId);
            }
        }

        public override async Task<(Document, Extras)> GetById(int id, GetByIdArguments args, CancellationToken cancellation)
        {
            var (entity, extras) = await base.GetById(id, args, cancellation);
            if (entity.OpenedAt == null)
            {
                var userInfo = await _repo.GetUserInfoAsync(cancellation);
                var userId = userInfo.UserId.Value;

                if (entity.AssigneeId == userId)
                {
                    // Mark the entity's OpenedAt both in the DB and in the returned entity
                    var assignedAt = entity.AssignedAt.Value;
                    var openedAt = DateTimeOffset.Now;
                    var infos = await _repo.Documents__Open(entity.Id, assignedAt, openedAt);
                    entity.OpenedAt = openedAt;

                    // Notify the user
                    var tenantId = _tenantIdAccessor.GetTenantId();
                    await _hubContext.NotifyInboxAsync(tenantId, infos);
                }
            }

            return (entity, extras);
        }

        protected override IRepository GetRepository()
        {
            string filter = $"{nameof(Document.DefinitionId)} {Ops.eq} '{DefinitionId}'";
            return new FilteredRepository<Document>(_repo, filter);
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            var permissions = (await _repo.UserPermissions(action, View, cancellation)).ToList();

            // Add a special permission that lets you see the documents that were assigned to you
            permissions.AddRange(DocumentServiceUtil.HardCodedPermissions());

            return permissions;
        }

        private DocumentDefinitionForClient CurrentDefinition
        {
            get
            {
                DocumentDefinitionForClient result = null;
                var definitions = _definitionsCache.GetCurrentDefinitionsIfCached()?.Data?.Documents;
                if (definitions != null)
                {
                    definitions.TryGetValue(DefinitionId, value: out result);
                }

                return result;
            }
        }

        protected override Query<Document> Search(Query<Document> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            var prefix = CurrentDefinition?.Prefix;
            var map = new List<(string Prefix, string DefinitionId)>
            {
                (prefix, DefinitionId)
            };
            return DocumentServiceUtil.SearchImpl(query, args, filteredPermissions, map);
        }

        protected override async Task<Extras> GetExtras(IEnumerable<Document> result, CancellationToken cancellation)
        {
            if (IncludeRequiredSignatures)
            {
                // DocumentIds parameter
                var docIds = result.Select(doc => new { doc.Id });
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

                var query = _repo.Query<RequiredSignature>()
                    .AdditionalParameters(docIdsTvp)
                    .Expand("Role,Agent,User,SignedBy,OnBehalfOfUser,ProxyRole")
                    .OrderBy(nameof(RequiredSignature.LineId));

                var requiredSignatures = await query.ToListAsync(cancellation);

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
            var docDef = Definition();
            var settings = _settingsCache.GetCurrentSettingsIfCached().Data;
            var functionalId = settings.FunctionalCurrencyId;

            // Set default values
            docs.ForEach(doc =>
            {
                if (!settings.IsMultiCenter)
                {
                    doc.InvestmentCenterIsCommon = false;
                }

                // Document defaults
                doc.MemoIsCommon ??= docDef.MemoVisibility != null ? doc.MemoIsCommon ?? false : false;
                doc.DebitAgentIsCommon = docDef.DebitAgentVisibility ? doc.DebitAgentIsCommon ?? false : false;
                doc.CreditAgentIsCommon = docDef.CreditAgentVisibility ? doc.CreditAgentIsCommon ?? false : false;
                doc.NotedAgentIsCommon = docDef.NotedAgentVisibility ? doc.NotedAgentIsCommon ?? false : false;
                doc.InvestmentCenterIsCommon = docDef.InvestmentCenterVisibility ? doc.InvestmentCenterIsCommon ?? false : false;
                doc.Time1IsCommon = docDef.Time1Visibility ? doc.Time1IsCommon ?? false : false;
                doc.Time2IsCommon = docDef.Time2Visibility ? doc.Time2IsCommon ?? false : false;
                doc.QuantityIsCommon = docDef.QuantityVisibility ? doc.QuantityIsCommon ?? false : false;
                doc.UnitIsCommon = docDef.UnitVisibility ? doc.UnitIsCommon ?? false : false;
                doc.CurrencyIsCommon = docDef.CurrencyVisibility ? doc.CurrencyIsCommon ?? false : false;

                doc.Clearance ??= 0; // Public
                doc.Lines ??= new List<LineForSave>();
                doc.Attachments ??= new List<AttachmentForSave>();

                doc.Lines.ForEach(line =>
                {
                    // Line defaults
                    line.Entries ??= new List<EntryForSave>();
                });
            });

            var lineDefinitions = _definitionsCache.GetCurrentDefinitionsIfCached()?.Data?.Lines;

            // Set common header values on the lines
            docs.ForEach(doc =>
            {
                // All fields that aren't marked  as common, set them to
                // null, the UI makes them invisible anyways
                doc.DebitAgentId = doc.DebitAgentIsCommon.Value ? doc.DebitAgentId : null;
                doc.CreditAgentId = doc.CreditAgentIsCommon.Value ? doc.CreditAgentId : null;
                doc.NotedAgentId = doc.NotedAgentIsCommon.Value ? doc.NotedAgentId : null;
                doc.InvestmentCenterId = doc.InvestmentCenterIsCommon.Value ? doc.InvestmentCenterId : null;
                doc.Time1 = doc.Time1IsCommon.Value ? doc.Time1 : null;
                doc.Time2 = doc.Time2IsCommon.Value ? doc.Time2 : null;
                doc.Quantity = doc.QuantityIsCommon.Value ? doc.Quantity : null;
                doc.UnitId = doc.UnitIsCommon.Value ? doc.UnitId : null;
                doc.CurrencyId = doc.CurrencyIsCommon.Value ? doc.CurrencyId : null;

                // All fields that are marked as common, copy the common value across to the 
                // lines and entries, we deal with the lines one definitionId at a time
                foreach (var linesGroup in doc.Lines.GroupBy(e => e.DefinitionId))
                {
                    var lineDef = lineDefinitions.GetValueOrDefault(linesGroup.Key);
                    if (lineDef == null)
                    {
                        // Validation takes care of this later on
                        continue;
                    }

                    foreach (var line in linesGroup)
                    {
                        // If the number of entries is not the same as the definition specifies, fix that
                        while (line.Entries.Count < lineDef.Entries.Count)
                        {
                            // If less, add the missing entries
                            var entryDef = lineDef.Entries[line.Entries.Count];
                            line.Entries.Add(new EntryForSave());
                        }

                        while (line.Entries.Count > lineDef.Entries.Count)
                        {
                            // If more, pop the excess entries from the end
                            line.Entries.RemoveAt(line.Entries.Count - 1);
                        }

                        // Copy the direction from the definition
                        for (var i = 0; i < line.Entries.Count; i++)
                        {
                            if (line.DefinitionId != ManualLine)
                            {
                                line.Entries[i].Direction = lineDef.Entries[i].Direction;
                            }
                        }

                        // Copy common values from the header if they are marked inherits from header
                        // IMPORTANT: Any changes to the switch statements must be mirrored in SaveValidateAsync
                        foreach (var columnDef in lineDef.Columns.Where(c => c.InheritsFromHeader ?? false))
                        {
                            if (columnDef.ColumnName == nameof(Line.Memo))
                            {
                                if (doc.MemoIsCommon.Value)
                                {
                                    line.Memo = doc.Memo;
                                }
                            }
                            else
                            {
                                if (columnDef.EntryIndex >= line.Entries.Count ||
                                    columnDef.EntryIndex >= lineDef.Entries.Count)
                                {
                                    // To avoid index out of bounds exception
                                    continue;
                                }

                                // Copy the common values
                                var entry = line.Entries[columnDef.EntryIndex];
                                switch (columnDef.ColumnName)
                                {
                                    case nameof(Entry.AgentId):
                                        var entryDef = lineDef.Entries[columnDef.EntryIndex];
                                        if (entryDef.Direction == 1 && doc.DebitAgentIsCommon.Value)
                                        {
                                            entry.AgentId = doc.DebitAgentId;
                                        }
                                        else if (entryDef.Direction == -1 && doc.CreditAgentIsCommon.Value)
                                        {
                                            entry.AgentId = doc.CreditAgentId;
                                        }

                                        break;

                                    case nameof(Entry.NotedAgentId):
                                        if (doc.NotedAgentIsCommon.Value)
                                        {
                                            entry.NotedAgentId = doc.NotedAgentId;
                                        }

                                        break;

                                    case nameof(Entry.CenterId):
                                        if (doc.InvestmentCenterIsCommon.Value)
                                        {
                                            entry.CenterId = doc.InvestmentCenterId;
                                        }

                                        break;

                                    case nameof(Entry.Time1):
                                        if (doc.Time1IsCommon.Value)
                                        {
                                            entry.Time1 = doc.Time1;
                                        }

                                        break;

                                    case nameof(Entry.Time2):
                                        if (doc.Time2IsCommon.Value)
                                        {
                                            entry.Time2 = doc.Time2;
                                        }

                                        break;

                                    case nameof(Entry.Quantity):
                                        if (doc.QuantityIsCommon.Value)
                                        {
                                            entry.Quantity = doc.Quantity;
                                        }

                                        break;

                                    case nameof(Entry.UnitId):
                                        if (doc.UnitIsCommon.Value)
                                        {
                                            entry.UnitId = doc.UnitId;
                                        }
                                        break;

                                    case nameof(Entry.CurrencyId):
                                        if (doc.CurrencyIsCommon.Value)
                                        {
                                            entry.CurrencyId = doc.CurrencyId;
                                        }

                                        break;

                                    default:
                                        break; // This property doesn't exist on the document, just ignore it
                                }
                            }
                        }
                    }
                }
            });

            // SQL server preprocessing
            await _repo.Documents__Preprocess(DefinitionId, docs);

            // C# Processing after SQL
            docs.ForEach(doc =>
            {
                doc.Lines.ForEach(line =>
                {
                    line.Entries.ForEach(entry =>
                    {
                        // If currency is functional, make sure that Value = MonetaryValue
                        if (entry.CurrencyId == settings.FunctionalCurrencyId)
                        {
                            if (line.DefinitionId == ManualLine)
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

                        // Other logic
                    });
                });

                ////// handle subtle exchange rate rounding bugs. IF...
                // (1) All non-manual entries have the same non-functional currency
                // (2) Monetary Value of non-manual entries is balanced
                // (3) Value of non-manual is not balanced
                // => Take the difference and distribute it evenly on the entries
                if (doc.Lines.Count > 0)
                {
                    var smartEntries = doc.Lines.Where(line => line.DefinitionId != ManualLine).SelectMany(line => line.Entries);
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

            return docs;
        }

        protected override async Task SaveValidateAsync(List<DocumentForSave> docs)
        {
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

            var settings = _settingsCache.GetCurrentSettingsIfCached().Data;
            var docDef = Definition();
            var lineDefs = _definitionsCache.GetCurrentDefinitionsIfCached()?.Data?.Lines;

            // TODO: Add definition-specific validation here

            // SQL may return keys representing line and entry properties that inherit from a common document property
            // This dictionary maps the keys of the former properties to the keys of the later properties, and is used
            // At the end to map the keys that return from SQL before serving them to the client
            var errorKeyMap = new Dictionary<string, string>();

            ///////// Document Validation
            for (int docIndex = 0; docIndex < docs.Count; docIndex++)
            {
                var doc = docs[docIndex];

                if (!docDef.IsOriginalDocument)
                {
                    // If not an original document, the serial number is required
                    if (doc.SerialNumber == null || doc.SerialNumber == 0)
                    {
                        ModelState.AddModelError($"[{docIndex}].{nameof(doc.SerialNumber)}",
                            _localizer[Services.Utilities.Constants.Error_TheField0IsRequired, _localizer["Document_SerialNumber"]]);
                    }
                    else if (duplicateSerialNumbers.ContainsKey(doc))
                    {
                        var serial = duplicateSerialNumbers[doc];
                        ModelState.AddModelError($"[{docIndex}].{nameof(doc.SerialNumber)}",
                            _localizer["Error_DuplicateSerial0", FormatSerial(serial, docDef.Prefix, docDef.CodeWidth)]);
                    }
                }

                // Date cannot be in the future
                if (doc.PostingDate > DateTime.Today.AddDays(1))
                {
                    ModelState.AddModelError($"[{docIndex}].{nameof(doc.PostingDate)}",
                        _localizer["Error_DateCannotBeInTheFuture"]);
                }

                // Date cannot be before archive date
                if (doc.PostingDate <= settings.ArchiveDate)
                {
                    ModelState.AddModelError($"[{docIndex}].{nameof(doc.PostingDate)}",
                        _localizer["Error_DateCannotBeBeforeArchiveDate0", settings.ArchiveDate.ToString("yyyy-MM-dd")]);
                }

                ///////// Line Validation
                for (int lineIndex = 0; lineIndex < doc.Lines.Count; lineIndex++)
                {
                    var line = doc.Lines[lineIndex];

                    var lineDef = lineDefs.GetValueOrDefault(line.DefinitionId);
                    if (lineDef == null)
                    {
                        ModelState.AddModelError(LinePath(docIndex, lineIndex, nameof(Line.Id)),
                            _localizer["Error_UnknownLineDefinitionId0", line.DefinitionId]);
                    }

                    // Prevent duplicate line Ids
                    if (duplicateLineIds.ContainsKey(line))
                    {
                        // This error indicates a bug
                        var id = duplicateLineIds[line];
                        ModelState.AddModelError(LinePath(docIndex, lineIndex, nameof(Line.Id)),
                            _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                    }

                    ///////// Entry Validation
                    for (int entryIndex = 0; entryIndex < line.Entries.Count; entryIndex++)
                    {
                        var entry = line.Entries[entryIndex];
                        // Prevent duplicate entry Ids
                        if (duplicateEntryIds.ContainsKey(entry))
                        {
                            var id = duplicateEntryIds[entry];
                            ModelState.AddModelError(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.Id)),
                                _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                        }

                        // If the currency is functional, value must equal monetary value
                        if (entry.CurrencyId == settings.FunctionalCurrencyId && entry.Value != entry.MonetaryValue)
                        {
                            var currencyName = _tenantInfoAccessor.GetCurrentInfo()
                                .Localize(settings.FunctionalCurrencyName,
                                            settings.FunctionalCurrencyName2,
                                            settings.FunctionalCurrencyName3);

                            // TODO: Use the proper field name from definition, instead of "Amount"
                            ModelState.AddModelError(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.MonetaryValue)),
                                _localizer["TheAmount0DoesNotMatchTheValue1EvenThoughBothIn2", entry.MonetaryValue ?? 0, entry.Value ?? 0, currencyName]);
                        }

                        if (ModelState.HasReachedMaxErrors)
                        {
                            break;
                        }
                    }

                    // If a common header property is different than any of its constitutents, return readonly error
                    // it means one of the constituents is readonly and has been changed by the preprocess SQL => return readonly error
                    // IMPORTANT: Any changes to the switch statements must be mirrored in SavePreprocessAsync
                    foreach (var columnDef in lineDef.Columns.Where(c => c.InheritsFromHeader ?? false))
                    {
                        if (columnDef.ColumnName == nameof(Line.Memo))
                        {
                            if (doc.MemoIsCommon.Value)
                            {
                                errorKeyMap.Add(LinePath(docIndex, lineIndex, nameof(Line.Memo)), $"[{docIndex}].{nameof(Document.Memo)}");
                                if (doc.Memo != line.Memo)
                                {
                                    AddReadOnlyError(docIndex, nameof(Document.Memo));
                                }
                            }
                        }
                        else
                        {
                            var entryIndex = columnDef.EntryIndex;
                            if (entryIndex >= line.Entries.Count ||
                                entryIndex >= lineDef.Entries.Count)
                            {
                                // To avoid index out of bounds exception
                                continue;
                            }

                            // Copy the common values
                            var entry = line.Entries[entryIndex];
                            switch (columnDef.ColumnName)
                            {
                                case nameof(Entry.AgentId):
                                    var entryDef = lineDef.Entries[entryIndex];
                                    if (entryDef.Direction == 1 && doc.DebitAgentIsCommon.Value)
                                    {
                                        errorKeyMap.Add(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.AgentId)), $"[{docIndex}].{nameof(Document.DebitAgentId)}");
                                        if (entry.AgentId != doc.DebitAgentId)
                                        {
                                            AddReadOnlyError(docIndex, nameof(Document.DebitAgentId));
                                        }
                                    }
                                    else if (entryDef.Direction == -1 && doc.CreditAgentIsCommon.Value)
                                    {
                                        errorKeyMap.Add(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.AgentId)), $"[{docIndex}].{nameof(Document.CreditAgentId)}");
                                        if (entry.AgentId != doc.CreditAgentId)
                                        {
                                            AddReadOnlyError(docIndex, nameof(Document.CreditAgentId));
                                        }
                                    }

                                    break;

                                case nameof(Entry.NotedAgentId):
                                    if (doc.NotedAgentIsCommon.Value)
                                    {
                                        errorKeyMap.Add(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.NotedAgentId)), $"[{docIndex}].{nameof(Document.NotedAgentId)}");
                                        if (entry.NotedAgentId != doc.NotedAgentId)
                                        {
                                            AddReadOnlyError(docIndex, nameof(Document.NotedAgentId));
                                        }
                                    }

                                    break;

                                case nameof(Entry.CenterId):
                                    if (doc.InvestmentCenterIsCommon.Value)
                                    {
                                        errorKeyMap.Add(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.CenterId)), $"[{docIndex}].{nameof(Document.InvestmentCenterId)}");
                                        if (entry.CenterId != doc.InvestmentCenterId)
                                        {
                                            AddReadOnlyError(docIndex, nameof(Document.InvestmentCenterId));
                                        }
                                    }

                                    break;

                                case nameof(Entry.Time1):
                                    if (doc.Time1IsCommon.Value)
                                    {
                                        errorKeyMap.Add(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.Time1)), $"[{docIndex}].{nameof(Document.Time1)}");
                                        if (entry.Time1 != doc.Time1)
                                        {
                                            AddReadOnlyError(docIndex, nameof(Document.Time1));
                                        }
                                    }

                                    break;

                                case nameof(Entry.Time2):
                                    if (doc.Time2IsCommon.Value)
                                    {
                                        errorKeyMap.Add(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.Time2)), $"[{docIndex}].{nameof(Document.Time2)}");
                                        if (entry.Time2 != doc.Time2)
                                        {
                                            AddReadOnlyError(docIndex, nameof(Document.Time2));
                                        }
                                    }

                                    break;

                                case nameof(Entry.Quantity):
                                    if (doc.QuantityIsCommon.Value)
                                    {
                                        errorKeyMap.Add(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.Quantity)), $"[{docIndex}].{nameof(Document.Quantity)}");
                                        if (entry.Quantity != doc.Quantity)
                                        {
                                            AddReadOnlyError(docIndex, nameof(Document.Quantity));
                                        }
                                    }

                                    break;

                                case nameof(Entry.UnitId):
                                    if (doc.UnitIsCommon.Value)
                                    {
                                        errorKeyMap.Add(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.UnitId)), $"[{docIndex}].{nameof(Document.UnitId)}");
                                        if (entry.UnitId != doc.UnitId)
                                        {
                                            AddReadOnlyError(docIndex, nameof(Document.UnitId));
                                        }
                                    }
                                    break;

                                case nameof(Entry.CurrencyId):
                                    if (doc.CurrencyIsCommon.Value)
                                    {
                                        errorKeyMap.Add(EntryPath(docIndex, lineIndex, entryIndex, nameof(Entry.CurrencyId)), $"[{docIndex}].{nameof(Document.CurrencyId)}");
                                        if (entry.CurrencyId != doc.CurrencyId)
                                        {
                                            AddReadOnlyError(docIndex, nameof(Document.CurrencyId));
                                        }
                                    }

                                    break;

                                default:
                                    break; // This property doesn't exist on the document, just ignore it
                            }
                        }

                        if (ModelState.HasReachedMaxErrors)
                        {
                            break;
                        }
                    }

                    if (ModelState.HasReachedMaxErrors)
                    {
                        break;
                    }
                }

                if (ModelState.HasReachedMaxErrors)
                {
                    break;
                }

                ///////// Attachment Validation
                for (var attIndex = 0; attIndex < doc.Attachments.Count; attIndex++)
                {
                    var att = doc.Attachments[attIndex];

                    if (att.Id != 0 && att.File != null)
                    {
                        ModelState.AddModelError($"[{docIndex}].{nameof(doc.Attachments)}[{attIndex}]",
                            _localizer["Error_OnlyNewAttachmentsCanIncludeFileBytes"]);
                    }

                    if (att.Id == 0 && att.File == null)
                    {
                        ModelState.AddModelError($"[{docIndex}].{nameof(doc.Attachments)}[{attIndex}]",
                            _localizer["Error_NewAttachmentsMustIncludeFileBytes"]);
                    }

                    if (ModelState.HasReachedMaxErrors)
                    {
                        break;
                    }
                }

                if (ModelState.HasReachedMaxErrors)
                {
                    break;
                }
            }

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Documents_Validate__Save(DefinitionId, docs, top: remainingErrorCount);

            // Update the key of mapped errors
            foreach (var sqlError in sqlErrors)
            {
                sqlError.Key = errorKeyMap.GetValueOrDefault(sqlError.Key) ?? sqlError.Key;
            }

            // Make the key and error name unique again
            sqlErrors = sqlErrors.GroupBy(e => new { e.Key, e.ErrorName })
                .Select(g => new ValidationError
                {
                    Key = g.Key.Key,
                    ErrorName = g.Key.ErrorName,
                    Argument1 = g.First().Argument1,
                    Argument2 = g.First().Argument2,
                    Argument3 = g.First().Argument3,
                    Argument4 = g.First().Argument4,
                    Argument5 = g.First().Argument5,
                });

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        private void AddReadOnlyError(int docIndex, string propName)
        {
            if (ModelState.HasReachedMaxErrors)
            {
                return;
            }

            var key = $"[{docIndex}].{propName}";
            if (!ModelState.ContainsKey(key))
            {
                var propDisplayName = _modelMetadataProvider.GetMetadataForProperty(typeof(DocumentForSave), propName)?.DisplayName ?? _localizer["Document_NotedAgent"];
                ModelState.AddModelError(key, _localizer["Error_TheField0IsReadOnly", propDisplayName]);
            }
        }

        private string FormatSerial(int serial, string prefix, int codeWidth)
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

        private string EntryPath(int docIndex, int lineIndex, int entryIndex, string propName)
        {
            return $"[{docIndex}].{nameof(Document.Lines)}[{lineIndex}].{nameof(Line.Entries)}[{entryIndex}].{propName}";
        }

        private string LinePath(int docIndex, int lineIndex, string propName)
        {
            return $"[{docIndex}].{nameof(Document.Lines)}[{lineIndex}].{propName}";
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<DocumentForSave> entities, bool returnIds)
        {
            var blobsToSave = new List<(string, byte[])>();

            // Prepare the list of attachments with extras
            var attachments = new List<AttachmentWithExtras>();
            foreach (var (doc, docIndex) in entities.Select((d, i) => (d, i)))
            {
                if (doc.Attachments != null)
                {
                    doc.Attachments.ForEach(att =>
                    {
                        var attWithExtras = new AttachmentWithExtras
                        {
                            Id = att.Id,
                            FileName = att.FileName,
                            FileExtension = att.FileExtension,
                            DocumentIndex = docIndex,
                        };

                        // If new attachment 
                        if (att.Id == 0)
                        {
                            // Add extras: file Id and size
                            byte[] file = att.File;
                            string fileId = Guid.NewGuid().ToString();
                            attWithExtras.FileId = fileId;
                            attWithExtras.Size = file.LongLength;

                            // Also add to blobsToCreate
                            string blobName = BlobName(fileId);
                            blobsToSave.Add((blobName, file));
                        }

                        attachments.Add(attWithExtras);
                    });
                }
            }

            // Save the documents
            var (notificationInfos, fileIdsToDelete, ids) = await _repo.Documents__SaveAndRefresh(
                DefinitionId,
                documents: entities,
                attachments: attachments,
                returnIds: returnIds);

            // Notify affected users
            await _hubContext.NotifyInboxAsync(TenantId, notificationInfos);

            // Delete the file Ids retrieved earlier if any
            if (fileIdsToDelete.Any())
            {
                var blobsToDelete = fileIdsToDelete.Select(fileId => BlobName(fileId));
                await _blobService.DeleteBlobsAsync(blobsToDelete);
            }

            // Save new blobs if any
            if (blobsToSave.Any())
            {
                await _blobService.SaveBlobsAsync(blobsToSave);
            }

            // Return the new Ids
            return ids;
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Documents_Validate__Delete(DefinitionId, ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                var (notificationInfos, fileIdsToDelete) = await _repo.Documents__Delete(ids);

                await _hubContext.NotifyInboxAsync(TenantId, notificationInfos);

                // Delete the file Ids retrieved earlier if any
                if (fileIdsToDelete.Any())
                {
                    var blobsToDelete = fileIdsToDelete.Select(fileId => BlobName(fileId));
                    await _blobService.DeleteBlobsAsync(blobsToDelete);
                }
            }
            catch (ForeignKeyViolationException)
            {
                // TODO: test
                var definition = Definition();
                var tenantInfo = await _repo.GetTenantInfoAsync(cancellation: default);
                var titleSingular = tenantInfo.Localize(definition.TitleSingular, definition.TitleSingular2, definition.TitleSingular3);

                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", titleSingular]);
            }
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse($"{nameof(Document.SerialNumber)} desc");
        }

        private string BlobName(string guid)
        {
            return $"{TenantId}/Attachments/{guid}";
        }

        public DocumentsService SetIncludeRequiredSignatures(bool val)
        {
            _includeRequiredSignaturesOverride = val;
            return this;
        }

        public DocumentsService SetDefinitionId(string definitionId)
        {
            _definitionIdOverride = definitionId;
            return this;
        }

        #region Details Select

        protected override SelectExpression ParseSelect(string select)
        {
            // We provide a shorthand notation for common and huge select
            // strings, this one is usually requested from the document details
            // screen and it contains over 260 atoms
            var shorthand = "$Details";
            if (select == null)
            {
                return null;
            }
            else if (select.Trim() == shorthand)
            {
                return _detailsSelectExpression;
            }
            else
            {
                select = select.Replace(shorthand, _detailsSelect);
                return base.ParseSelect(select);
            }
        }

        private static readonly string _detailsSelect = string.Join(',', DocumentPaths());
        private static readonly SelectExpression _detailsSelectExpression = new SelectExpression(DocumentPaths().Select(a => SelectAtom.Parse(a)));

        // ------------------------------------------------
        // Paths to return on the level of each entity type
        // ------------------------------------------------

        public static IEnumerable<string> DocumentPaths() => DocumentProps
            .Concat(LinePaths(nameof(Document.Lines)))
            .Concat(AttachmentPaths(nameof(Document.Attachments)))
            .Concat(DocumentStateChangePaths(nameof(Document.StatesHistory)))
            .Concat(DocumentAssignmentPaths(nameof(Document.AssignmentsHistory)))
            .Concat(AgentPaths(nameof(Document.DebitAgent)))
            .Concat(AgentPaths(nameof(Document.CreditAgent)))
            .Concat(AgentPaths(nameof(Document.NotedAgent)))
            .Concat(CenterPaths(nameof(Document.InvestmentCenter)))
            .Concat(UnitPaths(nameof(Document.Unit)))
            .Concat(CurrencyPaths(nameof(Document.Currency)))
            .Concat(LookupPaths(nameof(Document.DocumentLookup1)))
            .Concat(LookupPaths(nameof(Document.DocumentLookup2)))
            .Concat(LookupPaths(nameof(Document.DocumentLookup3)))
            .Concat(UserPaths(nameof(Document.CreatedBy)))
            .Concat(UserPaths(nameof(Document.ModifiedBy)))
            .Concat(UserPaths(nameof(Document.Assignee)));
        public static IEnumerable<string> LinePaths(string path = null) => LineProps
            .Concat(EntryPaths(nameof(Line.Entries)))
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> EntryPaths(string path = null) => EntryProps
            .Concat(AccountPaths(nameof(Entry.Account)))
            .Concat(CurrencyPaths(nameof(Entry.Currency)))
            .Concat(EntryResourcePaths(nameof(Entry.Resource)))
            .Concat(AgentPaths(nameof(Entry.Agent)))
            .Concat(EntryTypePaths(nameof(Entry.EntryType)))
            .Concat(AgentPaths(nameof(Entry.NotedAgent)))
            .Concat(CenterPaths(nameof(Entry.Center)))
            .Concat(UnitPaths(nameof(Entry.Unit)))
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> AttachmentPaths(string path = null) => AttachmentProps
            .Concat(UserPaths(nameof(Attachment.CreatedBy)))
            .Concat(UserPaths(nameof(Attachment.ModifiedBy)))
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> DocumentStateChangePaths(string path = null) => DocumentStateChangeProps
            .Concat(UserPaths(nameof(DocumentStateChange.ModifiedBy)))
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> DocumentAssignmentPaths(string path = null) => DocumentAssignmentProps
            .Concat(UserPaths(nameof(DocumentAssignment.CreatedBy)))
            .Concat(UserPaths(nameof(DocumentAssignment.Assignee)))
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> AgentPaths(string path = null) => AgentProps
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> EntryResourcePaths(string path = null) => ResourcePaths(path)
            .Concat( // Entry Resource also adds the Currency
                CurrencyPaths(nameof(Resource.Currency)).Select(p => path == null ? p : $"{path}/{p}")
            );
        public static IEnumerable<string> ResourcePaths(string path = null) => ResourceProps
            .Concat(ResourceUnitPaths(nameof(Resource.Units)))
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> ResourceUnitPaths(string path = null) => ResourceUnitsProps
            .Concat(UnitPaths(nameof(ResourceUnit.Unit)))
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> CenterPaths(string path = null) => CenterProps
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> UnitPaths(string path = null) => UnitProps
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> CurrencyPaths(string path = null) => CurrencyProps
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> UserPaths(string path = null) => UserProps
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> LookupPaths(string path = null) => LookupProps
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> EntryTypePaths(string path = null) => EntryTypeProps
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> AccountPaths(string path = null) => AccountProps
            .Concat(AccountTypePaths(nameof(Account.AccountType)))
            .Concat(CenterPaths(nameof(Account.Center)))
            .Concat(EntryTypePaths(nameof(Account.EntryType)))
            .Concat(CurrencyPaths(nameof(Account.Currency)))
            .Concat(AgentPaths(nameof(Account.Agent)))
            .Concat(ResourcePaths(nameof(Account.Resource)))
            .Select(p => path == null ? p : $"{path}/{p}");
        public static IEnumerable<string> AccountTypePaths(string path = null) => AccountTypeProps
            .Select(p => path == null ? p : $"{path}/{p}");

        // -------------------------------------------------------------
        // Simple properties to include on the level of each entity type
        // -------------------------------------------------------------

        private static IEnumerable<string> DocumentProps => typeof(Document).GetMappedProperties().Select(p => p.Name);
        private static IEnumerable<string> LineProps => typeof(Line).GetMappedProperties().Select(p => p.Name);
        private static IEnumerable<string> EntryProps => typeof(Entry).GetMappedProperties().Select(p => p.Name);
        private static IEnumerable<string> AttachmentProps => typeof(Attachment).GetMappedProperties().Select(p => p.Name);
        private static IEnumerable<string> DocumentAssignmentProps => typeof(DocumentAssignment).GetMappedProperties().Select(p => p.Name);
        private static IEnumerable<string> DocumentStateChangeProps => typeof(DocumentStateChange).GetMappedProperties().Select(p => p.Name);
        private static IEnumerable<string> UnitProps => Enum(nameof(Unit.Name), nameof(Unit.Name2), nameof(Unit.Name3));
        private static IEnumerable<string> CurrencyProps => Enum(nameof(Currency.Name), nameof(Currency.Name2), nameof(Currency.Name3), nameof(Currency.E));
        private static IEnumerable<string> UserProps => Enum(nameof(Entities.User.Name), nameof(Entities.User.Name2), nameof(Entities.User.Name3), nameof(Entities.User.ImageId));
        private static IEnumerable<string> ResourceProps => Enum(nameof(Resource.Name), nameof(Resource.Name2), nameof(Resource.Name3), nameof(Resource.DefinitionId));
        private static IEnumerable<string> ResourceUnitsProps => Enum(nameof(ResourceUnit.Multiplier));
        private static IEnumerable<string> LookupProps => Enum(nameof(Lookup.Name), nameof(Lookup.Name2), nameof(Lookup.Name3), nameof(Lookup.DefinitionId));
        private static IEnumerable<string> AgentProps => Enum(nameof(Agent.Name), nameof(Agent.Name2), nameof(Agent.Name3), nameof(Agent.DefinitionId));
        private static IEnumerable<string> CenterProps => Enum(nameof(Center.Name), nameof(Center.Name2), nameof(Center.Name3));
        private static IEnumerable<string> AccountProps => Enum(nameof(Account.Name), nameof(Account.Name2), nameof(Account.Name3));
        private static IEnumerable<string> EntryTypeProps => Enum(nameof(EntryType.Name), nameof(EntryType.Name2), nameof(EntryType.Name3));
        private static IEnumerable<string> AccountTypeProps => Enum(
            // Names
            nameof(AccountType.Name),
            nameof(AccountType.Name2),
            nameof(AccountType.Name3),

            // Misc
            nameof(AccountType.EntryTypeParentId),
            nameof(AccountType.IsResourceClassification),

            // Definitions
            nameof(AccountType.AgentDefinitionId),
            nameof(AccountType.NotedAgentDefinitionId),
            nameof(AccountType.ResourceDefinitionId),

            // Assignments
            nameof(AccountType.CurrencyAssignment),
            nameof(AccountType.AgentAssignment),
            nameof(AccountType.ResourceAssignment),
            nameof(AccountType.CenterAssignment),
            nameof(AccountType.EntryTypeAssignment),
            nameof(AccountType.IdentifierAssignment),
            nameof(AccountType.NotedAgentAssignment),

            // Labels
            nameof(AccountType.DueDateLabel), nameof(AccountType.DueDateLabel2), nameof(AccountType.DueDateLabel3),
            nameof(AccountType.Time1Label), nameof(AccountType.Time1Label2), nameof(AccountType.Time1Label3),
            nameof(AccountType.Time2Label), nameof(AccountType.Time2Label), nameof(AccountType.Time2Label),
            nameof(AccountType.ExternalReferenceLabel), nameof(AccountType.ExternalReferenceLabel), nameof(AccountType.ExternalReferenceLabel),
            nameof(AccountType.AdditionalReferenceLabel), nameof(AccountType.AdditionalReferenceLabel), nameof(AccountType.AdditionalReferenceLabel),
            nameof(AccountType.NotedAgentNameLabel), nameof(AccountType.NotedAgentNameLabel), nameof(AccountType.NotedAgentNameLabel),
            nameof(AccountType.NotedAmountLabel), nameof(AccountType.NotedAmountLabel), nameof(AccountType.NotedAmountLabel),
            nameof(AccountType.NotedDateLabel), nameof(AccountType.NotedDateLabel), nameof(AccountType.NotedDateLabel)
         );

        // Helper method
        private static IEnumerable<string> Enum(params string[] ps)
        {
            foreach (var p in ps)
            {
                yield return p;
            }
        }

        #endregion
    }

    [Route("api/" + DocumentsController.BASE_ADDRESS)]
    [ApplicationController]
    public class DocumentsGenericController : FactWithIdControllerBase<Document, int>
    {
        private readonly DocumentsGenericService _service;

        public DocumentsGenericController(DocumentsGenericService service, ILogger<DocumentsGenericController> logger) : base(logger)
        {
            _service = service;
        }

        protected override FactWithIdServiceBase<Document, int> GetFactWithIdService()
        {
            return _service;
        }
    }

    public class DocumentsGenericService : FactWithIdServiceBase<Document, int>
    {
        private readonly ApplicationRepository _repo;
        private readonly IDefinitionsCache _definitionsCache;

        public DocumentsGenericService(
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo,
            IDefinitionsCache definitionsCache) : base(localizer)
        {
            _repo = repo;
            _definitionsCache = definitionsCache;
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            // Get all permissions pertaining to documents
            string prefix = DocumentsController.BASE_ADDRESS;
            var permissions = (await _repo.GenericUserPermissions(action, prefix, cancellation)).ToList();

            // Massage the permissions by adding definitionId = definitionId as an extra clause 
            // (since the controller will not filter the results per any specific definition Id)
            foreach (var permission in permissions.Where(e => e.View != "all"))
            {
                string definitionId = permission.View.Remove(0, prefix.Length).Replace("'", "''");
                string definitionPredicate = $"{nameof(Document.DefinitionId)} {Ops.eq} '{definitionId}'";
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
            permissions.AddRange(DocumentServiceUtil.HardCodedPermissions());

            // Return the massaged permissions
            return permissions;
        }

        protected override Query<Document> Search(Query<Document> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            // Get a map from all serial prefixes to definitionIds
            var prefixMap = _definitionsCache.GetCurrentDefinitionsIfCached()?
                .Data?.Documents? // Get document definitions for client from the cache
                .Select(e => (e.Value.Prefix, e.Key)) ?? // Select all (Prefix, DefinitionId)
                new List<(string, string)>(); // Avoiding null reference exception at all cost

            return DocumentServiceUtil.SearchImpl(query, args, filteredPermissions, prefixMap);
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse($"{nameof(Document.PostingDate)} desc");
        }
    }

    internal class DocumentServiceUtil
    {
        /// <summary>
        /// This is needed in both the generic and specific controllers, so we move it out here
        /// </summary>
        internal static Query<Document> SearchImpl(Query<Document> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions, IEnumerable<(string Prefix, string DefinitionId)> prefixMap)
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

                if (definitionId != null && int.TryParse(searchLower.Remove(0, prefix.Length), out int serial))
                {
                    var serialNumberProp = nameof(Document.SerialNumber);
                    var definitionIdProp = nameof(Document.DefinitionId);

                    // Prepare the filter string
                    var filterString = $"{serialNumberProp} {Ops.eq} {serial} and {definitionIdProp} {Ops.eq} '{definitionId}'";

                    // Apply the filter
                    query = query.Filter(filterString);
                }

                // ELSE: search the memo, posting date, etc normally
                else
                {
                    search = search.Replace("'", "''"); // escape quotes by repeating them

                    var memoProp = nameof(Document.Memo);
                    var serialNumberProp = nameof(Document.SerialNumber);
                    var postingDateProp = nameof(Document.PostingDate);

                    // Prepare the filter string
                    var filterString = $"{memoProp} {Ops.contains} '{search}'";

                    // If the search is a number, include documents with that serial number
                    if (int.TryParse(search.Trim(), out int searchNumber))
                    {
                        filterString = $"{filterString} or {serialNumberProp} {Ops.eq} {searchNumber}";
                    }

                    // If the search is a date, include documents with that date
                    if (DateTime.TryParse(search.Trim(), out DateTime searchDate))
                    {
                        filterString = $"{filterString} or {postingDateProp} {Ops.eq} {searchDate.ToString("yyyy-MM-dd")}";
                    }

                    // Apply the filter
                    query = query.Filter(FilterExpression.Parse(filterString));
                }
            }

            return query;
        }

        internal static IEnumerable<AbstractPermission> HardCodedPermissions()
        {
            // If someone assigns the document to you, you can read it
            // and forward it to someone else, until it either gets modified
            // or forwarded again (the second condition so that you can 
            // refresh the document immediately after forwarding)
            //yield return new AbstractPermission
            //{
            //    View = "documents", // Not important
            //    Action = "Read",
            //    Criteria = "AssigneeId eq me OR (AssignedById eq me AND ModifiedAt lt AssignedAt)"
            //};

            return new List<AbstractPermission>();
        }
    }
}