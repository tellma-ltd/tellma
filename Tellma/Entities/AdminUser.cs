using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "AdminUser", Plural = "AdminUsers")]
    public class AdminUserForSave<TPermission> : EntityWithKey<int>
    {
        [Display(Name = "Name")]
        [Required]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name { get; set; }

        [Display(Name = "User_Email")]
        [Required]
        [EmailAddress(ErrorMessage = nameof(EmailAddressAttribute))]
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
        [ChoiceList(new object[] { "Invited", "Member" },
            new string[] { "User_Invited", "User_Member" })]
        public string State { get; set; }

        [Display(Name = "User_LastActivity")]
        public DateTimeOffset? LastAccess { get; set; }

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

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public AdminUser CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public AdminUser ModifiedBy { get; set; }
    }
}
