using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Entities
{
    public class IfrsConcept : EntityWithKey<string> // There is no DTO for save
    {
        [Display(Name = "IfrsConcepts_IfrsType")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [ChoiceList(new object[] { "Amendment", "Extension", "Regulatory" },
            new string[] { "IfrsConcept_Amendment", "IfrsConcept_Extension", "IfrsConcept_Regulatory" })]
        [AlwaysAccessible]
        public string IfrsType { get; set; }

        [MultilingualDisplay(Name = "IfrsConcepts_Label", Language = Language.Primary)]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label { get; set; }

        [MultilingualDisplay(Name = "IfrsConcepts_Label", Language = Language.Secondary)]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label2 { get; set; }

        [MultilingualDisplay(Name = "IfrsConcepts_Label", Language = Language.Ternary)]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Label3 { get; set; }

        [MultilingualDisplay(Name = "IfrsConcepts_Documentation", Language = Language.Primary)]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        public string Documentation { get; set; }

        [MultilingualDisplay(Name = "IfrsConcepts_Documentation", Language = Language.Secondary)]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        public string Documentation2 { get; set; }

        [MultilingualDisplay(Name = "IfrsConcepts_Documentation", Language = Language.Ternary)]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        public string Documentation3 { get; set; }

        [Display(Name = "IfrsConcepts_EffectiveDate")]
        public DateTime? EffectiveDate { get; set; }

        [Display(Name = "IfrsConcepts_ExpiryDate")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "IsActive")]
        [AlwaysAccessible]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }
    }
}
