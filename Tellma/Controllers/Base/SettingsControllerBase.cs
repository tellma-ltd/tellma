using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Entities;
using Tellma.Services.ApiAuthentication;
using Tellma.Services.MultiTenancy;
using Tellma.Services.Utilities;

namespace Tellma.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type using OData-like parameters
    /// </summary>
    [AuthorizeJwtBearer]
    [ApplicationController]
    [ApiController]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public abstract class SettingsControllerBase<TSettingsForSave, TSettings> : ControllerBase
        where TSettings : Entity
        where TSettingsForSave : Entity
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<SettingsControllerBase<TSettingsForSave, TSettings>> _logger;

        public SettingsControllerBase(IServiceProvider sp)
        {
            _sp = sp;
            _logger = _sp.GetRequiredService<ILogger<SettingsControllerBase<TSettingsForSave, TSettings>>>();
        }

        [HttpGet]
        public virtual async Task<ActionResult<GetEntityResponse<TSettings>>> GetSettings([FromQuery] SelectExpandArguments args, CancellationToken cancellation)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var _service = GetSettingsService();
                var settings = await _service.GetSettings(args, cancellation);

                var singleton = new TSettings[] { settings };
                var relatedEntities = ControllerUtilities.FlattenAndTrim(singleton, cancellation: default);

                var result = new GetEntityResponse<TSettings>
                {
                    Result = settings,
                    RelatedEntities = relatedEntities
                };

                return Ok(result);
            },
            _logger);
        }

        [HttpPost]
        public async Task<ActionResult<SaveSettingsResponse<TSettings>>> Save([FromBody] TSettingsForSave settingsForSave, [FromQuery] SaveArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var _service = GetSettingsService();
                var (settings, settingsForClient) = await _service.SaveSettings(settingsForSave, args);

                var singleton = new TSettings[] { settings };
                var relatedEntities = ControllerUtilities.FlattenAndTrim(singleton, cancellation: default);

                var result = new SaveSettingsResponse<TSettings>
                {
                    Result = settings,
                    RelatedEntities = relatedEntities,
                    SettingsForClient = settingsForClient
                };

                return Ok(result);
            },
            _logger);
        }

        protected abstract SettingsServiceBase<TSettingsForSave, TSettings> GetSettingsService();
    }

    public abstract class SettingsServiceBase<TSettingsForSave, TSettings> : ServiceBase
        where TSettings : Entity
        where TSettingsForSave : Entity
    {
        private readonly IServiceProvider _sp;
        private readonly ISettingsCache _settingsCache;
        
        protected readonly ITenantIdAccessor _tenantIdAccessor;
        protected readonly ApplicationRepository _repo;

        public SettingsServiceBase(IServiceProvider sp)
        {
            _sp = sp;
            _settingsCache = _sp.GetRequiredService<ISettingsCache>();
            _tenantIdAccessor = _sp.GetRequiredService<ITenantIdAccessor>();
            _repo = _sp.GetRequiredService<ApplicationRepository>();
        }

        #region API

        public async Task<TSettings> GetSettings(SelectExpandArguments args, CancellationToken cancellation)
        {
            var permissions = await UserPermissions(Constants.Read, cancellation);
            if (!permissions.Any())
            {
                throw new ForbiddenException();
            }

            return await GetExecute(args, cancellation);
        }

        public async Task<(TSettings, Versioned<SettingsForClient>)> SaveSettings(TSettingsForSave settingsForSave, SaveArguments args)
        {
            var updatePermissions = await UserPermissions(Constants.Update, cancellation: default);
            if (!updatePermissions.Any())
            {
                throw new ForbiddenException();
            }

            // Trim all string fields just in case
            settingsForSave.TrimStringProperties();

            // Preprocess
            await Preprocess(settingsForSave);

            // Validate
            await SaveValidate(settingsForSave);
            ModelState.ThrowIfInvalid();

            // Persist
            await SaveExecute(settingsForSave, args);

            // Update the settings cache
            var tenantId = _tenantIdAccessor.GetTenantId();
            var settingsForClient = await LoadSettingsForClient(GetRepository(), cancellation: default);
            _settingsCache.SetSettings(tenantId, settingsForClient);

            // If requested, return the updated entity
            if (args.ReturnEntities ?? false)
            {
                // If requested, return the same response you would get from a GET
                var res = await GetSettings(args, cancellation: default);
                return (res, settingsForClient);
            }
            else
            {
                return default;
            }
        }

        #endregion

        #region Abstract and Virtual

        protected abstract Task<TSettings> GetExecute(SelectExpandArguments args, CancellationToken cancellation);

        protected virtual Task Preprocess(TSettingsForSave settingsForSave)
        {
            return Task.CompletedTask;
        }
        
        protected abstract Task SaveValidate(TSettingsForSave settingsForSave);

        protected abstract Task SaveExecute(TSettingsForSave settingsForSave, SelectExpandArguments args);

        /// <summary>
        /// Retrieves the user permissions for the current view and the specified action
        /// </summary>
        protected abstract Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation);

        /// <summary>
        /// Get the <see cref="ApplicationRepository"/> on which the controller is based
        /// </summary>
        /// <returns></returns>
        protected abstract ApplicationRepository GetRepository();

        #endregion

        #region Helpers

        public static async Task<Versioned<SettingsForClient>> LoadSettingsForClient(ApplicationRepository repo, CancellationToken cancellation)
        {
            var (singleBusinessUnitId, generalSettings, financialSettings) = await repo.Settings__Load(cancellation);
            if (generalSettings == null)
            {
                // This should never happen
                throw new BadRequestException("Settings have not been initialized");
            }

            // Prepare the settings for client
            SettingsForClient settingsForClient = new SettingsForClient();
            foreach (var forClientProp in typeof(SettingsForClient).GetProperties())
            {
                var settingsProp = typeof(GeneralSettings).GetProperty(forClientProp.Name);
                if (settingsProp != null)
                {
                    var value = settingsProp.GetValue(generalSettings);
                    forClientProp.SetValue(settingsForClient, value);
                }
            }

            // Is Multi Business Unit
            settingsForClient.SingleBusinessUnitId = singleBusinessUnitId;

            // Financial Settings
            settingsForClient.FunctionalCurrencyId = financialSettings.FunctionalCurrencyId;
            settingsForClient.TaxIdentificationNumber = financialSettings.TaxIdentificationNumber;
            settingsForClient.ArchiveDate = financialSettings.ArchiveDate ?? DateTime.MinValue;
            settingsForClient.FunctionalCurrencyDecimals = financialSettings.FunctionalCurrency.E ?? 0;
            settingsForClient.FunctionalCurrencyName = financialSettings.FunctionalCurrency.Name;
            settingsForClient.FunctionalCurrencyName2 = financialSettings.FunctionalCurrency.Name2;
            settingsForClient.FunctionalCurrencyName3 = financialSettings.FunctionalCurrency.Name3;
            settingsForClient.FunctionalCurrencyDescription = financialSettings.FunctionalCurrency.Description;
            settingsForClient.FunctionalCurrencyDescription2 = financialSettings.FunctionalCurrency.Description2;
            settingsForClient.FunctionalCurrencyDescription3 = financialSettings.FunctionalCurrency.Description3;

            // Language
            settingsForClient.PrimaryLanguageName = GetCultureDisplayName(settingsForClient.PrimaryLanguageId);
            settingsForClient.SecondaryLanguageName = GetCultureDisplayName(settingsForClient.SecondaryLanguageId);
            settingsForClient.TernaryLanguageName = GetCultureDisplayName(settingsForClient.TernaryLanguageId);

            // Tag the settings for client with their current version
            var result = new Versioned<SettingsForClient>
            (
                version: generalSettings.SettingsVersion.ToString(),
                data: settingsForClient
            );

            return result;
        }

        private static string GetCultureDisplayName(string cultureName)
        {
            if (cultureName is null)
            {
                return null;
            }

            return System.Globalization.CultureInfo.GetCultureInfo(cultureName)?.NativeName;
        }

        #endregion
    }
}
