using BSharp.Controllers.Dto;
using BSharp.Controllers.Utilities;
using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.Entities;
using BSharp.Services.BlobStorage;
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
    public class AgentsController : CrudControllerBase<AgentForSave, Agent, int>
    {
        public const string BASE_ADDRESS = "agents/";

        private readonly ILogger<AgentsController> _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;
        private readonly ITenantIdAccessor _tenantIdAccessor;
        private readonly IBlobService _blobService;
        private readonly IDefinitionsCache _definitionsCache;
        private readonly IModelMetadataProvider _modelMetadataProvider;

        private string DefinitionId => RouteData.Values["definitionId"]?.ToString() ??
            throw new BadRequestException("URI must be of the form 'api/" + BASE_ADDRESS + "{definitionId}'");
        private AgentDefinitionForClient Definition() => _definitionsCache.GetCurrentDefinitionsIfCached()?.Data?.Agents?
            .GetValueOrDefault(DefinitionId) ?? throw new InvalidOperationException($"Definition for '{DefinitionId}' was missing from the cache");

        private string ViewId => $"{BASE_ADDRESS}{DefinitionId}";

        public AgentsController(ILogger<AgentsController> logger, IStringLocalizer<Strings> localizer,
            ApplicationRepository repo, ITenantIdAccessor tenantIdAccessor, IBlobService blobService,
            IDefinitionsCache definitionsCache,
            IModelMetadataProvider modelMetadataProvider) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
            _tenantIdAccessor = tenantIdAccessor;
            _blobService = blobService;
            _definitionsCache = definitionsCache;
            _modelMetadataProvider = modelMetadataProvider;
        }

        [HttpGet("{id}/image")]
        public async Task<ActionResult> GetImage(int id)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                // GetByIdImplAsync() enforces read permissions
                var response = await GetByIdImplAsync(id, new GetByIdArguments { Select = nameof(Agent.ImageId) });
                string imageId = response.Result.ImageId;

                // Get the blob name
                if (imageId != null)
                {
                    // Get the bytes
                    string blobName = BlobName(imageId);
                    var imageBytes = await _blobService.LoadBlob(blobName);

                    Response.Headers.Add("x-image-id", imageId);
                    return File(imageBytes, "image/jpeg");
                }
                else
                {
                    return NotFound("This agent does not have a picture");
                }
            }, _logger);
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Agent>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    isActive: true)
            , _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Agent>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    isActive: false)
            , _logger);
        }

        private async Task<ActionResult<EntitiesResponse<Agent>>> Activate([FromBody] List<int> ids, bool returnEntities, string expand, bool isActive)
        {
            // Parse parameters
            var expandExp = ExpandExpression.Parse(expand);
            var idsArray = ids.ToArray();

            // Check user permissions
            await CheckActionPermissions("IsActive", idsArray);

            // Execute and return
            using (var trx = ControllerUtilities.CreateTransaction())
            {
                await _repo.Agents__Activate(ids, isActive);

                if (returnEntities)
                {
                    var response = await GetByIdListAsync(idsArray, expandExp);

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

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return _repo.UserPermissions(action, ViewId);
        }

        protected override Query<Agent> GetAsQuery(List<AgentForSave> entities)
        {
            return _repo.Agents__AsQuery(DefinitionId, entities);
        }

        protected override Query<Agent> Search(Query<Agent> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return AgentControllerUtil.SearchImpl(query, args, filteredPermissions);
        }

        protected override async Task SaveValidateAsync(List<AgentForSave> entities)
        {
            // TODO: Add definition validation and defaults here

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

        protected override async Task<List<int>> SaveExecuteAsync(List<AgentForSave> entities, ExpandExpression expand, bool returnIds)
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
                .ToListAsync();

            var blobsToDelete = dbEntitiesWithImageIds
                .Select(e => BlobName(e.ImageId))
                .ToList();

            try
            {
                using (var trx = ControllerUtilities.CreateTransaction())
                {
                    await _repo.Agents__Delete(ids);

                    if (blobsToDelete.Any())
                    {
                        await _blobService.DeleteBlobsAsync(blobsToDelete);
                    }

                    trx.Complete();
                }
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["Agent"]]);
            }
        }
    }

    [Route("api/" + AgentsController.BASE_ADDRESS)]
    [ApplicationApi]
    public class AgentsGenericController : FactWithIdControllerBase<Agent, int>
    {
        private readonly ApplicationRepository _repo;

        public AgentsGenericController(
            ILogger<AgentsGenericController> logger,
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
            // Get all permissions pertaining to agents
            string prefix = AgentsController.BASE_ADDRESS;
            var permissions = await _repo.GenericUserPermissions(action, prefix);

            // Massage the permissions by adding definitionId = definitionId as an extra clause 
            // (since the controller will not filter the results per any specific definition Id)
            foreach (var permission in permissions.Where(e => e.ViewId != "all"))
            {
                string definitionId = permission.ViewId.Remove(0, prefix.Length).Replace("'", "''");
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
            return AgentControllerUtil.SearchImpl(query, args, filteredPermissions);
        }
    }

    internal class AgentControllerUtil
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