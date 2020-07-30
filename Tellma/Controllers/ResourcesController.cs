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
    // Specific API, works with a certain definitionId, and allows read-write
    [Route("api/" + BASE_ADDRESS + "{definitionId}")]
    [ApplicationController]
    public class ResourcesController : CrudControllerBase<ResourceForSave, Resource, int>
    {
        public const string BASE_ADDRESS = "resources/";

        private readonly ResourcesService _service;

        public ResourcesController(ResourcesService service, IServiceProvider sp) : base(sp)
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
        public async Task<ActionResult<EntitiesResponse<Resource>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Activate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            },
            _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Resource>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Deactivate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            },
            _logger);
        }

        protected override CrudServiceBase<ResourceForSave, Resource, int> GetCrudService()
        {
            return _service;
        }
    }

    public class ResourcesService : CrudServiceBase<ResourceForSave, Resource, int>
    {
        private readonly ApplicationRepository _repo;
        private readonly IDefinitionsCache _definitionsCache;
        private readonly IBlobService _blobService;
        private readonly ITenantIdAccessor _tenantIdAccessor;
        private readonly ISettingsCache _settingsCache;
        private readonly IHttpContextAccessor _contextAccessor;

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

                throw new BadRequestException($"Bug: DefinitoinId could not be determined in {nameof(ResourcesService)}");
            }
        }

        /// <summary>
        /// Overrides the default behavior of reading the definition Id from the route data
        /// </summary>
        public ResourcesService SetDefinitionId(int definitionId)
        {
            _definitionIdOverride = definitionId;
            return this;
        }

        private ResourceDefinitionForClient Definition() => _definitionsCache.GetCurrentDefinitionsIfCached()?.Data?.Resources?
            .GetValueOrDefault(DefinitionId.Value) ?? throw new InvalidOperationException($"Resource Definition with Id = {DefinitionId} is missing from the cache");

        private string View => $"{ResourcesController.BASE_ADDRESS}{DefinitionId}";

        public ResourcesService(
            ApplicationRepository repo,
            IDefinitionsCache definitionsCache,
            IBlobService blobService,
            ITenantIdAccessor tenantIdAccessor,
            ISettingsCache settingsCache,
            IHttpContextAccessor contextAccessor,
            IServiceProvider sp) : base(sp)
        {
            _repo = repo;
            _definitionsCache = definitionsCache;
            _blobService = blobService;
            _tenantIdAccessor = tenantIdAccessor;
            _settingsCache = settingsCache;
            _contextAccessor = contextAccessor;
        }

        public async Task<(string ImageId, byte[] ImageBytes)> GetImage(int id, CancellationToken cancellation)
        {
            // This enforces read permissions
            var (resource, _) = await GetById(id, new GetByIdArguments { Select = nameof(Resource.ImageId) }, cancellation);
            string imageId = resource.ImageId;

            // Get the blob name
            if (imageId != null)
            {
                // Get the bytes
                string blobName = BlobName(imageId);
                var imageBytes = await _blobService.LoadBlob(blobName, cancellation);

                return (imageId, imageBytes);
            }
            else
            {
                throw new NotFoundException<int>(id);
            }
        }

        private string BlobName(string guid)
        {
            int tenantId = _tenantIdAccessor.GetTenantId();
            return $"{tenantId}/Resources/{guid}";
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.PermissionsFromCache(View, action, cancellation);
        }

        protected override IRepository GetRepository()
        {
            string filter = $"{nameof(Resource.DefinitionId)} {Ops.eq} {DefinitionId}";
            return new FilteredRepository<Resource>(_repo, filter);
        }

        protected override Query<Resource> Search(Query<Resource> query, GetArguments args)
        {
            return ResourceServiceUtil.SearchImpl(query, args);
        }

        protected override async Task<List<ResourceForSave>> SavePreprocessAsync(List<ResourceForSave> entities)
        {
            var def = Definition();

            // Creating new entities forbidden if the definition is archived
            if (entities.Any(e => e?.Id == 0) && def.State == DefStates.Archived) // Insert
            {
                var msg = _localizer["Error_DefinitionIsArchived"];
                throw new BadRequestException(msg);
            }

            // Set default values
            //SetDefaultValue(entities, e => e.Identifier, def.IdentifierDefaultValue);
            //SetDefaultValue(entities, e => e.CurrencyId, def.CurrencyDefaultValue);
            //SetDefaultValue(entities, e => e.MonetaryValue, def.MonetaryValueDefaultValue);
            //SetDefaultValue(entities, e => e.Description, definition.DescriptionDefaultValue);
            //SetDefaultValue(entities, e => e.Description2, definition.Description2DefaultValue);
            //SetDefaultValue(entities, e => e.Description3, definition.Description3DefaultValue);
            //SetDefaultValue(entities, e => e.ReorderLevel, def.ReorderLevelDefaultValue);
            //SetDefaultValue(entities, e => e.EconomicOrderQuantity, def.EconomicOrderQuantityDefaultValue);
            //SetDefaultValue(entities, e => e.FromDate, def.FromDateDefaultValue);
            //SetDefaultValue(entities, e => e.ToDate, def.ToDateDefaultValue);
            //SetDefaultValue(entities, e => e.Decimal1, def.Decimal1DefaultValue);
            //SetDefaultValue(entities, e => e.Decimal2, def.Decimal2DefaultValue);
            //SetDefaultValue(entities, e => e.Int1, def.Int1DefaultValue);
            //SetDefaultValue(entities, e => e.Int2, def.Int2DefaultValue);
            //SetDefaultValue(entities, e => e.Lookup1Id, def.Lookup1DefaultValue);
            //SetDefaultValue(entities, e => e.Lookup2Id, def.Lookup2DefaultValue);
            //SetDefaultValue(entities, e => e.Lookup3Id, def.Lookup3DefaultValue);
            //SetDefaultValue(entities, e => e.Lookup4Id, def.Lookup4DefaultValue);
            //SetDefaultValue(entities, e => e.Lookup5Id, definition.Lookup5DefaultValue);
            //SetDefaultValue(entities, e => e.Text1, def.Text1DefaultValue);
            //SetDefaultValue(entities, e => e.Text2, def.Text2DefaultValue);

            // Set defaults
            entities.ForEach(entity =>
            {
                entity.UnitId ??= def.DefaultUnitId;
                entity.UnitMassUnitId ??= def.DefaultUnitMassUnitId;
            });

            entities.ForEach(e =>
            {
                // Makes everything that follows easier
                e.Units ??= new List<ResourceUnitForSave>();
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

            var settings = _settingsCache.GetCurrentSettingsIfCached()?.Data;
            var functionalId = settings.FunctionalCurrencyId;

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
            await _repo.Resources__Preprocess(DefinitionId.Value, entities);
            return entities;
        }

        private bool IsVisible(string visibility)
        {
            return visibility == Visibility.Optional || visibility == Visibility.Required;
        }

        protected override async Task SaveValidateAsync(List<ResourceForSave> entities)
        {
            var def = Definition();
            var unitIsRequired = def.UnitCardinality != null; // "None" is mapped to null

            foreach (var (e, i) in entities.Select((e, i) => (e, i)))
            {
                if (e.EntityMetadata.LocationJsonParseError != null)
                {
                    ModelState.AddModelError($"[{i}].{nameof(e.LocationJson)}", e.EntityMetadata.LocationJsonParseError);
                    if (ModelState.HasReachedMaxErrors)
                    {
                        return;
                    }
                }
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Resources_Validate__Save(DefinitionId.Value, entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        //private void SetDefaultValue<TKey>(List<ResourceForSave> entities, Expression<Func<ResourceForSave, TKey>> selector, TKey defaultValue)
        //{
        //    if (defaultValue != null)
        //    {
        //        Func<ResourceForSave, TKey> getPropValue = selector.Compile(); // The function to access the property value
        //        Action<ResourceForSave, TKey> setPropValue = ControllerUtilities.GetAssigner(selector).Compile();

        //        entities.ForEach(entity =>
        //        {
        //            if (getPropValue(entity) == null)
        //            {
        //                setPropValue(entity, defaultValue);
        //            }
        //        });
        //    }
        //}

        protected override async Task<List<int>> SaveExecuteAsync(List<ResourceForSave> entities, bool returnIds)
        {
            var (blobsToDelete, blobsToSave, imageIds) = await ImageUtilities.ExtractImages<Resource, ResourceForSave>(_repo, entities, BlobName);

            _blobsToDelete = blobsToDelete;
            _blobsToSave = blobsToSave;

            // Save the Resources
            var ids = await _repo.Resources__Save(
                DefinitionId.Value,
                entities,
                imageIds: imageIds,
                returnIds: returnIds);

            return ids;
        }

        protected override async Task NonTransactionalSideEffectsForSave(List<ResourceForSave> entities, List<Resource> data)
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
            var sqlErrors = await _repo.Resources_Validate__Delete(DefinitionId.Value, ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            // For the entities we're about to delete retrieve their imageIds (if any) to delete from the blob storage
            var dbEntitiesWithImageIds = await _repo.Resources
                .Select(nameof(Resource.ImageId))
                .Filter($"{nameof(Resource.ImageId)} ne null")
                .FilterByIds(ids.ToArray())
                .ToListAsync(cancellation: default);

            var blobsToDelete = dbEntitiesWithImageIds
                .Select(e => BlobName(e.ImageId))
                .ToList();

            try
            {
                using var trx = ControllerUtilities.CreateTransaction();
                await _repo.Resources__Delete(ids);

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

        protected override SelectExpression ParseSelect(string select) => ResourceServiceUtil.ParseSelect(select, baseFunc: base.ParseSelect);

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
            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.Resources__Activate(ids, isActive);

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
            var wkbProp = mapping.SimpleProperty(nameof(Resource.LocationWkb));

            if (wkbProp != null)
            {
                mapping.SimpleProperties = mapping.SimpleProperties.Where(p => p != wkbProp);
                mapping.NormalizeIndices(); // Fix the gap we created in the previous line
            }

            return base.ProcessDefaultMapping(mapping);
        }
    }

    // Generic API, allows reading all resources

    [Route("api/" + ResourcesController.BASE_ADDRESS)]
    [ApplicationController]
    public class ResourcesGenericController : FactWithIdControllerBase<Resource, int>
    {
        private readonly ResourcesGenericService _service;

        public ResourcesGenericController(ResourcesGenericService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override FactWithIdServiceBase<Resource, int> GetFactWithIdService()
        {
            return _service;
        }
    }

    public class ResourcesGenericService : FactWithIdServiceBase<Resource, int>
    {
        private readonly ApplicationRepository _repo;

        public ResourcesGenericService(IServiceProvider sp, ApplicationRepository repo) : base(sp)
        {
            _repo = repo;
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            // Get all permissions pertaining to resources
            string prefix = ResourcesController.BASE_ADDRESS;
            var permissions = await _repo.GenericPermissionsFromCache(prefix, action, cancellation);

            // Massage the permissions by adding definitionId = definitionId as an extra clause 
            // (since the controller will not filter the results per any specific definition Id)
            foreach (var permission in permissions.Where(e => e.View != "all"))
            {
                string definitionIdString = permission.View.Remove(0, prefix.Length).Replace("'", "''");
                if (!int.TryParse(definitionIdString, out int definitionId))
                {
                    throw new BadRequestException($"Could not parse definition Id {definitionIdString} to a valid integer");
                }

                string definitionPredicate = $"{nameof(Resource.DefinitionId)} {Ops.eq} {definitionId}";
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

        protected override Query<Resource> Search(Query<Resource> query, GetArguments args)
        {
            return ResourceServiceUtil.SearchImpl(query, args);
        }

        protected override SelectExpression ParseSelect(string select) => ResourceServiceUtil.ParseSelect(select, baseFunc: base.ParseSelect);
    }

    internal class ResourceServiceUtil
    {
        /// <summary>
        /// This is needed in both the generic and specific controllers, so we move it out here
        /// </summary>
        public static Query<Resource> SearchImpl(Query<Resource> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Resource.Name);
                var name2 = nameof(Resource.Name2);
                var name3 = nameof(Resource.Name3);
                var code = nameof(Resource.Code);

                query = query.Filter($"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'");
            }

            return query;
        }

        public static SelectExpression ParseSelect(string select, Func<string, SelectExpression> baseFunc)
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

        private static readonly string _documentDetailsSelect = string.Join(',', DocDetails.EntryResourcePaths());
    }
}
