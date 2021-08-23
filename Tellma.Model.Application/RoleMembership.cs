using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    // Note: The RoleMembership is a semi-weak entity, meaning it does not have its own screen or API
    // RoleMemberships are always retrieved and saved as a child collection of some other strong entity
    // We call it "semi"- weak because it comes associated with more than one strong entity

    [Display(Name = "RoleMembership", GroupName = "RoleMemberships")]
    public class RoleMembershipForSave : EntityWithKey<int>
    {
        [Display(Name = "RoleMembership_User")]
        [Required]
        public int? UserId { get; set; }

        [Display(Name = "RoleMembership_Role")]
        [Required]
        public int? RoleId { get; set; }

        [Display(Name = "Memo")]
        [StringLength(255)]
        public string Memo { get; set; }
    }

    public class RoleMembership : RoleMembershipForSave
    {
        [Display(Name = "ModifiedBy")]
        [Required]
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
