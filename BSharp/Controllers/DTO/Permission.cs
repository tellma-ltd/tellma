using BSharp.Controllers.Misc;
using BSharp.Services.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.DTO
{
    // Note: The permissions is a semi-weak entity, meaning it does not have its own screen or API
    // Permissions are always retrieved and saved as a child collection of some other strong entity
    // I call it "semi"- weak because it comes associated with more than one entity, therefore to stress
    // its weakness and for type-safety, we have two different DTOs

    public class PermissionForSave : DtoForSaveKeyBase<int?>
    {
        [BasicField]
        [ForeignKey]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Permission_View")]
        public string ViewId { get; set; }

        [BasicField]
        [ForeignKey]
        [Display(Name = "Permission_Role")]
        public int? RoleId { get; set; }

        [BasicField]
        [ChoiceList(new object[] { Constants.Read, Constants.Update, "IsActive", "ResendInvitationEmail", "All" },
            new string[] { "Permission_Read", "Permission_Update", "Permission_IsActive", "ResendInvitationEmail", "View_All" })]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [Display(Name = "Permission_Action")]
        public string Action { get; set; }

        [BasicField]
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Permission_Criteria")]
        public string Criteria { get; set; }

        [BasicField]
        [StringLength(2048, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Permission_Mask")]
        public string Mask { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Memo")]
        public string Memo { get; set; }
    }

    public class Permission : PermissionForSave, IAuditedDto
    {
        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [ForeignKey]
        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }
        
        [ForeignKey]
        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [BasicField]
        [NavigationProperty(ForeignKey = nameof(ViewId))]
        public View View { get; set; }

        [BasicField]
        [NavigationProperty(ForeignKey = nameof(RoleId))]
        public Role Role { get; set; }

        [NavigationProperty(ForeignKey = nameof(CreatedById))]
        public LocalUser CreatedBy { get; set; }

        [NavigationProperty(ForeignKey = nameof(ModifiedById))]
        public LocalUser ModifiedBy { get; set; }
    }


    // The two DTOs below carry permission information to the client so
    // the client can adjust the UI accordingly, the string key in the dictionary
    // represents the ViewId, its mere presence indicates that the user has read access
    // and the 3 boolean values indicate whether the user can create, update or sign
    // the associated view id
    public class PermissionsForClient : Dictionary<string, Dictionary<string, bool>>
    {
    }

    public class ViewPermissionsForClient : Dictionary<string, bool>
    {
        // The mere presence of this ViewPermission means that 
        // the user has read access over the associated viewId

        //public bool? Read { get; set; }
        //public bool? Create { get; set; }
        //public bool? Update { get; set; }
        //public bool? Sign { get; set; }
    }
}
