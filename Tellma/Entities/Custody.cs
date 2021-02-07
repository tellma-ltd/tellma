using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Custody", Plural = "Custodies")]
    public class CustodyForSaveBase : EntityWithKey<int>, ILocationEntityForSave, IEntityWithImage
    {
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required]
        [NotNull]
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

        #region Common with Resource

        [Display(Name = "Entity_Currency")]
        [StringLength(3)]
        public string CurrencyId { get; set; }

        [Display(Name = "Entity_Center")]
        public int? CenterId { get; set; }

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
        [StringLength(50)]
        public string Text1 { get; set; }

        [Display(Name = "Entity_Text2")]
        [StringLength(50)]
        public string Text2 { get; set; }

        [Display(Name = "Custody_Custodian")]
        public int? CustodianId { get; set; }

        [NotMapped]
        [Display(Name = "Image")]
        public byte[] Image { get; set; }

        #endregion

        #region Custody Only

        [Display(Name = "Custody_ExternalReference")]
        [StringLength(34)]
        public string ExternalReference { get; set; }

        #endregion
    }

    public class CustodyForSave : CustodyForSaveBase
    {

    }

    public class Custody : CustodyForSaveBase, ILocationEntity
    {
        #region Common with Resource

        [Display(Name = "Definition")]
        [NotNull]
        public int? DefinitionId { get; set; }

        public string ImageId { get; set; }

        [Display(Name = "IsActive")]
        [NotNull]
        [AlwaysAccessible]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        [NotNull]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [NotNull]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [NotNull]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "Definition")]
        [ForeignKey(nameof(DefinitionId))]
        public CustodyDefinition Definition { get; set; }

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

        #endregion

        #region Custody Only

        [Display(Name = "Custody_Custodian")]
        [ForeignKey(nameof(CustodianId))]
        public Relation Custodian { get; set; }

        #endregion
    }
}