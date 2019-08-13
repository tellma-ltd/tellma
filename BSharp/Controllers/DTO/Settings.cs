using BSharp.EntityModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.Dto
{
    // TODO: delete (Not the ForClient part)

    public class SettingsForSave : Entity
    {
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "Settings_ShortCompanyName", Language = Language.Primary)]
        public string ShortCompanyName { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "Settings_ShortCompanyName", Language = Language.Secondary)]
        public string ShortCompanyName2 { get; set; }

        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Settings_PrimaryLanguage")]
        public string PrimaryLanguageId { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Settings_PrimaryLanguageSymbol")]
        public string PrimaryLanguageSymbol { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Settings_SecondaryLanguage")]
        public string SecondaryLanguageId { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Settings_SecondaryLanguageSymbol")]
        public string SecondaryLanguageSymbol { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Settings_BrandColor")]
        public string BrandColor { get; set; } // e.g. #0284AB
    }

    public class Settings : SettingsForSave
    {
        /// <summary>
        /// Changes whenever the client views and the specs change
        /// </summary>
        public Guid ViewsAndSpecsVersion { get; set; }

        /// <summary>
        /// Changes whenever the client settings change
        /// </summary>
        public Guid SettingsVersion { get; set; }

        /// <summary>
        /// When was this tenant provisioned
        /// </summary>
        [Display(Name = "Settings_ProvisionedAt")]
        public DateTimeOffset ProvisionedAt { get; set; }

        /// <summary>
        /// Audit Info
        /// </summary>
        [Display(Name = "ModifiedAt")]
        public DateTimeOffset ModifiedAt { get; set; }

        /// <summary>
        /// Audit Info
        /// </summary>
        [Display(Name = "ModifiedBy")]
        public int ModifiedById { get; set; }
    }


    public class SettingsForClient
    {
        public string ShortCompanyName { get; set; }

        public string ShortCompanyName2 { get; set; }

        public string PrimaryLanguageId { get; set; }

        public string PrimaryLanguageName { get; set; }

        public string PrimaryLanguageSymbol { get; set; }

        public string SecondaryLanguageId { get; set; }

        public string SecondaryLanguageName { get; set; }

        public string SecondaryLanguageSymbol { get; set; }

        public string BrandColor { get; set; }
               
        public DateTimeOffset ProvisionedAt { get; set; }
    }
}
