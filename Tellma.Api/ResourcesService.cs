using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.ImportExport;
using Tellma.Model.Application;
using Tellma.Repository.Common;
using Tellma.Utilities.Blobs;

namespace Tellma.Api
{
    public class ResourcesService : CrudServiceBase<ResourceForSave, Resource, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer _localizer;
        private readonly IBlobService _blobService;

        // Shared across multiple methods
        private List<string> _blobsToDelete;
        private List<(string, byte[])> _blobsToSave;

        public ResourcesService(
            ApplicationFactServiceBehavior behavior,
            CrudServiceDependencies deps,
            IBlobService blobService) : base(deps)
        {
            _behavior = behavior;
            _blobService = blobService;
            _localizer = deps.Localizer;
        }

        protected override string View => $"resources/{DefinitionId}";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        /// <summary>
        /// The current <see cref="DefinitionId"/>, if null throws an exception.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private new int DefinitionId => base.DefinitionId ??
            throw new InvalidOperationException($"DefinitionId was not set in {nameof(ResourcesService)}.");

        /// <summary>
        /// The current TenantId.
        /// </summary>
        private new int TenantId => _behavior.TenantId;

        /// <summary>
        /// Helper method for retrieving the <see cref="ResourceDefinitionForClient"/> 
        /// that corresponds to the current <see cref="DefinitionId"/>.
        /// </summary>
        /// <param name="cancellation">The cancellation instruction.</param>
        private async Task<ResourceDefinitionForClient> Definition(CancellationToken cancellation = default)
        {
            var defs = await _behavior.Definitions(cancellation);
            var docDef = defs.Resources.GetValueOrDefault(DefinitionId) ??
                throw new InvalidOperationException($"Resource definition with Id = {DefinitionId} could not be found.");

            return docDef;
        }

