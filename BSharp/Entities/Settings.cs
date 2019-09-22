using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    [StrongEntity]
    public class SettingsForSave : Entity
    {
        [MultilingualDisplay(Name = "Settings_ShortCompanyName", Language = Language.Primary)]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string ShortCompanyName { get; set; }

        [MultilingualDisplay(Name = "Settings_ShortCompanyName", Language = Language.Secondary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string ShortCompanyName2 { get; set; }


        [MultilingualDisplay(Name = "Settings_ShortCompanyName", Language = Language.Ternary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string ShortCompanyName3 { get; set; }

        [Display(Name = "Settings_PrimaryLanguage")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(2, ErrorMessage = nameof(StringLengthAttribute))]
        [Culture]
        public string PrimaryLanguageId { get; set; }

        [Display(Name = "Settings_PrimaryLanguageSymbol")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string PrimaryLanguageSymbol { get; set; }

        [Display(Name = "Settings_SecondaryLanguage")]
        [StringLength(2, ErrorMessage = nameof(StringLengthAttribute))]
        [Culture]
        public string SecondaryLanguageId { get; set; }

        [Display(Name = "Settings_SecondaryLanguageSymbol")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string SecondaryLanguageSymbol { get; set; }

        [Display(Name = "Settings_TernaryLanguage")]
        [StringLength(2, ErrorMessage = nameof(StringLengthAttribute))]
        [Culture]
        public string TernaryLanguageId { get; set; }

        [Display(Name = "Settings_TernaryLanguageSymbol")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string TernaryLanguageSymbol { get; set; }

        [Display(Name = "Settings_BrandColor")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string BrandColor { get; set; } // e.g. #0284AB
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
        public int CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int ModifiedById { get; set; }

        // For Query

        [Display(Name = "CreatedBy")]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
