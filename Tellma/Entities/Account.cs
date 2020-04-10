using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    public class AccountForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_Center")]
        public int? CenterId { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
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

        [Display(Name = "Account_Type")]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [AlwaysAccessible]
        public int? AccountTypeId { get; set; }

        [Display(Name = "Account_CustomClassification")]
        public int? CustomClassificationId { get; set; }




        // HIDDEN

        public bool? HasIdentifier { get; set; }

        // END HIDDEN

        [Display(Name = "Account_IsRelated")]
        public bool? IsRelated { get; set; }

        [Display(Name = "Account_HasExternalReference")]
        public bool? HasExternalReference { get; set; }

        [Display(Name = "Account_HasAdditionalReference")]
        public bool? HasAdditionalReference { get; set; }

        [Display(Name = "Account_HasNotedAgentId")]
        public bool? HasNotedAgentId { get; set; }

        [Display(Name = "Account_HasNotedAgentName")]
        public bool? HasNotedAgentName { get; set; }

        [Display(Name = "Account_HasNotedAmount")]
        public bool? HasNotedAmount { get; set; }

        [Display(Name = "Account_HasNotedDate")]
        public bool? HasNotedDate { get; set; }

        [Display(Name = "Account_Agent")]
        public int? AgentId { get; set; }

        [Display(Name = "Account_Resource")]
        public int? ResourceId { get; set; }

        [Display(Name = "Account_Currency")]
        [StringLength(3, ErrorMessage = nameof(StringLengthAttribute))]
        public string CurrencyId { get; set; }

        [Display(Name = "Account_Identifier")]
        [StringLength(10, ErrorMessage = nameof(StringLengthAttribute))]
        public string Identifier { get; set; }

        [Display(Name = "Account_EntryType")]
        public int? EntryTypeId { get; set; }
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

        [Display(Name = "Account_CustomClassification")]
        [ForeignKey(nameof(CustomClassificationId))]
        public CustomClassification CustomClassification { get; set; }

        [Display(Name = "Account_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Account_Center")]
        [ForeignKey(nameof(CenterId))]
        public Center Center { get; set; }

        [Display(Name = "Account_Agent")]
        [ForeignKey(nameof(AgentId))]
        public Agent Agent { get; set; }

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
    }
}
