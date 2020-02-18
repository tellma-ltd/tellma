using System;
using System.ComponentModel.DataAnnotations;

namespace Tellma.Entities
{
    public class IdentityServerUser : EntityWithKey<string>
    {
        [Display(Name = "User_Email")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [EmailAddress(ErrorMessage = nameof(EmailAddressAttribute))]
        [StringLength(256, ErrorMessage = nameof(StringLengthAttribute))]
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
