using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Center", Plural = "Centers")]
    public class CenterForSave : EntityWithKey<int>
    {
        [NotMapped]
        public int? ParentIndex { get; set; }

        [Display(Name = "TreeParent")]
        [AlwaysAccessible]
        [SelfReferencing(nameof(ParentIndex))]
        public int? ParentId { get; set; }

        [Display(Name = "Center_CenterType")]
        [Required]
        [StringLength(255)]
        [ChoiceList(new object[] {
            "Abstract", 
            "BusinessUnit", 
            "CostOfSales",
            "SellingGeneralAndAdministration", 
            "SharedExpenseControl",
            "InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl",
            "CurrentInventoriesInTransitExpendituresControl", 
            "ConstructionInProgressExpendituresControl", 
            "WorkInProgressExpendituresControl",
            "OtherPL"
        },
            new string[] {
                "Center_CenterType_Abstract",
                "Center_CenterType_BusinessUnit",
                "Center_CenterType_CostOfSales",
                "Center_CenterType_SellingGeneralAndAdministration",
                "Center_CenterType_SharedExpenseControl",
                "Center_CenterType_InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl",
                "Center_CenterType_CurrentInventoriesInTransitExpendituresControl",
                "Center_CenterType_ConstructionInProgressExpendituresControl",
                "Center_CenterType_WorkInProgressExpendituresControl",
                "Center_CenterType_OtherPL"
            })]
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
        [Required]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Code { get; set; }
    }

    public class Center : CenterForSave
    {
        [Display(Name = "Center_IsSegment")]
        public bool? IsSegment { get; set; }

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
