using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "Permission", GroupName = "Permissions")]
    public class PermissionForSave : EntityWithKey<int>
    {
        [Display(Name = "Permission_View")]
        [Required]
        [StringLength(255)]
        public string View { get; set; }

        [Display(Name = "Permission_Action")]
        [Required]
        [ChoiceList(new object[] { 
                PermissionActions.Read, 
                PermissionActions.Update, 
                PermissionActions.Delete, 
                "IsActive", 
                "ResendInvitationEmail", 
                "State", 
                "All" },
            new string[] { 
                "Permission_Read", 
                "Permission_Update", 
                "Permission_Delete", 
                "Permission_IsActive", 
                "ResendInvitationEmail", 
                "Permission_State", 
                "View_All" })]
        public string Action { get; set; }

        [Display(Name = "Permission_Criteria")]
        [StringLength(1024)]
        public string Criteria { get; set; }

        [Display(Name = "Permission_Mask")]
        [StringLength(2048)]
        public string Mask { get; set; }

        [Display(Name = "Memo")]
        [StringLength(255)]
        public string Memo { get; set; }
    }

    public class Permission : PermissionForSave
    {
        [Display(Name = "Permission_Role")]
        [Required]
        public int? RoleId { get; set; }

        [Display(Name = "ModifiedBy")]
        [Required]
        public int? SavedById { get; set; }

        // For Query

        [Display(Name = "Permission_Role")]
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(SavedById))]
        public User SavedBy { get; set; }
    }
}
