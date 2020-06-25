using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "ContractUser", Plural = "ContractUsers")]
    public class ContractUserForSave : EntityWithKey<int>
    {
        [Display(Name = "ContractUser_User")]
        [Required]
        public int? UserId { get; set; }
    }

    public class ContractUser : ContractUserForSave
    {
        public int? ContractId { get; set; }

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

        [Display(Name = "ContractUser_User")]
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
