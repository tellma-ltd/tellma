using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Model.Admin;
using Tellma.Repository.Common;
using Xunit;

namespace Tellma.Repository.Admin.Tests
{
    [Collection(nameof(AdminRepositoryCollection))]
    public class AdminUsers__Save : IClassFixture<AdminRepositoryFixture>
    {
        private readonly AdminRepository _repo;
        private readonly int _userId;
        private readonly QueryContext _ctx;

        public AdminUsers__Save(AdminRepositoryFixture fixture)
        {
            _repo = fixture.Repo;
            _userId = fixture.UserId;
            _ctx = new QueryContext(_userId);
        }

        [Fact(DisplayName = "Saving an AdminUser without a Name fails")]
        public async Task MissingName_Fails()
        {
            // Arrange
            var users = new List<AdminUserForSave>
            {
                new AdminUserForSave
                {
                     Id = 0,
                     Email = "test@test.com",
                     Name = null, // Error!
                     Permissions = new List<AdminPermissionForSave>
                     {
                          new AdminPermissionForSave
                          {
                              Id = 0,
                              View = "all",
                              Action = "All",
                              Memo = "Test"
                          }
                     }
                }
            };

            // Act
            using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var result = await _repo.AdminUsers__Save(users, returnIds: true, _userId);

            // Assert
            Assert.True(result.IsError);
            Assert.Collection(result.Errors, error =>
            {
                Assert.Equal("[0].Name", error.Key);
                Assert.Equal("Error_Field0IsRequired", error.ErrorName);
                Assert.Equal("localize:Name", error.Argument1);
            });

            Assert.Empty(result.Ids);
        }

        [Fact(DisplayName = "Saving a valid AdminUser succeeds")]
        public async Task ValidInput_Succeeds()
        {
            // Arrange
            var users = new List<AdminUserForSave>
            {
                new AdminUserForSave
                {
                     Id = 0,
                     Email = "test@test.com",
                     Name = "Test User",
                     Permissions = new List<AdminPermissionForSave>
                     {
                          new AdminPermissionForSave
                          {
                              Id = 0,
                              View = "all",
                              Action = "All",
                              Memo = "Test"
                          }
                     }
                }
            };

            // Act
            using var trx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var result = await _repo.AdminUsers__Save(users, returnIds: true, _userId);

            // Assert
            Assert.False(result.IsError);
            var id = Assert.Single(result.Ids);

            var user = await _repo.AdminUsers
                .Filter($"Id eq {id}")
                .FirstOrDefaultAsync(_ctx, cancellation: default);

            Assert.Equal(users[0].Email, user.Email);
        }
    }
}
