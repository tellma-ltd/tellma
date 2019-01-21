using BSharp.Controllers.Misc;
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
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Permission_View")]
        public string ViewId { get; set; }

        [Display(Name = "Permission_Role")]
        public int? RoleId { get; set; }

        [ChoiceList(new object[] { "Read", "Update", "Create", "ReadAndCreate", "Sign" },  new string[] {
            "Permission_Read", "Permission_Update", "Permission_Create", "Permission_ReadAndCreate", "Permission_Sign" })]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Permission_Level")]
        public string Level { get; set; }

        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Permission_Criteria")]
        public string Criteria { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Memo")]
        public string Memo { get; set; }
    }

    public class Permission : PermissionForSave, IAuditedDto
    {
        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public string CreatedBy { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public string ModifiedBy { get; set; }
    }
}
