using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tellma.Services.Utilities;
using System.Threading;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class AccountsController : CrudControllerBase<AccountForSave, Account, int>
    {
        public const string BASE_ADDRESS = "accounts";

        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;
        private readonly ISettingsCache _settingsCache;

        private string View => BASE_ADDRESS;

        public AccountsController(
            ILogger<AccountsController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo,
            ISettingsCache settingsCache) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
            _settingsCache = settingsCache;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Account>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(() => ActivateImpl(ids: ids, args, isDeprecated: false), _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Account>>> Deprecate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(() => ActivateImpl(ids: ids, args, isDeprecated: true), _logger);
        }

        private async Task<ActionResult<EntitiesResponse<Account>>> ActivateImpl(List<int> ids, ActionArguments args, bool isDeprecated)
        {
            // Check user permissions
            await CheckActionPermissions("IsDeprecated", ids);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.Accounts__Deprecate(ids, isDeprecated);

            if (args.ReturnEntities ?? false)
            {
                var response = await LoadDataByIdsAndTransform(ids, args);

                trx.Complete();
                return Ok(response);
            }
            else
            {
                trx.Complete();
                return Ok();
            }
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return await _repo.UserPermissions(action, View, cancellation);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<Account> Search(Query<Account> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Account.Name);
                var name2 = nameof(Account.Name2);
                var name3 = nameof(Account.Name3);
                var code = nameof(Account.Code);

                query = query.Filter($"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {code} {Ops.contains} '{search}'");
            }

            return query;
        }

        protected override async Task<List<AccountForSave>> SavePreprocessAsync(List<AccountForSave> entities)
        {
            // Defaults
            //var settings = _settingsCache.GetCurrentSettingsIfCached().Data;
            entities.ForEach(entity =>
            {
                // Set defaults
                entity.IsRelated ??= false;
                entity.IsSmart ??= false;

                // Set invisible fields to NULL when IsSmart = false
                if (!entity.IsSmart.Value)
                {
                    entity.ResourceId = null;
                    entity.AgentId = null;
                    entity.Identifier = null;
                    entity.EntryTypeId = null;
                }
            });

            // SQL Preprocessing
            await _repo.Accounts__Preprocess(entities);
            return entities;
        }

        protected override async Task SaveValidateAsync(List<AccountForSave> entities)
        {
            // Find duplicate codes within the saved list
            var duplicateCodes = entities
                .Where(e => e.Code != null)
                .GroupBy(e => e.Code)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .ToHashSet();

            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                // Check that the code is unique within the saved list
                if (duplicateCodes.Contains(entity))
                {
                    ModelState.AddModelError($"[{index}].Code", _localizer["Error_TheCode0IsDuplicated", entity.Code]);
                }

                if (entity.IsSmart.Value)
                {
                    // Can we add any validation here?
                } 
                else
                {
                    // These are required for smart accounts
                    if (entity.CurrencyId == null)
                    {
                        string path = $"[{index}].{nameof(AccountForSave.CurrencyId)}";
                        string propDisplayName = _localizer["Account_Currency"];
                        string errorMsg = _localizer[Services.Utilities.Constants.Error_TheField0IsRequired, propDisplayName];

                        ModelState.AddModelError(path, errorMsg);
                    }

                    //// These are required for smart accounts
                    //if (entity.CenterId == null)
                    //{
                    //    string path = $"[{index}].{nameof(AccountForSave.CenterId)}";
                    //    string propDisplayName = _localizer["Account_Center"];
                    //    string errorMsg = _localizer[Services.Utilities.Constants.Error_TheField0IsRequired, propDisplayName];

                    //    ModelState.AddModelError(path, errorMsg);
                    //}
                }

                if (ModelState.HasReachedMaxErrors)
                {
                    // No need to keep going forever
                    break;
                }
            }

            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                // null Ids will cause an error when calling the SQL validation
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Accounts_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<AccountForSave> entities, bool returnIds)
        {
            return await _repo.Accounts__Save(entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Accounts_Validate__Delete(ids, top: remainingErrorCount);

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
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["Account"]]);
            }
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse(nameof(Account.Code));
        }

        protected override SelectExpression ParseSelect(string select)
        {
            string shorthand = "$DocumentDetails";
            if (select == null)
            {
                return null;
            }
            else
            {
                select = select.Replace(shorthand, _documentDetailsSelect);
                return base.ParseSelect(select);
            }
        }

        private static readonly string _documentDetailsSelect = string.Join(',', DocumentsController.AccountPaths());
    }
}
