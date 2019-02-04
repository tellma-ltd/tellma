using BSharp.Controllers.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    [CollectionName("LocalUsers")]
    public class LocalUserForSave<TRoleMembership> : DtoForSaveKeyBase<int?>
    {
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        public string Name { get; set; }

        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        public string Name2 { get; set; }

        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [EmailAddress(ErrorMessage = nameof(EmailAddressAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "User_Email")]
        public string Email { get; set; }

        [Display(Name = "Permissions")]
        public List<TRoleMembership> Roles { get; set; }

        [Display(Name = "User_Agent")]
        public int? AgentId { get; set; }
    }

    public class LocalUserForSave : LocalUserForSave<RoleMembershipForSave> { }

    public class LocalUser : LocalUserForSave<RoleMembership>, IAuditedDto
    {
        public string ExternalId { get; set; }

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
