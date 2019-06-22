using BSharp.Controllers.Misc;
using System;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.DTO
{
    public class IfrsConcept : DtoKeyBase<string> // There is no DTO for save
    {
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [ChoiceList(new object[] { "Amendment", "Extension", "Regulatory" },
            new string[] { "IfrsConcept_Amendment", "IfrsConcept_Extension", "IfrsConcept_Regulatory" })]
        [BasicField]
        [Display(Name = "IfrsConcepts_IfrsType")]
        public string IfrsType { get; set; }

        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [BasicField]
        [MultilingualDisplay(Name = "IfrsConcepts_Label", Language = Language.Primary)]
        public string Label { get; set; }

        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [BasicField]
        [MultilingualDisplay(Name = "IfrsConcepts_Label", Language = Language.Secondary)]
        public string Label2 { get; set; }

        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [BasicField]
        [MultilingualDisplay(Name = "IfrsConcepts_Label", Language = Language.Ternary)]
        public string Label3 { get; set; }

        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "IfrsConcepts_Documentation", Language = Language.Primary)]
        public string Documentation { get; set; }

        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "IfrsConcepts_Documentation", Language = Language.Secondary)]
        public string Documentation2 { get; set; }

        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "IfrsConcepts_Documentation", Language = Language.Ternary)]
        public string Documentation3 { get; set; }

        [Display(Name = "IfrsConcepts_EffectiveDate")]
        public DateTime? EffectiveDate { get; set; }

        [Display(Name = "IfrsConcepts_ExpiryDate")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "IsActive")]
        [BasicField]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [ForeignKey]
        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [ForeignKey]
        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }
    }
}
