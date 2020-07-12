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
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS + "{definitionId}")]
    [ApplicationController]
    public class LookupsController : CrudControllerBase<LookupForSave, Lookup, int>
    {
        public const string BASE_ADDRESS = "lookups/";

        private readonly LookupsService _service;

        public LookupsController(LookupsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Lookup>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
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
        public async Task<ActionResult<EntitiesResponse<Lookup>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Deactivate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);

            }, _logger);
        }

        protected override CrudServiceBase<LookupForSave, Lookup, int> GetCrudService()
        {
            return _service;
        }
    }

    public class LookupsService : CrudServiceBase<LookupForSave, Lookup, int>
    {
        private readonly ILogger _logger;
        private readonly ApplicationRepository _repo;
        private readonly IDefinitionsCache _definitionsCache;
        private readonly IHttpContextAccessor _contextAccessor;
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

        private LookupDefinitionForClient Definition() => _definitionsCache.GetCurrentDefinitionsIfCached()?.Data?.Lookups?
            .GetValueOrDefault(DefinitionId.Value) ?? throw new InvalidOperationException($"Lookup Definition with Id = {DefinitionId} is missing from the cache");

        private string View => $"{LookupsController.BASE_ADDRESS}{DefinitionId}";

        public LookupsService(
            ILogger<LookupsController> logger,
            ApplicationRepository repo,
            IDefinitionsCache definitionsCache,
            IHttpContextAccessor contextAccessor,
            IServiceProvider sp) : base(sp)
        {
            _logger = logger;
            _repo = repo;
            _definitionsCache = definitionsCache;
            _contextAccessor = contextAccessor;
        }

        #region Public Members

        /// <summary>
        /// Overrides the default behavior of reading the definition Id from the route data
        /// </summary>
        public LookupsService SetDefinitionId(int definitionId)
        {
            _definitionIdOverride = definitionId;
            return this;
        }

        public Task<(List<Lookup>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<Lookup>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        #endregion

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.PermissionsFromCache(View, action, cancellation);
        }

        protected override IRepository GetRepository()
        {
            string filter = $"{nameof(Lookup.DefinitionId)} eq {DefinitionId}";
            return new FilteredRepository<Lookup>(_repo, filter);
        }

        protected override Query<Lookup> Search(Query<Lookup> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return LookupServiceUtil.SearchImpl(query, args, filteredPermissions);

        }


        protected override Task<List<LookupForSave>> SavePreprocessAsync(List<LookupForSave> entities)
        {
            var def = Definition();

            // Creating new entities forbidden if the definition is archived
            if (entities.Any(e => e?.Id == 0) && def.State == DefStates.Archived) // Insert
            {
                var msg = _localizer["Error_DefinitionIsArchived"];
                throw new BadRequestException(msg);
            }

            return Task.FromResult(entities);
        }

        protected override async Task SaveValidateAsync(List<LookupForSave> entities)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Lookups_Validate__Save(DefinitionId.Value, entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<LookupForSave> entities, bool returnIds)
        {
            return await _repo.Lookups__Save(DefinitionId.Value, entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Lookups_Validate__Delete(DefinitionId.Value, ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.Lookups__Delete(DefinitionId.Value, ids);
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

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse("SortKey,Id desc");
        }

        private async Task<(List<Lookup>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            // Check user permissions
            await CheckActionPermissions("IsActive", ids);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.Lookups__Activate(ids, isActive);

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

    [Route("api/" + LookupsController.BASE_ADDRESS)]
    [ApplicationController]
    public class LookupsGenericController : FactWithIdControllerBase<Lookup, int>
    {
        private readonly LookupsGenericService _service;

        public LookupsGenericController(LookupsGenericService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        protected override FactWithIdServiceBase<Lookup, int> GetFactWithIdService()
        {
            return _service;
        }
    }

    public class LookupsGenericService : FactWithIdServiceBase<Lookup, int>
    {
        private readonly ApplicationRepository _repo;

        public LookupsGenericService(IServiceProvider sp, ApplicationRepository repo) : base(sp)
        {
            _repo = repo;
        }

        protected override IRepository GetRepository() => _repo;

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            // Get all permissions pertaining to Lookups
            string prefix = LookupsController.BASE_ADDRESS;
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

                string definitionPredicate = $"{nameof(Lookup.DefinitionId)} {Ops.eq} {definitionId}";
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

        protected override Query<Lookup> Search(Query<Lookup> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return LookupServiceUtil.SearchImpl(query, args, filteredPermissions);
        }
    }

    internal class LookupServiceUtil
    {
        /// <summary>
        /// This is needed in both the generic and specific controllers, so we move it out here
        /// </summary>
        public static Query<Lookup> SearchImpl(Query<Lookup> query, GetArguments args, IEnumerable<AbstractPermission> _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Lookup.Name);
                var name2 = nameof(Lookup.Name2);
                var name3 = nameof(Lookup.Name3);
                var code = nameof(Lookup.Code);

                var filterString = $"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'";
                query = query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }
    }
}
