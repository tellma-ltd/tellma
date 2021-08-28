using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Agent", GroupName = "Agents")]
    public class AgentForSave<TAgentUser, TAttachment> : EntityWithKey<int>, ILocationEntityForSave, IEntityWithImage
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

        #region Common with Resource

        [Display(Name = "Entity_Currency")]
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

        [Display(Name = "Agent_DateOfBirth")]
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
        [StringLength(255)]
        public string Text1 { get; set; }

        [Display(Name = "Entity_Text2")]
        [StringLength(255)]
        public string Text2 { get; set; }

        [Display(Name = "Entity_Text3")]
        [StringLength(255)]
        public string Text3 { get; set; }

        [Display(Name = "Entity_Text4")]
        [StringLength(255)]
        public string Text4 { get; set; }

        [NotMapped]
        [Display(Name = "Image")]
        public byte[] Image { get; set; }

        #endregion

        #region Agent Only

        [Display(Name = "Agent_TaxIdentificationNumber")]
        [StringLength(18)]
        public string TaxIdentificationNumber { get; set; }

        [Display(Name = "Agent_BankAccountNumber")]
        [StringLength(34)]
        public string BankAccountNumber { get; set; }

        [Display(Name = "Agent_ExternalReference")]
        [StringLength(255)]
        public string ExternalReference { get; set; }

        [Display(Name = "Agent_User")]
        public int? UserId { get; set; }

        [NotMapped]
        public int? Agent1Index { get; set; }

        [Display(Name = "Agent_Agent1")]
        [SelfReferencing(nameof(Agent1Index))]
        public int? Agent1Id { get; set; }

        [Display(Name = "Agent_Users")]
        [ForeignKey(nameof(AgentUser.AgentId))]
        public List<TAgentUser> Users { get; set; }

        [Display(Name = "Agent_Attachments")]
        [ForeignKey(nameof(AgentAttachment.AgentId))]
        public List<TAttachment> Attachments { get; set; }

        #endregion
    }

    public class AgentForSave : AgentForSave<AgentUserForSave, AgentAttachmentForSave>
    {
    }

    public class Agent : AgentForSave<AgentUser, AgentAttachment>, ILocationEntity
    {
        #region Common with Resource

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
        public AgentDefinition Definition { get; set; }

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

        #region Agent Only

        [Display(Name = "Agent_UserId")]
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Display(Name = "Agent_Agent1")]
        [ForeignKey(nameof(Agent1Id))]
        public Agent Agent1 { get; set; }

        #endregion
    }
}