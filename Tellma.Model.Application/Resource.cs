using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Resource", GroupName = "Resources")]
    public class ResourceForSaveBase<TResourceUnit> : EntityWithKey<int>, ILocationEntityForSave, IEntityWithImage
    {
        [Display(Name = "Name")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string Name { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name2 { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name3 { get; set; }

        [Display(Name = "Code")]
        [StringLength(50)]
        public string Code { get; set; }

        #region Common with Relation

        [Display(Name = "Entity_Currency")]
        [Required]
        [StringLength(3)]
        public string CurrencyId { get; set; }

        [Display(Name = "Entity_Center")]
        public int? CenterId { get; set; }

        [Display(Name = "Description")]
        [StringLength(2048)]
        public string Description { get; set; }

        [Display(Name = "Description")]
        [StringLength(2048)]
        public string Description2 { get; set; }

        [Display(Name = "Description")]
        [StringLength(2048)]
        public string Description3 { get; set; }

        [Display(Name = "Entity_LocationJson")]
        public string LocationJson { get; set; }

        // Auto computed from the GeoJSON property, not visible to clients
        public byte[] LocationWkb { get; set; } 

        [Display(Name = "Entity_FromDate")]
        public DateTime? FromDate { get; set; }

        [Display(Name = "Entity_ToDate")]
        public DateTime? ToDate { get; set; }

        [Display(Name = "Entity_Decimal1")]
        public decimal? Decimal1 { get; set; }

        [Display(Name = "Entity_Decimal2")]
        public decimal? Decimal2 { get; set; }

        [Display(Name = "Entity_Int1")]
        public int? Int1 { get; set; }

        [Display(Name = "Entity_Int2")]
        public int? Int2 { get; set; }

        [Display(Name = "Entity_Lookup1")]
        public int? Lookup1Id { get; set; }

        [Display(Name = "Entity_Lookup2")]
        public int? Lookup2Id { get; set; }

        [Display(Name = "Entity_Lookup3")]
        public int? Lookup3Id { get; set; }

        [Display(Name = "Entity_Lookup4")]
        public int? Lookup4Id { get; set; }

        //[Display(Name = "Entity_Lookup5")]
        //public int? Lookup5Id { get; set; }

        [Display(Name = "Entity_Text1")]
        [StringLength(255)]
        public string Text1 { get; set; }

        [Display(Name = "Entity_Text2")]
        [StringLength(255)]
        public string Text2 { get; set; }

        [NotMapped]
        [Display(Name = "Image")]
        public byte[] Image { get; set; }

        #endregion

        #region Resource Only

        [Display(Name = "Resource_Identifier")]
        [StringLength(50)]
        public string Identifier { get; set; }

        [Display(Name = "Resource_VatRate")]
        public decimal? VatRate { get; set; }

        [Display(Name = "Resource_ReorderLevel")]
        public decimal? ReorderLevel { get; set; }

        [Display(Name = "Resource_EconomicOrderQuantity")]
        public decimal? EconomicOrderQuantity { get; set; }

        [Display(Name = "Resource_Unit")]
        public int? UnitId { get; set; }

        [Display(Name = "Resource_UnitMass")]
        public decimal? UnitMass { get; set; }

        [Display(Name = "Resource_UnitMassUnit")]
        public int? UnitMassUnitId { get; set; }

        [Display(Name = "Resource_MonetaryValue")]
        public decimal? MonetaryValue { get; set; }

        [Display(Name = "Resource_Participant")]
        public int? ParticipantId { get; set; }

        [NotMapped]
        public int? Resource1Index { get; set; }

        [Display(Name = "Resource_Resource1")]
        [SelfReferencing(nameof(Resource1Index))]
        public int? Resource1Id { get; set; }

        [Display(Name = "Resource_Units")]
        [ForeignKey(nameof(ResourceUnit.ResourceId))]
        public List<TResourceUnit> Units { get; set; }
        
        #endregion
    }

    public class ResourceForSave : ResourceForSaveBase<ResourceUnitForSave>
    {

    }

    public class Resource : ResourceForSaveBase<ResourceUnit>, ILocationEntity
    {
        [Display(Name = "Definition")]
        [Required]
        public int? DefinitionId { get; set; }

        public string ImageId { get; set; }

        [Display(Name = "IsActive")]
        [Required]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [Required]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [Required]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
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

        [Display(Name = "Resource_Unit")]
        [ForeignKey(nameof(UnitId))]
        public Unit Unit { get; set; }

        [Display(Name = "Resource_UnitMassUnit")]
        [ForeignKey(nameof(UnitMassUnitId))]
        public Unit UnitMassUnit { get; set; }

        [Display(Name = "Resource_Participant")]
        [ForeignKey(nameof(ParticipantId))]
        public Relation Participant { get; set; }

        [Display(Name = "Resource_Resource1")]
        [ForeignKey(nameof(Resource1Id))]
        public Resource Resource1 { get; set; }
    }
}
