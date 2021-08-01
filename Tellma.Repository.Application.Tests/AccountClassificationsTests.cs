using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Model.Application;
using Tellma.Repository.Common;
using Xunit;

namespace Tellma.Repository.Application.Tests
{
    [Collection(nameof(ApplicationRepositoryCollection))]
    public class AccountClassificationsTests : IClassFixture<ApplicationRepositoryFixture>
    {
        #region Lifecycle

        private readonly ApplicationRepository _repo;
        private readonly int _userId;
        private readonly QueryContext _ctx;

        public AccountClassificationsTests(ApplicationRepositoryFixture fixture)
        {
            _repo = fixture.Repo;
            _userId = fixture.UserId;
            _ctx = new QueryContext(_userId);
        }

        public static AccountClassificationForSave NonCurrentAssets()
        {
            return new AccountClassificationForSave
            {
                Id = 0,
                Code = "102200",
                Name = "Non-Current Assets",
                Name2 = "أصول غير متداولة",
            };
        }

        public static AccountClassificationForSave CurrentInventories()
        {
            return new AccountClassificationForSave
            {
                Id = 0,
                Code = "102201",
                Name = "Current Inventories",
                Name2 = "مخزون متداول",
            };
        }

        #endregion

        [Fact(DisplayName = "Saving two AccountClassifications with the same code fails")]
        public async Task SavingDuplicateCodesFails()
        {
            var nca = NonCurrentAssets();
            var ci = CurrentInventories();

            ci.Code = "102200"; // Error!

            // Arrange
            var entity = new List<AccountClassificationForSave> { nca, ci };

            // Act
            using var trx = Transactions.ReadCommitted();
            var result = await _repo.AccountClassifications__Save(entity, returnIds: true, validateOnly: false, top: int.MaxValue, _userId);

            // Assert
            Assert.True(result.IsError);
            Assert.Collection(result.Errors,
                error =>
                {
                    Assert.Equal("[0].Code", error.Key);
                    Assert.Equal("Error_TheCode0IsDuplicated", error.ErrorName);
                    Assert.Equal("102200", error.Argument1.ToLower());
                },
                error =>
                {
                    Assert.Equal("[1].Code", error.Key);
                    Assert.Equal("Error_TheCode0IsDuplicated", error.ErrorName);
                    Assert.Equal("102200", error.Argument1.ToLower());
                }
            );

            Assert.Empty(result.Ids);
        }

        [Fact(DisplayName = "Saving a valid AccountClassification succeeds")]
        public async Task SavingValidAccountClassificationSucceeds()
        {
            // Arrange
            var nca = NonCurrentAssets();
            var ci = CurrentInventories();
            ci.ParentIndex = 0;

            var entities = new List<AccountClassificationForSave> { nca, ci };

            // Act
            using var trx = Transactions.ReadCommitted();
            var result = await _repo.AccountClassifications__Save(entities, returnIds: true, validateOnly: false, top: int.MaxValue, _userId);

            // Assert
            Assert.False(result.IsError);

            var dbAccountClassifications = await _repo.AccountClassifications
                .FilterByIds(result.Ids)
                .ToListAsync(_ctx);

            var dbNca = dbAccountClassifications.FirstOrDefault(e => e.Code == nca.Code);
            Assert.NotNull(dbNca);
            Assert.Equal(nca.Name, dbNca.Name);
            Assert.Equal(nca.Name2, dbNca.Name2);
            Assert.Equal(nca.Name3, dbNca.Name3);

            var dbCi = dbAccountClassifications.FirstOrDefault(e => e.Code == ci.Code);
            Assert.NotNull(dbCi);
            Assert.Equal(ci.Name, dbCi.Name);
            Assert.Equal(ci.Name2, dbCi.Name2);
            Assert.Equal(ci.Name3, dbCi.Name3);

            // Parent Id was set correctly
            Assert.Equal(dbNca.Id, dbCi.ParentId);
        }
    }
}
