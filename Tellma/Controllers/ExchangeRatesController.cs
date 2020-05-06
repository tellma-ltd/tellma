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
using System;
using System.Threading;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class ExchangeRatesController : CrudControllerBase<ExchangeRateForSave, ExchangeRate, int>
    {
        public const string BASE_ADDRESS = "exchange-rates";

        private readonly ExchangeRatesService _service;
        private readonly ILogger _logger;

        public ExchangeRatesController(ExchangeRatesService service, ILogger<ExchangeRatesController> logger) : base(logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("convert-to-functional")]
        public async Task<ActionResult<decimal>> ConvertToFunctional(DateTime date, string currencyId, decimal amount, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var result = await _service.ConvertToFunctional(date, currencyId, amount, cancellation);
                return Ok(result);
            },
            _logger);
        }

        protected override CrudServiceBase<ExchangeRateForSave, ExchangeRate, int> GetCrudService()
        {
            return _service;
        }
    }

    public class ExchangeRatesService : CrudServiceBase<ExchangeRateForSave, ExchangeRate, int>
    {
        public const string BASE_ADDRESS = "exchange-rates";

        private readonly IStringLocalizer _localizer;
        private readonly ApplicationRepository _repo;
        private readonly ISettingsCache _settingsCache;

        private string View => BASE_ADDRESS;

        public ExchangeRatesService(
            IStringLocalizer<Strings> localizer,
            ApplicationRepository repo,
            ISettingsCache settingsCache) : base(localizer)
        {
            _localizer = localizer;
            _repo = repo;
            _settingsCache = settingsCache;
        }

        public async Task<decimal> ConvertToFunctional(DateTime date, string currencyId, decimal amount, CancellationToken cancellation)
        {
            var result = await _repo.ConvertToFunctional(date, currencyId, amount, cancellation);
            if (result == null)
            {
                throw new NotFoundException<(DateTime, string)>((date, currencyId));
            }
            else
            {
                return result.Value;
            }
        }

        protected override Query<ExchangeRate> GetAsQuery(List<ExchangeRateForSave> entities)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task SaveValidateAsync(List<ExchangeRateForSave> entities)
        {
            int index = 0;
            var functionalId = _settingsCache.GetCurrentSettingsIfCached()?.Data?.FunctionalCurrencyId;

            var currencyDateHash = entities
                .GroupBy(e => new { e.CurrencyId, e.ValidAsOf })
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .ToHashSet();

            // Get the currencies that contribute to duplications, so that we can put their names in the error message
            // Get them outside the loop in a single DB query (for performance)
            var currencyIdArray = currencyDateHash.Select(e => e.CurrencyId).Distinct().ToArray();
            var currencies = (await _repo.Currencies.FilterByIds(currencyIdArray).ToListAsync(cancellation: default)).ToDictionary(e => e.Id);

            foreach (var entity in entities)
            {
                // Currency cannot be functional
                if (entity.CurrencyId == functionalId)
                {
                    ModelState.AddModelError($"[{index}].{nameof(ExchangeRate.CurrencyId)}",
                        _localizer["Error_TheCurrencyMustBeDifferentThanFunctional"]);
                }

                if (entity.ValidAsOf > DateTime.Today.AddDays(1))
                {
                    ModelState.AddModelError($"[{index}].{nameof(ExchangeRate.ValidAsOf)}",
                        _localizer["Error_TheValidAsOfDateCannotBeInTheFuture"]);
                }

                // Amounts must be >= 1
                if (entity.AmountInCurrency <= 0m)
                {
                    ModelState.AddModelError($"[{index}].{nameof(ExchangeRate.AmountInCurrency)}",
                        _localizer["Error_TheAmountInCurrencyMustBeGreaterThanZero"]);
                }

                // Amounts must be >= 1
                if (entity.AmountInFunctional <= 0m)
                {
                    ModelState.AddModelError($"[{index}].{nameof(ExchangeRate.AmountInFunctional)}",
                        _localizer["Error_TheAmountInFunctionalMustBeGreaterThanZero"]);
                }

                // Currency and date must not be duplicated in the uploaded list
                if (currencyDateHash.Contains(entity))
                {
                    var currency = currencies[entity.CurrencyId];
                    var currencyName = _repo.GetTenantInfo().Localize(currency.Name, currency.Name2, currency.Name3);

                    ModelState.AddModelError($"[{index}].{nameof(ExchangeRate.CurrencyId)}",
                        _localizer["Error_TheCurrency0Date1AreDuplicated", currencyName, entity.ValidAsOf.Value.ToString("yyyy-MM-dd")]);
                }

                if (ModelState.HasReachedMaxErrors)
                {
                    // No need to keep going forever
                    return;
                }

                index++;
            }


            // No need to invoke SQL if the model state is full of errors
            if (ModelState.HasReachedMaxErrors)
            {
                // null Ids will cause an error when calling the SQL validation
                return;
            }

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.ExchangeRates_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<ExchangeRateForSave> entities, bool returnIds)
        {
            return await _repo.ExchangeRates__Save(entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.ExchangeRates_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.ExchangeRates__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["ExchangeRate"]]);
            }
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return await _repo.UserPermissions(action, View, cancellation);
        }

        protected override Query<ExchangeRate> Search(Query<ExchangeRate> query, GetArguments args, IEnumerable<AbstractPermission> filteredPermissions)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var currencyProp = nameof(ExchangeRate.Currency);
                var idProp = $"{currencyProp}/{nameof(Currency.Id)}";
                var nameProp = $"{currencyProp}/{nameof(Currency.Name)}";
                var name2Prop = $"{currencyProp}/{nameof(Currency.Name2)}";
                var name3Prop = $"{currencyProp}/{nameof(Currency.Name3)}";
                var descProp = $"{currencyProp}/{nameof(Currency.Description)}";
                var desc2Prop = $"{currencyProp}/{nameof(Currency.Description2)}";
                var desc3Prop = $"{currencyProp}/{nameof(Currency.Description3)}";

                // Prepare the filter string
                var filterString = $"{idProp} {Ops.contains} '{search}' or {nameProp} {Ops.contains} '{search}' or {name2Prop} {Ops.contains} '{search}' or {name3Prop} {Ops.contains} '{search}' or {descProp} {Ops.contains} '{search}' or {desc2Prop} {Ops.contains} '{search}' or {desc3Prop} {Ops.contains} '{search}'";

                // If the search is a date, include documents with that date
                if (DateTime.TryParse(search.Trim(), out DateTime searchDate))
                {
                    var validAsOfProp = nameof(ExchangeRate.ValidAsOf);
                    filterString = $"{filterString} or {validAsOfProp} {Ops.eq} {searchDate.ToString("yyyy-MM-dd")}";
                }

                // Apply the filter
                query = query.Filter(FilterExpression.Parse(filterString));

            }

            return query;
        }

        protected override OrderByExpression DefaultOrderBy()
        {
            return OrderByExpression.Parse(nameof(ExchangeRate.ValidAsOf) + " desc");
        }
    }
}
