using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    public class AgentForSave<TAgentRate> : EntityWithKey<int>, IEntityWithImageForSave
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
        [StringLength(30, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Code { get; set; }

        [Display(Name = "Agent_IsRelated")]
        [Required]
        [AlwaysAccessible]
        public bool? IsRelated { get; set; }

        [Display(Name = "Agent_TaxIdentificationNumber")]
        [StringLength(30, ErrorMessage = nameof(StringLengthAttribute))]
        public string TaxIdentificationNumber { get; set; }

        [Display(Name = "Agent_StartDate")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Agent_Job")]
        public int? JobId { get; set; }

        [Display(Name = "Agent_BankAccountNumber")]
        [StringLength(34, ErrorMessage = nameof(StringLengthAttribute))]
        public string BankAccountNumber { get; set; }

        public string CostObjectType { get; set; } // TODO: Deal with this

        [Display(Name = "Agent_User")]
        public int? UserId { get; set; }

        [NotMapped]
        [Display(Name = "Image")]
        public byte[] Image { get; set; }

        [Display(Name = "Agent_Rates")]
        [ForeignKey(nameof(AgentRate.AgentId))]
        public List<TAgentRate> Rates { get; set; }
    }

    public class AgentForSave : AgentForSave<AgentRateForSave>
    {

    }

    public class Agent : AgentForSave<AgentRate>, IEntityWithImage
    {
        [Display(Name = "Definition")]
        public string DefinitionId { get; set; }

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
        // [Display(Name = "Agent_Job")]
        // [ForeignKey(nameof(JobId))]
        // public Job Job { get; set; }
        
        [Display(Name = "Agent_User")]
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
