using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "Agent", Plural = "Agents")]
    public class AgentForSave : EntityWithKey<int>
    {
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required]
        [NotNull]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Name2 { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [StringLength(50)]
        [AlwaysAccessible]
        public string Name3 { get; set; }

        //[Display(Name = "Code")]
        //[StringLength(255)]
        //[AlwaysAccessible]
        //public string Code { get; set; }

        [Display(Name = "Agent_IsRelated")]
        [NotNull]
        public bool? IsRelated { get; set; }
    }

    public class Agent : AgentForSave
    {
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

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
