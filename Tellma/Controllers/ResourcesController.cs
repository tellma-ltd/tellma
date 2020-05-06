using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
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
        private readonly IModelMetadataProvider _modelMetadataProvider;

        private string _definitionIdOverride;

        private string DefinitionId => _definitionIdOverride ??
            _contextAccessor.HttpContext?.Request?.RouteValues?.GetValueOrDefault("definitionId")?.ToString() ??
            throw new BadRequestException($"Bug: DefinitoinId could not be determined in {nameof(ResourcesService)}");

        /// <summary>
        /// Overrides the default behavior of reading the definition Id from the route data
        /// </summary>
        public ResourcesService SetDefinitionId(string definitionId)
        {
            _definitionIdOverride = definitionId;
            return this;
        }

        private ResourceDefinitionForClient Definition() => _definitionsCache.GetCurrentDefinitionsIfCached()?.Data?.Resources?
            .GetValueOrDefault(DefinitionId) ?? throw new InvalidOperationException($"Definition for '{DefinitionId}' was missing from the cache");

        private string View => $"{ResourcesController.BASE_ADDRESS}{DefinitionId}";

        public ResourcesService(
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo,
            IDefinitionsCache definitionsCache,
            ISettingsCache settingsCache,
            IHttpContextAccessor contextAccessor,
            IModelMetadataProvider modelMetadataProvider) : base(localizer)
        {
            _localizer = localizer;
            _repo = repo;
            _definitionsCache = definitionsCache;
            _settingsCache = settingsCache;
            _contextAccessor = contextAccessor;
            _modelMetadataProvider = modelMetadataProvider;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return await _repo.UserPermissions(action, View, cancellation);
        }

        protected override IRepository GetRepository()
        {
            string filter = $"{nameof(Resource.DefinitionId)} {Ops.eq} '{DefinitionId}'";
            return new FilteredRepository<Resource>(_repo, filter);
        }

        protected override Query<Resource> Search(Query<Resource> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return ResourceControllerUtil.SearchImpl(query, args, filteredPermissions);
        }

        protected override async Task<List<ResourceForSave>> SavePreprocessAsync(List<ResourceForSave> entities)
        {
            var definition = Definition();

            // Set default values
            SetDefaultValue(entities, e => e.Identifier, definition.IdentifierDefaultValue);
            SetDefaultValue(entities, e => e.CurrencyId, definition.CurrencyDefaultValue);
            SetDefaultValue(entities, e => e.MonetaryValue, definition.MonetaryValueDefaultValue);
            //SetDefaultValue(entities, e => e.Description, definition.DescriptionDefaultValue);
            //SetDefaultValue(entities, e => e.Description2, definition.Description2DefaultValue);
            //SetDefaultValue(entities, e => e.Description3, definition.Description3DefaultValue);
            SetDefaultValue(entities, e => e.ReorderLevel, definition.ReorderLevelDefaultValue);
            SetDefaultValue(entities, e => e.EconomicOrderQuantity, definition.EconomicOrderQuantityDefaultValue);
            SetDefaultValue(entities, e => e.AvailableSince, definition.AvailableSinceDefaultValue);
            SetDefaultValue(entities, e => e.AvailableTill, definition.AvailableTillDefaultValue);
            SetDefaultValue(entities, e => e.Decimal1, definition.Decimal1DefaultValue);
            SetDefaultValue(entities, e => e.Decimal2, definition.Decimal2DefaultValue);
            SetDefaultValue(entities, e => e.Int1, definition.Int1DefaultValue);
            SetDefaultValue(entities, e => e.Int2, definition.Int2DefaultValue);
            SetDefaultValue(entities, e => e.Lookup1Id, definition.Lookup1DefaultValue);
            SetDefaultValue(entities, e => e.Lookup2Id, definition.Lookup2DefaultValue);
            SetDefaultValue(entities, e => e.Lookup3Id, definition.Lookup3DefaultValue);
            SetDefaultValue(entities, e => e.Lookup4Id, definition.Lookup4DefaultValue);
            //SetDefaultValue(entities, e => e.Lookup5Id, definition.Lookup5DefaultValue);
            SetDefaultValue(entities, e => e.Text1, definition.Text1DefaultValue);
            SetDefaultValue(entities, e => e.Text2, definition.Text2DefaultValue);

            var settings = _settingsCache.GetCurrentSettingsIfCached()?.Data;
            var functionalId = settings.FunctionalCurrencyId;

            if (IsVisible(definition.ResidualMonetaryValueVisibility))
            {
                entities.ForEach(entity =>
                {
                    entity.CurrencyId ??= functionalId;
                });
            }

            // For resources that use residual monetary value, if currency id is functional
            // copy residual monetary value into residual value
            if (IsVisible(definition.ResidualMonetaryValueVisibility) && IsVisible(definition.ResidualValueVisibility))
            {
                entities.ForEach(entity =>
                {
                    if (entity.CurrencyId == functionalId && entity.ResidualMonetaryValue != null)
                    {
                        entity.ResidualValue = entity.ResidualMonetaryValue;
                    }
                });
            }

            // SQL Preprocessing
            await _repo.Resources__Preprocess(DefinitionId, entities);
            return entities;
        }

        private bool IsVisible(string visibility)
        {
            return visibility == Visibility.Optional || visibility == Visibility.Required;
        }

        protected override async Task SaveValidateAsync(List<ResourceForSave> entities)
        {
            var definition = Definition();

            // Validate required stuff
            ValidateIfRequired(entities, e => e.Identifier, definition.IdentifierVisibility);
            ValidateIfRequired(entities, e => e.CurrencyId, definition.CurrencyVisibility);
            ValidateIfRequired(entities, e => e.MonetaryValue, definition.MonetaryValueVisibility);
            ValidateIfRequired(entities, e => e.Description, definition.DescriptionVisibility);
            ValidateIfRequired(entities, e => e.Description2, definition.DescriptionVisibility);
            ValidateIfRequired(entities, e => e.Description3, definition.DescriptionVisibility);
            ValidateIfRequired(entities, e => e.AssetTypeId, definition.AssetTypeVisibility);
            ValidateIfRequired(entities, e => e.RevenueTypeId, definition.RevenueTypeVisibility);
            ValidateIfRequired(entities, e => e.ExpenseTypeId, definition.ExpenseTypeVisibility);
            ValidateIfRequired(entities, e => e.ExpenseEntryTypeId, definition.ExpenseEntryTypeVisibility);
            ValidateIfRequired(entities, e => e.CenterId, definition.CenterVisibility);

            ValidateIfRequired(entities, e => e.ResidualMonetaryValue, definition.ResidualMonetaryValueVisibility);
            ValidateIfRequired(entities, e => e.ResidualValue, definition.ResidualValueVisibility);
            ValidateIfRequired(entities, e => e.ReorderLevel, definition.ReorderLevelVisibility);
            ValidateIfRequired(entities, e => e.EconomicOrderQuantity, definition.EconomicOrderQuantityVisibility);
            ValidateIfRequired(entities, e => e.AvailableSince, definition.AvailableSinceVisibility);
            ValidateIfRequired(entities, e => e.AvailableTill, definition.AvailableTillVisibility);
            ValidateIfRequired(entities, e => e.Decimal1, definition.Decimal1Visibility);
            ValidateIfRequired(entities, e => e.Decimal2, definition.Decimal2Visibility);
            ValidateIfRequired(entities, e => e.Int1, definition.Decimal1Visibility);
            ValidateIfRequired(entities, e => e.Int2, definition.Decimal2Visibility);
            ValidateIfRequired(entities, e => e.Lookup1Id, definition.Lookup1Visibility);
            ValidateIfRequired(entities, e => e.Lookup2Id, definition.Lookup2Visibility);
            ValidateIfRequired(entities, e => e.Lookup3Id, definition.Lookup3Visibility);
            ValidateIfRequired(entities, e => e.Lookup4Id, definition.Lookup4Visibility);
            //ValidateIfRequired(entities, e => e.Lookup5Id, definition.Lookup5Visibility);
            ValidateIfRequired(entities, e => e.Text1, definition.Text1Visibility);
            ValidateIfRequired(entities, e => e.Text2, definition.Text2Visibility);

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                // null Ids will cause an error when calling the SQL validation
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Resources_Validate__Save(DefinitionId, entities, top: remainingErrorCount);

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

        private void ValidateIfRequired<TKey>(List<ResourceForSave> entities, Expression<Func<ResourceForSave, TKey>> selector, string visibility)
        {
            if (visibility == Visibility.Required && !ModelState.HasReachedMaxErrors)
            {
                Func<ResourceForSave, TKey> getPropValue = selector.Compile(); // The function to access the property value

                foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
                {
                    if (getPropValue(entity) == null)
                    {
                        string propName = (selector.Body as MemberExpression).Member.Name; // The name of the property we're validating
                        string path = $"[{index}].{propName}";
                        string propDisplayName = _modelMetadataProvider.GetMetadataForProperty(typeof(ResourceForSave), propName)?.DisplayName;
                        string errorMsg = _localizer[Services.Utilities.Constants.Error_TheField0IsRequired, propDisplayName];

                        ModelState.AddModelError(path, errorMsg);

                        if (ModelState.HasReachedMaxErrors)
                        {
                            // No need to keep going forever
                            break;
                        }
                    }
                }
            }
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<ResourceForSave> entities, bool returnIds)
        {
            return await _repo.Resources__Save(DefinitionId, entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Resources_Validate__Delete(DefinitionId, ids, top: remainingErrorCount);

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

        public ResourcesGenericService(IStringLocalizer<Strings> localizer, ApplicationRepository repo) : base(localizer)
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
                string definitionId = permission.View.Remove(0, prefix.Length).Replace("'", "''");
                string definitionPredicate = $"{nameof(Resource.DefinitionId)} {Ops.eq} '{definitionId}'";
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
            string shorthand = "$DocumentDetailsForEntry";
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
