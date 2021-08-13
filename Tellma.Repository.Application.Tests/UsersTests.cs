using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Model.Application;
using Tellma.Utilities.Calendars;
using Xunit;

namespace Tellma.Repository.Application.Tests
{
    [Collection(nameof(ApplicationRepositoryCollection))]
    public class UsersTests : TestsBase, IClassFixture<ApplicationRepositoryFixture>
    {
        private const int SarahUserId = 1001;
        private const int LouayUserId = 1002;
        private const int LucyUserId = 1003;
        
        #region Lifecycle

        public UsersTests(ApplicationRepositoryFixture fixture) : base(fixture)
        {
        }

        #endregion

        public static UserForSave Chloe() => new()
        {
            Id = 0,
            Email = "chloe@tellma.com",
            Name = "Chloe",
            Name2 = "كلوي",
            PreferredLanguage = "en",
            PreferredCalendar = Calendars.Gregorian,
            ContactEmail = "chloe@tellma.com",
            ContactMobile = "+1-202-555-0115",
            NormalizedContactMobile = "+12025550115",
            EmailNewInboxItem = true,
            SmsNewInboxItem = false,
            PushNewInboxItem = false,
            PreferredChannel = "Email",
            Roles = new List<RoleMembershipForSave>
                 {
                     new RoleMembershipForSave
                     {
                          Id = 0,
                          RoleId = Declarations.AdminRoleId,
                          Memo = "Testing 1",
                     }
                 }
        };

        public static UserForSave Jordan() => new()
        {
            Id = 0,
            Email = "jordan@tellma.com",
            Name = "Jordan",
            Name2 = "جوردان",
            PreferredLanguage = "en",
            PreferredCalendar = Calendars.Gregorian,
            ContactEmail = "jordan@tellma.com",
            ContactMobile = "+1-202-555-0173",
            NormalizedContactMobile = "+12025550173",
            EmailNewInboxItem = false,
            SmsNewInboxItem = true,
            PushNewInboxItem = false,
            PreferredChannel = "Sms",
            Roles = new List<RoleMembershipForSave>
                 {
                     new RoleMembershipForSave
                     {
                          Id = 0,
                          RoleId = Declarations.AdminRoleId,
                          Memo = "Testing 2",
                     }
                 }
        };

        [Fact(DisplayName = "Saving two Users with the same email fails")]
        public async Task SavingDuplicateEmailsFails()
        {
            var chloe = Chloe();
            var jordan = Jordan();

            jordan.Email = chloe.Email; // Error!

            // Arrange
            var users = new List<UserForSave> { chloe, jordan };

            // Act
            using var trx = TransactionFactory.ReadCommitted();
            var result = await Repo.Users__Save(users, returnIds: true, validateOnly: false, top: Top, userId: UserId);

            // Assert
            Assert.True(result.IsError);
            Assert.Collection(result.Errors,
                error =>
                {
                    Assert.Equal("[0].Email", error.Key);
                    Assert.Equal("Error_TheEmail0IsDuplicated", error.ErrorName);
                    Assert.Equal(chloe.Email.ToLower(), error.Argument1.ToLower());
                },
                error =>
                {
                    Assert.Equal("[1].Email", error.Key);
                    Assert.Equal("Error_TheEmail0IsDuplicated", error.ErrorName);
                    Assert.Equal(chloe.Email.ToLower(), error.Argument1.ToLower());
                }
            );
        }

        [Fact(DisplayName = "Saving a User with an existing email fails")]
        public async Task SavingExistingEmailFails()
        {
            var jordan = Jordan();

            jordan.Email = Declarations.AdminEmail; // Error!

            // Arrange
            var users = new List<UserForSave> { jordan };

            // Act
            using var trx = TransactionFactory.ReadCommitted();
            var result = await Repo.Users__Save(users, returnIds: true, validateOnly: false, top: Top, userId: UserId);

            // Assert
            Assert.True(result.IsError);
            Assert.Collection(result.Errors,
                error =>
                {
                    Assert.Equal("[0].Email", error.Key);
                    Assert.Equal("Error_TheEmail0IsUsed", error.ErrorName);
                    Assert.Equal(jordan.Email.ToLower(), error.Argument1.ToLower());
                }
            );
        }

