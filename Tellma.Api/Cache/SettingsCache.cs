using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Repository.Application;
using Tellma.Utilities.Caching;

namespace Tellma.Api
{
    internal class SettingsCache : VersionCache<int, SettingsForClient>, ISettingsCache
    {
        private readonly IApplicationRepositoryFactory _repoFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsCache"/> class.
        /// </summary>
        public SettingsCache(IApplicationRepositoryFactory repoFactory)
        {
            _repoFactory = repoFactory;
        }

        /// <summary>
        /// Implementation of <see cref="VersionCache{TKey, TData}"/>.
        /// </summary>
        protected override async Task<(SettingsForClient data, string version)> GetDataFromSource(int tenantId, CancellationToken cancellation)
        {
            var repo = _repoFactory.GetRepository(tenantId);

            SettingsOutput settingsResult = await repo.Settings__Load(cancellation);

            var version = settingsResult.Version.ToString();
            var generalSettings = settingsResult.GeneralSettings;
            var financialSettings = settingsResult.FinancialSettings;
            var singleBusinessUnitId = settingsResult.SingleBusinessUnitId;
            var featureFlags = settingsResult.FeatureFlags;

            // Prepare the settings for client
            var forClient = new SettingsForClient();
            foreach (var forClientProp in typeof(SettingsForClient).GetProperties())
            {
                var settingsProp = typeof(GeneralSettings).GetProperty(forClientProp.Name);
                if (settingsProp != null)
                {
                    var value = settingsProp.GetValue(generalSettings);
                    forClientProp.SetValue(forClient, value);
                }
            }

            // Single Business Unit Id
            forClient.SingleBusinessUnitId = singleBusinessUnitId;

            // Financial Settings
            forClient.FunctionalCurrencyId = financialSettings.FunctionalCurrencyId;
            forClient.TaxIdentificationNumber = financialSettings.TaxIdentificationNumber;
            forClient.ArchiveDate = financialSettings.ArchiveDate ?? DateTime.MinValue;
            forClient.FunctionalCurrencyDecimals = financialSettings.FunctionalCurrency.E ?? 0;
            forClient.FunctionalCurrencyName = financialSettings.FunctionalCurrency.Name;
            forClient.FunctionalCurrencyName2 = financialSettings.FunctionalCurrency.Name2;
            forClient.FunctionalCurrencyName3 = financialSettings.FunctionalCurrency.Name3;
            forClient.FunctionalCurrencyDescription = financialSettings.FunctionalCurrency.Description;
            forClient.FunctionalCurrencyDescription2 = financialSettings.FunctionalCurrency.Description2;
            forClient.FunctionalCurrencyDescription3 = financialSettings.FunctionalCurrency.Description3;

            // Language
            forClient.PrimaryLanguageName = GetCultureDisplayName(forClient.PrimaryLanguageId);
            forClient.SecondaryLanguageName = GetCultureDisplayName(forClient.SecondaryLanguageId);
            forClient.TernaryLanguageName = GetCultureDisplayName(forClient.TernaryLanguageId);

            // Feature flags
            forClient.FeatureFlags = featureFlags.ToDictionary(e => e.Key, e => e.Value);

            // Custom fields
            var fields = JsonConvert.DeserializeObject<GeneralSettings.Custom>(generalSettings.CustomFieldsJson ?? "{}");

            forClient.BuildingNumber = fields.BuildingNumber;
            forClient.Street = fields.Street;
            forClient.Street2 = fields.Street2;
            forClient.Street3 = fields.Street3;
            forClient.SecondaryNumber = fields.SecondaryNumber;
            forClient.District = fields.District;
            forClient.District2 = fields.District2;
            forClient.District3 = fields.District3;
            forClient.PostalCode = fields.PostalCode;
            forClient.City = fields.City;
            forClient.City2 = fields.City2;
            forClient.City3 = fields.City3;
            forClient.CommercialRegistrationNumber = fields.CommercialRegistrationNumber;

            return (forClient, version);
        }

        /// <summary>
        /// Returns the company settings from the cache if <paramref name="version"/> matches 
        /// the cached version, otherwise retrieves the settings from the database.
        /// <para/>
        /// Note: The calling service has to retrieve the <paramref name="version"/> independently using 
        /// <see cref="ApplicationRepository.OnConnect"/>, all services already do that to retrieve the 
        /// user Id so they retrieve the <paramref name="version"/> in the same database call as a performance optimization.
        /// </summary>
        /// <param name="tenantId">The ID of the company whose settings to load.</param>
        /// <param name="version">The latest version of the settings.</param>
        /// <param name="cancellation">The cancellation instruction.</param>
        /// <returns>The company's settings packaged in a <see cref="SettingsForClient"/> object, together with their version.</returns>
        public async Task<Versioned<SettingsForClient>> GetSettings(int tenantId, string version, CancellationToken cancellation = default)
        {
            var (data, newVersion) = await GetData(tenantId, version, cancellation);
            return new Versioned<SettingsForClient>(data, newVersion);
        }

        private static string GetCultureDisplayName(string cultureName)
        {
            if (cultureName is null)
            {
                return null;
            }

            return CultureInfo.GetCultureInfo(cultureName)?.NativeName;
        }
    }
}
