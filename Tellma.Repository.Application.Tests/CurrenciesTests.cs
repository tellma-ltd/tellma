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
//    public class CurrenciesTests : IClassFixture<ApplicationRepositoryFixture>
//    {
//        #region Lifecycle

//        private readonly ApplicationRepository _repo;
//        private readonly int _userId;
//        private readonly QueryContext _ctx;

//        public CurrenciesTests(ApplicationRepositoryFixture fixture)
//        {
//            _repo = fixture.Repo;
//            _userId = fixture.UserId;
//            _ctx = new QueryContext(_userId);
//        }

//        public static CurrencyForSave WakandanPound()
//        {
//            return new CurrencyForSave
//            {
//                Id = "WAP",
//                Name = "Wakandan Pound",
//                Name2 = "جنيه واكاندي",
//                Description = "Wakandan Pound",
//                Description2 = "جنيه واكاندي",
//                E = 2,
//                NumericCode = 45
//            };
//        }

//        public static CurrencyForSave AtlantianDollar()
//        {
//            return new CurrencyForSave
//            {
//                Id = "ATD",
//                Name = "Atlantian Dollar",
//                Name2 = "دولار أطلطني",
//                Description = "Atlantian Dollar",
//                Description2 = "دولار أطلطني",
//                E = 0,
//                NumericCode = 46
//            };
//        }

//        #endregion

//        [Fact(DisplayName = "Saving two Currencies with the same name fails")]
//        public async Task SavingDuplicateCurrenciesFails()
//        {
//            var wp = WakandanPound();
//            var ad = AtlantianDollar();

//            ad.Name = "Wakandan Pound"; // Error!

//            // Arrange
//            var Currencies = new List<CurrencyForSave> { wp, ad };

//            // Act
//            using var trx = TransactionFactory.ReadCommitted();
//            var result = await _repo.Currencies__Save(Currencies, validateOnly: false, top: int.MaxValue, userId: _userId);

//            // Assert
//            Assert.True(result.IsError);
//            Assert.Collection(result.Errors,
//                error =>
//                {
//                    Assert.Equal("[0].Name", error.Key);
//                    Assert.Equal("Error_TheName0IsDuplicated", error.ErrorName);
//                    Assert.Equal("wakandan pound", error.Argument1.ToLower());
//                },
//                error =>
//                {
//                    Assert.Equal("[1].Name", error.Key);
//                    Assert.Equal("Error_TheName0IsDuplicated", error.ErrorName);
//                    Assert.Equal("wakandan pound", error.Argument1.ToLower());
//                }
//            );
//        }

//        [Fact(DisplayName = "Saving two Currencies with the same Id fails")]
//        public async Task SavingDuplicateDescriptionsFails()
//        {
//            var wp = WakandanPound();
//            var ad = AtlantianDollar();

//            ad.Id = "WAP"; // Error!

//            // Arrange
//            var Currencies = new List<CurrencyForSave> { wp, ad };

//            // Act
//            using var trx = TransactionFactory.ReadCommitted();
//            var result = await _repo.Currencies__Save(Currencies, validateOnly: false, top: int.MaxValue, userId: _userId);

//            // Assert
//            Assert.True(result.IsError);
//            Assert.Collection(result.Errors,
//                error =>
//                {
//                    Assert.Equal("[0].Id", error.Key);
//                    Assert.Equal("Error_TheCode0IsDuplicated", error.ErrorName);
//                    Assert.Equal("wap", error.Argument1.ToLower());
//                },
//                error =>
//                {
//                    Assert.Equal("[1].Id", error.Key);
//                    Assert.Equal("Error_TheCode0IsDuplicated", error.ErrorName);
//                    Assert.Equal("wap", error.Argument1.ToLower());
//                }
//            );
//        }

//        [Fact(DisplayName = "Saving a valid Currency succeeds")]
//        public async Task SavingValidCurrenciesSucceeds()
//        {
//            // Arrange
//            var currency = WakandanPound();
//            var currencies = new List<CurrencyForSave> { currency };

//            // Act
//            using var trx = TransactionFactory.ReadCommitted();
//            var result = await _repo.Currencies__Save(currencies, validateOnly: false, top: int.MaxValue, userId: _userId);

//            // Assert
//            Assert.False(result.IsError);
//            var id = currency.Id;

//            var dbCurrency = await _repo.Currencies
//                .Filter($"Id eq '{id}'")
//                .FirstOrDefaultAsync(_ctx);

//            Assert.Equal(currency.Id, dbCurrency.Id);
//            Assert.Equal(currency.Name, dbCurrency.Name);
//            Assert.Equal(currency.Name2, dbCurrency.Name2);
//            Assert.Equal(currency.Name3, dbCurrency.Name3);
//            Assert.Equal(currency.Description, dbCurrency.Description);
//            Assert.Equal(currency.Description2, dbCurrency.Description2);
//            Assert.Equal(currency.Description3, dbCurrency.Description3);
//            Assert.Equal(currency.E, dbCurrency.E);
//            Assert.Equal(currency.NumericCode, dbCurrency.NumericCode);
//        }

//        [Fact(DisplayName = "Deleting an existing Currency succeeds")]
//        public async Task DeletingCurrenciesSucceeds()
//        {
//            // Arrange
//            using var trx = TransactionFactory.ReadCommitted();
//            var currency = await _repo.Currencies.FirstOrDefaultAsync(_ctx);
//            var currencyId = currency.Id;
//            var idsToDelete = new List<string> { currencyId };

//            // Act
//            var deleteResult = await _repo.Currencies__Delete(idsToDelete, validateOnly: false, top: int.MaxValue, userId: _userId);
//            var afterCurrency = await _repo.Currencies
//                .FilterByIds(idsToDelete)
//                .FirstOrDefaultAsync(_ctx);

//            // Assert
//            Assert.False(deleteResult.IsError);
//            Assert.Empty(deleteResult.Errors);
//            Assert.Null(afterCurrency);
//        }

//        [Fact(DisplayName = "Deactivating an active Currency succeeds")]
//        public async Task DeactivatingCurrenciesSucceeds()
//        {
//            // Arrange
//            using var trx = TransactionFactory.ReadCommitted();
//            var currency = await _repo.Currencies.Filter("IsActive").FirstOrDefaultAsync(_ctx);
//            var currencyId = currency.Id;
//            var ids = new List<string> { currencyId };

//            // Act
//            bool isActive = false;
//            var result = await _repo.Currencies__Activate(ids, isActive, validateOnly: false, top: int.MaxValue, userId: _userId);
//            var afterCurrency = await _repo.Currencies.FilterByIds(ids).FirstOrDefaultAsync(_ctx);

//            // Assert
//            Assert.True(currency.IsActive);
//            Assert.False(afterCurrency.IsActive);
//        }
//    }
//}
