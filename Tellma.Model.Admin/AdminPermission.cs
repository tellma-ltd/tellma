using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Admin
{
    [Display(Name = "Permission", GroupName = "Permissions")]
    public class AdminPermissionForSave : EntityWithKey<int>
    {
        [Display(Name = "Permission_View")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string View { get; set; }

        [Display(Name = "Permission_Action")]
        [Required, ValidateRequired]
        [ChoiceList(new object[] { "Read", "Update", "Delete", "IsActive", "SendInvitationEmail", "All" },
            new string[] { "Permission_Read", "Permission_Update", "Permission_Delete", "Permission_IsActive", "SendInvitationEmail", "View_All" })]
        public string Action { get; set; }

        [Display(Name = "Permission_Criteria")]
        [StringLength(1024)]
        public string Criteria { get; set; }

        [Display(Name = "Memo")]
        [StringLength(255)]
        public string Memo { get; set; }
    }

    public class AdminPermission : AdminPermissionForSave
    {
        [Required]
        public int? AdminUserId { get; set; }

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
