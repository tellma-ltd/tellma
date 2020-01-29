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

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS + "{definitionId}")]
    [ApplicationApi]
    public class DocumentsController : CrudControllerBase<DocumentForSave, Document, int>
    {
        public const string BASE_ADDRESS = "documents/";

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
                    Select = $"{attachments}/{nameof(Attachment.FileId)},{attachments}/{nameof(Attachment.FileName)}"
                });

                // Get the blob name
                var attachment = response.Result.Attachments?.FirstOrDefault(att => att.Id == attachmentId);
                if (attachment != null && !string.IsNullOrWhiteSpace(attachment.FileId))
                {
                    // Get the bytes
                    string blobName = BlobName(attachment.FileId);
                    var fileBytes = await _blobService.LoadBlob(blobName);
                    var fileName = attachment.FileName ?? "Attachment";
                    var contentType = ContentType(fileName);

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
                var idsArray = ids.ToArray();

                // TODO: Check user permissions
                // await CheckActionPermissions("IsActive", idsArray);

                // Execute and return
                using var trx = ControllerUtilities.CreateTransaction();
                // TODO: Validate assign

                await _repo.Documents__Assign(ids, args.AssigneeId, args.Comment);

                if (args.ReturnEntities ?? false)
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
        public async Task<ActionResult<EntitiesResponse<Document>>> SignLines([FromBody] List<int> ids, [FromQuery] SignArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Parse parameters
                var selectExp = SelectExpression.Parse(args.Select);
                var expandExp = ExpandExpression.Parse(args.Expand);
                var idsArray = ids.ToArray();

                // TODO: Check user permissions
                // await CheckActionPermissions("IsActive", idsArray);

                // Execute and return
                using var trx = ControllerUtilities.CreateTransaction();
                // TODO: Validate sign

                var documentIds = await _repo.Lines__Sign(
                    ids,
                    args.ToState,
                    args.ReasonId,
                    args.ReasonDetails,
                    args.OnBehalfOfUserId,
                    args.RoleId,
                    args.SignedAt ?? DateTimeOffset.Now);

                if (args.ReturnEntities ?? false)
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

        protected override Query<Document> Search(Query<Document> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return DocumentControllerUtil.SearchImpl(query, args, filteredPermissions);
        }

        protected override async Task<List<DocumentForSave>> SavePreprocessAsync(List<DocumentForSave> entities)
        {
            // Set default values
            entities.ForEach(doc =>
            {
                // Document defaults
                doc.MemoIsCommon ??= true;
                doc.Lines ??= new List<LineForSave>();

                doc.Lines.ForEach(line =>
                {
                    // Line defaults
                    line.Entries ??= new List<EntryForSave>();

                    line.Entries.ForEach(entry =>
                    {
                        // Entry defaults
                        entry.EntryNumber ??= 0;
                    });
                });
            });

            // Set common header values on the lines
            entities.ForEach(doc =>
            {
                if (doc.MemoIsCommon.Value)
                {
                    doc.Lines.ForEach(line => line.Memo = doc.Memo);
                }
            });

            // SQL server preprocessing
            await _repo.Documents__Preprocess(DefinitionId, entities);
            return entities;
        }

        protected override async Task SaveValidateAsync(List<DocumentForSave> docs)
        {
            // TODO: Add definition validation and defaults here

            // Find lines with duplicate Ids
            var duplicateLineIds = docs.SelectMany(doc => doc.Lines) // All lines
                .Where(line => line.Id != 0).GroupBy(line => line.Id).Where(g => g.Count() > 1) // Duplicate Ids
                .SelectMany(g => g).ToDictionary(line => line, line => line.Id); // to dictionary

            // Find entries with duplicate Ids
            var duplicateEntryIds = docs.SelectMany(doc => doc.Lines).SelectMany(line => line.Entries)
                .Where(entry => entry.Id != 0).GroupBy(entry => entry.Id).Where(g => g.Count() > 1)
                .SelectMany(g => g).ToDictionary(entry => entry, entry => entry.Id);

            var settings = _settingsCache.GetCurrentSettingsIfCached().Data;

            ///////// Document Validation
            int docIndex = 0;
            foreach (var doc in docs)
            {
                // Check that the date is not in the future
                if (doc.DocumentDate > DateTime.Today.AddDays(1))
                {
                    ModelState.AddModelError($"[{docIndex}].{nameof(doc.DocumentDate)}",
                        _localizer["Error_DateCannotBeInTheFuture"]);
                }

                // Check that the date is not before archive date
                if (doc.DocumentDate <= settings.ArchiveDate)
                {
                    ModelState.AddModelError($"[{docIndex}].{nameof(doc.DocumentDate)}",
                        _localizer["Error_DateCannotBeBeforeArchiveDate0", settings.ArchiveDate.ToString("yyyy-MM-dd")]);
                }

                ///////// Line Validation
                int lineIndex = 0;
                foreach (var line in doc.Lines)
                {
                    // Prevent duplicate line Ids
                    if (duplicateLineIds.ContainsKey(line))
                    {
                        // This error indicates a bug
                        var id = duplicateLineIds[line];
                        ModelState.AddModelError($"[{docIndex}].{nameof(doc.Lines)}[{lineIndex}].{nameof(line.Id)}",
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
                            ModelState.AddModelError($"[{docIndex}].{nameof(doc.Lines)}[{lineIndex}].{nameof(line.Entries)}[{entryIndex}].{nameof(entry.Id)}",
                                _localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", id]);
                        }

                        // If the currency is functional, value must equal monetary value
                        if (entry.CurrencyId == settings.FunctionalCurrencyId && entry.Value != entry.MonetaryValue)
                        {
                            var currencyDesc = _tenantInfoAccessor.GetCurrentInfo()
                                .Localize(settings.FunctionalCurrencyDescription,
                                            settings.FunctionalCurrencyDescription2,
                                            settings.FunctionalCurrencyDescription3);

                            // TODO: Use the proper field name from definition, instead of "Amount"
                            ModelState.AddModelError($"[{docIndex}].{nameof(doc.Lines)}[{lineIndex}].{nameof(line.Entries)}[{entryIndex}].{nameof(entry.MonetaryValue)}",
                                _localizer["TheAmount0DoesNotMatchTheValue1EvenThoughBothIn2", entry.MonetaryValue ?? 0, entry.Value ?? 0, currencyDesc]);
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
            var (ids, fileIdsToDelete) = await _repo.Documents__Save(
                DefinitionId,
                documents: entities,
                attachments: attachments,
                returnIds: returnIds);

            // Assign new documents to the current user
            var userInfo = await _repo.GetUserInfoAsync();
            var currentUserId = userInfo.UserId.Value;
            var newDocIds = entities.Select((doc, index) => (doc, index)).Where(e => e.doc.Id == 0).Select(e => ids[e.index]);
            await _repo.Documents__Assign(newDocIds, currentUserId, null);

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

        public DocumentsGenericController(
            ILogger<DocumentsGenericController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _repo = repo;
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
            return DocumentControllerUtil.SearchImpl(query, args, filteredPermissions);
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse($"{nameof(Document.DocumentDate)} desc");
        }
    }

    internal class DocumentControllerUtil
    {
        /// <summary>
        /// This is needed in both the generic and specific controllers, so we move it out here
        /// </summary>
        public static Query<Document> SearchImpl(Query<Document> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                // TODO:
                // IF the search starts with the serial prefix (case insensitive) then search the serial numbers exclusively 

                // ELSE: search the memo etc normally
                var memo = nameof(Document.Memo);
                var serialNumber = nameof(Document.SerialNumber);
                var filterString = $"{memo} {Ops.contains} '{search}'";

                // If the search is a number, include the result with that serial number
                if (int.TryParse(search.Trim(), out int searchNumber))
                {
                    filterString = $"{filterString} or {serialNumber} eq {searchNumber}";
                }

                query = query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }
    }
}