        public async Task<(string imageId, byte[] imageBytes)> GetImage(int id, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // This enforces read permissions
            var (resource, _) = await GetById(id, new GetByIdArguments { Select = nameof(Resource.ImageId) }, cancellation);
            string imageId = resource.ImageId;

            // Get the blob name
            if (imageId != null)
            {
                try
                {
                    // Get the bytes
                    string blobName = ImageBlobName(imageId);
                    var imageBytes = await _blobService.LoadBlob(TenantId, blobName, cancellation);

                    return (imageId, imageBytes);
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

        private static string ImageBlobName(string guid)
        {
            return $"Resources/{guid}";
        }

        protected override Task<EntityQuery<Resource>> Search(EntityQuery<Resource> query, GetArguments args, CancellationToken cancellation)
        {
            return ResourceServiceUtil.SearchImpl(query, args, cancellation);
        }

        protected override async Task<List<ResourceForSave>> SavePreprocessAsync(List<ResourceForSave> entities)
        {
            var def = await Definition();
            var settings = await _behavior.Settings();

            // Creating new entities forbidden if the definition is archived
            if (entities.Any(e => e?.Id == 0) && def.State == DefStates.Archived) // Insert
            {
                var msg = _localizer["Error_DefinitionIsArchived"];
                throw new ServiceException(msg);
            }

            // Set defaults
            entities.ForEach(entity =>
            {
                // Makes everything that follows easier
                entity.Units ??= new List<ResourceUnitForSave>();

                entity.UnitId ??= def.DefaultUnitId;
                entity.UnitMassUnitId ??= def.DefaultUnitMassUnitId;
                entity.VatRate ??= def.DefaultVatRate;

                if (def.CurrencyVisibility == null)
                {
                    entity.CurrencyId ??= settings.FunctionalCurrencyId;
                }
            });

            // Unit + Units
            if (def.UnitCardinality != Cardinality.Multiple)
            {
                // Remove units
                entities.ForEach(entity =>
                {
                    entity.Units.Clear();
                });

                // If cardinality is "None", SQL server will set UnitId to pure
            }

            var functionalId = settings.FunctionalCurrencyId;

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
            await _behavior.Repository.Resources__Preprocess(DefinitionId, entities, UserId);
            return entities;
        }

        private static bool IsVisible(string visibility)
        {
            return visibility == Visibility.Optional || visibility == Visibility.Required;
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<ResourceForSave> entities, bool returnIds)
        {
            var def = await Definition();
            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                if (entity.EntityMetadata.LocationJsonParseError != null)
                {
                    ModelState.AddError($"[{index}].{nameof(entity.LocationJson)}", entity.EntityMetadata.LocationJsonParseError);
                }

                if (entity.VatRate < 0m || entity.VatRate > 1m)
                {
                    var path = $"[{index}].{nameof(Resource.VatRate)}";
                    var msg = _localizer["Error_VatRateMustBeBetweenZeroAndOne"];

                    ModelState.AddError(path, msg);
                }

                if (entity.CurrencyId == null && def.CurrencyVisibility != null)
                {
                    var path = $"[{index}].{nameof(Resource.CurrencyId)}";
                    var msg = _localizer[Metadata.ErrorMessages.Error_Field0IsRequired, _localizer["Entity_Currency"]];

                    ModelState.AddError(path, msg);
                }
            }

            // The new images
            _blobsToSave = BaseUtil.ExtractImages(entities, ImageBlobName).ToList();

            // Save the Resources
            SaveWithImagesResult result = await _behavior.Repository.Resources__Save(
                    definitionId: DefinitionId,
                    entities: entities,
                    returnIds: returnIds,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            // Validation
            AddErrorsAndThrowIfInvalid(result.Errors);

            // Add any attachment Ids that we must delete
            _blobsToDelete = result.DeletedImageIds.Select(ImageBlobName).ToList();

            return result.Ids;
        }

        protected override async Task NonTransactionalSideEffectsForSave(List<ResourceForSave> entities, List<Resource> data)
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
            List<string> blobsToDelete; // Both image Ids and attachment Ids

            DeleteWithImagesResult result = await _behavior.Repository.Resources__Delete(
                definitionId: DefinitionId,
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            // Validation
            AddErrorsAndThrowIfInvalid(result.Errors);

            blobsToDelete = result.DeletedImageIds.Select(ImageBlobName).ToList();

            if (blobsToDelete.Any())
            {
                await _blobService.DeleteBlobsAsync(TenantId, blobsToDelete);
            }
        }

        protected override ExpressionSelect ParseSelect(string select) => ResourceServiceUtil.ParseSelect(select, baseFunc: base.ParseSelect);

        public Task<(List<Resource>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<Resource>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<Resource>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            await Initialize();

            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            OperationResult result = await _behavior.Repository.Resources__Activate(
                    definitionId: DefinitionId,
                    ids: ids,
                    isActive: isActive,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            List<Resource> data = null;
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
            var wkbProp = mapping.SimplePropertyByName(nameof(Resource.LocationWkb));

            if (wkbProp != null)
            {
                mapping.SimpleProperties = mapping.SimpleProperties.Where(p => p != wkbProp);
                mapping.NormalizeIndices(); // Fix the gap we created in the previous line
            }

            return base.ProcessDefaultMapping(mapping);
        }
    }

    public class ResourcesGenericService : FactWithIdServiceBase<Resource, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IPermissionsCache _permissionsCache;

        public ResourcesGenericService(ApplicationFactServiceBehavior behavior,
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
            // Get all permissions pertaining to resources
            string prefix = "resources/";
            var permissions = (await _permissionsCache
                .GenericPermissionsFromCache(
                    tenantId: _behavior.TenantId,
                    userId: UserId,
                    version: _behavior.PermissionsVersion,
                    viewPrefix: prefix,
                    action: action,
                    cancellation: cancellation)).ToList();

            // Massage the permissions by adding definitionId = definitionId as an extra clause 
            // (since the controller will not filter the results per any specific definition Id)
            foreach (var permission in permissions.Where(e => e.View != "all"))
            {
                string definitionIdString = permission.View.Remove(0, prefix.Length).Replace("'", "''");
                if (!int.TryParse(definitionIdString, out int definitionId))
                {
                    throw new ServiceException($"Could not parse definition Id '{definitionIdString}' to a valid integer");
                }

                string definitionPredicate = $"{nameof(Resource.DefinitionId)} eq {definitionId}";
                if (!string.IsNullOrWhiteSpace(permission.Criteria))
                {
                    permission.Criteria = $"({definitionPredicate}) and ({permission.Criteria})";
                }
                else
                {
                    permission.Criteria = definitionPredicate;
                }
            }

            // Return the massaged permissions
            return permissions;
        }

        protected override Task<EntityQuery<Resource>> Search(EntityQuery<Resource> query, GetArguments args, CancellationToken cancellation)
        {
            return ResourceServiceUtil.SearchImpl(query, args, cancellation);
        }

        protected override ExpressionSelect ParseSelect(string select) => ResourceServiceUtil.ParseSelect(select, baseFunc: base.ParseSelect);
    }

    internal class ResourceServiceUtil
    {
        private static readonly string _documentDetailsSelect = string.Join(',', DocDetails.EntryResourcePaths());

        /// <summary>
        /// This is needed in both the generic and specific controllers, so we move it out here
        /// </summary>
        public static Task<EntityQuery<Resource>> SearchImpl(EntityQuery<Resource> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Resource.Name);
                var name2 = nameof(Resource.Name2);
                var name3 = nameof(Resource.Name3);
                var code = nameof(Resource.Code);
                var identifier = nameof(Resource.Identifier);

                query = query.Filter($"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}' or {identifier} contains '{search}'");
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
