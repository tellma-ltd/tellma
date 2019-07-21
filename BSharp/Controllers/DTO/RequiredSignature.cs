using BSharp.Controllers.Misc;
using BSharp.Services.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{

    public class RequiredSignatureForSave : DtoForSaveKeyBase<int?>
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
        [StringLength(1024, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Permission_Criteria")]
        public string Criteria { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Memo")]
        public string Memo { get; set; }
    }

    public class RequiredSignature : RequiredSignatureForSave, IAuditedDto
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
}
