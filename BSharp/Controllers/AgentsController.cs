using BSharp.Controllers.Dto;
using BSharp.Controllers.Misc;
using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.Entities;
using BSharp.Services.BlobStorage;
using BSharp.Services.ImportExport;
using BSharp.Services.MultiTenancy;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
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
using System.Transactions;

namespace BSharp.Controllers
{
    [Route("api/agents")]
    [ApplicationApi]
    public class AgentsController : CrudControllerBase<AgentForSave, Agent, int>
    {
        private readonly ILogger<AgentsController> _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;
        private readonly ITenantIdAccessor _tenantIdAccessor;
        private readonly IBlobService _blobService;

        public string VIEW => "agents";

        public AgentsController(ILogger<AgentsController> logger, IStringLocalizer<Strings> localizer,
            ApplicationRepository repo, ITenantIdAccessor tenantIdAccessor, IBlobService blobService) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
            _tenantIdAccessor = tenantIdAccessor;
            _blobService = blobService;
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



        [HttpGet("{id}/image")]
        public async Task<ActionResult> GetImage(int id)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                string imageId;
                if (id == _repo.GetUserInfo().UserId)
                {
                    // A user can always view their own image, so we bypass read permissions
                    Agent me = await _repo.Agents.Filter("Id eq me").Select(nameof(Agent.ImageId)).FirstOrDefaultAsync();
                    imageId = me.ImageId;
                }
                else
                {
                    // GetByIdImplAsync() enforces read permissions
                    var agentResponse = await GetByIdImplAsync(id, new GetByIdArguments { Select = nameof(Agent.ImageId) });
                    imageId = agentResponse.Result.ImageId;
                }

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
                    return NotFound("This user does not have a picture");
                }
            }, _logger);
        }

        private string BlobName(string guid)
        {
            int tenantId = _tenantIdAccessor.GetTenantId();
            return $"{tenantId}/Agents/{guid}";
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

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return _repo.UserPermissions(action, VIEW);
        }

        protected override Query<Agent> GetAsQuery(List<AgentForSave> entities)
        {
            return _repo.Agents__AsQuery(entities);
        }

        protected override Query<Agent> Search(Query<Agent> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
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

        protected override async Task SaveValidateAsync(List<AgentForSave> entities)
        {
            // Check that codes are not duplicated within the arriving collection
            var duplicateCodes = entities.Where(e => e.Code != null).GroupBy(e => e.Code).Where(g => g.Count() > 1);
            if (duplicateCodes.Any())
            {
                // Hash the entities' indices for performance
                Dictionary<AgentForSave, int> indices = entities.ToIndexDictionary();

                foreach (var groupWithDuplicateCodes in duplicateCodes)
                {
                    foreach (var entity in groupWithDuplicateCodes)
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        ModelState.AddModelError($"[{index}].Id", _localizer["Error_TheCode0IsDuplicated", entity.Code]);
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
            var sqlErrors = await _repo.Agents_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<AgentForSave> entities, ExpandExpression expand, bool returnIds)
        {
            var blobsToDelete = new List<string>();
            var blobsToSave = new List<(string, byte[])>();
            var imageIds = new List<IndexedImageId>(); // For the repository

            var idsWithNewImages = entities
                .Where(e => e.Image != null && e.Id != 0)
                .Select(e => e.Id)
                .ToArray();

            if (idsWithNewImages.Any())
            {
                // Get old image Ids that should be deleted from Blob storage
                var dbEntitiesWithNewImages = await _repo.Agents
                    .Select(nameof(Agent.ImageId))
                    .Filter($"{nameof(Agent.ImageId)} ne null")
                    .FilterByIds(idsWithNewImages)
                    .ToListAsync();

                blobsToDelete = dbEntitiesWithNewImages
                    .Select(e => BlobName(e.ImageId))
                    .ToList();
            }

            // Get new image Ids and bytes that should be added to blob storage
            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                byte[] imageBytes = entity.Image;
                if (imageBytes != null)
                {
                    if (imageBytes.Length == 0) // This means delete the image
                    {
                        if (entity.Id != 0)
                        {
                            // Specify that ImageId should be set to NULL
                            imageIds.Add(new IndexedImageId
                            {
                                Index = index,
                                ImageId = null
                            });
                        }
                    }
                    else
                    {
                        // Specify that ImageId should be set to a new GUID
                        string imageId = Guid.NewGuid().ToString();
                        imageIds.Add(new IndexedImageId
                        {
                            Index = index,
                            ImageId = imageId
                        });

                        // Below we process the new image bytes
                        // We make the image smaller and turn it into JPEG
                        using (var image = Image.Load(imageBytes))
                        {
                            // Resize to 128x128px
                            image.Mutate(c => c.Resize(new ResizeOptions
                            {
                                // 'Max' mode maintains the aspect ratio and keeps the entire image
                                Mode = ResizeMode.Max,
                                Size = new Size(128),
                                Position = AnchorPositionMode.Center
                            }));

                            // Some image formats that support transparent regions
                            // these regions will turn black in JPEG format unless we do this
                            image.Mutate(c => c.BackgroundColor(Rgba32.White)); ;

                            // Save as JPEG
                            var memoryStream = new MemoryStream();
                            image.SaveAsJpeg(memoryStream);
                            imageBytes = memoryStream.ToArray();

                            // Note: JPEG is the format of choice for photography.
                            // It provides better quality at a lower size for photographs
                            // which is what most of these pictures are expected to be
                        }

                        // Add it to blobs to create
                        blobsToSave.Add((BlobName(imageId), imageBytes));
                    }
                }
            }

            // Save the agents
            var ids = await _repo.Agents__Save(
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
            var sqlErrors = await _repo.Agents_Validate__Delete(ids, top: remainingErrorCount);

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
}