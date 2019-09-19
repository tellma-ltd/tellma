using BSharp.Controllers.Dto;
using BSharp.Controllers.Utilities;
using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers
{   
    // Specific API, works with a certain definitionId, and allows read-write
    [Route("api/resources/{definitionId}")]
    [ApplicationApi]
    public class ResourcesController : CrudControllerBase<ResourceForSave, Resource, int>
    {
        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;

        private string VIEW => RouteData.Values["definitionId"]?.ToString() ?? 
            throw new BadRequestException("URI must be of the form 'api/resources/{definitionId}'");

        public ResourcesController(
            ILogger<ResourcesController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Resource>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
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
        public async Task<ActionResult<EntitiesResponse<Resource>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    isActive: false)
            , _logger);
        }

        private async Task<ActionResult<EntitiesResponse<Resource>>> Activate([FromBody] List<int> ids, bool returnEntities, string expand, bool isActive)
        {
            // Parse parameters
            var expandExp = ExpandExpression.Parse(expand);
            var idsArray = ids.ToArray();

            // Check user permissions
            await CheckActionPermissions("IsActive", idsArray);

            // Execute and return
            using (var trx = ControllerUtilities.CreateTransaction())
            {
                await _repo.Resources__Activate(ids, isActive);

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

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return await _repo.UserPermissions(action, VIEW);
        }

        protected override IRepository GetRepository()
        {
            string filter = $"{nameof(Resource.ResourceDefinitionId)} eq '{VIEW}'";
            return new FilteredRepository<Resource>(_repo, filter);
        }

        protected override Query<Resource> Search(Query<Resource> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return ResourceControllerUtil.SearchImpl(query, args, filteredPermissions);
        }

        protected override async Task SaveValidateAsync(List<ResourceForSave> entities)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Resources_Validate__Save(VIEW, entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<ResourceForSave> entities, ExpandExpression expand, bool returnIds)
        {
            return await _repo.Resources__Save(VIEW, entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Resources_Validate__Delete(VIEW, ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.Resources__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["Resource"]]);
            }
        }

        protected override Query<Resource> GetAsQuery(List<ResourceForSave> entities)
        {
            return _repo.Resources__AsQuery(VIEW, entities);
        }
    }

    // Generic API, allows reading all resources

    [Route("api/resources")]
    [ApplicationApi]
    public class ResourcesGenericController : FactWithIdControllerBase<Resource, int>
    {
        private readonly ApplicationRepository _repo;
        private readonly IDefinitionsCache _definitionsCache;

        public ResourcesGenericController(
            ILogger<ResourcesController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo, 
            IDefinitionsCache definitionsCache) : base(logger, localizer)
        {
            _repo = repo;
            _definitionsCache = definitionsCache;
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            // Retrieve the definitions
            var resourceDefinitions = _definitionsCache.GetCurrentDefinitionsIfCached()?.Data?.Resources;
            if(resourceDefinitions == null)
            {
                // Programmer mistake
                throw new BadRequestException("Resource definitions cache was empty");
            }

            // Retrieve all the permissions pertaining to the above definitions
            var viewIds = resourceDefinitions.Keys.ToArray();
            var permissions = await _repo.UserPermissions(action, viewIds);

            // Massage the permissions by adding definitionId = definitionId as an extra clause 
            // (since the controller will not filter the results per any specific definition Id)
            foreach(var permission in permissions.Where(e => e.ViewId != "all"))
            {
                string definitionPredicate = $"{nameof(Resource.ResourceDefinitionId)} eq '{permission.ViewId}'";
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

        protected override Query<Resource> Search(Query<Resource> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return ResourceControllerUtil.SearchImpl(query, args, filteredPermissions);
        }
    }

    internal class ResourceControllerUtil
    {
        /// <summary>
        /// This is needed in both the generic and specific controllers, so we move it out here
        /// </summary>
        public static Query<Resource> SearchImpl(Query<Resource> query, GetArguments args, IEnumerable<AbstractPermission> _)
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
    }
}
