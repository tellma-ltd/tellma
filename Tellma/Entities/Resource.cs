using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Resource", Plural = "Resources")]
    public class ResourceForSave<TResourceUnit> : EntityWithKey<int>, ILocationEntityForSave
    {
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

        [Display(Name = "Resource_Identifier")]
        [StringLength(10)]
        [AlwaysAccessible]
        public string Identifier { get; set; }

        [Display(Name = "Code")]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Code { get; set; }

        [Display(Name = "Resource_Currency")]
        public string CurrencyId { get; set; }

        [Display(Name = "Resource_MonetaryValue")]
        public decimal? MonetaryValue { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Primary)]
        [StringLength(2048)]
        [AlwaysAccessible]
        public string Description { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Secondary)]
        [StringLength(2048)]
        [AlwaysAccessible]
        public string Description2 { get; set; }

        [MultilingualDisplay(Name = "Description", Language = Language.Ternary)]
        [StringLength(2048)]
        [AlwaysAccessible]
        public string Description3 { get; set; }

        [Display(Name = "Resource_LocationJson")]
        public string LocationJson { get; set; }

        // Auto computed from the GeoJSON property, not visible to clients
        public byte[] LocationWkb { get; set; } 

        [Display(Name = "Resource_Center")]
        public int? CenterId { get; set; }

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
        [StringLength(255)]
        public string Text1 { get; set; }

        [Display(Name = "Resource_Text2")]
        [StringLength(255)]
        public string Text2 { get; set; }

        [Display(Name = "Resource_Units")]
        [ForeignKey(nameof(ResourceUnit.ResourceId))]
        public List<TResourceUnit> Units { get; set; }
    }

    public class ResourceForSave : ResourceForSave<ResourceUnitForSave>
    {

    }

    public class Resource : ResourceForSave<ResourceUnit>, ILocationEntity
    {
        [Display(Name = "Definition")]
        public int? DefinitionId { get; set; }

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

        public Geography Location { get; set; }

        public EntryType ExpenseEntryType { get; set; }

        [Display(Name = "Resource_Center")]
        [ForeignKey(nameof(CenterId))]
        public Center Center { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }

        [Display(Name = "Resource_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

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
