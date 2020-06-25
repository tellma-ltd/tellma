using GeoJSON.Net;
using GeoJSON.Net.Contrib.Wkb;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;

namespace Tellma.Controllers
{
    // Specific API, works with a certain definitionId, and allows read-write
    [Route("api/" + BASE_ADDRESS + "{definitionId}")]
    [ApplicationController]
    public class ResourcesController : CrudControllerBase<ResourceForSave, Resource, int>
    {
        public const string BASE_ADDRESS = "resources/";

        private readonly ResourcesService _service;
        private readonly ILogger _logger;

        public ResourcesController(ResourcesService service, ILogger<ResourcesController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
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
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;
        private readonly IDefinitionsCache _definitionsCache;
        private readonly ISettingsCache _settingsCache;
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
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo,
            IDefinitionsCache definitionsCache,
            ISettingsCache settingsCache,
            IHttpContextAccessor contextAccessor,
            IServiceProvider sp) : base(sp)
        {
            _localizer = localizer;
            _repo = repo;
            _definitionsCache = definitionsCache;
            _settingsCache = settingsCache;
            _contextAccessor = contextAccessor;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return await _repo.UserPermissions(action, View, cancellation);
        }

        protected override IRepository GetRepository()
        {
            string filter = $"{nameof(Resource.DefinitionId)} {Ops.eq} {DefinitionId}";
            return new FilteredRepository<Resource>(_repo, filter);
        }

        protected override Query<Resource> Search(Query<Resource> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return ResourceControllerUtil.SearchImpl(query, args, filteredPermissions);
        }

        protected override async Task<List<ResourceForSave>> SavePreprocessAsync(List<ResourceForSave> entities)
        {
            var def = Definition();

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

            var settings = _settingsCache.GetCurrentSettingsIfCached()?.Data;
            var functionalId = settings.FunctionalCurrencyId;

            if (IsVisible(def.ResidualMonetaryValueVisibility))
            {
                entities.ForEach(entity =>
                {
                    entity.CurrencyId ??= functionalId;
                });
            }

            // For resources that use residual monetary value, if currency id is functional
            // copy residual monetary value into residual value
            if (IsVisible(def.ResidualMonetaryValueVisibility) && IsVisible(def.ResidualValueVisibility))
            {
                entities.ForEach(entity =>
                {
                    if (entity.CurrencyId == functionalId && entity.ResidualMonetaryValue != null)
                    {
                        entity.ResidualValue = entity.ResidualMonetaryValue;
                    }
                });
            }

            // TODO: Move the logic to a more central place so other entities can be locationed
            if (IsVisible(def.LocationVisibility))
            {
                entities.ForEach(entity =>
                {
                    // Here we convert the GeoJson to Well-Known Binary
                    var json = entity.LocationJson;
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        entity.LocationWkb = null;
                        return;
                    }

                    try
                    {
                        var spy = JsonConvert.DeserializeObject<GeoJsonSpy>(json);
                        if (spy.Type == GeoJSONObjectType.Feature)
                        {
                            // A simple feature can be turned in to a simple WKB
                            var feature = JsonConvert.DeserializeObject<Feature>(json);

                            var geometry = feature?.Geometry;
                            entity.LocationWkb = geometry?.ToWkb();
                        }
                        else if (spy.Type == GeoJSONObjectType.FeatureCollection)
                        {
                            // A feature collection must be converted to a geometry collection and then turned to WKB
                            var coll = JsonConvert.DeserializeObject<FeatureCollection>(json);
                            var geometries = coll?.Features?.Select(feat => feat.Geometry)?.Where(e => e != null) ?? new List<IGeometryObject>();

                            if (geometries.Count() == 1)
                            {
                                // If it's just a single geometry, no need to wrap it in a geometry collection
                                var geometry = geometries.Single();
                                entity.LocationWkb = geometry?.ToWkb();
                            }
                            else
                            {
                                // If it's zero or multiple geometries, wrap in a geometry collection
                                var geomCollection = new GeometryCollection(geometries);
                                entity.LocationWkb = geomCollection?.ToWkb();
                            }
                        }
                        else
                        {
                            // I don't know what'd be the point of localizing this message
                            throw new InvalidOperationException("Root GeoJSON element must be a feature or a feature collection");
                        }
                    }
                    catch (Exception ex)
                    {
                        entity.EntityMetadata.LocationJsonParseError = ex.Message;
                        return;
                    }
                });
            } 
            else
            {
                entities.ForEach(entity =>
                {
                    entity.LocationJson = null;
                    entity.LocationWkb = null;
                });
            }

            // SQL Preprocessing
            await _repo.Resources__Preprocess(DefinitionId.Value, entities);
            return entities;
        }

        /// <summary>
        /// Used to peek at the root element of a GeoJson string using JSON.NET
        /// </summary>
        public class GeoJsonSpy : IGeometryObject
        {
            [JsonProperty(PropertyName = "type")]
            public GeoJSONObjectType Type { get; set; }
        }

        private bool IsVisible(string visibility)
        {
            return visibility == Visibility.Optional || visibility == Visibility.Required;
        }

        protected override async Task SaveValidateAsync(List<ResourceForSave> entities)
        {
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

        private void SetDefaultValue<TKey>(List<ResourceForSave> entities, Expression<Func<ResourceForSave, TKey>> selector, TKey defaultValue)
        {
            if (defaultValue != null)
            {
                Func<ResourceForSave, TKey> getPropValue = selector.Compile(); // The function to access the property value
                Action<ResourceForSave, TKey> setPropValue = ControllerUtilities.GetAssigner(selector).Compile();

                entities.ForEach(entity =>
                {
                    if (getPropValue(entity) == null)
                    {
                        setPropValue(entity, defaultValue);
                    }
                });
            }
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<ResourceForSave> entities, bool returnIds)
        {
            return await _repo.Resources__Save(DefinitionId.Value, entities, returnIds: returnIds);
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
            try
            {
                await _repo.Resources__Delete(ids);
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

        protected override SelectExpression ParseSelect(string select) => ResourceControllerUtil.ParseSelect(select, baseFunc: base.ParseSelect);

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
            await CheckActionPermissions("IsActive", ids);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.Resources__Activate(ids, isActive);

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

    // Generic API, allows reading all resources

    [Route("api/" + ResourcesController.BASE_ADDRESS)]
    [ApplicationController]
    public class ResourcesGenericController : FactWithIdControllerBase<Resource, int>
    {
        private readonly ResourcesGenericService _service;

        public ResourcesGenericController(ResourcesGenericService service, ILogger<ResourcesGenericController> logger) : base(logger)
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

        protected override Query<Resource> Search(Query<Resource> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return ResourceControllerUtil.SearchImpl(query, args, filteredPermissions);
        }

        protected override SelectExpression ParseSelect(string select) => ResourceControllerUtil.ParseSelect(select, baseFunc: base.ParseSelect);
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

        private static readonly string _documentDetailsSelect = string.Join(',', DocumentsService.EntryResourcePaths());
    }
}
