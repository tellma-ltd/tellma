using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.BlobStorage;
using Tellma.Services.MultiTenancy;
using Tellma.Services.Utilities;
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
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS + "{definitionId}")]
    [ApplicationController]
    public class AgentsController : CrudControllerBase<AgentForSave, Agent, int>
    {
        public const string BASE_ADDRESS = "agents/";

        private readonly AgentsService _service;
        private readonly ILogger _logger;

        public AgentsController(AgentsService service, ILogger<AgentsController> logger) : base(logger)
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
        public async Task<ActionResult<EntitiesResponse<Agent>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
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
        public async Task<ActionResult<EntitiesResponse<Agent>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Deactivate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            }, _logger);
        }

        protected override CrudServiceBase<AgentForSave, Agent, int> GetCrudService() => _service;
    }

    public class AgentsService : CrudServiceBase<AgentForSave, Agent, int>
    {
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;
        private readonly ITenantIdAccessor _tenantIdAccessor;
        private readonly IBlobService _blobService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IDefinitionsCache _definitionsCache;
        private readonly IModelMetadataProvider _modelMetadataProvider;

        private string _definitionIdOverride;

        private string DefinitionId => _definitionIdOverride ?? 
            _contextAccessor.HttpContext?.Request?.RouteValues?.GetValueOrDefault("definitionId")?.ToString() ??
            throw new BadRequestException($"Bug: DefinitoinId could not be determined in {nameof(AgentsService)}");

        /// <summary>
        /// Overrides the default behavior of reading the definition Id from the route data
        /// </summary>
        public AgentsService SetDefinitionId(string definitionId)
        {
            _definitionIdOverride = definitionId;
            return this;
        }

        private string View => $"{AgentsController.BASE_ADDRESS}{DefinitionId}";

        public AgentsService(IStringLocalizer<Strings> localizer, ApplicationRepository repo,
            ITenantIdAccessor tenantIdAccessor, IBlobService blobService, IHttpContextAccessor contextAccessor,
            IDefinitionsCache definitionsCache, IModelMetadataProvider modelMetadataProvider) : base(localizer)
        {
            _localizer = localizer;
            _repo = repo;
            _tenantIdAccessor = tenantIdAccessor;
            _blobService = blobService;
            _contextAccessor = contextAccessor;
            _definitionsCache = definitionsCache;
            _modelMetadataProvider = modelMetadataProvider;
        }

        public async Task<(string ImageId, byte[] ImageBytes)> GetImage(int id, CancellationToken cancellation)
        {
            // This enforces read permissions
            var (agent, _) = await GetById(id, new GetByIdArguments { Select = nameof(Agent.ImageId) }, cancellation);
            string imageId = agent.ImageId;

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
            return $"{tenantId}/Agents/{guid}";
        }

        protected override IRepository GetRepository()
        {
            string filter = $"{nameof(Agent.DefinitionId)} {Ops.eq} '{DefinitionId}'";
            return new FilteredRepository<Agent>(_repo, filter);
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.UserPermissions(action, View, cancellation);
        }

        protected override Query<Agent> Search(Query<Agent> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return AgentServiceUtil.SearchImpl(query, args, filteredPermissions);
        }

        protected override Task<List<AgentForSave>> SavePreprocessAsync(List<AgentForSave> entities)
        {
            // TODO: Add definition defaults here
            return base.SavePreprocessAsync(entities);
        }

        protected override async Task SaveValidateAsync(List<AgentForSave> entities)
        {
            // TODO: Add definition validation here

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Agents_Validate__Save(DefinitionId, entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<AgentForSave> entities, bool returnIds)
        {
            var (blobsToDelete, blobsToSave, imageIds) = await ImageUtilities.ExtractImages<Agent, AgentForSave>(_repo, entities, BlobName);

            // Save the agents
            var ids = await _repo.Agents__Save(
                DefinitionId,
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
            var sqlErrors = await _repo.Agents_Validate__Delete(DefinitionId, ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            // For the entities we're about to delete retrieve their imageIds (if any) to delete from the blob storage
            var dbEntitiesWithImageIds = await _repo.Agents
                .Select(nameof(Agent.ImageId))
                .Filter($"{nameof(Agent.ImageId)} ne null")
                .FilterByIds(ids.ToArray())
                .ToListAsync(cancellation: default);

            var blobsToDelete = dbEntitiesWithImageIds
                .Select(e => BlobName(e.ImageId))
                .ToList();

            try
            {
                using var trx = ControllerUtilities.CreateTransaction();
                await _repo.Agents__Delete(ids);

                if (blobsToDelete.Any())
                {
                    await _blobService.DeleteBlobsAsync(blobsToDelete);
                }

                trx.Complete();
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["Agent"]]);
            }
        }

        public Task<(List<Agent>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<Agent>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<Agent>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            // Check user permissions
            await CheckActionPermissions("IsActive", ids);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.Agents__Activate(ids, isActive);

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

    [Route("api/" + AgentsController.BASE_ADDRESS)]
    [ApplicationController]
    public class AgentsGenericController : FactWithIdControllerBase<Agent, int>
    {
        private readonly AgentsGenericService _service;

        public AgentsGenericController(AgentsGenericService service, ILogger<AgentsGenericController> logger) : base(logger)
        {
            _service = service;
        }

        protected override FactWithIdServiceBase<Agent, int> GetFactWithIdService()
        {
            return _service;
        }
    }

    public class AgentsGenericService : FactWithIdServiceBase<Agent, int>
    {
        private readonly ApplicationRepository _repo;

        public AgentsGenericService(IStringLocalizer<Strings> localizer, ApplicationRepository repo) : base(localizer)
        {
            _repo = repo;
        }

        protected override IRepository GetRepository() => _repo;

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            // Get all permissions pertaining to agents
            string prefix = AgentsController.BASE_ADDRESS;
            var permissions = await _repo.GenericUserPermissions(action, prefix, cancellation);

            // Massage the permissions by adding definitionId = definitionId as an extra clause 
            // (since the controller will not filter the results per any specific definition Id)
            foreach (var permission in permissions.Where(e => e.View != "all"))
            {
                string definitionId = permission.View.Remove(0, prefix.Length).Replace("'", "''");
                string definitionPredicate = $"{nameof(Agent.DefinitionId)} {Ops.eq} '{definitionId}'";
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

        protected override Query<Agent> Search(Query<Agent> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return AgentServiceUtil.SearchImpl(query, args, filteredPermissions);
        }
    }

    internal class AgentServiceUtil
    {
        /// <summary>
        /// This is needed in both the generic and specific controllers, so we move it out here
        /// </summary>
        public static Query<Agent> SearchImpl(Query<Agent> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Agent.Name);
                var name2 = nameof(Agent.Name2);
                var name3 = nameof(Agent.Name3);
                var code = nameof(Agent.Code);

                var filterString = $"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'";
                query = query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }
    }
}