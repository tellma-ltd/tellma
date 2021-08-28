using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.ImportExport;
using Tellma.Api.Metadata;
using Tellma.Model.Application;
using Tellma.Repository.Common;
using Tellma.Utilities.Blobs;

namespace Tellma.Api
{
    public class AgentsService : CrudServiceBase<AgentForSave, Agent, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer _localizer;
        private readonly IBlobService _blobService;

        // Shared across multiple methods
        private List<string> _blobsToDelete;
        private List<(string, byte[])> _blobsToSave;

        public AgentsService(
            ApplicationFactServiceBehavior behavior,
            CrudServiceDependencies deps,
            IBlobService blobService) : base(deps)
        {
            _behavior = behavior;
            _blobService = blobService;
            _localizer = deps.Localizer;
        }

        protected override string View => $"agents/{DefinitionId}";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        /// <summary>
        /// The current <see cref="DefinitionId"/>, if null throws an exception.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private new int DefinitionId => base.DefinitionId ??
            throw new InvalidOperationException($"DefinitionId was not set in {nameof(AgentsService)}.");

        /// <summary>
        /// The current TenantId.
        /// </summary>
        private new int TenantId => _behavior.TenantId;

        /// <summary>
        /// Helper method for retrieving the <see cref="AgentDefinitionForClient"/> 
        /// that corresponds to the current <see cref="DefinitionId"/>.
        /// </summary>
        /// <param name="cancellation">The cancellation instruction.</param>
        private async Task<AgentDefinitionForClient> Definition(CancellationToken cancellation = default)
        {
            var defs = await _behavior.Definitions(cancellation);
            var docDef = defs.Agents.GetValueOrDefault(DefinitionId) ??
                throw new InvalidOperationException($"Agent definition with Id = {DefinitionId} could not be found.");

            return docDef;
        }

        public async Task<ImageResult> GetImage(int id, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // This enforces read permissions
            var result = await GetById(id, new GetByIdArguments { Select = nameof(Agent.ImageId) }, cancellation);
            var agent = result.Entity;
            string imageId = agent.ImageId;

            // Get the blob name
            if (imageId != null)
            {
                try
                {
                    // Get the bytes
                    string blobName = ImageBlobName(imageId);
                    var imageBytes = await _blobService.LoadBlob(TenantId, blobName, cancellation);

                    return new ImageResult(imageId, imageBytes);
                }
                catch (BlobNotFoundException)
                {
                    throw new NotFoundException<int>(id);
                }
            }
            else
            {
                throw new NotFoundException<int>(id);
            }
        }

