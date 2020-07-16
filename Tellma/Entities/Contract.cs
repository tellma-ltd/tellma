using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Contract", Plural = "Contracts")]
    public class ContractForSaveBase<TContractUser> : EntityWithKey<int>, ILocationEntityForSave, IEntityWithImageForSave
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
        [StringLength(255)]
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
        [StringLength(255)]
        public string Text1 { get; set; }

        [Display(Name = "Entity_Text2")]
        [StringLength(255)]
        public string Text2 { get; set; }

        [NotMapped]
        [Display(Name = "Image")]
        public byte[] Image { get; set; }

        #endregion

        #region Contract Only

        [Display(Name = "Contract_Agent")]
        public int? AgentId { get; set; }

        [Display(Name = "Contract_TaxIdentificationNumber")]
        [StringLength(30)]
        public string TaxIdentificationNumber { get; set; }

        [Display(Name = "Contract_Job")]
        public int? JobId { get; set; }

        [Display(Name = "Contract_BankAccountNumber")]
        [StringLength(34)]
        public string BankAccountNumber { get; set; }

        [Display(Name = "Contract_Users")]
        [ForeignKey(nameof(ContractUser.ContractId))]
        public List<TContractUser> Users { get; set; }

        #endregion
    }

    public class ContractForSave : ContractForSaveBase<ContractUserForSave>
    {

    }

    public class Contract : ContractForSaveBase<ContractUser>, ILocationEntity, IEntityWithImage
    {
        #region Common with Resource

        [Display(Name = "Definition")]
        [NotNull]
        public int? DefinitionId { get; set; }

        public string ImageId { get; set; }

        [Display(Name = "IsActive")]
        [AlwaysAccessible]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [NotNull]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "Definition")]
        [ForeignKey(nameof(DefinitionId))]
        public ContractDefinition Definition { get; set; }

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

        #region Contract Only

        [Display(Name = "Contract_Agent")]
        [ForeignKey(nameof(AgentId))]
        public Agent Agent { get; set; }

        #endregion
    }
}