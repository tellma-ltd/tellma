using BSharp.Controllers.Misc;
using System;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.DTO
{
    [StrongEntity]
    public class TranslationForSave : DtoForSaveKeyBase<string>
    {
        [BasicField]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(2048, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "T_Value")]
        public string Value { get; set; }
    }

    public class Translation : TranslationForSave
    {
        [BasicField]
        [Display(Name = "T_Name")]
        public string Name { get; set; }

        [ForeignKey]
        [Display(Name = "T_CultureId")]
        public string CultureId { get; set; }

        [Display(Name = "T_Tier")]
        public string Tier { get; set; }

        [Display(Name = "T_Notes")]
        public string Notes { get; set; }

        // For Query

        [NavigationProperty(ForeignKey = nameof(CultureId))]
        public Culture Culture { get; set; }
    }
}
