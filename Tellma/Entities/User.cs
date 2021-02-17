using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tellma.Entities
{
    [StrongEntity]
    [EntityDisplay(Singular = "User", Plural = "Users")]
    public class UserForSave<TRoleMembership> : EntityWithKey<int>, IEntityWithImage
    {
        [NotMapped]
        [Display(Name = "Image")]
        public byte[] Image { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Primary)]
        [Required]
        [NotNull]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Secondary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name2 { get; set; }

        [MultilingualDisplay(Name = "Name", Language = Language.Ternary)]
        [StringLength(255)]
        [AlwaysAccessible]
        public string Name3 { get; set; }

        [Display(Name = "User_Email")]
        [Required]
        [NotNull]
        [EmailAddress]
        [StringLength(255)]
        [AlwaysAccessible]
        [UserKey]
        public string Email { get; set; }

        [Display(Name = "User_PreferredLanguage")]
        [NotNull]
        [StringLength(2)]
        [CultureChoiceList]
        public string PreferredLanguage { get; set; }

        [Display(Name = "Entity_ContactEmail")]
        [EmailAddress]
        [StringLength(255)]
        public string ContactEmail { get; set; }

        [Display(Name = "Entity_ContactMobile")]
        [Phone]
        [StringLength(50)]
        public string ContactMobile { get; set; }

        [Display(Name = "Entity_NormalizedContactMobile")]
        [Phone]
        [StringLength(50)]
        public string NormalizedContactMobile { get; set; }

        [Display(Name = "User_PreferredChannel")]
        [ChoiceList(new object[] { "Email", "Sms", "Push" }, 
            new string[] { "User_PreferredChannel_Email", "User_PreferredChannel_Sms", "User_PreferredChannel_Push" })]
        public string PreferredChannel { get; set; }

        [Display(Name = "User_EmailNewInboxItem")]
        [NotNull]
        public bool? EmailNewInboxItem { get; set; }

        [Display(Name = "User_SmsNewInboxItem")]
        [NotNull]
        public bool? SmsNewInboxItem { get; set; }

        [Display(Name = "User_PushNewInboxItem")]
        [NotNull]
        public bool? PushNewInboxItem { get; set; }

        [Display(Name = "User_Roles")]
        [ForeignKey(nameof(RoleMembership.UserId))]
        public List<TRoleMembership> Roles { get; set; }
    }

    public class UserForSave : UserForSave<RoleMembershipForSave> { }

    public class User : UserForSave<RoleMembership>
    {
        [NotMapped]
        public string PushEndpoint { get; set; }

        [NotMapped]
        public string PushP256dh { get; set; }

        [NotMapped]
        public string PushAuth { get; set; }

        [Display(Name = "User_PushEnabled")]
        public bool? PushEnabled { get; set; }

        public string ImageId { get; set; }

        public string ExternalId { get; set; }

        [Display(Name = "State")]
        [ChoiceList(new object[] { "Invited", "Member" }, 
            new string[] { "User_Invited", "User_Member" })]
        public string State { get; set; }

        [Display(Name = "User_LastActivity")]
        public DateTimeOffset? LastAccess { get; set; }

        [Display(Name = "IsActive")]
        [NotNull]
        [AlwaysAccessible]
        public bool? IsActive { get; set; }

        [Display(Name = "CreatedAt")]
        [NotNull]
        public DateTimeOffset? CreatedAt { get; set; }

        [Display(Name = "CreatedBy")]
        [NotNull]
        public int? CreatedById { get; set; }

        [Display(Name = "ModifiedAt")]
        [NotNull]
        public DateTimeOffset? ModifiedAt { get; set; }

        [Display(Name = "ModifiedBy")]
        [NotNull]
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
