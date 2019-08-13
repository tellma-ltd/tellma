using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace BSharp.Services.EmbeddedIdentityServer
{
    /// <summary>
    /// The default implementation <see cref="IdentityErrorDescriber"/> doesn't provide localization, so we override it with our custom implementation
    /// </summary>
    public class LocalizedIdentityErrorDescriptor : IdentityErrorDescriber
    {
        private readonly IStringLocalizer _localizer;

        public LocalizedIdentityErrorDescriptor(IStringLocalizer<Strings> localizer)
        {
            _localizer = localizer;
        }

        public override IdentityError DefaultError() { return new IdentityError { Code = nameof(DefaultError), Description = _localizer[nameof(DefaultError)] }; }
        public override IdentityError PasswordMismatch() { return new IdentityError { Code = nameof(PasswordMismatch), Description = _localizer[nameof(PasswordMismatch)] }; }
        public override IdentityError InvalidToken() { return new IdentityError { Code = nameof(InvalidToken), Description = _localizer[nameof(InvalidToken)] }; }
        public override IdentityError LoginAlreadyAssociated() { return new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = _localizer[nameof(LoginAlreadyAssociated)] }; }
        public override IdentityError InvalidUserName(string userName) { return new IdentityError { Code = nameof(InvalidUserName), Description = _localizer[nameof(InvalidUserName), userName] }; }
        public override IdentityError InvalidEmail(string email) { return new IdentityError { Code = nameof(InvalidEmail), Description = _localizer["Identity_" + nameof(InvalidEmail), email] }; }
        public override IdentityError DuplicateUserName(string userName) { return new IdentityError { Code = nameof(DuplicateUserName), Description = _localizer[nameof(DuplicateUserName), userName] }; }
        public override IdentityError DuplicateEmail(string email) { return new IdentityError { Code = nameof(DuplicateEmail), Description = _localizer[nameof(DuplicateEmail)] }; }
        public override IdentityError InvalidRoleName(string role) { return new IdentityError { Code = nameof(InvalidRoleName), Description = _localizer[nameof(InvalidRoleName), role] }; }
        public override IdentityError DuplicateRoleName(string role) { return new IdentityError { Code = nameof(DuplicateRoleName), Description = _localizer[nameof(DuplicateRoleName), role] }; }
        public override IdentityError UserAlreadyHasPassword() { return new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = _localizer[nameof(UserAlreadyHasPassword)] }; }
        public override IdentityError UserLockoutNotEnabled() { return new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = _localizer[nameof(UserLockoutNotEnabled)] }; }
        public override IdentityError UserAlreadyInRole(string role) { return new IdentityError { Code = nameof(UserAlreadyInRole), Description = _localizer[nameof(UserAlreadyInRole), role] }; }
        public override IdentityError UserNotInRole(string role) { return new IdentityError { Code = nameof(UserNotInRole), Description = _localizer[nameof(UserNotInRole), role] }; }
        public override IdentityError PasswordTooShort(int length) { return new IdentityError { Code = nameof(PasswordTooShort), Description = _localizer[nameof(PasswordTooShort), length] }; }
        public override IdentityError PasswordRequiresNonAlphanumeric() { return new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = _localizer[nameof(PasswordRequiresNonAlphanumeric)] }; }
        public override IdentityError PasswordRequiresDigit() { return new IdentityError { Code = nameof(PasswordRequiresDigit), Description = _localizer[nameof(PasswordRequiresDigit)] }; }
        public override IdentityError PasswordRequiresLower() { return new IdentityError { Code = nameof(PasswordRequiresLower), Description = _localizer[nameof(PasswordRequiresLower)] }; }
        public override IdentityError PasswordRequiresUpper() { return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = _localizer[nameof(PasswordRequiresUpper)] }; }
        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) { return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = _localizer[nameof(PasswordRequiresUpper), uniqueChars] }; }
        public override IdentityError RecoveryCodeRedemptionFailed() { return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = _localizer[nameof(PasswordRequiresUpper)] }; }
    }
}
