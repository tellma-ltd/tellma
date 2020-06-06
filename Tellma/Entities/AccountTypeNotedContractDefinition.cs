using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "AccountTypeNotedContractDefinition", Plural = "AccountTypeNotedContractDefinitions")]
    public class AccountTypeNotedContractDefinitionForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_NotedContractDefinition")]
        public int? NotedContractDefinitionId { get; set; }
    }

    public class AccountTypeNotedContractDefinition : AccountTypeNotedContractDefinitionForSave
    {
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_NotedContractDefinition")]
        [ForeignKey(nameof(NotedContractDefinitionId))]
        public ContractDefinition NotedContractDefinition { get; set; }

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
