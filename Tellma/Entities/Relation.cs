using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Relation", Plural = "Relations")]
    public class RelationForSaveBase<TRelationUser> : EntityWithKey<int>, ILocationEntityForSave, IEntityWithImageForSave
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

        [Display(Name = "Relation_DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Entity_ContactEmail")]
        [EmailAddress]
        [StringLength(255)]
        public string ContactEmail { get; set; }

        [Display(Name = "Entity_ContactMobile")]
        [Phone]
        [StringLength(50)]
        public string ContactMobile { get; set; }

        [Display(Name = "Entity_NormalizedContactMobile")]
        public string NormalizedContactMobile { get; set; }

        [Display(Name = "Entity_ContactAddress")]
        [StringLength(255)]
        public string ContactAddress { get; set; }

        [Display(Name = "Entity_Date1")]
        public DateTime? Date1 { get; set; }

        [Display(Name = "Entity_Date2")]
        public DateTime? Date2 { get; set; }

        [Display(Name = "Entity_Date3")]
        public DateTime? Date3 { get; set; }

        [Display(Name = "Entity_Date4")]
        public DateTime? Date4 { get; set; }

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

        [Display(Name = "Entity_Lookup5")]
        public int? Lookup5Id { get; set; }

        [Display(Name = "Entity_Lookup6")]
        public int? Lookup6Id { get; set; }

        [Display(Name = "Entity_Lookup7")]
        public int? Lookup7Id { get; set; }

        [Display(Name = "Entity_Lookup8")]
        public int? Lookup8Id { get; set; }

        [Display(Name = "Entity_Text1")]
        [StringLength(50)]
        public string Text1 { get; set; }

        [Display(Name = "Entity_Text2")]
        [StringLength(50)]
        public string Text2 { get; set; }

        [Display(Name = "Entity_Text3")]
        [StringLength(50)]
        public string Text3 { get; set; }

        [Display(Name = "Entity_Text4")]
        [StringLength(50)]
        public string Text4 { get; set; }

        [NotMapped]
        [Display(Name = "Image")]
        public byte[] Image { get; set; }

        #endregion

        #region Relation Only

        [Display(Name = "Relation_Agent")]
        public int? AgentId { get; set; }

        [Display(Name = "Relation_TaxIdentificationNumber")]
        [StringLength(18)]
        public string TaxIdentificationNumber { get; set; }

        [Display(Name = "Relation_Job")]
        public int? JobId { get; set; }

        [Display(Name = "Relation_BankAccountNumber")]
        [StringLength(34)]
        public string BankAccountNumber { get; set; }

        [NotMapped]
        public int? Relation1Index { get; set; }

        [Display(Name = "Relation_Relation1")]
        [SelfReferencing(nameof(Relation1Index))]
        public int? Relation1Id { get; set; }

        [Display(Name = "Relation_Users")]
        [ForeignKey(nameof(RelationUser.RelationId))]
        public List<TRelationUser> Users { get; set; }

        #endregion
    }

    public class RelationForSave : RelationForSaveBase<RelationUserForSave>
    {
    }

    public class Relation : RelationForSaveBase<RelationUser>, ILocationEntity, IEntityWithImage
    {
        #region Common with Resource

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
        public RelationDefinition Definition { get; set; }

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

        [Display(Name = "Entity_Lookup5")]
        [ForeignKey(nameof(Lookup5Id))]
        public Lookup Lookup5 { get; set; }

        [Display(Name = "Entity_Lookup6")]
        [ForeignKey(nameof(Lookup6Id))]
        public Lookup Lookup6 { get; set; }

        [Display(Name = "Entity_Lookup7")]
        [ForeignKey(nameof(Lookup7Id))]
        public Lookup Lookup7 { get; set; }

        [Display(Name = "Entity_Lookup8")]
        [ForeignKey(nameof(Lookup8Id))]
        public Lookup Lookup8 { get; set; }

        #endregion

        #region Relation Only

        [Display(Name = "Relation_Agent")]
        [ForeignKey(nameof(AgentId))]
        public Agent Agent { get; set; }

        [Display(Name = "Relation_Relation1")]
        [ForeignKey(nameof(Relation1Id))]
        public Relation Relation1 { get; set; }

        #endregion
    }
}