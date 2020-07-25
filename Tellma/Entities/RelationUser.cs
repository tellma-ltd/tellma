using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "RelationUser", Plural = "RelationUsers")]
    public class RelationUserForSave : EntityWithKey<int>
    {
        [Display(Name = "RelationUser_User")]
        [Required]
        public int? UserId { get; set; }
    }

    public class RelationUser : RelationUserForSave
    {
        public int? RelationId { get; set; }

        // For Query

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "RelationUser_User")]
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
