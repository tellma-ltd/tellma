using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// I will put here the readonly entities added to build the JV

namespace BSharp.Entities
{
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
    public class ResourceForSave : EntityWithKey<int>
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

        //public string ResourceType { get; set; }
        public int? ResourceClassificationId { get; set; }
        //public bool? IsBatch { get; set; }
        //public int? UnitId { get; set; }
        public decimal? UnitMonetaryValue { get; set; }
        public int? CurrencyId { get; set; }
        public decimal? UnitMass { get; set; }
        public int? MassUnitId { get; set; }
        public decimal? UnitVolume { get; set; }
        public int? VolumeUnitId { get; set; }
        public decimal? UnitArea { get; set; }
        public int? AreaUnitId { get; set; }
        public decimal? UnitLength { get; set; }
        public int? LengthUnitId { get; set; }
        public decimal? UnitTime { get; set; }
        public int? TimeUnitId { get; set; }
        public decimal? UnitCount { get; set; }
        public int? CountUnitId { get; set; }
        public string SystemCode { get; set; }
        public string Memo { get; set; }
        public string CustomsReference { get; set; }
        //public string UniversalProductCode { get; set; }
        public int? PreferredSupplierId { get; set; }
        public int? ExpenseAccountId { get; set; }
        public int? RevenueAccountId { get; set; }
        public int? ProductCategoryId { get; set; }
    }

    public class Resource : ResourceForSave
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

        //public ResourceClassification ResourceClassification { get; set; }

        //[ForeignKey(nameof(UnitId))]
        //public MeasurementUnit Unit { get; set; }

        [ForeignKey(nameof(CurrencyId))]
        public MeasurementUnit Currency { get; set; }

        [ForeignKey(nameof(MassUnitId))]
        public MeasurementUnit MassUnit { get; set; }

        [ForeignKey(nameof(VolumeUnitId))]
        public MeasurementUnit VolumeUnit { get; set; }

        [ForeignKey(nameof(AreaUnitId))]
        public MeasurementUnit AreaUnit { get; set; }

        [ForeignKey(nameof(LengthUnitId))]
        public MeasurementUnit LengthUnit { get; set; }

        [ForeignKey(nameof(TimeUnitId))]
        public MeasurementUnit TimeUnit { get; set; }

        [ForeignKey(nameof(CountUnitId))]
        public MeasurementUnit CountUnit { get; set; }

        [ForeignKey(nameof(PreferredSupplierId))]
        public Agent PreferredSupplier { get; set; }

        //public int? ExpenseAccountId { get; set; }
        //public int? RevenueAccountId { get; set; }

        [ForeignKey(nameof(ProductCategoryId))]
        public ProductCategory ProductCategory { get; set; }

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
