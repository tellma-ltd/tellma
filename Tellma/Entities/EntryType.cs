using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "EntryType", Plural = "EntryTypes")]
    public class EntryTypeForSave : EntityWithKey<int>, ITreeEntityForSave<int>
    {
        [NotMapped]
        public int? ParentIndex { get; set; }

        [Display(Name = "TreeParent")]
        [AlwaysAccessible]
        public int? ParentId { get; set; }

        [Display(Name = "Code")]
        [Required]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Code { get; set; }


        [Display(Name = "EntryType_Concept")]
        [Required]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Concept { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name2 { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name3 { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Primary)]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Description { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Secondary)]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Description2 { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Ternary)]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Description3 { get; set; }

        [Display(Name = "IsAssignable")]
        [Required]
        [AlwaysAccessible]
        public bool? IsAssignable { get; set; }
    }

    public class EntryType : EntryTypeForSave, ITreeEntity<int>
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

        [Display(Name = "IsSystem")]
        [AlwaysAccessible]
        public bool? IsSystem { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [NotNull]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
        public int? ModifiedById { get; set; }

        // For Query

        [AlwaysAccessible]
        public HierarchyId Node { get; set; }

        [Display(Name = "TreeParent")]
        [ForeignKey(nameof(ParentId))]
        public EntryType Parent { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
