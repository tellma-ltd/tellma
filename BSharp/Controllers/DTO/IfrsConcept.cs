using BSharp.Controllers.Misc;
using BSharp.Services.OData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class IfrsConcept : DtoForSaveKeyBase<string> // There is no DTO for save
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

    }

    public class IfrsNote  : IfrsConcept
    {
        public HierarchyId Node { get; set; }

        public short? Level { get; set; }

        public HierarchyId ParentNode { get; set; }

        public string ParentId { get; set; }

        public int? ChildCount { get; set; }

        [Display(Name = "IfrsNotes_IsAggregate")]
        public bool? IsAggregate { get; set; }

        [Display(Name = "IfrsNotes_ForDebit")]
        public bool? ForDebit { get; set; }

        [Display(Name = "IfrsNotes_ForCredit")]
        public bool? ForCredit { get; set; }

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

    public class IfrsNoteForQuery : IfrsNote
    {
        [NavigationProperty(ForeignKey = nameof(ParentId))]
        [Display(Name = "IfrsNotes_Parent")]
        public IfrsNoteForQuery Parent { get; set; }

        [NavigationProperty(ForeignKey = nameof(CreatedById))]
        public LocalUserForQuery CreatedBy { get; set; }

        [NavigationProperty(ForeignKey = nameof(ModifiedById))]
        public LocalUserForQuery ModifiedBy { get; set; }
    }
}
