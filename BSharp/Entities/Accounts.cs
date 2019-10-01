using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    [StrongEntity]
    public class AccountForSave : EntityWithKey<int>
    {
        [Display(Name = "Account_Type")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string AccountTypeId { get; set; }

        [Display(Name = "Account_Classification")]
        public int? AccountClassificationId { get; set; }

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

        [Display(Name = "Account_PartyReference")]
        [StringLength(50, ErrorMessage = nameof(StringLengthAttribute))]
        public string PartyReference { get; set; }

        [Display(Name = "Account_ResponsibilityCenter")]
        public int? ResponsibilityCenterId { get; set; }

        [Display(Name = "Account_Custodian")]
        public int? CustodianId { get; set; }

        [Display(Name = "Account_Resource")]
        public int? ResourceId { get; set; }

        [Display(Name = "Account_Location")]
        public int? LocationId { get; set; }
    }

    public class Account : AccountForSave
    {
        public string AccountDefinitionId { get; set; }

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

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }

        [Display(Name = "Account_Type")]
        [ForeignKey(nameof(AccountTypeId))]
        public AccountType AccountType { get; set; }

        [Display(Name = "Account_Classification")]
        [ForeignKey(nameof(AccountClassificationId))]
        public AccountClassification AccountClassification { get; set; }

        [Display(Name = "Account_ResponsibilityCenter")]
        [ForeignKey(nameof(ResponsibilityCenterId))]
        public ResponsibilityCenter ResponsibilityCenter { get; set; }

        [Display(Name = "Account_Custodian")]
        [ForeignKey(nameof(CustodianId))]
        public Agent Custodian { get; set; }

        [Display(Name = "Account_Resource")]
        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; }

        [Display(Name = "Account_Location")]
        [ForeignKey(nameof(LocationId))]
        public Location Location { get; set; }
    }
}
