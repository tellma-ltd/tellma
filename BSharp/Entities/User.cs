using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSharp.Entities
{
    [StrongEntity]
    public class UserForSave<TRoleMembership> : EntityWithKey<int>, IValidatableObject
    {
        [Display(Name = "User_Email")]
        [Required(ErrorMessage = nameof(RequiredAttribute))]
        [EmailAddress(ErrorMessage = nameof(EmailAddressAttribute))]
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [AlwaysAccessible]
        public string Email { get; set; }

        [Display(Name = "User_Roles")]
        [ForeignKey(nameof(RoleMembership.AgentId))]
        public List<TRoleMembership> Roles { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            base.Validate(validationContext);

            if (Id == 0)
            {
                // User is in a 0..1-1 relationship with Agents. So the User's Id is required
                var localizer = validationContext.GetRequiredService<IStringLocalizer<Strings>>();
                var errorMessage = localizer["RequiredAttribute", localizer["User_Agent"]];
                var memberNames = new string[] { nameof(Id) };

                yield return new ValidationResult(errorMessage, memberNames);
            }
        }
    }

    public class UserForSave : UserForSave<RoleMembershipForSave> { }

    public class User : UserForSave<RoleMembership>
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

        public string ImageId { get; set; }

        public string ExternalId { get; set; }

        [Display(Name = "State")]
        public string State { get; set; }

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

        [Display(Name = "User_Agent")]
        [ForeignKey(nameof(Id))]
        public Agent Agent { get; set; }

        [Display(Name = "CreatedBy")]
        [ForeignKey(nameof(CreatedById))]
        public User CreatedBy { get; set; }

        [Display(Name = "ModifiedBy")]
        [ForeignKey(nameof(ModifiedById))]
        public User ModifiedBy { get; set; }
    }
}
