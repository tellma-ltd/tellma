using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// I will put here the readonly entities added to build the JV

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
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Code { get; set; }

        // Temp

        public int? AccountClassificationId { get; set; } // Nav
        public string IfrsAccountClassificationId { get; set; } // Nav
        public string PartyReference { get; set; }
        //public bool? IsMultiEntryClassification { get; set; }
        public string IfrsEntryClassificationId { get; set; } // Nav
        //public bool? IsMultiAgent { get; set; }
        //public int? AgentId { get; set; } // Nav
        //public bool? IsMultiResponsibilityCenter { get; set; }
        //public int? ResponsibilityCenterId { get; set; } // Nev
        //public bool? IsMultiResource { get; set; }
        //public int? ResourceId { get; set; } // Nev
        public int? ResponsibleActorId { get; set; } // Nev
        public int? ResponsibleRoleId { get; set; } // Nev
        public int? CustodianActorId { get; set; } // Nev
        public int? CustodianRoleId { get; set; } // Nev
    }

    public class Account : AccountForSave
    {
        [Display(Name = "IsActive")]
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

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }


        // Temp

        //[ForeignKey(nameof(AccountClassificationId))]
        //public AccountClassification AccountClassification { get; set; }

        [ForeignKey(nameof(IfrsAccountClassificationId))]
        public IfrsAccountClassification IfrsAccountClassification { get; set; }

        [ForeignKey(nameof(IfrsEntryClassificationId))]
        public IfrsEntryClassification IfrsEntryClassification { get; set; }

        //[ForeignKey(nameof(AgentId))]
        //public Agent Agent { get; set; }

        //[ForeignKey(nameof(ResponsibilityCenterId))]
        //public ResponsibilityCenter ResponsibilityCenter { get; set; }

        //[ForeignKey(nameof(ResourceId))]
        //public Resource Resource { get; set; }

        [ForeignKey(nameof(ResponsibleActorId))]
        public Agent ResponsibleActor { get; set; }

        [ForeignKey(nameof(ResponsibleRoleId))]
        public Role ResponsibleRole { get; set; }

        [ForeignKey(nameof(CustodianActorId))]
        public Agent CustodianActor { get; set; }

        [ForeignKey(nameof(CustodianRoleId))]
        public Role CustodianRole { get; set; }
    }

    [StrongEntity]
    public class VoucherBookletForSave : EntityWithKey<int>
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

        // Temp

        public int VoucherTypeId { get; set; }
        public string StringPrefix { get; set; }
        public int? NumericLength { get; set; }
        public int? RangeStarts { get; set; }
        public int? RangeEnds { get; set; }
    }

    public class VoucherBooklet : VoucherBookletForSave
    {
        [Display(Name = "IsActive")]
        [AlwaysAccessible]
        public bool? IsActive { get; set; }

        //[Display(Name = "CreatedAt")]
        //public DateTimeOffset? CreatedAt { get; set; }

        //[Display(Name = "CreatedBy")]
        //public int? CreatedById { get; set; }

        //[Display(Name = "ModifiedAt")]
        //public DateTimeOffset? ModifiedAt { get; set; }

        //[Display(Name = "ModifiedBy")]
        //public int? ModifiedById { get; set; }

        //[Display(Name = "CreatedBy")]
        //[ForeignKey(nameof(CreatedById))]
        //public User CreatedBy { get; set; }

        //[Display(Name = "CreatedBy")]
        //[ForeignKey(nameof(ModifiedById))]
        //public User ModifiedBy { get; set; }


        // Temp

        //[ForeignKey(nameof(VoucherTypeId))]
        //public VoucherType VoucherType { get; set; }
    }

    [StrongEntity]
    public class ResourcePickForSave : EntityWithKey<int>
    {
        // Where is Name ?? The name of the Resource itself?

        [Display(Name = "Code")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Code { get; set; }

        // Temp

        public int? ResourceId { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal? MonetaryValue { get; set; }
        public decimal? Mass { get; set; }
        public decimal? Volume { get; set; }
        public decimal? Area { get; set; }
        public decimal? Length { get; set; }
        public decimal? Time { get; set; }
        public decimal? Count { get; set; }
        public string Beneficiary { get; set; }
        public int? IssuingBankAccountId { get; set; }
        public int? IssuingBankId { get; set; }
    }

    public class ResourcePick : ResourcePickForSave
    {
        [Display(Name = "IsActive")]
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

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }

        // Temp

        [ForeignKey(nameof(ResourceId))]
        public Resource Resource { get; set; }

        //[ForeignKey(nameof(IssuingBankAccountId))]
        //public ??? IssuingBankAccount { get; set; }

        //[ForeignKey(nameof(IssuingBankId))]
        //public ??? IssuingBank { get; set; }

    }

    [StrongEntity]
    public class ResponsibilityCenterForSave : EntityWithKey<int>
    {
        [NotMapped]
        public int? ParentIndex { get; set; }

        [AlwaysAccessible]
        public int? ParentId { get; set; }

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
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Code { get; set; }
    }

    public class ResponsibilityCenter : ResponsibilityCenterForSave
    {
        //[AlwaysAccessible]
        //public short? Level { get; set; }

        //[AlwaysAccessible]
        //public int? ActiveChildCount { get; set; }

        //[AlwaysAccessible]
        //public int? ChildCount { get; set; }

        [Display(Name = "IsActive")]
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

        //[AlwaysAccessible]
        //public HierarchyId Node { get; set; }

        //[AlwaysAccessible]
        //public HierarchyId ParentNode { get; set; }

        [Display(Name = "TreeParent")]
        [ForeignKey(nameof(ParentId))]
        public ResponsibilityCenter Parent { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
