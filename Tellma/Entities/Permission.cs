using Tellma.Services.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "Permission", Plural = "Permissions")]
    public class PermissionForSave : EntityWithKey<int>
    {
        [Display(Name = "Permission_View")]
        [Required]
        [StringLength(255)]
        [AlwaysAccessible]
        public string View { get; set; }

        [Display(Name = "Permission_Action")]
        [Required]
        [ChoiceList(new object[] { Constants.Read, Constants.Update, "Delete", "IsActive", "IsDeprecated", "ResendInvitationEmail", "State", "All" },
            new string[] { "Permission_Read", "Permission_Update", "Permission_Delete", "Permission_IsActive", "Permission_IsDeprecated", "ResendInvitationEmail", "Permission_State", "View_All" })]
        [AlwaysAccessible]
        public string Action { get; set; }

        [Display(Name = "Permission_Criteria")]
        [StringLength(1024)]
        [AlwaysAccessible]
        public string Criteria { get; set; }

        [Display(Name = "Permission_Mask")]
        [StringLength(2048)]
        [AlwaysAccessible]
        public string Mask { get; set; }

        [Display(Name = "Memo")]
        [StringLength(255)]
        public string Memo { get; set; }
    }

    public class Permission : PermissionForSave
    {
        [Display(Name = "Permission_Role")]
        [AlwaysAccessible]
        public int? RoleId { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "Permission_Role")]
        [ForeignKey(nameof(RoleId))]
        [AlwaysAccessible]
        public Role Role { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
