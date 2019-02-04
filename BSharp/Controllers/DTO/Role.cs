using BSharp.Controllers.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.DTO
{
    [CollectionName("Roles")]
    public class RoleForSave<TPermission> : DtoForSaveKeyBase<int?>
    {
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        public string Name { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        public string Name2 { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Display(Name = "Role_IsPublic")]
        public bool IsPublic { get; set; }

        [Display(Name = "Permissions")]
        public List<TPermission> Permissions { get; set; }
    }

    public class RoleForSave : RoleForSave<PermissionForSave>
    {

    }

    public class Role : RoleForSave<Permission>, IAuditedDto
    {
        [Display(Name = "IsActive")]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }
    }
}
