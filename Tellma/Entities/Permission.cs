using Tellma.Services.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    // Note: The permissions is a semi-weak entity, meaning it does not have its own screen or API
    // Permissions are always retrieved and saved as a child collection of some other strong entity
    // We call it "semi"- weak because it comes associated with more than one strong entity

    public class PermissionForSave : EntityWithKey<int>
    {
        [Display(Name = "Permission_View")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string View { get; set; }

        [Display(Name = "Permission_Action")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [ChoiceList(new object[] { Constants.Read, Constants.Update, "Delete", "IsActive", "IsDeprecated", "UpdateState", "ResendInvitationEmail", "PostingState", "All" },
            new string[] { "Permission_Read", "Permission_Update", "Permission_Delete", "Permission_IsActive", "Permission_IsDeprecated", "Permission_UpdateState", "ResendInvitationEmail", "Permission_PostingState", "View_All" })]
        [AlwaysAccessible]
        public string Action { get; set; }

        [Display(Name = "Permission_Criteria")]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Criteria { get; set; }

        [Display(Name = "Permission_Mask")]
        [StringLength(2048, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Mask { get; set; }

        [Display(Name = "Memo")]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
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
