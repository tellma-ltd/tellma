using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "GeneralSettings", Plural = "GeneralSettings")]
    public class GeneralSettingsForSave : Entity
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

        [Display(Name = "Settings_PrimaryLanguage")]
        [Required]
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
        [Required]
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
    }

    public class GeneralSettings : GeneralSettingsForSave
    {
        /// <summary>
        /// Changes whenever the client views and the specs change
        /// </summary>
        public Guid DefinitionsVersion { get; set; }

        /// <summary>
        /// Changes whenever the client settings change
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
