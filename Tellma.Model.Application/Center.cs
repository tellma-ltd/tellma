using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Center", GroupName = "Centers")]
    public class CenterForSave : EntityWithKey<int>
    {
        [NotMapped]
        public int? ParentIndex { get; set; }

        [Display(Name = "TreeParent")]
        [SelfReferencing(nameof(ParentIndex))]
        public int? ParentId { get; set; }

        [Display(Name = "Center_CenterType")]
        [Required, ValidateRequired]
        [ChoiceList(new object[] {
               CenterTypes.Abstract,
               CenterTypes.BusinessUnit,
               CenterTypes.CostOfSales,
               CenterTypes.SellingGeneralAndAdministration,
               CenterTypes.SharedExpenseControl,
               CenterTypes.InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl,
               CenterTypes.CurrentInventoriesInTransitExpendituresControl,
               CenterTypes.ConstructionInProgressExpendituresControl,
               CenterTypes.WorkInProgressExpendituresControl,
               CenterTypes.OtherPL,
               CenterTypes.Administration,
               CenterTypes.Service,
               CenterTypes.Operation,
               CenterTypes.Sale
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
                "Center_CenterType_OtherPL",
                "Center_CenterType_Administration",
                "Center_CenterType_Service",
                "Center_CenterType_Operation",
                "Center_CenterType_Sale"
            })]
        public string CenterType { get; set; }

        [Display(Name = "Name")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string Name { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name2 { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name3 { get; set; }

        [Display(Name = "Center_Manager")]
        public int? ManagerId { get; set; }

        [Display(Name = "Code")]
        [Required, ValidateRequired]
        [StringLength(50)]
        public string Code { get; set; }
    }

    public class Center : CenterForSave
    {
        [Required]
        public short? Level { get; set; }

        [Required]
        public int? ActiveChildCount { get; set; }

        [Required]
        public int? ChildCount { get; set; }

        [Display(Name = "IsLeaf")]
        [Required]
        public bool? IsLeaf { get; set; }

        [Display(Name = "IsActive")]
        [Required]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [Required]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [Required]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "Center_Manager")]
        [ForeignKey(nameof(ManagerId))]
        public Agent Manager { get; set; }

        [Required]
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

    public static class CenterTypes
    {
        public const string Abstract = nameof(Abstract);
        public const string BusinessUnit = nameof(BusinessUnit);
        public const string CostOfSales = nameof(CostOfSales);
        public const string SellingGeneralAndAdministration = nameof(SellingGeneralAndAdministration);
        public const string SharedExpenseControl = nameof(SharedExpenseControl);
        public const string InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl = nameof(InvestmentPropertyUnderConstructionOrDevelopmentExpendituresControl);
        public const string CurrentInventoriesInTransitExpendituresControl = nameof(CurrentInventoriesInTransitExpendituresControl);
        public const string ConstructionInProgressExpendituresControl = nameof(ConstructionInProgressExpendituresControl);
        public const string WorkInProgressExpendituresControl = nameof(WorkInProgressExpendituresControl);
        public const string OtherPL = nameof(OtherPL);
        public const string Administration = nameof(Administration);
        public const string Service = nameof(Service);
        public const string Operation = nameof(Operation);
        public const string Sale = nameof(Sale);
    }
}
