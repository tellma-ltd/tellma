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

        [Display(Name = "Account_Custodian")]
        public int? CustodianId { get; set; }

        [Display(Name = "Account_CustodyDefinition")]
        public int? CustodyDefinitionId { get; set; }

        [Display(Name = "Account_Custody")]
        public int? CustodyId { get; set; }

        [Display(Name = "Account_Participant")]
        public int? ParticipantId { get; set; }

        [Display(Name = "Account_ResourceDefinition")]
        public int? ResourceDefinitionId { get; set; }

        [Display(Name = "Account_Resource")]
        public int? ResourceId { get; set; }

        [Display(Name = "Account_Currency")]
        [StringLength(3)]
        public string CurrencyId { get; set; }

        [Display(Name = "Account_EntryType")]
        public int? EntryTypeId { get; set; }
    }

    public class Account : AccountForSave
    {
        [AlwaysAccessible]
        [Display(Name = "IsActive")]
        public bool? IsActive { get; set; }

        [AlwaysAccessible]
        public bool? IsBusinessUnit { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }
        
        [NotMapped]
        public decimal? Balance { get; set; }

        // For Query

        [Display(Name = "Account_Type")]
        [ForeignKey(nameof(AccountTypeId))]
        public AccountType AccountType { get; set; }

        [Display(Name = "Account_Classification")]
        [ForeignKey(nameof(ClassificationId))]
        public AccountClassification Classification { get; set; }

        [Display(Name = "Account_CustodyDefinition")]
        [ForeignKey(nameof(CustodyDefinitionId))]
        public CustodyDefinition CustodyDefinition { get; set; }

        [Display(Name = "Account_ResourceDefinition")]
        [ForeignKey(nameof(ResourceDefinitionId))]
        public ResourceDefinition ResourceDefinition { get; set; }

        [Display(Name = "Account_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Account_Center")]
        [ForeignKey(nameof(CenterId))]
        public Center Center { get; set; }

        [Display(Name = "Account_Custodian")]
        [ForeignKey(nameof(CustodianId))]
        public Relation Custodian { get; set; }

        [Display(Name = "Account_Custody")]
        [ForeignKey(nameof(CustodyId))]
        public Custody Custody { get; set; }

        [Display(Name = "Account_Participant")]
        [ForeignKey(nameof(ParticipantId))]
        public Relation Participant { get; set; }

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
