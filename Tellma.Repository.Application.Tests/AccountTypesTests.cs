//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Transactions;
//using Tellma.Model.Application;
//using Tellma.Repository.Common;
//using Xunit;

//namespace Tellma.Repository.Application.Tests
//{
//    [Collection(nameof(ApplicationRepositoryCollection))]
//    public class AccountTypesTests : IClassFixture<ApplicationRepositoryFixture>
//    {
//        #region Lifecycle

//        private readonly ApplicationRepository _repo;
//        private readonly int _userId;
//        private readonly QueryContext _ctx;

//        public AccountTypesTests(ApplicationRepositoryFixture fixture)
//        {
//            _repo = fixture.Repo;
//            _userId = fixture.UserId;
//            _ctx = new QueryContext(_userId);
//        }

//        public static AccountTypeForSave NonCurrentAssets()
//        {
//            return new AccountTypeForSave
//            {
//                Id = 0,
//                Code = "102200",
//                Name = "Non-Current Assets 2",
//                Name2 = "أصول غير متداولة 2",
//                IsMonetary = true,
//                IsAssignable = true,
//                StandardAndPure = false,
//                Concept = "NonCurrentAssets2"
//            };
//        }

//        public static AccountTypeForSave CurrentInventories()
//        {
//            return new AccountTypeForSave
//            {
//                Id = 0,
//                Code = "102201",
//                Name = "Current Inventories 2",
//                Name2 = "مخزون متداول 2",
//                IsMonetary = true,
//                IsAssignable = true,
//                StandardAndPure = false,
//                Concept = "CurrentInventories2"
//            };
//        }

//        #endregion

//        [Fact(DisplayName = "Saving two AccountTypes with the same code fails")]
//        public async Task SavingDuplicateCodesFails()
//        {
//            var nca = NonCurrentAssets();
//            var ci = CurrentInventories();

//            ci.Code = "102200"; // Error!

//            // Arrange
//            var entity = new List<AccountTypeForSave> { nca, ci };

//            // Act
//            using var trx = TransactionFactory.ReadCommitted();
//            var result = await _repo.AccountTypes__Save(entity, returnIds: true, validateOnly: false, top: int.MaxValue, _userId);

//            // Assert
//            Assert.True(result.IsError);
//            Assert.Collection(result.Errors,
//                error =>
//                {
//                    Assert.Equal("[0].Code", error.Key);
//                    Assert.Equal("Error_TheCode0IsDuplicated", error.ErrorName);
//                    Assert.Equal("102200", error.Argument1.ToLower());
//                },
//                error =>
//                {
//                    Assert.Equal("[1].Code", error.Key);
//                    Assert.Equal("Error_TheCode0IsDuplicated", error.ErrorName);
//                    Assert.Equal("102200", error.Argument1.ToLower());
//                }
//            );

//            Assert.Empty(result.Ids);
//        }

//        [Fact(DisplayName = "Saving a valid AccountType succeeds")]
//        public async Task SavingValidAccountTypeSucceeds()
//        {
//            // Arrange
//            var nca = NonCurrentAssets();
//            var ci = CurrentInventories();
//            ci.ParentIndex = 0;

//            var entities = new List<AccountTypeForSave> { nca, ci };

//            // Act
//            using var trx = TransactionFactory.ReadCommitted();
//            var result = await _repo.AccountTypes__Save(entities, returnIds: true, validateOnly: false, top: int.MaxValue, _userId);

//            // Assert
//            Assert.False(result.IsError);

//            var dbAccountTypes = await _repo.AccountTypes
//                .FilterByIds(result.Ids)
//                .ToListAsync(_ctx);

//            var dbNca = dbAccountTypes.FirstOrDefault(e => e.Code == nca.Code);
//            Assert.NotNull(dbNca);
//            Assert.Equal(nca.Name, dbNca.Name);
//            Assert.Equal(nca.Name2, dbNca.Name2);
//            Assert.Equal(nca.Name3, dbNca.Name3);

//            var dbCi = dbAccountTypes.FirstOrDefault(e => e.Code == ci.Code);
//            Assert.NotNull(dbCi);
//            Assert.Equal(ci.Name, dbCi.Name);
//            Assert.Equal(ci.Name2, dbCi.Name2);
//            Assert.Equal(ci.Name3, dbCi.Name3);

//            // Parent Id was set correctly
//            Assert.Equal(dbNca.Id, dbCi.ParentId);
//        }
//    }
//}
