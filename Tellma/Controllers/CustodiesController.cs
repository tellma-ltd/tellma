using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    public class CustodiesController : CrudControllerBase<CustodyForSave, Custody, int>
    {
        public const string BASE_ADDRESS = "custodies/";

        private readonly CustodiesService _service;

        public CustodiesController(CustodiesService service, IServiceProvider sp) : base(sp)
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
        public async Task<ActionResult<EntitiesResponse<Custody>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
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
        public async Task<ActionResult<EntitiesResponse<Custody>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Deactivate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            }, _logger);
        }

        protected override CrudServiceBase<CustodyForSave, Custody, int> GetCrudService() => _service;
    }

    public class CustodiesService : CrudServiceBase<CustodyForSave, Custody, int>
    {
        private readonly ApplicationRepository _repo;
        private readonly ITenantIdAccessor _tenantIdAccessor;
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

                throw new BadRequestException($"Bug: DefinitoinId could not be determined in {nameof(CustodiesService)}");
            }
        }

        private CustodyDefinitionForClient Definition() => _definitionsCache.GetCurrentDefinitionsIfCached()?.Data?.Custodies?
            .GetValueOrDefault(DefinitionId.Value) ?? throw new InvalidOperationException($"Custody Definition with Id = {DefinitionId} is missing from the cache");

        /// <summary>
        /// Overrides the default behavior of reading the definition Id from the route data
        /// </summary>
        public CustodiesService SetDefinitionId(int definitionId)
        {
            _definitionIdOverride = definitionId;
            return this;
        }

        private string View => $"{CustodiesController.BASE_ADDRESS}{DefinitionId}";

        public CustodiesService(ApplicationRepository repo,
            ITenantIdAccessor tenantIdAccessor, IBlobService blobService, IHttpContextAccessor contextAccessor,
            IDefinitionsCache definitionsCache, IServiceProvider sp) : base(sp)
        {
            _repo = repo;
            _tenantIdAccessor = tenantIdAccessor;
            _blobService = blobService;
            _contextAccessor = contextAccessor;
            _definitionsCache = definitionsCache;
        }

        public async Task<(string ImageId, byte[] ImageBytes)> GetImage(int id, CancellationToken cancellation)
        {
            // This enforces read permissions
            var (relation, _) = await GetById(id, new GetByIdArguments { Select = nameof(Custody.ImageId) }, cancellation);
            string imageId = relation.ImageId;

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
            return $"{tenantId}/Custodies/{guid}";
        }

        protected override IRepository GetRepository()
        {
            string filter = $"{nameof(Custody.DefinitionId)} {Ops.eq} {DefinitionId}";
            return new FilteredRepository<Custody>(_repo, filter);
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.PermissionsFromCache(View, action, cancellation);
        }

        protected override Query<Custody> Search(Query<Custody> query, GetArguments args)
        {
            return CustodyServiceUtil.SearchImpl(query, args);
        }

        protected override Task<List<CustodyForSave>> SavePreprocessAsync(List<CustodyForSave> entities)
        {
            var def = Definition();

            // Creating new entities forbidden if the definition is archived
            if (entities.Any(e => e?.Id == 0) && def.State == DefStates.Archived) // Insert
            {
                var msg = _localizer["Error_DefinitionIsArchived"];
                throw new BadRequestException(msg);
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

            return base.SavePreprocessAsync(entities);
        }

        private bool IsVisible(string visibility)
        {
            return visibility == Visibility.Optional || visibility == Visibility.Required;
        }

        protected override async Task SaveValidateAsync(List<CustodyForSave> entities)
        {
            var def = Definition();

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

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Custodies_Validate__Save(DefinitionId.Value, entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<CustodyForSave> entities, bool returnIds)
        {
            var (blobsToDelete, blobsToSave, imageIds) = await ImageUtilities.ExtractImages<Custody, CustodyForSave>(_repo, entities, BlobName);

            _blobsToDelete = blobsToDelete;
            _blobsToSave = blobsToSave;

            // Save the custodies
            var ids = await _repo.Custodies__Save(
                DefinitionId.Value,
                entities: entities,
                imageIds: imageIds,
                returnIds: returnIds);

            return ids;
        }

        protected override async Task NonTransactionalSideEffectsForSave(List<CustodyForSave> entities, List<Custody> data)
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
            var sqlErrors = await _repo.Custodies_Validate__Delete(DefinitionId.Value, ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            // For the entities we're about to delete retrieve their imageIds (if any) to delete from the blob storage
            var dbEntitiesWithImageIds = await _repo.Custodies
                .Select(nameof(Custody.ImageId))
                .Filter($"{nameof(Custody.ImageId)} ne null")
                .FilterByIds(ids.ToArray())
                .ToListAsync(cancellation: default);

            var blobsToDelete = dbEntitiesWithImageIds
                .Select(e => BlobName(e.ImageId))
                .ToList();

            try
            {
                using var trx = ControllerUtilities.CreateTransaction();
                await _repo.Custodies__Delete(ids);

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

        protected override SelectExpression ParseSelect(string select) => CustodyServiceUtil.ParseSelect(select, baseFunc: base.ParseSelect);

        public Task<(List<Custody>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<Custody>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<Custody>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.Custodies__Activate(ids, isActive);

            List<Custody> data = null;
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

    [Route("api/" + CustodiesController.BASE_ADDRESS)]
    [ApplicationController]
    public class CustodiesGenericController : FactWithIdControllerBase<Custody, int>
    {
        private readonly CustodiesGenericService _service;

        public CustodiesGenericController(CustodiesGenericService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override FactWithIdServiceBase<Custody, int> GetFactWithIdService()
        {
            return _service;
        }
    }

    public class CustodiesGenericService : FactWithIdServiceBase<Custody, int>
    {
        private readonly ApplicationRepository _repo;

        public CustodiesGenericService(IServiceProvider sp, ApplicationRepository repo) : base(sp)
        {
            _repo = repo;
        }

        protected override IRepository GetRepository() => _repo;

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            // Get all permissions pertaining to custodies
            string prefix = CustodiesController.BASE_ADDRESS;
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

                string definitionPredicate = $"{nameof(Custody.DefinitionId)} {Ops.eq} {definitionId}";
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

        protected override Query<Custody> Search(Query<Custody> query, GetArguments args)
        {
            return CustodyServiceUtil.SearchImpl(query, args);
        }

        protected override SelectExpression ParseSelect(string select) => CustodyServiceUtil.ParseSelect(select, baseFunc: base.ParseSelect);
    }

    internal class CustodyServiceUtil
    {
        /// <summary>
        /// This is needed in both the generic and specific controllers, so we move it out here
        /// </summary>
        public static Query<Custody> SearchImpl(Query<Custody> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Custody.Name);
                var name2 = nameof(Custody.Name2);
                var name3 = nameof(Custody.Name3);
                var code = nameof(Custody.Code);

                var filterString = $"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'";
                query = query.Filter(FilterExpression.Parse(filterString));
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

        private static readonly string _documentDetailsSelect = string.Join(',', DocDetails.EntryCustodyPaths());
    }
}