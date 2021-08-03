using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Role", GroupName = "Roles")]
    public class RoleForSave<TPermission, TRoleMembership> : EntityWithKey<int>
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
        [StringLength(255)]
        public string Code { get; set; }

        [Display(Name = "Role_IsPublic")]
        [Required]
        public bool? IsPublic { get; set; }

        [Display(Name = "Permissions")]
        [ForeignKey(nameof(Permission.RoleId))]
        public List<TPermission> Permissions { get; set; } = new List<TPermission>();

        [Display(Name = "Members")]
        [ForeignKey(nameof(RoleMembership.RoleId))]
        public List<TRoleMembership> Members { get; set; } = new List<TRoleMembership>();
    }

    public class RoleForSave : RoleForSave<PermissionForSave, RoleMembershipForSave>
    {
    }

    public class Role : RoleForSave<Permission, RoleMembership>
    {
        [Display(Name = "IsActive")]
        [Required]
        public bool? IsActive { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
