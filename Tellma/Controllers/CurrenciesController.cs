using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class CurrenciesController : CrudControllerBase<CurrencyForSave, Currency, string>
    {
        public const string BASE_ADDRESS = "currencies";

        private readonly ILogger _logger;
        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;

        private string View => BASE_ADDRESS;

        public CurrenciesController(
            ILogger<CurrenciesController> logger,
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo) : base(logger, localizer)
        {
            _logger = logger;
            _localizer = localizer;
            _repo = repo;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Currency>>> Activate([FromBody] List<string> ids, [FromQuery] ActivateArguments args)
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
        public async Task<ActionResult<EntitiesResponse<Currency>>> Deactivate([FromBody] List<string> ids, [FromQuery] DeactivateArguments args)
        {
            bool returnEntities = args.ReturnEntities ?? false;

            return await ControllerUtilities.InvokeActionImpl(() =>
                Activate(ids: ids,
                    returnEntities: returnEntities,
                    expand: args.Expand,
                    isActive: false)
            , _logger);
        }

        private async Task<ActionResult<EntitiesResponse<Currency>>> Activate([FromBody] List<string> ids, bool returnEntities, string expand, bool isActive)
        {
            // Parse parameters
            var expandExp = ExpandExpression.Parse(expand);
            var idsArray = ids.ToArray();

            // Check user permissions
            await CheckActionPermissions("IsActive", idsArray);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.Currencies__Activate(ids, isActive);

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

        protected override Query<Currency> Search(Query<Currency> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Currency.Name);
                var name2 = nameof(Currency.Name2);
                var name3 = nameof(Currency.Name3);
                var desc = nameof(Currency.Description);
                var desc2 = nameof(Currency.Description2);
                var desc3 = nameof(Currency.Description3);

                var filterString = $"{name} {Ops.contains} '{search}' or {name2} {Ops.contains} '{search}' or {name3} {Ops.contains} '{search}' or {desc} {Ops.contains} '{search}' or {desc2} {Ops.contains} '{search}' or {desc3} {Ops.contains} '{search}'";
                query = query.Filter(FilterExpression.Parse(filterString));
            }

            return query;
        }

        protected override async Task SaveValidateAsync(List<CurrencyForSave> entities)
        {
            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            {
                // Ensure that Id is supplied
                if (string.IsNullOrEmpty(entity.Id))
                {
                    string path = $"[{index}].{nameof(entity.Id)}";
                    string msg = _localizer[Services.Utilities.Constants.Error_TheField0IsRequired, _localizer["Code"]];

                    ModelState.AddModelError(path, msg);
                }
                else if (entity.Id.Length > 3)
                {
                    string path = $"[{index}].{nameof(entity.Id)}";
                    string msg = _localizer[nameof(StringLengthAttribute), _localizer["Code"], 3];

                    ModelState.AddModelError(path, msg);
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
            var sqlErrors = await _repo.Currencies_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<string>> SaveExecuteAsync(List<CurrencyForSave> entities, ExpandExpression expand, bool returnIds)
        {
            await _repo.Currencies__Save(entities);
            return entities.Select(e => e.Id).ToList();
        }

        protected override async Task DeleteValidateAsync(List<string> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Currencies_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<string> ids)
        {
            try
            {
                await _repo.Currencies__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["Currency"]]);
            }
        }

        protected override Query<Currency> GetAsQuery(List<CurrencyForSave> entities)
        {
            return _repo.Currencies__AsQuery(entities);
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            // By default: Order currencies by name
            var tenantInfo = _repo.GetTenantInfo();
            string nameProperty = nameof(Currency.Name);
            if (tenantInfo.SecondaryLanguageId == CultureInfo.CurrentUICulture.Name)
            {
                nameProperty = $"{nameof(Currency.Name2)},{nameof(Currency.Name)}";
            }
            else if (tenantInfo.TernaryLanguageId == CultureInfo.CurrentUICulture.Name)
            {
                nameProperty = $"{nameof(Currency.Name3)},{nameof(Currency.Name)}";
            }

            return OrderByExpression.Parse(nameProperty);
        }
    }
}
