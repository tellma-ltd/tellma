using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
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
        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "RoleMembership_User")]
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Display(Name = "RoleMembership_Role")]
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
