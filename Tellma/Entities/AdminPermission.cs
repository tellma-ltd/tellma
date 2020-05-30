using Tellma.Services.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    // Note: The permissions is a semi-weak entity, meaning it does not have its own screen or API
    // Permissions are always retrieved and saved as a child collection of some other strong entity
    // We call it "semi"- weak because it comes associated with more than one strong entity
    [EntityDisplay(Singular = "Permission", Plural = "Permissions")]
    public class AdminPermissionForSave : EntityWithKey<int>
    {
        [Display(Name = "Permission_View")]
        [Required]
        [StringLength(255)]
        [AlwaysAccessible]
        public string View { get; set; }

        [Display(Name = "Permission_Action")]
        [Required]
        [ChoiceList(new object[] { Constants.Read, Constants.Update, "Delete", "IsActive", "ResendInvitationEmail", "All" },
            new string[] { "Permission_Read", "Permission_Update", "Permission_Delete", "Permission_IsActive", "ResendInvitationEmail", "View_All" })]
        [AlwaysAccessible]
        public string Action { get; set; }

        [Display(Name = "Permission_Criteria")]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Criteria { get; set; }

        [Display(Name = "Memo")]
        [StringLength(255)]
        public string Memo { get; set; }
    }

    public class AdminPermission : AdminPermissionForSave
    {
        public int? AdminUserId { get; set; }

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
