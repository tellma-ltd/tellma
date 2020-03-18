using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    public class ResourceForSave<TResourceUnit> : EntityWithKey<int>
    {
        [Display(Name = "Resource_AccountType")]
        [Required(ErrorMessage = Services.Utilities.Constants.Error_TheField0IsRequired)]
        [AlwaysAccessible]
        public int? AccountTypeId { get; set; }

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

        [Display(Name = "Resource_Identifier")]
        [StringLength(10, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Identifier { get; set; }

        [Display(Name = "Code")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Code { get; set; }

        [Display(Name = "Resource_Currency")]
        public string CurrencyId { get; set; }

        [Display(Name = "Resource_MonetaryValue")]
        public decimal? MonetaryValue { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Primary)]
        [StringLength(2048, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Description { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Secondary)]
        [StringLength(2048, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Description2 { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Ternary)]
        [StringLength(2048, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Description3 { get; set; }

        [Display(Name = "Resource_CostObject")]
        public int? CostObjectId { get; set; }

        [Display(Name = "Resource_ExpenseEntryType")]
        public int? ExpenseEntryTypeId { get; set; }

        [Display(Name = "Resource_ExpenseCenter")]
        public int? ExpenseCenterId { get; set; }

        [Display(Name = "Resource_InvestmentCenter")]
        public int? InvestmentCenterId { get; set; }

        [Display(Name = "Resource_ResidualMonetaryValue")]
        public decimal? ResidualMonetaryValue { get; set; }

        [Display(Name = "Resource_ResidualValue")]
        public decimal? ResidualValue { get; set; }

        [Display(Name = "Resource_ReorderLevel")]
        public decimal? ReorderLevel { get; set; }

        [Display(Name = "Resource_EconomicOrderQuantity")]
        public decimal? EconomicOrderQuantity { get; set; }

        [Display(Name = "Resource_AvailableSince")]
        public DateTime? AvailableSince { get; set; }

        [Display(Name = "Resource_AvailableTill")]
        public DateTime? AvailableTill { get; set; }

        [Display(Name = "Resource_Decimal1")]
        public decimal? Decimal1 { get; set; }

        [Display(Name = "Resource_Decimal2")]
        public decimal? Decimal2 { get; set; }

        [Display(Name = "Resource_Int1")]
        public int? Int1 { get; set; }

        [Display(Name = "Resource_Int2")]
        public int? Int2 { get; set; }

        [Display(Name = "Resource_Lookup1")]
        public int? Lookup1Id { get; set; }

        [Display(Name = "Resource_Lookup2")]
        public int? Lookup2Id { get; set; }

        [Display(Name = "Resource_Lookup3")]
        public int? Lookup3Id { get; set; }

        [Display(Name = "Resource_Lookup4")]
        public int? Lookup4Id { get; set; }

        //[Display(Name = "Resource_Lookup5")]
        //public int? Lookup5Id { get; set; }
        
        [Display(Name = "Resource_Text1")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string Text1 { get; set; }

        [Display(Name = "Resource_Text2")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string Text2 { get; set; }

        [Display(Name = "Resource_Units")]
        [ForeignKey(nameof(ResourceUnit.ResourceId))]
        public List<TResourceUnit> Units { get; set; }
    }

    public class ResourceForSave : ResourceForSave<ResourceUnitForSave>
    {

    }

    public class Resource : ResourceForSave<ResourceUnit>
    {
        [Display(Name = "Definition")]
        public string DefinitionId { get; set; }

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

        [Display(Name = "Resource_CostObject")]
        [ForeignKey(nameof(CostObjectId))]
        public Agent CostObject { get; set; }

        [Display(Name = "Resource_ExpenseEntryType")]
        [ForeignKey(nameof(ExpenseEntryTypeId))]
        public EntryType ExpenseEntryType { get; set; }

        [Display(Name = "Resource_ExpenseCenter")]
        [ForeignKey(nameof(ExpenseCenterId))]
        public Center ExpenseCenter { get; set; }

        [Display(Name = "Resource_InvestmentCenter")]
        [ForeignKey(nameof(InvestmentCenterId))]
        public Center InvestmentCenter { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }

        [Display(Name = "Resource_AccountType")]
        [ForeignKey(nameof(AccountTypeId))]
        public AccountType AccountType { get; set; }

        [Display(Name = "Resource_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        //[Display(Name = "Resource_MassUnit")]
        //[ForeignKey(nameof(MassUnitId))]
        //public Unit MassUnit { get; set; }

        //[Display(Name = "Resource_VolumeUnit")]
        //[ForeignKey(nameof(VolumeUnitId))]
        //public Unit VolumeUnit { get; set; }

        //[Display(Name = "Resource_TimeUnit")]
        //[ForeignKey(nameof(TimeUnitId))]
        //public Unit TimeUnit { get; set; }

        //[Display(Name = "Resource_CountUnit")]
        //[ForeignKey(nameof(CountUnitId))]
        //public Unit CountUnit { get; set; }

        [Display(Name = "Resource_Lookup1")]
        [ForeignKey(nameof(Lookup1Id))]
        public Lookup Lookup1 { get; set; }

        [Display(Name = "Resource_Lookup2")]
        [ForeignKey(nameof(Lookup2Id))]
        public Lookup Lookup2 { get; set; }

        [Display(Name = "Resource_Lookup3")]
        [ForeignKey(nameof(Lookup3Id))]
        public Lookup Lookup3 { get; set; }

        [Display(Name = "Resource_Lookup4")]
        [ForeignKey(nameof(Lookup4Id))]
        public Lookup Lookup4 { get; set; }

        //[Display(Name = "Resource_Lookup5")]
        //[ForeignKey(nameof(Lookup5Id))]
        //public Lookup Lookup5 { get; set; }
    }
}
