using BSharp.Controllers.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    public class RoleMembershipForSave : DtoForSaveKeyBase<int?>
    {
        [BasicField]
        [ForeignKey]
        [Display(Name = "RoleMembership_User")]
        public int? UserId { get; set; }

      //  [BasicField]
        [ForeignKey]
        [Display(Name = "RoleMembership_Role")]
        public int? RoleId { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "Memo")]
        public string Memo { get; set; }
    }

    public class RoleMembership : RoleMembershipForSave
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
    }

    public class RoleMembershipForQuery : RoleMembership, IAuditedDto
    {
        [NavigationProperty(ForeignKey = nameof(UserId))]
        public LocalUserForQuery User { get; set; }

        [NavigationProperty(ForeignKey = nameof(RoleId))]
        public RoleForQuery Role { get; set; }

        [NavigationProperty(ForeignKey = nameof(CreatedById))]
        public LocalUserForQuery CreatedBy { get; set; }

        [NavigationProperty(ForeignKey = nameof(ModifiedById))]
        public LocalUserForQuery ModifiedBy { get; set; }
    }
}
