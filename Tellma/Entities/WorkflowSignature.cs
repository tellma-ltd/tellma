using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "WorkflowSignature", Plural = "WorkflowSignatures")]
    public class WorkflowSignatureForSave : EntityWithKey<int>
    {
        [Display(Name = "WorkflowSignature_RuleType")]
        [ChoiceList(new object[] { "ByRole", "ByCustodian", "ByUser", "Public" }, 
            new string[] { "RuleType_ByRole", "RuleType_ByCustodian", "RuleType_ByUser", "RuleType_Public" })]
        public string RuleType { get; set; }

        [Display(Name = "WorkflowSignature_RuleTypeEntryIndex")]
        public int? RuleTypeEntryIndex { get; set; }

        [Display(Name = "WorkflowSignature_Role")]
        public int? RoleId { get; set; } // FK

        [Display(Name = "WorkflowSignature_User")]
        public int? UserId { get; set; } // FK

        [Display(Name = "WorkflowSignature_PredicateType")]
        [ChoiceList(new object[] { "ValueGreaterOrEqual" },
            new string[] { "PredicateType_ValueGreaterOrEqual" })]        
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
}
