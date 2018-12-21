using System;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.DTO
{
    public class TranslationForSave : DtoForSaveKeyBase<string>
    {
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(2048, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "T_Value")]
        public string Value { get; set; }
    }

    public class Translation : TranslationForSave
    {
        [Display(Name = "T_Name")]
        public string Name { get; set; }

        [Display(Name = "T_CultureId")]
        public string CultureId { get; set; }

        [Display(Name = "T_Tier")]
        public string Tier { get; set; }

        [Display(Name = "T_Notes")]
        public string Notes { get; set; }
    }
}
