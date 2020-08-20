using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Resource", Plural = "Resources")]
    public class ResourceForSaveBase<TResourceUnit> : EntityWithKey<int>, ILocationEntityForSave, IEntityWithImageForSave
    {
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name { get; set; } // Check

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name2 { get; set; } // Check

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name3 { get; set; } // Check

        [Display(Name = "Code")]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Code { get; set; } // Check

        #region Common with Relation

        [Display(Name = "Entity_Currency")]
        [StringLength(3)]
        public string CurrencyId { get; set; } // Check

        [Display(Name = "Entity_Center")]
        public int? CenterId { get; set; } // Check

        [MultilingualDisplay(Name = "Description", Language = Language.Primary)]
        [StringLength(2048)]
        [AlwaysAccessible]
        public string Description { get; set; } // Check

        [MultilingualDisplay(Name = "Description", Language = Language.Secondary)]
        [StringLength(2048)]
        [AlwaysAccessible]
        public string Description2 { get; set; } // Check

        [MultilingualDisplay(Name = "Description", Language = Language.Ternary)]
        [StringLength(2048)]
        [AlwaysAccessible]
        public string Description3 { get; set; } // Check

        [Display(Name = "Entity_LocationJson")]
        public string LocationJson { get; set; }

        // Auto computed from the GeoJSON property, not visible to clients
        public byte[] LocationWkb { get; set; } 

        [Display(Name = "Entity_FromDate")]
        public DateTime? FromDate { get; set; } // Check

        [Display(Name = "Entity_ToDate")]
        public DateTime? ToDate { get; set; } // Check

        [Display(Name = "Entity_Decimal1")]
        public decimal? Decimal1 { get; set; } // Check

        [Display(Name = "Entity_Decimal2")]
        public decimal? Decimal2 { get; set; } // Check

        [Display(Name = "Entity_Int1")]
        public int? Int1 { get; set; } // Check

        [Display(Name = "Entity_Int2")]
        public int? Int2 { get; set; } // Check

        [Display(Name = "Entity_Lookup1")]
        public int? Lookup1Id { get; set; } // Check

        [Display(Name = "Entity_Lookup2")]
        public int? Lookup2Id { get; set; } // Check

        [Display(Name = "Entity_Lookup3")]
        public int? Lookup3Id { get; set; } // Check

        [Display(Name = "Entity_Lookup4")]
        public int? Lookup4Id { get; set; } // Check

        //[Display(Name = "Entity_Lookup5")]
        //public int? Lookup5Id { get; set; }

        [Display(Name = "Entity_Text1")]
        [StringLength(50)]
        public string Text1 { get; set; } // Check

        [Display(Name = "Entity_Text2")]
        [StringLength(50)]
        public string Text2 { get; set; } // Check

        [NotMapped]
        [Display(Name = "Image")]
        public byte[] Image { get; set; }

        #endregion

        #region Resource Only

        [Display(Name = "Resource_Identifier")]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Identifier { get; set; } // Check

        [Display(Name = "Resource_VatRate")]
        public decimal? VatRate { get; set; }

        [Display(Name = "Resource_ReorderLevel")]
        public decimal? ReorderLevel { get; set; } // Check

        [Display(Name = "Resource_EconomicOrderQuantity")]
        public decimal? EconomicOrderQuantity { get; set; } // Check

        [Display(Name = "Resource_Unit")]
        public int? UnitId { get; set; }

        [Display(Name = "Resource_UnitMass")]
        public decimal? UnitMass { get; set; }

        [Display(Name = "Resource_UnitMassUnit")]
        public int? UnitMassUnitId { get; set; }

        [Display(Name = "Resource_MonetaryValue")]
        public decimal? MonetaryValue { get; set; } // Check

        [Display(Name = "Resource_Participant")]
        public int? ParticipantId { get; set; }

        [Display(Name = "Resource_Units")]
        [ForeignKey(nameof(ResourceUnit.ResourceId))]
        public List<TResourceUnit> Units { get; set; }
        
        #endregion
    }

    public class ResourceForSave : ResourceForSaveBase<ResourceUnitForSave>
    {

    }

    public class Resource : ResourceForSaveBase<ResourceUnit>, ILocationEntity, IEntityWithImage
    {
        [Display(Name = "Definition")]
        public int? DefinitionId { get; set; }

        public string ImageId { get; set; }

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

        [Display(Name = "Definition")]
        [ForeignKey(nameof(DefinitionId))]
        public ResourceDefinition Definition { get; set; }

        public Geography Location { get; set; }

        [Display(Name = "Entity_Center")]
        [ForeignKey(nameof(CenterId))]
        public Center Center { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }

        [Display(Name = "Entity_Currency")]
        [ForeignKey(nameof(CurrencyId))]
        public Currency Currency { get; set; }

        [Display(Name = "Entity_Lookup1")]
        [ForeignKey(nameof(Lookup1Id))]
        public Lookup Lookup1 { get; set; }

        [Display(Name = "Entity_Lookup2")]
        [ForeignKey(nameof(Lookup2Id))]
        public Lookup Lookup2 { get; set; }

        [Display(Name = "Entity_Lookup3")]
        [ForeignKey(nameof(Lookup3Id))]
        public Lookup Lookup3 { get; set; }

        [Display(Name = "Entity_Lookup4")]
        [ForeignKey(nameof(Lookup4Id))]
        public Lookup Lookup4 { get; set; }

        //[Display(Name = "Entity_Lookup5")]
        //[ForeignKey(nameof(Lookup5Id))]
        //public Lookup Lookup5 { get; set; }

        [Display(Name = "Resource_Unit")]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; }

        [Display(Name = "Resource_UnitMassUnit")]
        [ForeignKey(nameof(UnitMassUnitId))]
        public Unit UnitMassUnit { get; set; }

        [Display(Name = "Resource_Participant")]
        [ForeignKey(nameof(ParticipantId))]
        public Relation Participant { get; set; }
    }
}
