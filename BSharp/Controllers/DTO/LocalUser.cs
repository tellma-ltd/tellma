using BSharp.Controllers.Misc;
using BSharp.Services.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using S = System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Controllers.DTO
{
    public class LocalUserForSave<TRoleMembership> : DtoForSaveKeyBase<int?>, IMultilingualName
    {
        [BasicField]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        public string Name { get; set; }

        [BasicField]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        public string Name2 { get; set; }

        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [EmailAddress(ErrorMessage = nameof(EmailAddressAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Display(Name = "User_Email")]
        public string Email { get; set; }

        [NavigationProperty(ForeignKey = nameof(RoleMembershipForQuery.UserId))]
        [Display(Name = "User_Roles")]
        public List<TRoleMembership> Roles { get; set; } = new List<TRoleMembership>();

        [ForeignKey]
        [Display(Name = "User_Agent")]
        public int? AgentId { get; set; }

        [S.NotMapped]
        [Display(Name = "Image")]
        public byte[] Image { get; set; }
    }

    public class LocalUserForSave : LocalUserForSave<RoleMembershipForSave> { }

    public class LocalUser<TRoleMembership> : LocalUserForSave<TRoleMembership>
    {
        public string ExternalId { get; set; }

        public string State { get; set; }

        public string ImageId { get; set; }

        [BasicField]
        [Display(Name = "IsActive")]
        public bool? IsActive { get; set; }

        [Display(Name = "User_LastActivity")]
        public DateTimeOffset? LastAccess { get; set; }

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


    public class LocalUser : LocalUser<RoleMembership> { }

    public class LocalUserForQuery : LocalUser<RoleMembershipForQuery>, IAuditedDto
    {
        [NavigationProperty(ForeignKey = nameof(AgentId))]
        public AgentForQuery Agent { get; set; }

        [NavigationProperty(ForeignKey = nameof(CreatedById))]
        public LocalUserForQuery CreatedBy { get; set; }

        [NavigationProperty(ForeignKey = nameof(ModifiedById))]
        public LocalUserForQuery ModifiedBy { get; set; }
    }


    //////////////////// For Client

    /// <summary>
    /// Represents all user settings that a user can save: TODO
    /// </summary>
    public class UserSettingsForClientForSave
    {
    }

    /// <summary>
    /// Represents all user settings in a particular tenant
    /// </summary>
    public class UserSettingsForClient : UserSettingsForClientForSave
    {
        public int? UserId { get; set; }

        public string ImageId { get; set; }

        public string Name { get; set; }

        public string Name2 { get; set; }

        public Dictionary<string, string> CustomSettings { get; set; }
    }
}
