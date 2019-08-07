using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.EntityModel
{
    [StrongEntity]
    public class UserForSave<TRoleMembership> : EntityWithKey<int>
    {
        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Name2 { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Name3 { get; set; }

        [Display(Name = "User_Email")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [EmailAddress(ErrorMessage = nameof(EmailAddressAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        public string Email { get; set; }

        [Display(Name = "User_Roles")]
        [ForeignKey(nameof(RoleMembership.UserId))]
        public List<TRoleMembership> Roles { get; set; }

        //[Display(Name = "User_Agent")]
        //public int? AgentId { get; set; }

        [NotMapped]
        [Display(Name = "Image")]
        public byte[] Image { get; set; }
    }

    public class UserForSave : UserForSave<RoleMembershipForSave> { }

    public class User : UserForSave<RoleMembership>
    {
        public string ExternalId { get; set; }

        [Display(Name = "State")]
        public string State { get; set; }

        public string ImageId { get; set; }

        [Display(Name = "IsActive")]
        [AlwaysAccessible]
        public bool? IsActive { get; set; }

        [Display(Name = "User_LastActivity")]
        public DateTimeOffset? LastAccess { get; set; }

        [Display(Name = "CreatedAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        public int? ModifiedById { get; set; }

        // For Query

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
