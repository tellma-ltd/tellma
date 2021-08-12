using System.Collections.Generic;
using System.Threading.Tasks;
using Tellma.Model.Application;
using Xunit;

namespace Tellma.Repository.Application.Tests
{
    [Collection(nameof(ApplicationRepositoryCollection))]
    public class RolesTests : TestsBase, IClassFixture<ApplicationRepositoryFixture>
    {
        private const int ConsultantRoleId = 1001;

        #region Lifecycle

        public RolesTests(ApplicationRepositoryFixture fixture) : base(fixture)
        {
        }

        #endregion

        public static RoleForSave Accountant() => new()
        {
            Id = 0,
            Name = "Accountant",
            Name2 = "محاسب",
            Code = "Accountant",
            IsPublic = false,
            Members = new List<RoleMembershipForSave>
            {
                new RoleMembershipForSave
                {
                     Id = 0,
                     UserId = Declarations.PeterUserId,
                     Memo = "Test 1"
                }
            },
            Permissions = new List<PermissionForSave>
            {
                new PermissionForSave
                {
                    Id = 0,
                    View = "centers",
                    Action = "All",
                    Criteria = "Code descof '5'",
                    Memo = "Test 2",
                }
            }
        };

        public static RoleForSave Auditor() => new()
        {
            Id = 0,
            Name = "Auditor",
            Name2 = "مراجع",
            Code = "Auditor",
            IsPublic = false,
            Members = new List<RoleMembershipForSave>
            {
            },
            Permissions = new List<PermissionForSave>
            {
                new PermissionForSave
                {
                    Id = 0,
                    View = "centers",
                    Action = "Read",
                    Criteria = "Code descof '5'",
                    Memo = "Test 3",
                },
                new PermissionForSave
                {
                    Id = 0,
                    View = "accounts",
                    Action = "Read",
                    Criteria = null,
                    Memo = "Test 4",
                }
            }
        };

        [Fact(DisplayName = "Saving two Roles with the same code fails")]
        public async Task SavingDuplicateEmailsFails()
        {
            var accountant = Accountant();
            var auditor = Auditor();

            accountant.Code = auditor.Code; // Error!

            // Arrange
            var roles = new List<RoleForSave> { accountant, auditor };

            // Act
            using var trx = TransactionFactory.ReadCommitted();
            var result = await Repo.Roles__Save(roles, returnIds: true, validateOnly: false, top: Top, userId: UserId);

            // Assert
            Assert.True(result.IsError);
            Assert.Collection(result.Errors,
                error =>
                {
                    Assert.Equal("[0].Code", error.Key);
                    Assert.Equal("Error_TheCode0IsDuplicated", error.ErrorName);
                    Assert.Equal(accountant.Code.ToLower(), error.Argument1.ToLower());
                },
                error =>
                {
                    Assert.Equal("[1].Code", error.Key);
                    Assert.Equal("Error_TheCode0IsDuplicated", error.ErrorName);
                    Assert.Equal(auditor.Code.ToLower(), error.Argument1.ToLower());
                }
            );
        }

        [Fact(DisplayName = "Saving a Role with an existing code fails")]
        public async Task SavingExistingEmailFails()
        {
            var accountant = Accountant();

            accountant.Code = "Reader"; // Already exists!

            // Arrange
            var roles = new List<RoleForSave> { accountant };

            // Act
            using var trx = TransactionFactory.ReadCommitted();
            var result = await Repo.Roles__Save(roles, returnIds: true, validateOnly: false, top: Top, userId: UserId);

            // Assert
            Assert.True(result.IsError);
            Assert.Collection(result.Errors,
                error =>
                {
                    Assert.Equal("[0].Code", error.Key);
                    Assert.Equal("Error_TheCode0IsUsed", error.ErrorName);
                    Assert.Equal(accountant.Code, error.Argument1);
                }
            );
        }

        [Fact(DisplayName = "Saving a valid Role succeeds")]
        public async Task SavingSucceeds()
        {
            // Arrange
            var role = Accountant();
            var roles = new List<RoleForSave> { role };

            // Act
            using var trx = TransactionFactory.ReadCommitted();
            var result = await Repo.Roles__Save(roles, returnIds: true, validateOnly: false, top: Top, userId: UserId);

            // Assert
            Assert.False(result.IsError);
            var id = Assert.Single(result.Ids);

            var dbRole = await Repo.Roles
                .Filter($"Id eq {id}")
                .Expand($"{nameof(Role.Members)},{nameof(Role.Permissions)}")
                .FirstOrDefaultAsync(Context);

            Assert.Equal(role.Code, dbRole.Code);
            Assert.Equal(role.Name, dbRole.Name);
            Assert.Equal(role.Name2, dbRole.Name2);
            Assert.Equal(role.Name3, dbRole.Name3);
            Assert.Equal(role.IsPublic, dbRole.IsPublic);
            Assert.NotNull(dbRole.SavedAt);
            Assert.Equal(UserId, dbRole.SavedById);

            Assert.Collection(dbRole.Members, dbMembership =>
            {
                var membership = role.Members[0];

                Assert.Equal(id, dbMembership.RoleId);
                Assert.Equal(membership.UserId, dbMembership.UserId);
                Assert.Equal(membership.Memo, dbMembership.Memo);
            });

            Assert.Collection(dbRole.Permissions, dbPerm =>
            {
                var perm = role.Permissions[0];

                Assert.Equal(id, dbPerm.RoleId);
                Assert.Equal(perm.View, dbPerm.View);
                Assert.Equal(perm.Action, dbPerm.Action);
                Assert.Equal(perm.Criteria, dbPerm.Criteria);
                Assert.Equal(perm.Memo, dbPerm.Memo);
            });
        }

        [Fact(DisplayName = "Deleting an existing Role succeeds")]
        public async Task DeletionSucceeds()
        {
            // Arrange
            using var trx = TransactionFactory.ReadCommitted();
            var idsToDelete = new List<int> { Declarations.ReaderRoleId };

            // Act
            var result = await Repo.Roles__Delete(idsToDelete, validateOnly: false, top: Top, userId: UserId);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Errors);

            var afterRole = await Repo.Roles
                .FilterByIds(idsToDelete)
                .FirstOrDefaultAsync(Context);
            Assert.Null(afterRole);
        }

        [Fact(DisplayName = "Deactivating an active Role succeeds")]
        public async Task DeactivationSucceeds()
        {
            // Arrange
            using var trx = TransactionFactory.ReadCommitted();
            var ids = new List<int> { Declarations.ReaderRoleId };

            // Act
            bool isActive = false;
            var result = await Repo.Roles__Activate(ids, isActive, validateOnly: false, top: Top, userId: UserId);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Errors);

            var afterRole = await Repo.Roles.FilterByIds(ids).FirstOrDefaultAsync(Context);
            Assert.False(afterRole.IsActive);
        }

        [Fact(DisplayName = "Activating an inactive Role succeeds")]
        public async Task ActivationSucceeds()
        {
            // Arrange
            using var trx = TransactionFactory.ReadCommitted();
            var ids = new List<int> { ConsultantRoleId }; // Inactive role

            // Act
            bool isActive = true;
            var result = await Repo.Roles__Activate(ids, isActive, validateOnly: false, top: Top, userId: UserId);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Errors);

            var afterRole = await Repo.Roles.FilterByIds(ids).FirstOrDefaultAsync(Context);
            Assert.True(afterRole.IsActive);
        }
    }
}