        public async Task<FileResult> GetAttachment(int docId, int attachmentId, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // This enforces read permissions
            string attachments = nameof(Agent.Attachments);
            var result = await GetById(docId, new GetByIdArguments
            {
                Select = $"{attachments}.{nameof(Attachment.FileId)},{attachments}.{nameof(Attachment.FileName)},{attachments}.{nameof(Attachment.FileExtension)}"
            },
            cancellation);

            // Get the blob name
            var attachment = result.Entity?.Attachments?.FirstOrDefault(att => att.Id == attachmentId);
            if (attachment != null && !string.IsNullOrWhiteSpace(attachment.FileId))
            {
                try
                {
                    // Get the bytes
                    string blobName = AttachmentBlobName(attachment.FileId);
                    var fileBytes = await _blobService.LoadBlob(TenantId, blobName, cancellation);

                    // Get the content type
                    var fileName = $"{attachment.FileName ?? "Attachment"}.{attachment.FileExtension}";
                    return new FileResult(fileBytes, fileName);
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

        private static string ImageBlobName(string guid)
        {
            return $"Agents/{guid}";
        }

        private static string AttachmentBlobName(string guid)
        {
            return $"AgentAttachments/{guid}";
        }

        protected override Task<EntityQuery<Agent>> Search(EntityQuery<Agent> query, GetArguments args, CancellationToken cancellation)
        {
            return AgentServiceUtil.SearchImpl(query, args, cancellation);
        }

        protected override async Task<List<AgentForSave>> SavePreprocessAsync(List<AgentForSave> entities)
        {
            var def = await Definition();

            // Creating new entities forbidden if the definition is archived
            if (entities.Any(e => e?.Id == 0) && def.State == DefStates.Archived) // Insert
            {
                var msg = _localizer["Error_DefinitionIsArchived"];
                throw new ServiceException(msg);
            }

            // ... Any definition defaults will go here

            entities.ForEach(entity =>
            {
                // Makes everything that follows easier
                entity.Users ??= new List<AgentUserForSave>();

                if (!(def.HasAttachments ?? false))
                {
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

                entity.NormalizedContactMobile = BaseUtil.ToE164(entity.ContactMobile);
            });

            // Users
            if (def.UserCardinality != Cardinality.Multiple)
            {
                // Remove all users if not multiple
                entities.ForEach(entity =>
                {
                    entity.Users.Clear();
                });
            }
            else if (def.UserCardinality == Cardinality.None)
            {
                // Remove the header user if not multiple or single
                entities.ForEach(entity =>
                {
                    entity.UserId = null;
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

            entities.ForEach(BaseUtil.SynchronizeWkbWithJson);

            // SQL Preprocessing
            await _behavior.Repository.Agents__Preprocess(DefinitionId, entities, userId: UserId);
            return entities;
        }

        private static bool IsVisible(string visibility)
        {
            return visibility == Visibility.Optional || visibility == Visibility.Required;
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<AgentForSave> entities, bool returnIds)
        {
            var def = await Definition();
            var definitionHasAttachments = def.HasAttachments ?? false;
            var userIsRequired = def.UserCardinality != null; // "None" is mapped to null

            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                if (entity.EntityMetadata.LocationJsonParseError != null)
                {
                    ModelState.AddError($"[{index}].{nameof(entity.LocationJson)}", entity.EntityMetadata.LocationJsonParseError);
                }

                if (entity.Attachments != null && definitionHasAttachments && def.AttachmentsCategoryDefinitionId != null)
                {
                    foreach (var (attachment, attachmentIndex) in entity.Attachments.Select((e, i) => (e, i)))
                    {
                        string path = $"[{index}].{nameof(entity.Attachments)}[{attachmentIndex}].{nameof(attachment.CategoryId)}";
                        string msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Attachment_Category"]];
                        ModelState.AddError(path, msg);
                    }
                }

                ///////// Attachment Validation
                if (entity.Attachments != null)
                {
                    foreach (var (att, attIndex) in entity.Attachments.Select((e, i) => (e, i)))
                    {
                        if (att.Id != 0 && att.File != null)
                        {
                            ModelState.AddError($"[{index}].{nameof(entity.Attachments)}[{attIndex}]",
                                _localizer["Error_OnlyNewAttachmentsCanIncludeFileBytes"]);
                        }

                        if (att.Id == 0 && att.File == null)
                        {
                            ModelState.AddError($"[{index}].{nameof(entity.Attachments)}[{attIndex}]",
                                _localizer["Error_NewAttachmentsMustIncludeFileBytes"]);
                        }
                    }
                }
            }

            #region Save

            // The new images
            _blobsToSave = new List<(string, byte[])>();
            _blobsToSave.AddRange(BaseUtil.ExtractImages(entities, ImageBlobName));
            _blobsToSave.AddRange(BaseUtil.ExtractAttachments(entities, e => e.Attachments, AttachmentBlobName));

            // Save the agents
            (SaveWithImagesOutput result, List<string> deletedAttachmentIds) = await _behavior.Repository.Agents__Save(
                    definitionId: DefinitionId,
                    entities: entities,
                    returnIds: returnIds,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            // Validation
            AddErrorsAndThrowIfInvalid(result.Errors);

            // Add any attachment Ids that we must delete
            _blobsToDelete = new List<string>();
            _blobsToDelete.AddRange(result.DeletedImageIds.Select(ImageBlobName));
            _blobsToDelete.AddRange(deletedAttachmentIds.Select(AttachmentBlobName));

            return result.Ids;

            #endregion
        }

        protected override async Task NonTransactionalSideEffectsForSave(List<AgentForSave> entities, IReadOnlyList<Agent> data)
        {
            // Delete the blobs retrieved earlier
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
            var blobsToDelete = new List<string>(); // Both image Ids and attachment Ids

            (DeleteWithImagesOutput result, List<string> deletedAttachmentIds) = await _behavior.Repository.Agents__Delete(
                    definitionId: DefinitionId,
                    ids: ids,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            // Validation
            AddErrorsAndThrowIfInvalid(result.Errors);

            blobsToDelete.AddRange(result.DeletedImageIds.Select(ImageBlobName));
            blobsToDelete.AddRange(deletedAttachmentIds.Select(AttachmentBlobName));

            if (blobsToDelete.Any())
            {
                await _blobService.DeleteBlobsAsync(TenantId, blobsToDelete);
            }
        }

        protected override ExpressionSelect ParseSelect(string select) => AgentServiceUtil.ParseSelect(select, baseFunc: base.ParseSelect);

        public Task<EntitiesResult<Agent>> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<EntitiesResult<Agent>> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<EntitiesResult<Agent>> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            await Initialize();

            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute
            using var trx = TransactionFactory.ReadCommitted();
            OperationOutput output = await _behavior.Repository.Agents__Activate(
                    definitionId: DefinitionId,
                    ids: ids,
                    isActive: isActive,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            // Validate
            AddErrorsAndThrowIfInvalid(output.Errors);

            var result = (args.ReturnEntities ?? false) ?
                await GetByIds(ids, args, action, cancellation: default) :
                EntitiesResult<Agent>.Empty();

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, result.Data);

            trx.Complete();
            return result;
        }

        protected override MappingInfo ProcessDefaultMapping(MappingInfo mapping)
        {
            // Remove the LocationWkb property from the template
            var wkbProp = mapping.SimplePropertyByName(nameof(Agent.LocationWkb));
            if (wkbProp != null)
            {
                mapping.SimpleProperties = mapping.SimpleProperties.Where(p => p != wkbProp);
            }

            // Remove the attachments, since they cannot be exported and imported in CSV or Excel
            var attachments = mapping.CollectionPropertyByName(nameof(Agent.Attachments));
            mapping.CollectionProperties = mapping.CollectionProperties.Where(p => p != attachments);

            // Fix the newly created gaps, if any
            mapping.NormalizeIndices();

            return base.ProcessDefaultMapping(mapping);
        }
    }

    public class AgentsGenericService : FactWithIdServiceBase<Agent, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IPermissionsCache _permissionsCache;

        public AgentsGenericService(ApplicationFactServiceBehavior behavior,
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
            // Get all permissions pertaining to agents
            string prefix = "agents/";
            var permissions = (await _permissionsCache
                .GenericPermissionsFromCache(
                    tenantId: _behavior.TenantId,
                    userId: UserId,
                    version: _behavior.PermissionsVersion,
                    viewPrefix: prefix,
                    action: action,
                    cancellation: cancellation)).ToList();

            // Massage the permissions by adding DefinitionId = definitionId as an extra clause 
            // (since the controller will not filter the results per any specific definition Id)
            foreach (var permission in permissions.Where(e => e.View != "all"))
            {
                string definitionIdString = permission.View.Remove(0, prefix.Length);
                if (!int.TryParse(definitionIdString, out int definitionId))
                {
                    throw new ServiceException($"Could not parse definition Id '{definitionIdString}' to a valid integer.");
                }

                string definitionPredicate = $"{nameof(Agent.DefinitionId)} eq {definitionId}";
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

        protected override Task<EntityQuery<Agent>> Search(EntityQuery<Agent> query, GetArguments args, CancellationToken cancellation)
        {
            return AgentServiceUtil.SearchImpl(query, args, cancellation);
        }

        protected override ExpressionSelect ParseSelect(string select) => AgentServiceUtil.ParseSelect(select, baseFunc: base.ParseSelect);
    }

    internal class AgentServiceUtil
    {
        private static readonly string _documentDetailsSelect = string.Join(',', DocDetails.EntryAgentPaths());

        /// <summary>
        /// This is needed in both the generic and specific controllers, so we move it out here
        /// </summary>
        public static Task<EntityQuery<Agent>> SearchImpl(EntityQuery<Agent> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Agent.Name);
                var name2 = nameof(Agent.Name2);
                var name3 = nameof(Agent.Name3);
                var code = nameof(Agent.Code);
                var tin = nameof(Agent.TaxIdentificationNumber);

                var filterString = $"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}' or {tin} contains '{search}'";
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return Task.FromResult(query);
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
    }
}
