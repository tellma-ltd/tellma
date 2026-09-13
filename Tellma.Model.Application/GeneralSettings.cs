using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "GeneralSettings", GroupName = "GeneralSettings")]
    public class GeneralSettingsForSave : Entity, IEntityWithCustomFields<GeneralSettingsForSave.Custom>
    {
        [Display(Name = "Settings_CompanyName")]
        [StringLength(255)]
        public string CompanyName { get; set; }

        [Display(Name = "Settings_CompanyName")]
        [StringLength(255)]
        public string CompanyName2 { get; set; }

        [Display(Name = "Settings_CompanyName")]
        [StringLength(255)]
        public string CompanyName3 { get; set; }

        [Display(Name = "Settings_CountryCode")]
        [StringLength(2)]
        public string CountryCode { get; set; }

        [Display(Name = "Settings_ShortCompanyName")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string ShortCompanyName { get; set; }

        [Display(Name = "Settings_ShortCompanyName")]
        [StringLength(255)]
        public string ShortCompanyName2 { get; set; }

        [Display(Name = "Settings_ShortCompanyName")]
        [StringLength(255)]
        public string ShortCompanyName3 { get; set; }

        [Display(Name = "Settings_PrimaryLanguage")]
        [Required, ValidateRequired]
        [StringLength(5)]
        [CultureChoiceList]
        public string PrimaryLanguageId { get; set; }

        [Display(Name = "Settings_PrimaryLanguageSymbol")]
        [StringLength(5)]
        public string PrimaryLanguageSymbol { get; set; }

        [Display(Name = "Settings_SecondaryLanguage")]
        [StringLength(5)]
        [CultureChoiceList]
        public string SecondaryLanguageId { get; set; }

        [Display(Name = "Settings_SecondaryLanguageSymbol")]
        [StringLength(5)]
        public string SecondaryLanguageSymbol { get; set; }

        [Display(Name = "Settings_TernaryLanguage")]
        [StringLength(5)]
        [CultureChoiceList]
        public string TernaryLanguageId { get; set; }

        [Display(Name = "Settings_TernaryLanguageSymbol")]
        [StringLength(5)]
        public string TernaryLanguageSymbol { get; set; }

        [Display(Name = "Settings_PrimaryCalendar")]
        [Required, ValidateRequired]
        [StringLength(2)]
        public string PrimaryCalendar { get; set; }

        [Display(Name = "Settings_SecondaryCalendar")]
        [StringLength(2)]
        public string SecondaryCalendar { get; set; }

        [Display(Name = "Settings_DateFormat")]
        [StringLength(50)]
        public string DateFormat { get; set; }

        [Display(Name = "Settings_TimeFormat")]
        [StringLength(50)]
        public string TimeFormat { get; set; }

        // Branding

        [Display(Name = "Settings_BrandColor")]
        [StringLength(7)]
        public string BrandColor { get; set; } // e.g. #0284AB

        [Display(Name = "Settings_SupportEmails")]
        [StringLength(255)]
        public string SupportEmails { get; set; } // e.g. #0284AB

        // Company policies

        [Display(Name = "Settings_Enforce2faOnLocalAccounts")]
        public bool? Enforce2faOnLocalAccounts { get; set; }

        [Display(Name = "Settings_EnforceNoExternalAccounts")]
        public bool? EnforceNoExternalAccounts { get; set; }

        #region Custom Fields

        [JsonIgnore]
        public string CustomFieldsJson { get; set; } // On Read this, takes precedent

        [NotMapped]
        public Custom CustomFields { get; set; } // On Save, this takes precedent

        public class Custom : CustomFieldsBase
        {
            public override int Version => 1;
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

            // Marmin (UAE e-invoicing over Peppol).
            //
            // These live in the CustomFields bag rather than as real Settings columns because the
            // bag is already plumbed end to end (map.GeneralSettings, api/dal.GeneralSettings__Save
            // all pass CustomFieldsJson as one NVARCHAR(MAX) parameter), so adding a field here
            // costs no SQL change at all. Only the secrets need real columns, because they must be
            // withheld from the browser.
            //
            // NOTE: GeneralSettingsService.SavePreprocess serializes whatever the client sent, so
            // every one of these MUST be bound by the General Settings screen or a save will wipe
            // the ones it left out.

            /// <summary>The organisation client id Marmin issued, e.g. "org_...". Not a secret.</summary>
            public string MarminAeClientId { get; set; }

            /// <summary>The business profile documents are issued from, e.g. "MBP-...".</summary>
            public string MarminAeBusinessProfileId { get; set; }

            /// <summary>
            /// The vendor org id, used only to sanity-check inbound webhooks: it catches one
            /// tenant being handed the other tenant callback URL.
            /// </summary>
            public string MarminAeOrgId { get; set; }

            /// <summary>
            /// The Peppol scheme the customer tax registration number is an identifier in, e.g.
            /// "0235". Tenant-wide, since both customers are UAE-registered.
            /// </summary>
            public string MarminAeEndpointSchemeId { get; set; }

            /// <summary>
            /// Fallback for profile_execution_id, the eight supply-scenario flags. The real value
            /// is per-document, from Documents.Lookup1Id (the same slot ZATCA uses for its
            /// InvoiceTypeTransactions code); this is only used when that lookup is not set,
            /// which is the common case for a tenant issuing a single supply scenario.
            /// </summary>
            public string MarminAeDefaultProfileExecutionId { get; set; }

            /// <summary>Fallback payment_means code when the sales invoice does not name one.</summary>
            public string MarminAeDefaultPaymentMeansCode { get; set; }

            /// <summary>
            /// Days added to the issue date to derive a due date when the document has no
            /// NotedDate. Zero means the invoice is due on issue.
            /// </summary>
            public int MarminAeDefaultPaymentTermDays { get; set; }
        }

        #endregion
    }

    public class GeneralSettings : GeneralSettingsForSave
    {
        /// <summary>
        /// Changes whenever the client views and the specs change.
        /// </summary>
        public Guid DefinitionsVersion { get; set; }

        /// <summary>
        /// Changes whenever the client settings change.
        /// </summary>
        public Guid SettingsVersion { get; set; }

        [Display(Name = "Settings_SmsEnabled")]
        public bool? SmsEnabled { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset GeneralModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? GeneralModifiedById { get; set; }

        // For Query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(GeneralModifiedById))]
        public User GeneralModifiedBy { get; set; }
    }
}
