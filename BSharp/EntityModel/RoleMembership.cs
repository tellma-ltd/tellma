using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.EntityModel
{
    public class RoleMembershipForSave : EntityWithKey<int>
    {
        [Display(Name = "RoleMembership_User")]
        [AlwaysAccessible]
        public int? UserId { get; set; }

        [Display(Name = "RoleMembership_Role")]
        public int? RoleId { get; set; }

        [Display(Name = "Memo")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string Memo { get; set; }
    }

    public class RoleMembership : RoleMembershipForSave
    {
        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }


        // For Query

        [Display(Name = "RoleMembership_User")]
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Display(Name = "RoleMembership_Role")]
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
