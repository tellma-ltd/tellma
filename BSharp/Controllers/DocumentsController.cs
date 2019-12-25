using BSharp.Controllers.Dto;
using BSharp.Controllers.Utilities;
using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.Entities;
using BSharp.Services.BlobStorage;
using BSharp.Services.ClientInfo;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers
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

        private string ViewId => $"{BASE_ADDRESS}{DefinitionId}";

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

        [HttpPut("assign")]
        public async Task<ActionResult<EntitiesResponse<Document>>> Assign([FromBody] List<int> ids, [FromQuery] AssignArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Parse parameters
                var selectExp = SelectExpression.Parse(args.Expand);
                var expandExp = ExpandExpression.Parse(args.Expand);
                var idsArray = ids.ToArray();

                // TODO: Check user permissions
                // await CheckActionPermissions("IsActive", idsArray);

                // Execute and return
                using (var trx = ControllerUtilities.CreateTransaction())
                {
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
            }
            , _logger);
        }

        [HttpPut("sign-lines")]
        public async Task<ActionResult<EntitiesResponse<Document>>> SignLines([FromBody] List<int> ids, [FromQuery] SignArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // Parse parameters
                var selectExp = SelectExpression.Parse(args.Expand);
                var expandExp = ExpandExpression.Parse(args.Expand);
                var idsArray = ids.ToArray();

                // TODO: Check user permissions
                // await CheckActionPermissions("IsActive", idsArray);

                // Execute and return
                using (var trx = ControllerUtilities.CreateTransaction())
                {
                    // TODO: Validate sign

                    var documentIds = await _repo.Lines__Sign(
                        ids, 
                        args.ToState, 
                        args.ReasonId, 
                        args.ReasonDetails, 
                        args.OnBehalfOfUserId, 
                        args.RoleId, 
                        args.SignedAt);

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
            return _repo.UserPermissions(action, ViewId);
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
                doc.MemoIsCommon = doc.MemoIsCommon ?? true;
                doc.Lines = doc.Lines ?? new List<LineForSave>();

                doc.Lines.ForEach(line =>
                {
                    // Line defaults
                    line.Entries = line.Entries ?? new List<EntryForSave>();

                    line.Entries.ForEach(entry =>
                    {
                        // Entry defaults
                        entry.EntryNumber = entry.EntryNumber ?? 0;
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
            return await _repo.Documents__Preprocess(DefinitionId, entities);
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
            // Save the documents
            var ids = await _repo.Documents__Save(
                DefinitionId,
                documents: entities,
                returnIds: returnIds);

            // Assign new documents to the current user
            var userInfo = await _repo.GetUserInfoAsync();
            var currentUserId = userInfo.UserId.Value;
            var newDocIds = entities.Select((doc, index) => (doc, index)).Where(e => e.doc.Id == 0).Select(e => ids[e.index]);
            await _repo.Documents__Assign(newDocIds, currentUserId, null);

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
                await _repo.Documents__Delete(ids);
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
            foreach (var permission in permissions.Where(e => e.ViewId != "all"))
            {
                string definitionId = permission.ViewId.Remove(0, prefix.Length).Replace("'", "''");
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