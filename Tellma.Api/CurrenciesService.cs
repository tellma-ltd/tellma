﻿using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Api.Metadata;
using Tellma.Model.Application;
using Tellma.Repository.Common;
using Tellma.Utilities.Common;

namespace Tellma.Api
{
    public class CurrenciesService : CrudServiceBase<CurrencyForSave, Currency, string>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IStringLocalizer _localizer;

        public CurrenciesService(ApplicationFactServiceBehavior behavior, CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
            _localizer = deps.Localizer;
        }

        protected override string View => "currencies";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<Currency>> Search(EntityQuery<Currency> query, GetArguments args, CancellationToken _)
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

            return Task.FromResult(query);
        }

        protected override async Task<List<string>> SaveExecuteAsync(List<CurrencyForSave> entities, bool returnIds)
        {
            foreach (var (entity, index) in entities.Indexed())
            {
                // Ensure that Id is supplied
                if (string.IsNullOrEmpty(entity.Id))
                {
                    string path = $"[{index}].{nameof(entity.Id)}";
                    string msg = _localizer[ErrorMessages.Error_Field0IsRequired, _localizer["Code"]];

                    ModelState.AddError(path, msg);
                }
                else if (entity.Id.Length > 3)
                {
                    string path = $"[{index}].{nameof(entity.Id)}";
                    string msg = _localizer[ErrorMessages.Error_Field0LengthMaximumOf1, _localizer["Code"], 3];

                    ModelState.AddError(path, msg);
                }
            }

            // Save
            OperationOutput result = await _behavior.Repository.Currencies__Save(
                entities: entities,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);

            // Return
            return entities.Select(e => e.Id).ToList();
        }

        protected override async Task DeleteExecuteAsync(List<string> ids)
        {
            DeleteOutput result = await _behavior.Repository.Currencies__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId);

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        public Task<EntitiesResult<Currency>> Activate(List<string> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<EntitiesResult<Currency>> Deactivate(List<string> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<EntitiesResult<Currency>> SetIsActive(List<string> ids, ActionArguments args, bool isActive)
        {
            await Initialize();

            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            var settings = await _behavior.Settings();

            foreach (var (id, index) in ids.Indexed())
            {
                if (ids.Any(id => id != null && id == settings.FunctionalCurrencyId))
                {
                    string path = $"[{index}]";
                    string msg = _localizer["Error_CannotDeactivateTheFunctionalCurrency"];

                    ModelState.AddError(path, msg);
                }
            }

            // Execute and return
            using var trx = TransactionFactory.ReadCommitted();
            OperationOutput output = await _behavior.Repository.Currencies__Activate(
                    ids: ids,
                    isActive: isActive,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId);

            AddErrorsAndThrowIfInvalid(output.Errors);

            var result = (args.ReturnEntities ?? false) ?
                await GetByIds(ids, args, action, cancellation: default) :
                EntitiesResult<Currency>.Empty();

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, result.Data);

            trx.Complete();
            return result;
        }

        protected override async Task<ExpressionOrderBy> DefaultOrderBy(CancellationToken cancellation)
        {
            // By default: Order currencies by name
            var settings = await _behavior.Settings(cancellation);
            string orderby = $"{nameof(Currency.Name)},{nameof(Currency.Id)}";
            if (settings.SecondaryLanguageId == CultureInfo.CurrentUICulture.Name)
            {
                orderby = $"{nameof(Currency.Name2)},{nameof(Currency.Name)},{nameof(Currency.Id)}";
            }
            else if (settings.TernaryLanguageId == CultureInfo.CurrentUICulture.Name)
            {
                orderby = $"{nameof(Currency.Name3)},{nameof(Currency.Name)},{nameof(Currency.Id)}";
            }

            return ExpressionOrderBy.Parse(orderby);
        }
    }
}
