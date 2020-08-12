using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "WorkflowSignature", Plural = "WorkflowSignatures")]
    public class WorkflowSignatureForSave : EntityWithKey<int>
    {
        [Display(Name = "WorkflowSignature_RuleType")]
        [ChoiceList(new object[] { RuleTypes.ByRole, RuleTypes.ByCustodian, RuleTypes.ByUser, RuleTypes.Public }, 
            new string[] { RuleTypeNames.ByRole, RuleTypeNames.ByCustodian, RuleTypeNames.ByUser, RuleTypeNames.Public })]
        [Required]
        public string RuleType { get; set; }

        [Display(Name = "WorkflowSignature_RuleTypeEntryIndex")]
        public int? RuleTypeEntryIndex { get; set; }

        [Display(Name = "WorkflowSignature_Role")]
        public int? RoleId { get; set; } // FK

        [Display(Name = "WorkflowSignature_User")]
        public int? UserId { get; set; } // FK

        [Display(Name = "WorkflowSignature_PredicateType")]
        [ChoiceList(new object[] { PredicateTypes.ValueGreaterOrEqual },
            new string[] { PredicateTypeNames.ValueGreaterOrEqual })]
        public string PredicateType { get; set; }

        [Display(Name = "WorkflowSignature_PredicateTypeEntryIndex")]
        public int? PredicateTypeEntryIndex { get; set; }

        [Display(Name = "WorkflowSignature_Value")]
        public decimal? Value { get; set; }

        [Display(Name = "WorkflowSignature_ProxyRole")]
        public int? ProxyRoleId { get; set; } // FK
    }

    public class WorkflowSignature : WorkflowSignatureForSave
    {
        [Display(Name = "WorkflowSignature_Workflow")]
        public int? WorkflowId { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "WorkflowSignature_Role")]
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }

        [Display(Name = "WorkflowSignature_User")]
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Display(Name = "WorkflowSignature_ProxyRole")]
        [ForeignKey(nameof(ProxyRoleId))]
        public Role ProxyRole { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }

    public static class RuleTypes
    {
        public const string ByRole = nameof(ByRole);
        public const string ByCustodian = nameof(ByCustodian);
        public const string ByUser = nameof(ByUser);
        public const string Public = nameof(Public);
    }

    public static class RuleTypeNames
    {
        private const string _prefix = "RuleType_";

        public const string ByRole = _prefix + RuleTypes.ByRole;
        public const string ByCustodian = _prefix + RuleTypes.ByCustodian;
        public const string ByUser = _prefix + RuleTypes.ByUser;
        public const string Public = _prefix + RuleTypes.Public;
    }

    public static class PredicateTypes
    {
        public const string ValueGreaterOrEqual = nameof(ValueGreaterOrEqual);
    }

    public static class PredicateTypeNames
    {
        private const string _prefix = "PredicateType_";

        public const string ValueGreaterOrEqual = _prefix + PredicateTypes.ValueGreaterOrEqual;
    }
}
