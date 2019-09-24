using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    [StrongEntity]
    public class RoleForSave<TPermission, TRoleMembership> : EntityWithKey<int>
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
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Code { get; set; }

        [Display(Name = "Role_IsPublic")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [AlwaysAccessible]
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
        [AlwaysAccessible]
        public bool? IsActive { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
