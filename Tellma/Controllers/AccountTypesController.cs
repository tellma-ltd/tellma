using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class AccountTypesController : CrudTreeControllerBase<AccountTypeForSave, AccountType, int>
    {
        public const string BASE_ADDRESS = "account-types";

        private readonly ILogger _logger;
        private readonly IStringLocalizer<Strings> _localizer;
        private readonly ApplicationRepository _repo;

        private string View => BASE_ADDRESS;

        public AccountTypesController(
            ILogger<AccountTypesController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<AccountType>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
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
        public async Task<ActionResult<EntitiesResponse<AccountType>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    isActive: false)
            , _logger);
        }

        private async Task<ActionResult<EntitiesResponse<AccountType>>> Activate(List<int> ids, bool returnEntities, string expand, bool isActive)
        {
            // Parse parameters
            var expandExp = ExpandExpression.Parse(expand);
            var idsArray = ids.ToArray();

            // Check user permissions
            await CheckActionPermissions("IsActive", idsArray);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.AccountTypes__Activate(ids, isActive);

            if (returnEntities)
            {
                var response = await LoadDataByIdsAndTransform(idsArray, expandExp);

                trx.Complete();
                return Ok(response);
            }
            else
            {
                trx.Complete();
                return Ok();
            }
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action)
        {
            return await _repo.UserPermissions(action, View);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<AccountType> Search(Query<AccountType> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(AccountType.Name);
                var name2 = nameof(AccountType.Name2);
                var name3 = nameof(AccountType.Name3);

                var filterString = $"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}'";
                query = query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }

        protected override Query<AccountType> GetAsQuery(List<AccountTypeForSave> entities)
        {
            throw new System.NotImplementedException();
        }
        
        protected override Task<List<AccountTypeForSave>> SavePreprocessAsync(List<AccountTypeForSave> entities)
        {
            // Set defaults
            entities.ForEach(entity =>
            {
                entity.IsAssignable ??= true;
                entity.AgentAssignment ??= 'N';
                entity.CenterAssignment ??= 'N';
                entity.CurrencyAssignment ??= 'N';
                entity.EntryTypeAssignment ??= 'N';
                entity.IdentifierAssignment ??= 'N';
                entity.NotedAgentAssignment ??= 'N';
                entity.ResourceAssignment ??= 'N';

                if (entity.EntryTypeAssignment == 'N')
                {
                    entity.EntryTypeParentId = null;
                }

                if (entity.AgentAssignment == 'N')
                {
                    entity.AgentDefinitionId = null;
                }

                if (entity.ResourceAssignment == 'N')
                {
                    entity.ResourceDefinitionId = null;
                }
            });

            return Task.FromResult(entities);
        }
        
        protected override async Task SaveValidateAsync(List<AccountTypeForSave> entities)
        {
            // Check that codes are not duplicated within the arriving collection
            var duplicateCodes = entities.Where(e => e.Code != null).GroupBy(e => e.Code).Where(g => g.Count() > 1);
            if (duplicateCodes.Any())
            {
                // Hash the entities' indices for performance
                Dictionary<AccountTypeForSave, int> indices = entities.ToIndexDictionary();

                foreach (var groupWithDuplicateCodes in duplicateCodes)
                {
                    foreach (var entity in groupWithDuplicateCodes)
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        ModelState.AddModelError($"[{index}].Code", _localizer["Error_TheCode0IsDuplicated", entity.Code]);
                    }
                }
            }

            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                // If EntryTypeAssignment is either Account of Entry, then EntryTypeParentId must be specified
                if (entity.EntryTypeAssignment != 'N' && entity.EntryTypeParentId == null)
                {
                    var errorMsg = _localizer[Constants.Error_TheField0IsRequired, _localizer["AccountType_EntryTypeParent"]];
                    ModelState.AddModelError($"[{index}].{nameof(AccountTypeForSave.EntryTypeParentId)}", errorMsg);
                }
            }

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.AccountTypes_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<AccountTypeForSave> entities, ExpandExpression expand, bool returnIds)
        {
            return await _repo.AccountTypes__Save(entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.AccountTypes_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.AccountTypes__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["AccountType"]]);
            }
        }
       
        protected override async Task ValidateDeleteWithDescendantsAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.AccountTypes_Validate__DeleteWithDescendants(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteWithDescendantsAsync(List<int> ids)
        {
            try
            {
                await _repo.AccountTypes__DeleteWithDescendants(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["AccountType"]]);
            }
        }

    }
}
