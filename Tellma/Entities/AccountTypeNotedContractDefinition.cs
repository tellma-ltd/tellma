using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "AccountTypeNotedContractDefinition", Plural = "AccountTypeNotedContractDefinitions")]
    public class AccountTypeNotedContractDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_NotedContractDefinition")]
        [Required]
        public int? NotedContractDefinitionId { get; set; }
    }

    public class AccountTypeNotedContractDefinition : AccountTypeNotedContractDefinitionForSave
    {
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_NotedContractDefinition")]
        [ForeignKey(nameof(NotedContractDefinitionId))]
        public ContractDefinition NotedContractDefinition { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
