using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tellma.Model.Common;

namespace Tellma.Model.Application
{
    [Display(Name = "User", GroupName = "Users")]
    public class UserForSave<TRoleMembership> : EntityWithKey<int>, IEntityWithImage where TRoleMembership: RoleMembershipForSave
    {
        [NotMapped]
        [Display(Name = "Image")]
        public byte[] Image { get; set; }

        [Display(Name = "Name")]
        [Required, ValidateRequired]
        [StringLength(255)]
        public string Name { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name2 { get; set; }

        [Display(Name = "Name")]
        [StringLength(255)]
        public string Name3 { get; set; }

        [Display(Name = "User_Email")]
        [Required, ValidateRequired]
        [EmailAddress]
        [StringLength(255)]
        [UserKey]
        public string Email { get; set; }

        [Display(Name = "User_PreferredLanguage")]
        [StringLength(2)]
        [CultureChoiceList]
        public string PreferredLanguage { get; set; }

        [Display(Name = "User_PreferredCalendar")]
        [StringLength(2)]
        public string PreferredCalendar { get; set; }        

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
        [ChoiceList(new object[] { 
                "Email", 
                "Sms", 
                "Push" }, 
            new string[] { 
                "User_PreferredChannel_Email", 
                "User_PreferredChannel_Sms", 
                "User_PreferredChannel_Push" 
            })]
        public string PreferredChannel { get; set; }

        [Display(Name = "User_EmailNewInboxItem")]
        [Required]
        public bool? EmailNewInboxItem { get; set; }

        [Display(Name = "User_SmsNewInboxItem")]
        [Required]
        public bool? SmsNewInboxItem { get; set; }

        [Display(Name = "User_PushNewInboxItem")]
        [Required]
        public bool? PushNewInboxItem { get; set; }

        [Display(Name = "User_Roles")]
        [ForeignKey(nameof(RoleMembership.UserId))]
        public List<TRoleMembership> Roles { get; set; }
    }

    public class UserForSave : UserForSave<RoleMembershipForSave> 
    { 
    }

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

        [Display(Name = "User_InvitedAt")]
        public DateTimeOffset? InvitedAt { get; set; }

        [Display(Name = "State")]
        [ChoiceList(new object[] {
                (byte)0,
                (byte)1,
                (byte)2 }, 
            new string[] {
                "User_New",
                "User_Invited",
                "User_Member" 
            })]
        public byte? State { get; set; }

        [Display(Name = "User_LastActivity")]
        public DateTimeOffset? LastAccess { get; set; }

        [Display(Name = "IsActive")]
        [Required]
        public bool? IsActive { get; set; }

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
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
