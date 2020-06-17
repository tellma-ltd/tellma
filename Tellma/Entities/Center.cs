using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Center", Plural = "Centers")]
    public class CenterForSave : EntityWithKey<int>, ITreeEntityForSave<int>
    {
        [NotMapped]
        public int? ParentIndex { get; set; }

        [Display(Name = "TreeParent")]
        [AlwaysAccessible]
        public int? ParentId { get; set; }

        [Display(Name = "Center_CenterType")]
        [Required]
        [StringLength(255)]
        [ChoiceList(new object[] { "Segment", "Profit", "Revenue", "Cost" },
            new string[] {
                "Center_CenterType_Segment",
                "Center_CenterType_Profit",
                "Center_CenterType_Revenue",
                "Center_CenterType_Cost" })]
        public string CenterType { get; set; }

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

        [Display(Name = "Center_Manager")]
        public int? ManagerId { get; set; }

        [Display(Name = "Code")]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Code { get; set; }
    }

    public class Center : CenterForSave, ITreeEntity<int>
    {
        [AlwaysAccessible]
        public short? Level { get; set; }

        [AlwaysAccessible]
        public int? ActiveChildCount { get; set; }

        [AlwaysAccessible]
        public int? ChildCount { get; set; }

        [Display(Name = "IsLeaf")]
        [AlwaysAccessible]
        public bool? IsLeaf { get; set; }

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

        [Display(Name = "Center_Manager")]
        [ForeignKey(nameof(ManagerId))]
        public Agent Manager { get; set; }

        [AlwaysAccessible]
        public HierarchyId Node { get; set; }

        [AlwaysAccessible]
        public HierarchyId ParentNode { get; set; }

        [Display(Name = "TreeParent")]
        [ForeignKey(nameof(ParentId))]
        public Center Parent { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
