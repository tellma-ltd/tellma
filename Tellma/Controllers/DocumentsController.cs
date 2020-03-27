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

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS + "{definitionId}")]
    [ApplicationApi]
    public class DocumentsController : CrudControllerBase<DocumentForSave, Document, int>
    {
        public const string BASE_ADDRESS = "documents/";

        private string ManualJournalVouchers => "manual-journal-vouchers";
        private string ManualLine => "ManualLine";
        private string Lines => "Lines";
        private string Entries => "Entries";

        private readonly ILogger<DocumentsController> _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;
        private readonly ITenantIdAccessor _tenantIdAccessor;
        private readonly IBlobService _blobService;
        private readonly IDefinitionsCache _definitionsCache;
        private readonly ISettingsCache _settingsCache;
        private readonly IClientInfoAccessor _clientInfo;
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ITenantInfoAccessor _tenantInfoAccessor;

        private string DefinitionId => RouteData.Values["definitionId"]?.ToString() ??
            throw new BadRequestException("URI must be of the form 'api/" + BASE_ADDRESS + "{definitionId}'");
        private DocumentDefinitionForClient Definition() => _definitionsCache.GetCurrentDefinitionsIfCached()?.Data?.Documents?
            .GetValueOrDefault(DefinitionId) ?? throw new InvalidOperationException($"Definition for '{DefinitionId}' was missing from the cache");

        private string View => $"{BASE_ADDRESS}{DefinitionId}";

        public DocumentsController(ILogger<DocumentsController> logger, IStringLocalizer<Strings> localizer,
            ApplicationRepository repo, ITenantIdAccessor tenantIdAccessor, IBlobService blobService,
            IDefinitionsCache definitionsCache, ISettingsCache settingsCache, IClientInfoAccessor clientInfo,
            IModelMetadataProvider modelMetadataProvider, ITenantInfoAccessor tenantInfoAccessor) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
            _tenantIdAccessor = tenantIdAccessor;
            _blobService = blobService;
            _definitionsCache = definitionsCache;
            _settingsCache = settingsCache;
            _clientInfo = clientInfo;
            _modelMetadataProvider = modelMetadataProvider;
            _tenantInfoAccessor = tenantInfoAccessor;
        }

        [HttpGet("{docId}/attachments/{attachmentId}")]
        public async Task<ActionResult> GetAttachment(int docId, int attachmentId)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // GetByIdImplAsync() enforces read permissions
                string attachments = nameof(Document.Attachments);
                var response = await GetByIdImplAsync(docId, new GetByIdArguments
                {
                    Select = $"{attachments}/{nameof(Attachment.FileId)},{attachments}/{nameof(Attachment.FileName)},{attachments}/{nameof(Attachment.FileExtension)}"
                });

                // Get the blob name
                var attachment = response.Result.Attachments?.FirstOrDefault(att => att.Id == attachmentId);
                if (attachment != null && !string.IsNullOrWhiteSpace(attachment.FileId))
                {
                    // Get the bytes
                    string blobName = BlobName(attachment.FileId);
                    var fileBytes = await _blobService.LoadBlob(blobName);

                    // Get the content type
                    var fileName = $"{attachment.FileName ?? "Attachment"}.{attachment.FileExtension}";
                    var contentType = ContentType(fileName);

                    // Return the file
                    return File(fileBytes, contentType);
                }
                else
                {
                    return NotFound($"Attachment with Id {attachmentId} was not found in document with Id {docId}");
                }

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
                // Parse parameters
                var selectExp = SelectExpression.Parse(args.Select);
                var expandExp = ExpandExpression.Parse(args.Expand);
                var returnEntities = args.ReturnEntities ?? false;
                var idsArray = ids.ToArray();

                // User permissions
                // TODO: Check the user can read the document
                await CheckActionPermissions("Read", idsArray);

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
                await _repo.Documents__Assign(ids, args.AssigneeId, args.Comment, recordInHistory: true);

                if (returnEntities)
                {
                    var response = await GetByIdListAsync(idsArray, expandExp, selectExp);

                    trx.Complete();
                    return Ok(response);
                }
                else
                {
                    trx.Complete();
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
                // Parse parameters
                var selectExp = SelectExpression.Parse(args.Select);
                var expandExp = ExpandExpression.Parse(args.Expand);
                var returnIds = args.ReturnEntities ?? false;

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
                    returnIds: returnIds);

                if (returnIds)
                {
                    var response = await GetByIdListAsync(documentIds.ToArray(), expandExp, selectExp);

                    trx.Complete();
                    return Ok(response);
                }
                else
                {
                    trx.Complete();
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
                // Parse parameters
                var selectExp = SelectExpression.Parse(args.Select);
                var expandExp = ExpandExpression.Parse(args.Expand);
                var returnIds = args.ReturnEntities ?? false;

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
                var documentIds = await _repo.LineSignatures__DeleteAndRefresh(signatureIds, returnIds: returnIds);
                if (returnIds)
                {
                    var response = await GetByIdListAsync(documentIds.ToArray(), expandExp, selectExp);

                    trx.Complete();
                    return Ok(response);
                }
                else
                {
                    trx.Complete();
                    return Ok();
                }
            }
            , _logger);
        }

        [HttpPut("post")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Post([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            return await UpdateDocumentState(ids, args, nameof(Post));
        }

        [HttpPut("unpost")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Unpost([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            return await UpdateDocumentState(ids, args, nameof(Unpost));
        }

        [HttpPut("cancel")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Cancel([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            return await UpdateDocumentState(ids, args, nameof(Cancel));
        }

        [HttpPut("uncancel")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Uncancel([FromBody] List<int> ids, [FromQuery] ActionArguments args)
        {
            return await UpdateDocumentState(ids, args, nameof(Uncancel));
        }

        private async Task<ActionResult<EntitiesResponse<Document>>> UpdateDocumentState([FromBody] List<int> ids, [FromQuery] ActionArguments args, string transition)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Parse parameters
                var selectExp = SelectExpression.Parse(args.Select);
                var expandExp = ExpandExpression.Parse(args.Expand);
                var returnEntities = args.ReturnEntities ?? false;
                var idsArray = ids.ToArray();

                // Check user permissions
                await CheckActionPermissions("State", idsArray);

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

                // Update state
                switch (transition)
                {
                    case nameof(Post):
                        await _repo.Documents__Post(ids);
                        break;
                    case nameof(Unpost):
                        await _repo.Documents__Unpost(ids);
                        break;
                    case nameof(Cancel):
                        await _repo.Documents__Cancel(ids);
                        break;
                    case nameof(Uncancel):
                        await _repo.Documents__Uncancel(ids);
                        break;
                    default:
                        throw new BadRequestException($"Unknown transition {transition}");
                }

                if (returnEntities)
                {
                    var response = await GetByIdListAsync(ids.ToArray(), expandExp, selectExp);

                    trx.Complete();
                    return Ok(response);
                }
                else
                {
                    trx.Complete();
                    return Ok();
                }
            }
            , _logger);
        }

        protected override IRepository GetRepository()
        {
            string filter = $"{nameof(Document.DefinitionId)} {Ops.eq} '{DefinitionId}'";
            return new FilteredRepository<Document>(_repo, filter);
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return _repo.UserPermissions(action, View);
        }

        protected override Query<Document> GetAsQuery(List<DocumentForSave> entities)
        {
            return _repo.Documents__AsQuery(DefinitionId, entities);
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
            return DocumentControllerUtil.SearchImpl(query, args, filteredPermissions, map);
        }

        protected override async Task<Dictionary<string, object>> GetExtras(IEnumerable<Document> result)
        {
            var includeRequiredSignature = Request.Query["includeRequiredSignatures"].FirstOrDefault()?.ToString()?.ToLower() == "true";
            if (includeRequiredSignature)
            {
                // DocumentIds parameter
                var docIds = result.Select(doc => new { doc.Id });
                if (!docIds.Any())
                {
                    return await base.GetExtras(result);
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

                var requiredSignatures = await query.ToListAsync();
                var relatedEntities = FlattenAndTrim(requiredSignatures, null);
                requiredSignatures.ForEach(rs => rs.EntityMetadata = null); // Smaller response size

                return new Dictionary<string, object>
                {
                    ["RequiredSignatures"] = requiredSignatures,
                    ["RequiredSignaturesRelatedEntities"] = relatedEntities
                };
            }
            else
            {
                return await base.GetExtras(result);
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
                // Document defaults
                doc.MemoIsCommon ??= docDef.MemoVisibility != null ? doc.MemoIsCommon ?? false : false;
                doc.DebitAgentIsCommon = docDef.DebitAgentVisibility ? doc.DebitAgentIsCommon ?? false : false;
                doc.CreditAgentIsCommon = docDef.CreditAgentVisibility ? doc.CreditAgentIsCommon ?? false : false;
                doc.InvestmentCenterIsCommon = false;
                doc.Time1IsCommon = docDef.Time1Visibility ? doc.Time1IsCommon ?? false : false;
                doc.Time2IsCommon = docDef.Time2Visibility ? doc.Time2IsCommon ?? false : false;
                doc.QuantityIsCommon = docDef.QuantityVisibility ? doc.QuantityIsCommon ?? false : false;
                doc.UnitIsCommon = docDef.UnitVisibility ? doc.UnitIsCommon ?? false : false;
                doc.CurrencyIsCommon = docDef.CurrencyVisibility ? doc.CurrencyIsCommon ?? false : false;

                doc.Clearance ??= 0; // Public
                doc.Lines ??= new List<LineForSave>();

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
                doc.Memo = doc.MemoIsCommon.Value ? doc.Memo : null;
                doc.DebitAgentId = doc.DebitAgentIsCommon.Value ? doc.DebitAgentId : null;
                doc.CreditAgentId = doc.CreditAgentIsCommon.Value ? doc.CreditAgentId : null;
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
                        foreach (var columnDef in lineDef.Columns.Where(c => c.InheritsFromHeader ?? false))
                        {
                            if (columnDef.TableName == Lines)
                            {
                                switch (columnDef.ColumnName)
                                {
                                    case nameof(Line.Memo):
                                        {
                                            if (doc.MemoIsCommon.Value)
                                            {
                                                line.Memo = doc.Memo;
                                            }

                                            break;
                                        }
                                    default:
                                        {
                                            throw new Exception($"Unkown column name '{columnDef.ColumnName}' in table '{columnDef.TableName}'");
                                        }
                                }
                            }
                            else if (columnDef.TableName == Entries)
                            {
                                if (columnDef.EntryIndex >= line.Entries.Count ||
                                    columnDef.EntryIndex >= lineDef.Entries.Count)
                                {
                                    // To avoid index out of bounds exception
                                    continue;
                                }

                                // Copy the common values
                                switch (columnDef.ColumnName)
                                {
                                    case nameof(Entry.AgentId):
                                        var entry = line.Entries[columnDef.EntryIndex];
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

                                    case nameof(Entry.CenterId):
                                        if (doc.InvestmentCenterIsCommon.Value)
                                        {
                                            line.Entries[columnDef.EntryIndex].CenterId = doc.InvestmentCenterId;
                                        }

                                        break;

                                    case nameof(Entry.Time1):
                                        if (doc.Time1IsCommon.Value)
                                        {
                                            line.Entries[columnDef.EntryIndex].Time1 = doc.Time1;
                                        }

                                        break;

                                    case nameof(Entry.Time2):
                                        if (doc.Time2IsCommon.Value)
                                        {
                                            line.Entries[columnDef.EntryIndex].Time2 = doc.Time2;
                                        }

                                        break;

                                    case nameof(Entry.Quantity):
                                        if (doc.QuantityIsCommon.Value)
                                        {
                                            line.Entries[columnDef.EntryIndex].Quantity = doc.Quantity;
                                        }

                                        break;

                                    case nameof(Entry.UnitId):
                                        if (doc.UnitIsCommon.Value)
                                        {
                                            line.Entries[columnDef.EntryIndex].UnitId = doc.UnitId;
                                        }
                                        break;

                                    case nameof(Entry.CurrencyId):
                                        if (doc.CurrencyIsCommon.Value)
                                        {
                                            line.Entries[columnDef.EntryIndex].CurrencyId = doc.CurrencyId;
                                        }

                                        break;

                                    default:
                                        break; // This property doesn't exist on the document, just ignore it
                                }
                            }
                            else
                            {
                                // Developer mistake
                                throw new Exception($"Unrecognized table name '{columnDef.TableName}'");
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

                ////// handle subtle exchange rate rounding bugs
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
                                // TODO: 
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

            // TODO: Add definition validation and defaults here

            ///////// Document Validation
            {
                int docIndex = 0;
                foreach (var doc in docs)
                {
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
                    int lineIndex = 0;
                    foreach (var line in doc.Lines)
                    {
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
                        int entryIndex = 0;
                        foreach (var entry in line.Entries)
                        {
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

                            entryIndex++;
                        }

                        lineIndex++;
                    }

                    ///////// Attachment Validation
                    int attIndex = 0;
                    foreach (var att in doc.Attachments)
                    {
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

                        attIndex++;
                    }

                    docIndex++;
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

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);

            // Post-process errors to move common property errors to the header
            //foreach (var lineDefId in docDef.LineDefinitions.Select(e => e.LineDefinitionId))
            //{
            //    var lineDef = lineDefs.GetValueOrDefault(lineDefId);
            //    if (lineDef == null)
            //    {
            //        continue;
            //    }

            //    var inheritingColumns = lineDef.Columns.Where(c => c.InheritsFromHeader ?? false).ToList();

            //    int docIndex = 0;
            //    docs.ForEach(doc =>
            //    {
            //        var errorsMemo = new HashSet<string>();
            //        var errorsDebitAgent = new HashSet<string>();
            //        var errorsCreditAgent = new HashSet<string>();
            //        var errorsInvestmentCenter = new HashSet<string>();
            //        var errorsTime1 = new HashSet<string>();
            //        var errorsTime2 = new HashSet<string>();
            //        var errorsQuantity = new HashSet<string>();
            //        var errorsUnit = new HashSet<string>();
            //        var errorsCurrency = new HashSet<string>();

            //        foreach (var columnDef in inheritingColumns)
            //        {
            //            int lineIndex = 0;
            //            doc.Lines.ForEach(line =>
            //            {
            //                if (columnDef.TableName == Lines)
            //                {
            //                    switch (columnDef.ColumnName)
            //                    {
            //                        case nameof(Line.Memo):
            //                            if (doc.MemoIsCommon.Value)
            //                            {
            //                                var path = LinePath(docIndex, lineIndex, nameof(Line.Memo));
            //                                if (ModelState.ContainsKey(path))
            //                                {
            //                                    foreach (var lineError in ModelState[path].Errors)
            //                                    {
            //                                        errorsMemo.Add(lineError.ErrorMessage);
            //                                    }

            //                                    ModelState[path].Errors.Clear();
            //                                }
            //                            }

            //                            break;
            //                        default:
            //                            throw new Exception($"Unkown column name '{columnDef.ColumnName}' in table '{columnDef.TableName}'");

            //                    }
            //                }
            //                else if (columnDef.TableName == Entries)
            //                {
            //                    if (columnDef.EntryIndex >= line.Entries.Count ||
            //                        columnDef.EntryIndex >= lineDef.Entries.Count)
            //                    {
            //                        // To avoid index out of bounds exception
            //                        return;
            //                    }

            //                    // Copy the common values
            //                    switch (columnDef.ColumnName)
            //                    {
            //                        case nameof(Entry.AgentId):
            //                            var entry = line.Entries[columnDef.EntryIndex];
            //                            var entryDef = lineDef.Entries[columnDef.EntryIndex];
            //                            if (entryDef.Direction == 1 && doc.DebitAgentIsCommon.Value)
            //                            {
            //                                var path = EntryPath(docIndex, lineIndex, columnDef.EntryIndex, nameof(Entry.AgentId));
            //                                if (ModelState.ContainsKey(path))
            //                                {
            //                                    foreach (var entityError in ModelState[path].Errors)
            //                                    {
            //                                        errorsDebitAgent.Add(entityError.ErrorMessage);
            //                                    }

            //                                    ModelState[path].Errors.Clear();
            //                                }
            //                            }
            //                            else if (entryDef.Direction == -1 && doc.CreditAgentIsCommon.Value)
            //                            {
            //                                var path = EntryPath(docIndex, lineIndex, columnDef.EntryIndex, nameof(Entry.AgentId));
            //                                if (ModelState.ContainsKey(path))
            //                                {
            //                                    foreach (var entityError in ModelState[path].Errors)
            //                                    {
            //                                        errorsCreditAgent.Add(entityError.ErrorMessage);
            //                                    }

            //                                    ModelState[path].Errors.Clear();
            //                                }
            //                            }

            //                            break;

            //                        case nameof(Entry.CenterId):
            //                            if (doc.InvestmentCenterIsCommon.Value)
            //                            {
            //                                var path = EntryPath(docIndex, lineIndex, columnDef.EntryIndex, nameof(Entry.CenterId));
            //                                if (ModelState.ContainsKey(path))
            //                                {
            //                                    foreach (var entityError in ModelState[path].Errors)
            //                                    {
            //                                        errorsInvestmentCenter.Add(entityError.ErrorMessage);
            //                                    }

            //                                    ModelState[path].Errors.Clear();
            //                                }
            //                            }

            //                            break;

            //                        case nameof(Entry.Time1):
            //                            if (doc.Time1IsCommon.Value)
            //                            {
            //                                var path = EntryPath(docIndex, lineIndex, columnDef.EntryIndex, nameof(Entry.Time1));
            //                                if (ModelState.ContainsKey(path))
            //                                {
            //                                    foreach (var entityError in ModelState[path].Errors)
            //                                    {
            //                                        errorsTime1.Add(entityError.ErrorMessage);
            //                                    }

            //                                    ModelState[path].Errors.Clear();
            //                                }
            //                            }

            //                            break;

            //                        case nameof(Entry.Time2):
            //                            if (doc.Time2IsCommon.Value)
            //                            {
            //                                var path = EntryPath(docIndex, lineIndex, columnDef.EntryIndex, nameof(Entry.Time2));
            //                                if (ModelState.ContainsKey(path))
            //                                {
            //                                    foreach (var entityError in ModelState[path].Errors)
            //                                    {
            //                                        errorsTime2.Add(entityError.ErrorMessage);
            //                                    }

            //                                    ModelState[path].Errors.Clear();
            //                                }
            //                            }

            //                            break;

            //                        case nameof(Entry.Quantity):
            //                            if (doc.QuantityIsCommon.Value)
            //                            {
            //                                var path = EntryPath(docIndex, lineIndex, columnDef.EntryIndex, nameof(Entry.Quantity));
            //                                if (ModelState.ContainsKey(path))
            //                                {
            //                                    foreach (var entityError in ModelState[path].Errors)
            //                                    {
            //                                        errorsQuantity.Add(entityError.ErrorMessage);
            //                                    }

            //                                    ModelState[path].Errors.Clear();
            //                                }
            //                            }

            //                            break;

            //                        case nameof(Entry.UnitId):
            //                            if (doc.UnitIsCommon.Value)
            //                            {
            //                                var path = EntryPath(docIndex, lineIndex, columnDef.EntryIndex, nameof(Entry.UnitId));
            //                                if (ModelState.ContainsKey(path))
            //                                {
            //                                    foreach (var entityError in ModelState[path].Errors)
            //                                    {
            //                                        errorsUnit.Add(entityError.ErrorMessage);
            //                                    }

            //                                    ModelState[path].Errors.Clear();
            //                                }
            //                            }
            //                            break;

            //                        case nameof(Entry.CurrencyId):
            //                            if (doc.CurrencyIsCommon.Value)
            //                            {
            //                                var path = EntryPath(docIndex, lineIndex, columnDef.EntryIndex, nameof(Entry.CurrencyId));
            //                                if (ModelState.ContainsKey(path))
            //                                {
            //                                    foreach (var entityError in ModelState[path].Errors)
            //                                    {
            //                                        errorsCurrency.Add(entityError.ErrorMessage);
            //                                    }

            //                                    ModelState[path].Errors.Clear();
            //                                }
            //                            }

            //                            break;

            //                        default:
            //                            break; // This property doesn't exist on the document, just ignore it
            //                    }
            //                }
            //                else
            //                {
            //                    // Developer mistake
            //                    throw new Exception($"Unrecognized table name '{columnDef.TableName}'");
            //                }

            //                lineIndex++;
            //            });
            //        }

            //        // Memo
            //        if (doc.MemoIsCommon ?? false && errorsMemo.Any())
            //        {
            //            var docPath = $"[{docIndex}].{nameof(Document.Memo)}";
            //            if (ModelState.ContainsKey(docPath))
            //            {
            //                foreach (ModelError modelError in ModelState[docPath].Errors)
            //                {
            //                    errorsMemo.Add(modelError.ErrorMessage);
            //                }

            //                ModelState[docPath].Errors.Clear();
            //            }

            //            foreach (string error in errorsMemo)
            //            {
            //                ModelState.AddModelError(docPath, error);
            //            }
            //        }

            //        // DebitAgent
            //        if (doc.DebitAgentIsCommon ?? false && errorsDebitAgent.Any())
            //        {
            //            var docPath = $"[{docIndex}].{nameof(Document.DebitAgentId)}";
            //            foreach (string error in errorsDebitAgent)
            //            {
            //                ModelState.AddModelError(docPath, error);
            //            }
            //        }

            //        // CreditAgent
            //        if (doc.CreditAgentIsCommon ?? false && errorsCreditAgent.Any())
            //        {
            //            var docPath = $"[{docIndex}].{nameof(Document.CreditAgentId)}";
            //            foreach (string error in errorsCreditAgent)
            //            {
            //                ModelState.AddModelError(docPath, error);
            //            }
            //        }

            //        // InvestmentCenter
            //        if (doc.InvestmentCenterIsCommon ?? false && errorsInvestmentCenter.Any())
            //        {
            //            var docPath = $"[{docIndex}].{nameof(Document.InvestmentCenterId)}";
            //            foreach (string error in errorsInvestmentCenter)
            //            {
            //                ModelState.AddModelError(docPath, error);
            //            }
            //        }

            //        // Time1
            //        if (doc.Time1IsCommon ?? false && errorsTime1.Any())
            //        {
            //            var docPath = $"[{docIndex}].{nameof(Document.Time1)}";
            //            foreach (string error in errorsTime1)
            //            {
            //                ModelState.AddModelError(docPath, error);
            //            }
            //        }

            //        // Time2
            //        if (doc.Time2IsCommon ?? false && errorsTime2.Any())
            //        {
            //            var docPath = $"[{docIndex}].{nameof(Document.Time2)}";
            //            foreach (string error in errorsTime2)
            //            {
            //                ModelState.AddModelError(docPath, error);
            //            }
            //        }

            //        // Quantity
            //        if (doc.QuantityIsCommon ?? false && errorsQuantity.Any())
            //        {
            //            var docPath = $"[{docIndex}].{nameof(Document.Quantity)}";
            //            foreach (string error in errorsQuantity)
            //            {
            //                ModelState.AddModelError(docPath, error);
            //            }
            //        }

            //        // Unit
            //        if (doc.UnitIsCommon ?? false && errorsUnit.Any())
            //        {
            //            var docPath = $"[{docIndex}].{nameof(Document.UnitId)}";
            //            foreach (string error in errorsUnit)
            //            {
            //                ModelState.AddModelError(docPath, error);
            //            }
            //        }

            //        // Currency
            //        if (doc.CurrencyIsCommon ?? false && errorsCurrency.Any())
            //        {
            //            var docPath = $"[{docIndex}].{nameof(Document.CurrencyId)}";
            //            foreach (string error in errorsCurrency)
            //            {
            //                ModelState.AddModelError(docPath, error);
            //            }
            //        }

            //        docIndex++;
            //    });
            //}
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

        protected override async Task<List<int>> SaveExecuteAsync(List<DocumentForSave> entities, ExpandExpression expand, bool returnIds)
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
            var (ids, fileIdsToDelete) = await _repo.Documents__SaveAndRefresh(
                DefinitionId,
                documents: entities,
                attachments: attachments,
                returnIds: returnIds);

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
                var fileIdsToDelete = await _repo.Documents__Delete(ids);

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
                var tenantInfo = await _repo.GetTenantInfoAsync();
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
            int tenantId = _tenantIdAccessor.GetTenantId();
            return $"{tenantId}/Attachments/{guid}";
        }
    }

    [Route("api/" + DocumentsController.BASE_ADDRESS)]
    [ApplicationApi]
    public class DocumentsGenericController : FactWithIdControllerBase<Document, int>
    {
        private readonly ApplicationRepository _repo;
        private readonly IDefinitionsCache _definitionsCache;

        public DocumentsGenericController(
            ILogger<DocumentsGenericController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo,
            IDefinitionsCache definitionsCache) : base(logger, localizer)
        {
            _repo = repo;
            _definitionsCache = definitionsCache;
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            // Get all permissions pertaining to documents
            string prefix = DocumentsController.BASE_ADDRESS;
            var permissions = await _repo.GenericUserPermissions(action, prefix);

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

            return DocumentControllerUtil.SearchImpl(query, args, filteredPermissions, prefixMap);
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse($"{nameof(Document.PostingDate)} desc");
        }
    }

    internal class DocumentControllerUtil
    {
        /// <summary>
        /// This is needed in both the generic and specific controllers, so we move it out here
        /// </summary>
        public static Query<Document> SearchImpl(Query<Document> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions, IEnumerable<(string Prefix, string DefinitionId)> prefixMap)
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
    }
}