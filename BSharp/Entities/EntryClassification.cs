using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    [StrongEntity]
    public class EntryClassificationForSave : EntityWithKey<int>, ITreeEntityForSave<int>
    {
        [NotMapped]
        public int? ParentIndex { get; set; }

        [Display(Name = "TreeParent")]
        [AlwaysAccessible]
        public int? ParentId { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Name2 { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Name3 { get; set; }

        [Display(Name = "Code")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Code { get; set; }

        [Display(Name = "IsAssignable")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [AlwaysAccessible]
        public bool? IsAssignable { get; set; }

        [Display(Name = "EntryClassifications_ForDebit")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        public bool? ForDebit { get; set; }

        [Display(Name = "EntryClassifications_ForCredit")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        public bool? ForCredit { get; set; }
    }

    public class EntryClassification : EntryClassificationForSave, ITreeEntity<int>
    {
        [AlwaysAccessible]
        public short? Level { get; set; }

        [AlwaysAccessible]
        public int? ActiveChildCount { get; set; }

        [AlwaysAccessible]
        public int? ChildCount { get; set; }

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

        // For Query

        [AlwaysAccessible]
        public HierarchyId Node { get; set; }

        [AlwaysAccessible]
        public HierarchyId ParentNode { get; set; }

        [Display(Name = "TreeParent")]
        [ForeignKey(nameof(ParentId))]
        public EntryClassification Parent { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
