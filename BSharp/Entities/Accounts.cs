using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    [StrongEntity]
    public class AccountForSave : EntityWithKey<int>
    {
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Name2 { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Name3 { get; set; }

        [Display(Name = "Code")]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Code { get; set; }

        [Display(Name = "Account_IsSmart")]
        [AlwaysAccessible]
        public bool? IsSmart { get; set; }

        [Display(Name = "Account_Type")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string AccountTypeId { get; set; }

        [Display(Name = "Account_Classification")]
        public int? AccountClassificationId { get; set; }

        [Display(Name = "Account_Currency")]
        [StringLength(3, ErrorMessage = nameof(StringLengthAttribute))]
        public string CurrencyId { get; set; }

        [Display(Name = "Account_ResponsibilityCenter")]
        public int? ResponsibilityCenterId { get; set; }

        [Display(Name = "Account_ContractType")]
        public string ContractType { get; set; }

        [Display(Name = "Account_AgentDefinition")]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string AgentDefinitionId { get; set; }

        [Display(Name = "Account_ResourceClassification")]
        public int? ResourceClassificationId { get; set; }

        [Display(Name = "Account_IsCurrent")]
        public bool? IsCurrent { get; set; }

        [Display(Name = "Account_Agent")]
        public int? AgentId { get; set; }

        [Display(Name = "Account_Resource")]
        public int? ResourceId { get; set; }

        [Display(Name = "Account_Identifier")]
        [StringLength(10, ErrorMessage = nameof(StringLengthAttribute))]
        public string Identifier { get; set; }

        [Display(Name = "Account_EntryClassification")]
        public int? EntryClassificationId { get; set; }
    }

    public class Account : AccountForSave
    {
        [Display(Name = "Account_IsDeprecated")]
        [AlwaysAccessible]
        public bool? IsDeprecated { get; set; }

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

        [Display(Name = "Account_Type")]
        [ForeignKey(nameof(AccountTypeId))]
        public AccountType AccountType { get; set; }

        [Display(Name = "Account_Classification")]
        [ForeignKey(nameof(AccountClassificationId))]
        public AccountClassification AccountClassification { get; set; }

        [Display(Name = "Account_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Account_ResponsibilityCenter")]
        [ForeignKey(nameof(ResponsibilityCenterId))]
        public ResponsibilityCenter ResponsibilityCenter { get; set; }

        //[Display(Name = "Account_AgentDefinition")]
        //[ForeignKey(nameof(AgentDefinitionId))]
        //public AgentDefinition AgentDefinition { get; set; }

        [Display(Name = "Account_ResourceClassification")]
        [ForeignKey(nameof(ResourceClassificationId))]
        public ResourceClassification ResourceClassification { get; set; }

        [Display(Name = "Account_Agent")]
        [ForeignKey(nameof(AgentId))]
        public Agent Agent { get; set; }

        [Display(Name = "Account_Resource")]
        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; }

        [Display(Name = "Account_EntryClassification")]
        [ForeignKey(nameof(EntryClassificationId))]
        public EntryClassification EntryClassification { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
