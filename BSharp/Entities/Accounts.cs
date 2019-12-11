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
        [Required(ErrorMessage = nameof(RequiredAttribute))]
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
        public string CurrencyId { get; set; }

        // Fields below are not used for now....

        public int? ResponsibilityCenterId { get; set; }
        public string ContractType { get; set; }
        public string AgentDefinitionId { get; set; }
        public int? ResourceClassificationId { get; set; }
        public bool? IsCurrent { get; set; }
        public int? AgentId { get; set; }
        public int? ResourceId { get; set; }
        public string Identifier { get; set; }
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

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
