using BSharp.Controllers.Dto;
using BSharp.Controllers.Utilities;
using BSharp.Data;
using BSharp.Data.Queries;
using BSharp.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    // Specific API, works with a certain definitionId, and allows read-write
    [Route("api/" + BASE_ADDRESS + "{definitionId}")]
    [ApplicationApi]
    public class AccountsController : CrudControllerBase<AccountForSave, Account, int>
    {
        public const string BASE_ADDRESS = "accounts/";

        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;
        private readonly IDefinitionsCache _definitionsCache;
        private readonly IModelMetadataProvider _modelMetadataProvider;

        private string DefinitionId => RouteData.Values["definitionId"]?.ToString() ??
            throw new BadRequestException($"URI must be of the form 'api/" + BASE_ADDRESS + "{definitionId}'");

        private AccountDefinitionForClient Definition() => _definitionsCache.GetCurrentDefinitionsIfCached()?.Data?.Accounts?
            .GetValueOrDefault(DefinitionId) ?? throw new InvalidOperationException($"Definition for '{DefinitionId}' was missing from the cache");

        private string ViewId => $"{BASE_ADDRESS}{DefinitionId}";

        public AccountsController(
            ILogger<AccountsController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo,
            IDefinitionsCache definitionsCache,
            IModelMetadataProvider modelMetadataProvider) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
            _definitionsCache = definitionsCache;
            _modelMetadataProvider = modelMetadataProvider;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Account>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    isDeprecated: false)
            , _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Account>>> Deprecate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    isDeprecated: true)
            , _logger);
        }

        private async Task<ActionResult<EntitiesResponse<Account>>> Activate([FromBody] List<int> ids, bool returnEntities, string expand, bool isDeprecated)
        {
            // Parse parameters
            var expandExp = ExpandExpression.Parse(expand);
            var idsArray = ids.ToArray();

            // Check user permissions
            await CheckActionPermissions("IsDeprecated", idsArray);

            // Execute and return
            using (var trx = ControllerUtilities.CreateTransaction())
            {
                await _repo.Accounts__Deprecate(ids, isDeprecated);

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
            return await _repo.UserPermissions(action, ViewId);
        }

        protected override IRepository GetRepository()
        {
            string filter = $"{nameof(Account.DefinitionId)} eq '{DefinitionId}'";
            return new FilteredRepository<Account>(_repo, filter);
        }

        protected override Query<Account> Search(Query<Account> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return AccountControllerUtil.SearchImpl(query, args, filteredPermissions);
        }

        protected override async Task SaveValidateAsync(List<AccountForSave> entities)
        {
            var definition = Definition();

            // Set default values
            SetDefaultValue(entities, e => e.ResourceId, definition.ResourceDefaultValue);
            SetDefaultValue(entities, e => e.CustodianId, definition.CustodianDefaultValue);
            SetDefaultValue(entities, e => e.ResponsibilityCenterId, definition.ResponsibilityCenterDefaultValue);
            SetDefaultValue(entities, e => e.LocationId, definition.LocationDefaultValue);

            // Validate required stuff
            ValidateIfRequired(entities, e => e.PartyReference, definition.PartyReferenceVisibility);
            ValidateIfRequired(entities, e => e.ResourceId, definition.ResourceVisibility);
            ValidateIfRequired(entities, e => e.CustodianId, definition.CustodianVisibility);
            ValidateIfRequired(entities, e => e.ResponsibilityCenterId, definition.ResponsibilityCenterVisibility);
            ValidateIfRequired(entities, e => e.LocationId, definition.LocationVisibility);

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                // null Ids will cause an error when calling the SQL validation
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Accounts_Validate__Save(DefinitionId, entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        private void SetDefaultValue<TKey>(List<AccountForSave> entities, Expression<Func<AccountForSave, TKey>> selector, TKey defaultValue)
        {
            if (defaultValue != null)
            {
                Func<AccountForSave, TKey> getPropValue = selector.Compile(); // The function to access the property value
                Action<AccountForSave, TKey> setPropValue = ControllerUtilities.GetAssigner(selector).Compile();

                entities.ForEach(entity =>
                {
                    if (getPropValue(entity) == null)
                    {
                        setPropValue(entity, defaultValue);
                    }
                });
            }
        }

        private void ValidateIfRequired<TKey>(List<AccountForSave> entities, Expression<Func<AccountForSave, TKey>> selector, string visibility)
        {
            if (visibility == "RequiredInAccount" && !ModelState.HasReachedMaxErrors)
            {
                Func<AccountForSave, TKey> getPropValue = selector.Compile(); // The function to access the property value

                foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
                {
                    if (getPropValue(entity) == null)
                    {
                        string propName = (selector.Body as MemberExpression).Member.Name; // The name of the property we're validating
                        string path = $"[{index}].{propName}";
                        string propDisplayName = _modelMetadataProvider.GetMetadataForProperty(typeof(AccountForSave), propName)? .DisplayName;
                        string errorMsg = _localizer[nameof(RequiredAttribute), propDisplayName];

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

        protected override async Task<List<int>> SaveExecuteAsync(List<AccountForSave> entities, ExpandExpression expand, bool returnIds)
        {
            return await _repo.Accounts__Save(DefinitionId, entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Accounts_Validate__Delete(DefinitionId, ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.Accounts__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                // TODO: test
                var definition = Definition();
                var tenantInfo = await _repo.GetTenantInfoAsync();
                var titleSingular = tenantInfo.Localize(definition.TitleSingular, definition.TitleSingular2, definition.TitleSingular3);

                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", titleSingular]);
            }
        }

        protected override Query<Account> GetAsQuery(List<AccountForSave> entities)
        {
            return _repo.Accounts__AsQuery(DefinitionId, entities);
        }
    }

    // Generic API, allows reading all Accounts

    [Route("api/accounts")]
    [ApplicationApi]
    public class AccountsGenericController : FactWithIdControllerBase<Account, int>
    {
        private readonly ApplicationRepository _repo;

        public AccountsGenericController(
            ILogger<AccountsController> logger,
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
            // Get all permissions pertaining to Accounts
            string prefix = AccountsController.BASE_ADDRESS;
            var permissions = await _repo.GenericUserPermissions(action, prefix);

            // Massage the permissions by adding definitionId = definitionId as an extra clause 
            // (since the controller will not filter the results per any specific definition Id)
            foreach (var permission in permissions.Where(e => e.ViewId != "all"))
            {
                string definitionId = permission.ViewId.Remove(0, prefix.Length).Replace("'", "''");
                string definitionPredicate = $"{nameof(Account.DefinitionId)} eq '{definitionId}'";
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

        protected override Query<Account> Search(Query<Account> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            return AccountControllerUtil.SearchImpl(query, args, filteredPermissions);
        }
    }

    internal class AccountControllerUtil
    {
        /// <summary>
        /// This is needed in both the generic and specific controllers, so we move it out here
        /// </summary>
        public static Query<Account> SearchImpl(Query<Account> query, GetArguments args, IEnumerable<AbstractPermission> _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Account.Name);
                var name2 = nameof(Account.Name2);
                var name3 = nameof(Account.Name3);
                var code = nameof(Account.Code);
                var partyRef = nameof(Account.PartyReference);

                query = query.Filter($"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}' or {partyRef} {Ops.contains} '{search}'");
            }

            return query;
        }
    }
}
