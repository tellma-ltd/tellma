using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Settings", Plural = "Settings")]
    public class SettingsForSave : Entity
    {
        [MultilingualDisplay(Name = "Settings_ShortCompanyName", Language = Language.Primary)]
        [Required]
        [StringLength(255)]
        public string ShortCompanyName { get; set; }

        [MultilingualDisplay(Name = "Settings_ShortCompanyName", Language = Language.Secondary)]
        [StringLength(255)]
        public string ShortCompanyName2 { get; set; }

        [MultilingualDisplay(Name = "Settings_ShortCompanyName", Language = Language.Ternary)]
        [StringLength(255)]
        public string ShortCompanyName3 { get; set; }

        [Display(Name = "Settings_FunctionalCurrency")]
        [Required]
        [StringLength(3)]
        public string FunctionalCurrencyId { get; set; }

        [Display(Name = "Settings_PrimaryLanguage")]
        [Required]
        [StringLength(2)]
        [Culture]
        public string PrimaryLanguageId { get; set; }

        [Display(Name = "Settings_PrimaryLanguageSymbol")]
        [StringLength(255)]
        public string PrimaryLanguageSymbol { get; set; }

        [Display(Name = "Settings_SecondaryLanguage")]
        [StringLength(2)]
        [Culture]
        public string SecondaryLanguageId { get; set; }

        [Display(Name = "Settings_SecondaryLanguageSymbol")]
        [StringLength(255)]
        public string SecondaryLanguageSymbol { get; set; }

        [Display(Name = "Settings_TernaryLanguage")]
        [StringLength(2)]
        [Culture]
        public string TernaryLanguageId { get; set; }

        [Display(Name = "Settings_TernaryLanguageSymbol")]
        [StringLength(255)]
        public string TernaryLanguageSymbol { get; set; }

        // Branding

        [Display(Name = "Settings_BrandColor")]
        [StringLength(255)]
        public string BrandColor { get; set; } // e.g. #0284AB

        // Financial

        [Display(Name = "Settings_ArchiveDate")]
        public DateTime? ArchiveDate { get; set; }
    }

    public class Settings : SettingsForSave
    {
        /// <summary>
        /// Changes whenever the client views and the specs change
        /// </summary>
        public Guid DefinitionsVersion { get; set; }

        /// <summary>
        /// Changes whenever the client settings change
        /// </summary>
        public Guid SettingsVersion { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "Settings_FunctionalCurrency")]
        [ForeignKey(nameof(FunctionalCurrencyId))]
        public Currency FunctionalCurrency { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
