using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    [StrongEntity]
    public class AgentForSave : EntityWithKey<int>
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

        [Display(Name = "Agent_AgentType")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(30, ErrorMessage = nameof(StringLengthAttribute))]
        [ChoiceList(new object[] { "Individual", "Organization", "System" },
            new string[] { "Agent_AgentType_Individual", "Agent_AgentType_Organization", "Agent_AgentType_System" })]
        [AlwaysAccessible]
        public string AgentType { get; set; }

        [Display(Name = "Agent_IsRelated")]
        [Required]
        [AlwaysAccessible]
        public bool? IsRelated { get; set; }

        [Display(Name = "Agent_TaxIdentificationNumber")]
        [StringLength(30, ErrorMessage = nameof(StringLengthAttribute))]
        public string TaxIdentificationNumber { get; set; }

        [Display(Name = "Agent_PreferredLanguage")]
        [Culture]
        [StringLength(2, ErrorMessage = nameof(StringLengthAttribute))]
        public string PreferredLanguage { get; set; }

        [NotMapped]
        [Display(Name = "Image")]
        public byte[] Image { get; set; }
    }

    public class Agent : AgentForSave
    {
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

        [Display(Name = "Agent_User")]
        [ForeignKey(nameof(Id))]
        public User User { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
