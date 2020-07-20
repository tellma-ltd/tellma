using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "AccountTypeContractDefinition", Plural = "AccountTypeContractDefinitions")]
    public class AccountTypeContractDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_ContractDefinition")]
        [Required]
        public int? ContractDefinitionId { get; set; }
    }

    public class AccountTypeContractDefinition : AccountTypeContractDefinitionForSave
    {
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_ContractDefinition")]
        [ForeignKey(nameof(ContractDefinitionId))]
        public ContractDefinition ContractDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
