using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class SettingsForSave : DtoBase
    {
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Settings_ShortCompanyName")]
        public string ShortCompanyName { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Settings_ShortCompanyName")]
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
}
