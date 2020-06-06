using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "AccountTypeContractDefinition", Plural = "AccountTypeContractDefinitions")]
    public class AccountTypeContractDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_ContractDefinition")]
        public int? ContractDefinitionId { get; set; }
    }

    public class AccountTypeContractDefinition : AccountTypeContractDefinitionForSave
    {
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_ContractDefinition")]
        [ForeignKey(nameof(ContractDefinitionId))]
        public ContractDefinition ContractDefinition { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
