using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    [StrongEntity]
    public class IfrsEntryClassification : EntityWithKey<string>
    {
        [AlwaysAccessible]
        public short? Level { get; set; }

        [AlwaysAccessible]
        public string ParentId { get; set; }

        [AlwaysAccessible]
        public int? ActiveChildCount { get; set; }

        [AlwaysAccessible]
        public int? ChildCount { get; set; }

        [Display(Name = "IfrsConcepts_IsLeaf")]
        [AlwaysAccessible]
        public bool? IsLeaf { get; set; }

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

        [MultilingualDisplay(Name = "IfrsoConcepts_Documentation", Language = Language.Secondary)]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        public string Documentation2 { get; set; }

        [MultilingualDisplay(Name = "IfrsConcepts_Documentation", Language = Language.Ternary)]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        public string Documentation3 { get; set; }

        [Display(Name = "IfrsConcepts_EffectiveDate")]
        public DateTime? EffectiveDate { get; set; }

        [Display(Name = "IfrsConcepts_ExpiryDate")]
        public DateTime? ExpiryDate { get; set; }

        // Specific to IFRS Entry Classification

        [Display(Name = "IfrsNotes_ForDebit")]
        public bool? ForDebit { get; set; }

        [Display(Name = "IfrsNotes_ForCredit")]
        public bool? ForCredit { get; set; }

        // End specific to IFRS Entry

        [Display(Name = "IsActive")]
        [AlwaysAccessible]
        public bool? IsActive { get; set; }

        //[Display(Name = "CreatedAt")]
        //public DateTimeOffset? CreatedAt { get; set; }

        //[Display(Name = "CreatedBy")]
        //public int? CreatedById { get; set; }

        //[Display(Name = "ModifiedAt")]
        //public DateTimeOffset? ModifiedAt { get; set; }

        //[Display(Name = "ModifiedBy")]
        //public int? ModifiedById { get; set; }

        // For Query

        [AlwaysAccessible]
        public HierarchyId Node { get; set; }

        [AlwaysAccessible]
        public HierarchyId ParentNode { get; set; }

        [Display(Name = "TreeParent")]
        [ForeignKey(nameof(ParentId))]
        public IfrsEntryClassification Parent { get; set; }

        //[Display(Name = "CreatedBy")]
        //[ForeignKey(nameof(CreatedById))]
        //public User CreatedBy { get; set; }

        //[Display(Name = "ModifiedBy")]
        //[ForeignKey(nameof(ModifiedById))]
        //public User ModifiedBy { get; set; }
    }
}
