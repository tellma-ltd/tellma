using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class ExchangeRatesService : CrudServiceBase<ExchangeRateForSave, ExchangeRate, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer _localizer;

        protected override string View => "exchange-rates";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        public ExchangeRatesService(
            ApplicationFactServiceBehavior behavior,
            CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
            _localizer = deps.Localizer;
        }

        public async Task<decimal> ConvertToFunctional(DateTime date, string currencyId, decimal amount, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            var result = await _behavior.Repository.ConvertToFunctional(date, currencyId, amount, cancellation);
            if (result == null)
            {
                // The client side shows this error in a friendly way
                throw new ServiceException("Exchange rate was not found.");
            }
            else
            {
                return result.Value;
            }
        }

        protected override Task<List<ExchangeRateForSave>> SavePreprocessAsync(List<ExchangeRateForSave> entities)
        {
            entities.ForEach(e =>
            {
                e.AmountInCurrency = 1;
            });

            return base.SavePreprocessAsync(entities);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<ExchangeRateForSave> entities, bool returnIds)
        {
            #region Validate

            var settings = await _behavior.Settings();
            var functionalId = settings.FunctionalCurrencyId;

            var currencyDateHash = entities
                .GroupBy(e => new { e.CurrencyId, e.ValidAsOf })
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .ToHashSet();

            // Get the currencies that contribute to duplications, so that we can put their names in the error message
            // Get them outside the loop in a single DB query (for performance)
            string[] duplicateCurrenciesIdArray = null;
            List<Currency> duplicateCurrencies = null;
            Dictionary<string, Currency> duplicateCurrenciesDictionary = null;

            foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
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
                    // Populate these ones for efficient subsequent passes
                    duplicateCurrenciesIdArray ??= currencyDateHash.Select(e => e.CurrencyId).Distinct().ToArray();
                    duplicateCurrencies ??= await _behavior.Repository.Currencies.FilterByIds(duplicateCurrenciesIdArray).ToListAsync(QueryContext, cancellation: default);
                    duplicateCurrenciesDictionary ??= duplicateCurrencies.ToDictionary(e => e.Id);

                    var currency = duplicateCurrenciesDictionary[entity.CurrencyId];
                    var currencyName = settings.Localize(currency.Name, currency.Name2, currency.Name3);

                    ModelState.AddModelError($"[{index}].{nameof(ExchangeRate.CurrencyId)}",
                        _localizer["Error_TheCurrency0Date1AreDuplicated", currencyName, entity.ValidAsOf.Value.ToString("yyyy-MM-dd")]);
                }

                if (ModelState.HasReachedMaxErrors)
                {
                    // No need to keep going forever
                    break;
                }
            }

            if (!ModelState.IsValid)
            {
                // No need to keep going forever
                return null;
            }

            #endregion

            #region Save

            SaveResult result = await _behavior.Repository.ExchangeRates__Save(entities, returnIds: returnIds, UserId);
            AddLocalizedErrors(result.Errors);

            return result.Ids;

            #endregion
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                DeleteResult result = await _behavior.Repository.ExchangeRates__Delete(ids, userId: UserId);
                AddLocalizedErrors(result.Errors);
            }
            catch (ForeignKeyViolationException)
            {
                var meta = await GetMetadata(cancellation: default);
                throw new ServiceException(_localizer["Error_CannotDelete0AlreadyInUse", meta.SingularDisplay()]);
            }
        }

        protected override Task<EntityQuery<ExchangeRate>> Search(EntityQuery<ExchangeRate> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var currencyProp = nameof(ExchangeRate.Currency);
                var idProp = $"{currencyProp}.{nameof(Currency.Id)}";
                var nameProp = $"{currencyProp}.{nameof(Currency.Name)}";
                var name2Prop = $"{currencyProp}.{nameof(Currency.Name2)}";
                var name3Prop = $"{currencyProp}.{nameof(Currency.Name3)}";
                var descProp = $"{currencyProp}.{nameof(Currency.Description)}";
                var desc2Prop = $"{currencyProp}.{nameof(Currency.Description2)}";
                var desc3Prop = $"{currencyProp}.{nameof(Currency.Description3)}";

                // Prepare the filter string
                var filterString = $"{idProp} contains '{search}' or {nameProp} contains '{search}' or {name2Prop} contains '{search}' or {name3Prop} contains '{search}' or {descProp} contains '{search}' or {desc2Prop} contains '{search}' or {desc3Prop} contains '{search}'";

                // If the search is a date, include documents with that date
                if (DateTime.TryParse(search.Trim(), out DateTime searchDate))
                {
                    var validAsOfProp = nameof(ExchangeRate.ValidAsOf);
                    filterString = $"{filterString} or {validAsOfProp} eq {searchDate:yyyy-MM-dd}";
                }

                // Apply the filter
                query = query.Filter(ExpressionFilter.Parse(filterString));

            }

            return Task.FromResult(query);
        }

        protected override Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken _)
        {
            var result = ExpressionOrderBy.Parse(nameof(ExchangeRate.ValidAsOf) + " desc");
            return Task.FromResult(result);
        }
    }
}
