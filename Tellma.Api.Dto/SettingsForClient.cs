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

        public string SupportEmails { get; set; }

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


        // Server Only Fields


        [JsonIgnore]
        public string ZatcaEncryptedPrivateKey { get; set; }

        [JsonIgnore]
        public string ZatcaEncryptedSecret { get; set; }

        [JsonIgnore]
        public string ZatcaEncryptedSecurityToken { get; set; }

        [JsonIgnore]
        public int ZatcaEncryptionKeyIndex { get; set; }

        public bool ZatcaUseSandbox { get; set; }
    }
}
