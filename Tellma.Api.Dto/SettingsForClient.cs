using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tellma.Api.Dto
{
    public class SettingsForClient
    {
        public string CompanyName { get; set; }

        public string CompanyName2 { get; set; }

        public string CompanyName3 { get; set; }

        public string ShortCompanyName { get; set; }

        public string ShortCompanyName2 { get; set; }

        public string ShortCompanyName3 { get; set; }

        public string FunctionalCurrencyId { get; set; }

        public string FunctionalCurrencyName { get; set; }

        public string FunctionalCurrencyName2 { get; set; }

        public string FunctionalCurrencyName3 { get; set; }

        public string FunctionalCurrencyDescription { get; set; }

        public string FunctionalCurrencyDescription2 { get; set; }

        public string FunctionalCurrencyDescription3 { get; set; }

        public short FunctionalCurrencyDecimals { get; set; }

        public DateTime ArchiveDate { get; set; }

        public DateTime FreezeDate { get; set; }

        public string TaxIdentificationNumber { get; set; }

        public string PrimaryLanguageId { get; set; }

        public string PrimaryLanguageName { get; set; }

        public string PrimaryLanguageSymbol { get; set; }

        public string SecondaryLanguageId { get; set; }

        public string SecondaryLanguageName { get; set; }

        public string SecondaryLanguageSymbol { get; set; }

        public string TernaryLanguageId { get; set; }

        public string TernaryLanguageName { get; set; }

        public string TernaryLanguageSymbol { get; set; }

        public string PrimaryCalendar { get; set; }

        public string SecondaryCalendar { get; set; }

        public string DateFormat { get; set; }

        public string TimeFormat { get; set; }

        public string BrandColor { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public int? SingleBusinessUnitId { get; set; }

        public bool SmsEnabled { get; set; }

        public Dictionary<string, bool> FeatureFlags { get; set; }

        // Custom Fields
        public string BuildingNumber { get; set; }
        public string Street { get; set; }
        public string Street2 { get; set; }
        public string Street3 { get; set; }
        public string SecondaryNumber { get; set; }
        public string District { get; set; }
        public string District2 { get; set; }
        public string District3 { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string City2 { get; set; }
        public string City3 { get; set; }
        public string CommercialRegistrationNumber { get; set; }

        // Banner

        public string BannerKey { get; set; }
        public bool BannerIsDismissable { get; set; }
        public string BannerType { get; set; }
        public string BannerText { get; set; }
        public string BannerText2 { get; set; }
        public string BannerText3 { get; set; }
        public int BannerHeight { get; set; }


        // Server Only Fields

        [JsonIgnore]
        public string SupportEmails { get; set; }

        [JsonIgnore]
        public string ZatcaEncryptedSecret { get; set; }

        [JsonIgnore]
        public string ZatcaEncryptedSecurityToken { get; set; }

        [JsonIgnore]
        public int ZatcaEncryptionKeyIndex { get; set; }

        public string ZatcaEnvironment { get; set; }

        // Marmin (UAE). The two secrets are [JsonIgnore] for the same reason the ZATCA ones are:
        // SettingsForClient is serialized straight to the browser, and these must never leave the
        // server. They are still carried on this type because it is the per-tenant settings cache
        // that DocumentsService and the webhook handler both read from.

        [JsonIgnore]
        public string MarminAeEncryptedClientSecret { get; set; }

        [JsonIgnore]
        public string MarminAeEncryptedWebhookSecret { get; set; }

        [JsonIgnore]
        public int MarminAeEncryptionKeyIndex { get; set; }

        /// <summary>Sandbox or Production. Safe to expose; the client uses it to warn on sandbox.</summary>
        public string MarminAeEnvironment { get; set; }

        // The non-secret Marmin configuration, flattened out of GeneralSettings.CustomFields the
        // same way the address and banner fields above are. None of it is sensitive.
        public string MarminAeClientId { get; set; }
        public string MarminAeBusinessProfileId { get; set; }
        public string MarminAeOrgId { get; set; }
        public string MarminAeEndpointSchemeId { get; set; }
        public string MarminAeDefaultProfileExecutionId { get; set; }
        public string MarminAeDefaultPaymentMeansCode { get; set; }
        public int MarminAeDefaultPaymentTermDays { get; set; }
    }
}
