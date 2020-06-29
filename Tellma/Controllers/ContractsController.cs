using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
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
    public class ContractsController : CrudControllerBase<ContractForSave, Contract, int>
    {
        public const string BASE_ADDRESS = "contracts/";

        private readonly ContractsService _service;
        private readonly ILogger _logger;

        public ContractsController(ContractsService service, ILogger<ContractsController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
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
        public async Task<ActionResult<EntitiesResponse<Contract>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
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
        public async Task<ActionResult<EntitiesResponse<Contract>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Deactivate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            }, _logger);
        }

        protected override CrudServiceBase<ContractForSave, Contract, int> GetCrudService() => _service;
    }

    public class ContractsService : CrudServiceBase<ContractForSave, Contract, int>
    {
        private readonly ApplicationRepository _repo;
        private readonly ITenantIdAccessor _tenantIdAccessor;
        private readonly IBlobService _blobService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IDefinitionsCache _definitionsCache;

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

        private ContractDefinitionForClient Definition() => _definitionsCache.GetCurrentDefinitionsIfCached()?.Data?.Contracts?
            .GetValueOrDefault(DefinitionId.Value) ?? throw new InvalidOperationException($"Contract Definition with Id = {DefinitionId} is missing from the cache");

        /// <summary>
        /// Overrides the default behavior of reading the definition Id from the route data
        /// </summary>
        public ContractsService SetDefinitionId(int definitionId)
        {
            _definitionIdOverride = definitionId;
            return this;
        }

        private string View => $"{ContractsController.BASE_ADDRESS}{DefinitionId}";

        public ContractsService(ApplicationRepository repo,
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
            var (contract, _) = await GetById(id, new GetByIdArguments { Select = nameof(Contract.ImageId) }, cancellation);
            string imageId = contract.ImageId;

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
            return $"{tenantId}/Contracts/{guid}";
        }

        protected override IRepository GetRepository()
        {
            string filter = $"{nameof(Contract.DefinitionId)} {Ops.eq} {DefinitionId}";
            return new FilteredRepository<Contract>(_repo, filter);
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.UserPermissions(action, View, cancellation);
        }

        protected override Query<Contract> Search(Query<Contract> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return ContractServiceUtil.SearchImpl(query, args);
        }

        protected override Task<List<ContractForSave>> SavePreprocessAsync(List<ContractForSave> entities)
        {
            var def = Definition();

            // Creating new entities forbidden if the definition is archived
            if (entities.Any(e => e?.Id == 0) && def.State == DefStates.Archived) // Insert
            {
                var msg = _localizer["Error_DefinitionIsArchived"];
                throw new BadRequestException(msg);
            }

            // ... Any definition defaults will go here

            entities.ForEach(e =>
            {
                // Makes everything that follows easier
                e.Users ??= new List<ContractUserForSave>();
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

            return base.SavePreprocessAsync(entities);
        }

        private bool IsVisible(string visibility)
        {
            return visibility == Visibility.Optional || visibility == Visibility.Required;
        }

        protected override async Task SaveValidateAsync(List<ContractForSave> entities)
        {
            var def = Definition();
            var userIsRequired = def.UserCardinality != null; // "None" is mapped to null

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
            var sqlErrors = await _repo.Contracts_Validate__Save(DefinitionId.Value, entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<ContractForSave> entities, bool returnIds)
        {
            var (blobsToDelete, blobsToSave, imageIds) = await ImageUtilities.ExtractImages<Contract, ContractForSave>(_repo, entities, BlobName);

            // Save the contracts
            var ids = await _repo.Contracts__Save(
                DefinitionId.Value,
                entities: entities,
                imageIds: imageIds,
                returnIds: returnIds);

            // Delete the blobs retrieved earlier
            if (blobsToDelete.Any())
            {
                await _blobService.DeleteBlobsAsync(blobsToDelete);
            }

            // Save new blobs if any
            if (blobsToSave.Any())
            {
                await _blobService.SaveBlobsAsync(blobsToSave);
            }

            return ids;
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Contracts_Validate__Delete(DefinitionId.Value, ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            // For the entities we're about to delete retrieve their imageIds (if any) to delete from the blob storage
            var dbEntitiesWithImageIds = await _repo.Contracts
                .Select(nameof(Contract.ImageId))
                .Filter($"{nameof(Contract.ImageId)} ne null")
                .FilterByIds(ids.ToArray())
                .ToListAsync(cancellation: default);

            var blobsToDelete = dbEntitiesWithImageIds
                .Select(e => BlobName(e.ImageId))
                .ToList();

            try
            {
                using var trx = ControllerUtilities.CreateTransaction();
                await _repo.Contracts__Delete(ids);

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

        public Task<(List<Contract>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<Contract>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<Contract>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            // Check user permissions
            await CheckActionPermissions("IsActive", ids);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.Contracts__Activate(ids, isActive);

            if (args.ReturnEntities ?? false)
            {
                var (data, extras) = await GetByIds(ids, args, cancellation: default);

                trx.Complete();
                return (data, extras);
            }
            else
            {
                trx.Complete();
                return (null, null);
            }
        }
    }

    [Route("api/" + ContractsController.BASE_ADDRESS)]
    [ApplicationController]
    public class ContractsGenericController : FactWithIdControllerBase<Contract, int>
    {
        private readonly ContractsGenericService _service;

        public ContractsGenericController(ContractsGenericService service, ILogger<ContractsGenericController> logger) : base(logger)
        {
            _service = service;
        }

        protected override FactWithIdServiceBase<Contract, int> GetFactWithIdService()
        {
            return _service;
        }
    }

    public class ContractsGenericService : FactWithIdServiceBase<Contract, int>
    {
        private readonly ApplicationRepository _repo;

        public ContractsGenericService(IServiceProvider sp, ApplicationRepository repo) : base(sp)
        {
            _repo = repo;
        }

        protected override IRepository GetRepository() => _repo;

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            // Get all permissions pertaining to contracts
            string prefix = ContractsController.BASE_ADDRESS;
            var permissions = await _repo.GenericUserPermissions(action, prefix, cancellation);

            // Massage the permissions by adding definitionId = definitionId as an extra clause 
            // (since the controller will not filter the results per any specific definition Id)
            foreach (var permission in permissions.Where(e => e.View != "all"))
            {
                string definitionIdString = permission.View.Remove(0, prefix.Length).Replace("'", "''");
                if (!int.TryParse(definitionIdString, out int definitionId))
                {
                    throw new BadRequestException($"Could not parse definition Id {definitionIdString} to a valid integer");
                }

                string definitionPredicate = $"{nameof(Contract.DefinitionId)} {Ops.eq} {definitionId}";
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

        protected override Query<Contract> Search(Query<Contract> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return ContractServiceUtil.SearchImpl(query, args);
        }
    }

    internal class ContractServiceUtil
    {
        /// <summary>
        /// This is needed in both the generic and specific controllers, so we move it out here
        /// </summary>
        public static Query<Contract> SearchImpl(Query<Contract> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Contract.Name);
                var name2 = nameof(Contract.Name2);
                var name3 = nameof(Contract.Name3);
                var code = nameof(Contract.Code);

                var filterString = $"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'";
                query = query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }
    }
}