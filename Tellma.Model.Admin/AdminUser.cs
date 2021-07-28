using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Admin
{
    [Display(Name = "AdminUser", GroupName = "AdminUsers")]
    public class AdminUserForSave<TPermission> : EntityWithKey<int>
    {
        [Display(Name = "Name")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string Name { get; set; }

        [Display(Name = "User_Email")]
        [Required, ValidateRequired]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [Display(Name = "User_Permissions")]
        [ForeignKey(nameof(AdminPermission.AdminUserId))]
        public List<TPermission> Permissions { get; set; }
    }

    public class AdminUserForSave : AdminUserForSave<AdminPermissionForSave>
    {
    }

    public class AdminUser : AdminUserForSave<AdminPermission>
    {
        public string ExternalId { get; set; }

        [Display(Name = "State")]
        [Required]
        [ChoiceList(new object[] { "Invited", "Member" }, new string[] { "User_Invited", "User_Member" })]
        public string State { get; set; }

        [Display(Name = "User_LastActivity")]
        public DateTimeOffset? LastAccess { get; set; }

        [Display(Name = "IsActive")]
        [Required]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        [Required]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [Required]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [Required]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public AdminUser CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public AdminUser ModifiedBy { get; set; }
    }
}
