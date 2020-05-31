using System;
using System.ComponentModel.DataAnnotations;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "IdentityServerUser", Plural = "IdentityServerUsers")]
    public class IdentityServerUser : EntityWithKey<string>
    {
        [Display(Name = "User_Email")]
        [Required]
        [EmailAddress(ErrorMessage = nameof(EmailAddressAttribute))]
        [StringLength(256)]
        [AlwaysAccessible]
        public string Email { get; set; }

        [Display(Name = "IdentityServerUser_EmailConfirmed")]
        public bool? EmailConfirmed { get; set; }

        [Display(Name = "IdentityServerUser_PasswordSet")]
        public bool? PasswordSet { get; set; }

        [Display(Name = "IdentityServerUser_TwoFactorEnabled")]
        public bool? TwoFactorEnabled { get; set; }

        [Display(Name = "IdentityServerUser_LockoutEnd")]
        public DateTimeOffset? LockoutEnd { get; set; }
    }
}
