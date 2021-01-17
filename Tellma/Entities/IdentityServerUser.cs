using System;
using System.ComponentModel.DataAnnotations;

namespace Tellma.Entities
{
    [EntityDisplay(Singular = "IdentityServerUser", Plural = "IdentityServerUsers")]
    public class IdentityServerUser : EntityWithKey<string>
    {
        [Display(Name = "User_Email")]
        [Required]
        [NotNull]
        [EmailAddress]
        [AlwaysAccessible]
        public string Email { get; set; }

        [Display(Name = "IdentityServerUser_EmailConfirmed")]
        [NotNull]
        public bool? EmailConfirmed { get; set; }

        [Display(Name = "IdentityServerUser_PasswordSet")]
        [NotNull]
        public bool? PasswordSet { get; set; }

        [Display(Name = "IdentityServerUser_TwoFactorEnabled")]
        [NotNull]
        public bool? TwoFactorEnabled { get; set; }

        [Display(Name = "IdentityServerUser_LockoutEnd")]
        public DateTimeOffset? LockoutEnd { get; set; }
    }
}
