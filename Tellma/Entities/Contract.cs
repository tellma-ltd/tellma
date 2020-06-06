using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Contract", Plural = "Contracts")]
    public class ContractForSaveBase : EntityWithKey<int>, IEntityWithImageForSave
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
        [StringLength(30)]
        [AlwaysAccessible]
        public string Code { get; set; }

        //[Display(Name = "Agent_IsRelated")]
        //[Required]
        //[AlwaysAccessible]
        //public bool? IsRelated { get; set; }

        [Display(Name = "Contract_TaxIdentificationNumber")]
        [StringLength(30)]
        public string TaxIdentificationNumber { get; set; }

        [Display(Name = "Contract_StartDate")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Contract_Job")]
        public int? JobId { get; set; }

        [Display(Name = "Contract_BankAccountNumber")]
        [StringLength(34)]
        public string BankAccountNumber { get; set; }

        [Display(Name = "Contract_User")]
        public int? UserId { get; set; }

        [NotMapped]
        [Display(Name = "Image")]
        public byte[] Image { get; set; }
    }

    public class ContractForSave : ContractForSaveBase
    {

    }

    public class Contract : ContractForSave, IEntityWithImage
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

        // TODO 
        // [Display(Name = "Contract_Job")]
        // [ForeignKey(nameof(JobId))]
        // public Job Job { get; set; }
        
        [Display(Name = "Contract_User")]
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