        [Fact(DisplayName = "Saving a valid User succeeds")]
        public async Task SavingSucceeds()
        {
            // Arrange
            var user = Jordan();
            var users = new List<UserForSave> { user };

            // Act
            using var trx = TransactionFactory.ReadCommitted();
            var result = await Repo.Users__Save(users, returnIds: true, validateOnly: false, top: Top, userId: UserId);

            // Assert
            Assert.False(result.IsError);
            var id = Assert.Single(result.Ids);

            var dbUser = await Repo.Users
                .Filter($"Id eq {id}")
                .Expand(nameof(User.Roles))
                .FirstOrDefaultAsync(Context);

            Assert.Equal(user.Email, dbUser.Email);
            Assert.Equal(user.Name, dbUser.Name);
            Assert.Equal(user.Name2, dbUser.Name2);
            Assert.Equal(user.Name3, dbUser.Name3);
            Assert.Equal(user.PreferredLanguage, dbUser.PreferredLanguage);
            Assert.Equal(user.PreferredCalendar, dbUser.PreferredCalendar);
            Assert.Equal(user.ContactEmail, dbUser.ContactEmail);
            Assert.Equal(user.ContactMobile, dbUser.ContactMobile);
            Assert.Equal(user.NormalizedContactMobile, dbUser.NormalizedContactMobile);
            Assert.Equal(user.EmailNewInboxItem, dbUser.EmailNewInboxItem);
            Assert.Equal(user.SmsNewInboxItem, dbUser.SmsNewInboxItem);
            Assert.Equal(user.PushNewInboxItem, dbUser.PushNewInboxItem);
            Assert.Equal(user.PreferredChannel, dbUser.PreferredChannel);
            Assert.NotNull(dbUser.CreatedAt);
            Assert.Equal(UserId, dbUser.CreatedById);
            Assert.NotNull(dbUser.ModifiedAt);
            Assert.Equal(UserId, dbUser.ModifiedById);
            Assert.Equal((byte)0, dbUser.State);
            Assert.Collection(dbUser.Roles, dbRole =>
            {
                var role = user.Roles[0];

                Assert.Equal(id, dbRole.UserId);
                Assert.Equal(role.RoleId, dbRole.RoleId);
                Assert.Equal(role.Memo, dbRole.Memo);
            });
        }

        [Fact(DisplayName = "Deleting an existing User succeeds")]
        public async Task DeletionSucceeds()
        {
            // Arrange
            using var trx = TransactionFactory.ReadCommitted();
            var idsToDelete = new List<int> { Declarations.PeterUserId };

            // Act
            var (result, emails) = await Repo.Users__Delete(idsToDelete, validateOnly: false, top: Top, userId: UserId);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Errors);

            var email = Assert.Single(emails);
            Assert.Equal("peter@tellma.com", email);

            var afterUser = await Repo.Users
                .FilterByIds(idsToDelete)
                .FirstOrDefaultAsync(Context);
            Assert.Null(afterUser);
        }

        [Fact(DisplayName = "Deactivating an active User succeeds")]
        public async Task DeactivationSucceeds()
        {
            // Arrange
            using var trx = TransactionFactory.ReadCommitted();
            var ids = new List<int> { Declarations.PeterUserId };

            // Act
            bool isActive = false;
            var result = await Repo.Users__Activate(ids, isActive, validateOnly: false, top: Top, userId: UserId);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Errors);

