using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Model.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class CurrenciesController : CrudControllerBase<CurrencyForSave, Currency, string>
    {
        public const string BASE_ADDRESS = "currencies";

        private readonly CurrenciesService _service;

        public CurrenciesController(CurrenciesService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Currency>>> Activate([FromBody] List<string> ids, [FromQuery] ActivateArguments args)
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
        public async Task<ActionResult<EntitiesResponse<Currency>>> Deactivate([FromBody] List<string> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Deactivate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);

            }, _logger);
        }

        protected override CrudServiceBase<CurrencyForSave, Currency, string> GetCrudService()
        {
            return _service;
        }
    }

    public class CurrenciesService : CrudServiceBase<CurrencyForSave, Currency, string>
    {
        private readonly ApplicationRepository _repo;

        private string View => CurrenciesController.BASE_ADDRESS;

        public CurrenciesService(ApplicationRepository repo, IServiceProvider sp) : base(sp)
        {
            _repo = repo;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.PermissionsFromCache(View, action, cancellation);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<Currency> Search(Query<Currency> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var id = nameof(Currency.Id);
                var name = nameof(Currency.Name);
                var name2 = nameof(Currency.Name2);
                var name3 = nameof(Currency.Name3);
                var desc = nameof(Currency.Description);
                var desc2 = nameof(Currency.Description2);
                var desc3 = nameof(Currency.Description3);

                var filterString = $"{id} contains '{search}' or {name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {desc} contains '{search}' or {desc2} contains '{search}' or {desc3} contains '{search}'";
                query = query.Filter(ExpressionFilter.Parse(filterString));
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
                    string msg = _localizer[Services.Utilities.Constants.Error_Field0IsRequired, _localizer["Code"]];

                    ModelState.AddModelError(path, msg);
                }
                else if (entity.Id.Length > 3)
                {
                    string path = $"[{index}].{nameof(entity.Id)}";
                    string msg = _localizer[Services.Utilities.Constants.Error_Field0LengthMaximumOf1, _localizer["Code"], 3];

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

        protected override async Task<List<string>> SaveExecuteAsync(List<CurrencyForSave> entities, bool returnIds)
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

        protected override ExpressionOrderBy DefaultOrderBy()
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

            return ExpressionOrderBy.Parse(nameProperty);
        }

        public Task<(List<Currency>, Extras)> Activate(List<string> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<Currency>, Extras)> Deactivate(List<string> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<Currency>, Extras)> SetIsActive(List<string> ids, ActionArguments args, bool isActive)
        {
            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.Currencies__Activate(ids, isActive);

            List<Currency> data = null;
            Extras extras = null;

            if (args.ReturnEntities ?? false)
            {
                (data, extras) = await GetByIds(ids, args, action, cancellation: default);
            }

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, data);

            trx.Complete();
            return (data, extras);
        }
    }
}
