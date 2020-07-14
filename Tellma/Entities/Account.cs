using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Account", Plural = "Accounts")]
    public class AccountForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_Type")]
        [Required]
        [AlwaysAccessible]
        [NotNull]
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_Center")]
        public int? CenterId { get; set; }

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

        [Display(Name = "Code")]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Code { get; set; }

        [Display(Name = "Account_Classification")]
        public int? ClassificationId { get; set; }

        [Display(Name = "Account_ContractDefinition")]
        public int? ContractDefinitionId { get; set; }

        [Display(Name = "Account_Contract")]
        public int? ContractId { get; set; }

        [Display(Name = "Account_ResourceDefinition")]
        public int? ResourceDefinitionId { get; set; }

        [Display(Name = "Account_Resource")]
        public int? ResourceId { get; set; }

        [Display(Name = "Account_Currency")]
        [StringLength(3)]
        public string CurrencyId { get; set; }

        [Display(Name = "Account_EntryType")]
        public int? EntryTypeId { get; set; }

        [Display(Name = "Account_NotedContractDefinition")]
        public int? NotedContractDefinitionId { get; set; }
    }

    public class Account : AccountForSave
    {
        [AlwaysAccessible]
        public bool? IsActive { get; set; }

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

        [Display(Name = "Account_Type")]
        [ForeignKey(nameof(AccountTypeId))]
        public AccountType AccountType { get; set; }

        [Display(Name = "Account_Classification")]
        [ForeignKey(nameof(ClassificationId))]
        public AccountClassification Classification { get; set; }

        [Display(Name = "Account_ContractDefinition")]
        [ForeignKey(nameof(ContractDefinitionId))]
        public ContractDefinition ContractDefinition { get; set; }

        [Display(Name = "Account_ResourceDefinition")]
        [ForeignKey(nameof(ResourceDefinitionId))]
        public ResourceDefinition ResourceDefinition { get; set; }

        [Display(Name = "Account_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Account_Center")]
        [ForeignKey(nameof(CenterId))]
        public Center Center { get; set; }

        [Display(Name = "Account_Contract")]
        [ForeignKey(nameof(ContractId))]
        public Contract Contract { get; set; }

        [Display(Name = "Account_Resource")]
        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; }

        [Display(Name = "Account_EntryType")]
        [ForeignKey(nameof(EntryTypeId))]
        public EntryType EntryType { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }

        [Display(Name = "Account_NotedContractDefinition")]
        [ForeignKey(nameof(NotedContractDefinitionId))]
        public ContractDefinition NotedContractDefinition { get; set; }
    }
}