            var afterUser = await Repo.Users.FilterByIds(ids).FirstOrDefaultAsync(Context);
            Assert.False(afterUser.IsActive);
        }

        [Fact(DisplayName = "Activating an inactive User succeeds")]
        public async Task ActivationSucceeds()
        {
            // Arrange
            using var trx = TransactionFactory.ReadCommitted();
            var ids = new List<int> { SarahUserId }; // Inactive user

            // Act
            bool isActive = true;
            var result = await Repo.Users__Activate(ids, isActive, validateOnly: false, top: Top, userId: UserId);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Errors);

            var afterUser = await Repo.Users.FilterByIds(ids).FirstOrDefaultAsync(Context);
            Assert.True(afterUser.IsActive);
        }

        [Fact(DisplayName = "Inviting a User who is already a member fails")]
        public async Task InvitingMemberFails()
        {
            // Arrange
            using var trx = TransactionFactory.ReadCommitted();
            var ids = new List<int> { Declarations.AdminId }; // Already member

            // Act
            var (result, users) = await Repo.Users__Invite(ids, validateOnly: false, top: Top, userId: UserId);

            // Assert
            Assert.True(result.IsError);
            Assert.Collection(result.Errors,
                error =>
                {
                    Assert.Equal("[0]", error.Key);
                    Assert.Equal("Error_ThisUserIsAlreadyAMember", error.ErrorName);
                }
            );
        }

        [Fact(DisplayName = "Inviting an new User succeeds")]
        public async Task InvitationSucceeds()
        {
            // Arrange
            using var trx = TransactionFactory.ReadCommitted();
            var id = LucyUserId; // New user
            var ids = new List<int> { id };

            // Act
            var (result, users) = await Repo.Users__Invite(ids, validateOnly: false, top: Top, userId: UserId);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Errors);

            var user = Assert.Single(users);
            Assert.Equal(id, user.Id);
            Assert.Equal("Lucy", user.Name);
            Assert.Equal("لوسي", user.Name2);
            Assert.Equal("lucy@tellma.com", user.Email);

            var afterUser = await Repo.Users.FilterByIds(ids).FirstOrDefaultAsync(Context);
            Assert.NotNull(afterUser.InvitedAt);
            Assert.Equal((byte)1, afterUser.State);
        }

        [Fact(DisplayName = "Setting a User's external id succeeds")]
        public async Task SettingExternalIdSucceeds()
        {
            // Arrange
            using var trx = TransactionFactory.ReadCommitted();
            var id = LouayUserId;
            string externalId = Guid.NewGuid().ToString("D");

            // Act
            await Repo.Users__SetExternalIdByUserId(id, externalId);

            // Assert
            User afterUser = await Repo.Users.Filter($"Id eq {id}").FirstOrDefaultAsync(Context);
            Assert.Equal(externalId, afterUser.ExternalId);
            Assert.Equal((byte)2, afterUser.State);
        }

        [Fact(DisplayName = "Setting a User's email succeeds")]
        public async Task SettingEmailSucceeds()
        {
            // Arrange
            using var trx = TransactionFactory.ReadCommitted();
            var id = Declarations.PeterUserId;
            string email = "different@tellma.com";

            // Act
            await Repo.Users__SetEmailByUserId(id, email);

            // Assert
            User afterUser = await Repo.Users.Filter($"Id eq {id}").FirstOrDefaultAsync(Context);
            Assert.Equal(email, afterUser.Email);
        }

        [Fact(DisplayName = "Setting a User's preferred language succeeds")]
        public async Task SettingPreferredLanguageSucceeds()
        {
            // Arrange
            using var trx = TransactionFactory.ReadCommitted();
            var id = Declarations.PeterUserId;
            string lang = "ar";

            // Act
            await Repo.Users__SavePreferredLanguage(lang, id, cancellation: default);

            // Assert
            User afterUser = await Repo.Users.Filter($"Id eq {id}").FirstOrDefaultAsync(Context);
            Assert.Equal(lang, afterUser.PreferredLanguage);
        }

        [Fact(DisplayName = "Setting a User's preferred calendar succeeds")]
        public async Task SettingPreferredCalendarSucceeds()
        {
            // Arrange
            using var trx = TransactionFactory.ReadCommitted();
            var id = Declarations.PeterUserId;
            string calendar = Calendars.Ethiopian;

            // Act
            await Repo.Users__SavePreferredCalendar(calendar, id, cancellation: default);

            // Assert
            User afterUser = await Repo.Users.Filter($"Id eq {id}").FirstOrDefaultAsync(Context);
            Assert.Equal(calendar, afterUser.PreferredCalendar);
        }

        [Fact(DisplayName = "Saving User settings succeeds")]
        public async Task SavingSettingsSucceeds()
        {
            // Arrange
            using var trx = TransactionFactory.ReadCommitted();
            var id = Declarations.AdminId;
            const string key = "Resources/select";
            const string value = "Name,Name2,Code";

            // Act
            await Repo.Users__SaveSettings(key, value, id);

            // Assert
            var settings = await Repo.UserSettings__Load(id, cancellation: default);
            var (dbKey, dbValue) = Assert.Single(settings.CustomSettings);

            Assert.Equal(key, dbKey);
            Assert.Equal(value, dbValue);
        }
    }
}
