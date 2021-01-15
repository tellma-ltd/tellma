using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.ImportExport;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.BlobStorage;
using Tellma.Services.MultiTenancy;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS + "{definitionId}")]
    [ApplicationController]
    public class RelationsController : CrudControllerBase<RelationForSave, Relation, int>
    {
        public const string BASE_ADDRESS = "relations/";

        private readonly RelationsService _service;

        public RelationsController(RelationsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpGet("{id}/image")]
        public async Task<ActionResult> GetImage(int id, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var (imageId, imageBytes) = await _service.GetImage(id, cancellation);
                Response.Headers.Add("x-image-id", imageId);
                return File(imageBytes, "image/jpeg");

            }, _logger);
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Relation>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Activate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            }, _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Relation>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Deactivate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            }, _logger);
        }

        [HttpGet("{docId}/attachments/{attachmentId}")]
        public async Task<ActionResult> GetAttachment(int docId, int attachmentId, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                using var _ = _instrumentation.Block("GetAttachment");

                var (fileBytes, fileName) = await _service.GetAttachment(docId, attachmentId, cancellation);
                var contentType = ControllerUtilities.ContentType(fileName);
                return File(fileContents: fileBytes, contentType: contentType, fileName);
            }, _logger);
        }


        protected override CrudServiceBase<RelationForSave, Relation, int> GetCrudService() => _service;
    }

    public class RelationsService : CrudServiceBase<RelationForSave, Relation, int>
    {
        private readonly ApplicationRepository _repo;
        private readonly IBlobService _blobService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IDefinitionsCache _definitionsCache;

        // Shared across multiple methods
        private List<string> _blobsToDelete;
        private List<(string, byte[])> _blobsToSave;

        private int? _definitionIdOverride;

        protected override int? DefinitionId
        {
            get
            {
                if (_definitionIdOverride != null)
                {
                    return _definitionIdOverride;
                }

                string routeDefId = _contextAccessor.HttpContext?.Request?.RouteValues?.GetValueOrDefault("definitionId")?.ToString();
                if (routeDefId != null)
                {
                    if (int.TryParse(routeDefId, out int definitionId))
                    {
                        return definitionId;
                    }
                    else
                    {
                        throw new BadRequestException($"DefinitoinId '{routeDefId}' cannot be parsed into an integer");
                    }
                }

                throw new BadRequestException($"Bug: DefinitoinId could not be determined in {nameof(RelationsService)}");
            }
        }

        private RelationDefinitionForClient Definition() => _definitionsCache.GetCurrentDefinitionsIfCached()?.Data?.Relations?
            .GetValueOrDefault(DefinitionId.Value) ?? throw new InvalidOperationException($"Relation Definition with Id = {DefinitionId} is missing from the cache");

        /// <summary>
        /// Overrides the default behavior of reading the definition Id from the route data
        /// </summary>
        public RelationsService SetDefinitionId(int definitionId)
        {
            _definitionIdOverride = definitionId;
            return this;
        }

        private string View => $"{RelationsController.BASE_ADDRESS}{DefinitionId}";

        public RelationsService(ApplicationRepository repo,
            IBlobService blobService, IHttpContextAccessor contextAccessor,
            IDefinitionsCache definitionsCache, IServiceProvider sp) : base(sp)
        {
            _repo = repo;
            _blobService = blobService;
            _contextAccessor = contextAccessor;
            _definitionsCache = definitionsCache;
        }

        public async Task<(string ImageId, byte[] ImageBytes)> GetImage(int id, CancellationToken cancellation)
        {
            // This enforces read permissions
            var (relation, _) = await GetById(id, new GetByIdArguments { Select = nameof(Relation.ImageId) }, cancellation);
            string imageId = relation.ImageId;

            // Get the blob name
            if (imageId != null)
            {
                // Get the bytes
                string blobName = ImageBlobName(imageId);
                var imageBytes = await _blobService.LoadBlob(blobName, cancellation);

                return (imageId, imageBytes);
            }
            else
            {
                throw new NotFoundException<int>(id);
            }
        }

        public async Task<(byte[] FileBytes, string FileName)> GetAttachment(int docId, int attachmentId, CancellationToken cancellation)
        {
            // This enforces read permissions
            string attachments = nameof(Relation.Attachments);
            var (entity, _) = await GetById(docId, new GetByIdArguments
            {
                Select = $"{attachments}/{nameof(Attachment.FileId)},{attachments}/{nameof(Attachment.FileName)},{attachments}/{nameof(Attachment.FileExtension)}"
            },
            cancellation);

            // Get the blob name
            var attachment = entity?.Attachments?.FirstOrDefault(att => att.Id == attachmentId);
            if (attachment != null && !string.IsNullOrWhiteSpace(attachment.FileId))
            {
                // Get the bytes
                string blobName = AttachmentBlobName(attachment.FileId);
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

        private string ImageBlobName(string guid)
        {
            int tenantId = _tenantIdAccessor.GetTenantId();
            return $"{tenantId}/Relations/{guid}";
        }

        private string AttachmentBlobName(string guid)
        {
            int tenantId = _tenantIdAccessor.GetTenantId();
            return $"{tenantId}/RelationAttachments/{guid}";
        }

        protected override IRepository GetRepository()
        {
            string filter = $"{nameof(Relation.DefinitionId)} eq {DefinitionId}";
            return new FilteredRepository<Relation>(_repo, filter);
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.PermissionsFromCache(View, action, cancellation);
        }

        protected override Query<Relation> Search(Query<Relation> query, GetArguments args)
        {
            return RelationServiceUtil.SearchImpl(query, args);
        }

        protected override async Task<List<RelationForSave>> SavePreprocessAsync(List<RelationForSave> entities)
        {
            var def = Definition();

            // Creating new entities forbidden if the definition is archived
            if (entities.Any(e => e?.Id == 0) && def.State == DefStates.Archived) // Insert
            {
                var msg = _localizer["Error_DefinitionIsArchived"];
                throw new BadRequestException(msg);
            }

            // ... Any definition defaults will go here

            entities.ForEach(entity =>
            {
                // Makes everything that follows easier
                entity.Users ??= new List<RelationUserForSave>();

                if (!(def.HasAttachments ?? false)) {
                    entity.Attachments = null;
                }

                // Contact Email
                if (string.IsNullOrWhiteSpace(entity.ContactEmail))
                {
                    entity.ContactEmail = null;
                }
                else
                {
                    entity.ContactEmail = entity.ContactEmail.ToLower();
                }

                // Contact mobile
                if (string.IsNullOrWhiteSpace(entity.ContactMobile))
                {
                    entity.ContactMobile = null;
                }
                entity.NormalizedContactMobile = ControllerUtilities.ToE164(entity.ContactMobile);
            });

            // Users
            if (def.UserCardinality == Cardinality.None)
            {
                // Remove all users
                entities.ForEach(entity =>
                {
                    entity.Users.Clear();
                });
            }
            else if (def.UserCardinality == Cardinality.Single)
            {
                // Remove all but the first user
                entities.ForEach(entity =>
                {
                    if (entity.Users.Count > 1)
                    {
                        entity.Users = entity.Users.Take(1).ToList();
                    }
                });
            }

            // No location means no location
            if (!IsVisible(def.LocationVisibility))
            {
                entities.ForEach(entity =>
                {
                    entity.LocationJson = null;
                });
            }

            entities.ForEach(ControllerUtilities.SynchronizeWkbWithJson);

            // SQL Preprocessing
            await _repo.Relations__Preprocess(DefinitionId.Value, entities);
            return entities;
        }

        private bool IsVisible(string visibility)
        {
            return visibility == Visibility.Optional || visibility == Visibility.Required;
        }

        protected override async Task SaveValidateAsync(List<RelationForSave> entities)
        {
            var def = Definition();
            var definitionHasAttachments = def.HasAttachments ?? false;
            var userIsRequired = def.UserCardinality != null; // "None" is mapped to null

            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                if (entity.EntityMetadata.LocationJsonParseError != null)
                {
                    ModelState.AddModelError($"[{index}].{nameof(entity.LocationJson)}", entity.EntityMetadata.LocationJsonParseError);
                    if (ModelState.HasReachedMaxErrors)
                    {
                        return;
                    }
                }

                if (entity.Attachments != null && definitionHasAttachments && def.AttachmentsCategoryDefinitionId != null)
                {
                    foreach(var (attachment, attachmentIndex) in entity.Attachments.Select((e, i) => (e, i)))
                    {
                        string path = $"[{index}].{nameof(entity.Attachments)}[{attachmentIndex}].{nameof(attachment.CategoryId)}";
                        string msg = _localizer[Constants.Error_Field0IsRequired, _localizer["Attachment_Category"]];
                        ModelState.AddModelError(path, msg);
                    }
                }

                ///////// Attachment Validation
                if (entity.Attachments != null)
                {
                    foreach (var (att, attIndex) in entity.Attachments.Select((e, i) => (e, i)))
                    {
                        if (att.Id != 0 && att.File != null)
                        {
                            ModelState.AddModelError($"[{index}].{nameof(entity.Attachments)}[{attIndex}]",
                                _localizer["Error_OnlyNewAttachmentsCanIncludeFileBytes"]);
                        }

                        if (att.Id == 0 && att.File == null)
                        {
                            ModelState.AddModelError($"[{index}].{nameof(entity.Attachments)}[{attIndex}]",
                                _localizer["Error_NewAttachmentsMustIncludeFileBytes"]);
                        }

                        if (ModelState.HasReachedMaxErrors)
                        {
                            break;
                        }
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
            var sqlErrors = await _repo.Relations_Validate__Save(DefinitionId.Value, entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<RelationForSave> entities, bool returnIds)
        {
            // The new images
            _blobsToSave = new List<(string, byte[])>();
            _blobsToSave.AddRange(ImageUtilities.ExtractImages(entities, ImageBlobName));
            _blobsToSave.AddRange(AttachmentUtilities.ExtractAttachments(entities, e => e.Attachments, AttachmentBlobName));

            // Save the relations
            var (deletedImageIds, deletedAttachmentIds, ids) = await _repo.Relations__Save(
                DefinitionId.Value,
                entities: entities,
                returnIds: returnIds);

            // Add any attachment Ids that we must delete
            _blobsToDelete = new List<string>();
            _blobsToDelete.AddRange(deletedImageIds.Select(imageId => ImageBlobName(imageId)));
            _blobsToDelete.AddRange(deletedAttachmentIds.Select(attachmentId => AttachmentBlobName(attachmentId)));

            return ids;
        }

        protected override async Task NonTransactionalSideEffectsForSave(List<RelationForSave> entities, List<Relation> data)
        {
            // Delete the blobs retrieved earlier
            if (_blobsToDelete.Any())
            {
                await _blobService.DeleteBlobsAsync(_blobsToDelete);
            }

            // Save new blobs if any
            if (_blobsToSave.Any())
            {
                await _blobService.SaveBlobsAsync(_blobsToSave);
            }
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Relations_Validate__Delete(DefinitionId.Value, ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            // For the entities we're about to delete retrieve their imageIds (if any) to delete from the blob storage
            var dbEntitiesWithImageIds = await _repo.Relations
                .Select(nameof(Relation.ImageId))
                .Filter($"{nameof(Relation.ImageId)} ne null")
                .FilterByIds(ids.ToArray())
                .ToListAsync(cancellation: default);

            var blobsToDelete = dbEntitiesWithImageIds
                .Select(e => ImageBlobName(e.ImageId))
                .ToList();

            try
            {
                using var trx = ControllerUtilities.CreateTransaction();
                await _repo.Relations__Delete(ids);

                if (blobsToDelete.Any())
                {
                    await _blobService.DeleteBlobsAsync(blobsToDelete);
                }

                trx.Complete();
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

        protected override ExpressionSelect ParseSelect(string select) => RelationServiceUtil.ParseSelect(select, baseFunc: base.ParseSelect);

        public Task<(List<Relation>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<Relation>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<Relation>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.Relations__Activate(ids, isActive);

            List<Relation> data = null;
            Extras extras = null;

            if (args.ReturnEntities ?? false)
            {
                (data, extras) = await GetByIds(ids, args, action, cancellation: default);
            }

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, data);

            trx.Complete();
            return (data, extras);
        }

        protected override MappingInfo ProcessDefaultMapping(MappingInfo mapping)
        {
            // Remove the RoleId property from the template, it's supposed to be hidden
            var wkbProp = mapping.SimpleProperty(nameof(Resource.LocationWkb));

            if (wkbProp != null)
            {
                mapping.SimpleProperties = mapping.SimpleProperties.Where(p => p != wkbProp);
            }

            //// Remove the attachments, since they cannot be exported and imported in CSV or Excel
            //var attachments = mapping.CollectionProperty(nameof(RelationForSave.Attachments));
            //mapping.CollectionProperties = mapping.CollectionProperties.Where(p => p != attachments);

            // Fix the newly created gaps, if any
            mapping.NormalizeIndices();

            return base.ProcessDefaultMapping(mapping);
        }
    }

    [Route("api/" + RelationsController.BASE_ADDRESS)]
    [ApplicationController]
    public class RelationsGenericController : FactWithIdControllerBase<Relation, int>
    {
        private readonly RelationsGenericService _service;

        public RelationsGenericController(RelationsGenericService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override FactWithIdServiceBase<Relation, int> GetFactWithIdService()
        {
            return _service;
        }
    }

    public class RelationsGenericService : FactWithIdServiceBase<Relation, int>
    {
        private readonly ApplicationRepository _repo;

        public RelationsGenericService(IServiceProvider sp, ApplicationRepository repo) : base(sp)
        {
            _repo = repo;
        }

        protected override IRepository GetRepository() => _repo;

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            // Get all permissions pertaining to relations
            string prefix = RelationsController.BASE_ADDRESS;
            var permissions = await _repo.GenericPermissionsFromCache(prefix, action, cancellation);

            // Massage the permissions by adding DefinitionId = definitionId as an extra clause 
            // (since the controller will not filter the results per any specific definition Id)
            foreach (var permission in permissions.Where(e => e.View != "all"))
            {
                string definitionIdString = permission.View.Remove(0, prefix.Length).Replace("'", "''");
                if (!int.TryParse(definitionIdString, out int definitionId))
                {
                    throw new BadRequestException($"Could not parse definition Id {definitionIdString} to a valid integer");
                }

                string definitionPredicate = $"{nameof(Relation.DefinitionId)} eq {definitionId}";
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

        protected override Query<Relation> Search(Query<Relation> query, GetArguments args)
        {
            return RelationServiceUtil.SearchImpl(query, args);
        }

        protected override ExpressionSelect ParseSelect(string select) => RelationServiceUtil.ParseSelect(select, baseFunc: base.ParseSelect);
    }

    internal class RelationServiceUtil
    {
        /// <summary>
        /// This is needed in both the generic and specific controllers, so we move it out here
        /// </summary>
        public static Query<Relation> SearchImpl(Query<Relation> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Relation.Name);
                var name2 = nameof(Relation.Name2);
                var name3 = nameof(Relation.Name3);
                var code = nameof(Relation.Code);
                var tin = nameof(Relation.TaxIdentificationNumber);

                var filterString = $"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}' or {tin} contains '{search}'";
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return query;
        }

        public static ExpressionSelect ParseSelect(string select, Func<string, ExpressionSelect> baseFunc)
        {
            string shorthand = "$DocumentDetails";
            if (select == null)
            {
                return null;
            }
            else
            {
                select = select.Replace(shorthand, _documentDetailsSelect);
                return baseFunc(select);
            }
        }

        private static readonly string _documentDetailsSelect = string.Join(',', DocDetails.RelationPaths());
    }
}