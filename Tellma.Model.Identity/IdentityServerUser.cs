using System;
using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Model.Identity
{
    [Display(Name = "IdentityServerUser", GroupName = "IdentityServerUsers")]
    public class IdentityServerUser : EntityWithKey<string>
    {
        [Display(Name = "User_Email")]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "IdentityServerUser_EmailConfirmed")]
        [Required]
        public bool? EmailConfirmed { get; set; }

        [Display(Name = "IdentityServerUser_PasswordSet")]
        [Required]
        public bool? PasswordSet { get; set; }

        [Display(Name = "IdentityServerUser_TwoFactorEnabled")]
        [Required]
        public bool? TwoFactorEnabled { get; set; }

        [Display(Name = "IdentityServerUser_LockoutEnd")]
        public DateTimeOffset? LockoutEnd { get; set; }
    }
}
