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
//    public class UnitsTests : IClassFixture<ApplicationRepositoryFixture>
//    {
//        #region Lifecycle

//        private readonly ApplicationRepository _repo;
//        private readonly int _userId;
//        private readonly QueryContext _ctx;

//        public UnitsTests(ApplicationRepositoryFixture fixture)
//        {
//            _repo = fixture.Repo;
//            _userId = fixture.UserId;
//            _ctx = new QueryContext(_userId);
//        }

//        public static UnitForSave LightYears()
//        {
//            return new UnitForSave
//            {
//                Id = 0,
//                Code = "ly",
//                Name = "ly",
//                Name2 = "س ض",
//                Description = "Lightyear",
//                Description2 = "سنة ضوئية",
//                BaseAmount = 1,
//                UnitAmount = 1,
//                UnitType = UnitTypes.Distance,
//            };
//        }

//        public static UnitForSave PlanksLength()
//        {
//            return new UnitForSave
//            {
//                Id = 0,
//                Code = "pl",
//                Name = "pl",
//                Name2 = "م ب",
//                Description = "Plank's Length",
//                Description2 = "مسافة بلانك",
//                BaseAmount = 1,
//                UnitAmount = 1,
//                UnitType = UnitTypes.Distance,
//            };
//        }

//        #endregion

//        [Fact(DisplayName = "Saving two Units with the same code fails")]
//        public async Task SavingDuplicateCodesFails()
//        {
//            var ly = LightYears();
//            var pl = PlanksLength();

//            pl.Code = "LY"; // Error!

//            // Arrange
//            var units = new List<UnitForSave> { ly, pl };

//            // Act
//            using var trx = TransactionFactory.ReadCommitted();
//            var result = await _repo.Units__Save(units, returnIds: true, validateOnly: false, top: int.MaxValue, userId: _userId);

//            // Assert
//            Assert.True(result.IsError);
//            Assert.Collection(result.Errors,
//                error =>
//                {
//                    Assert.Equal("[0].Code", error.Key);
//                    Assert.Equal("Error_TheCode0IsDuplicated", error.ErrorName);
//                    Assert.Equal("ly", error.Argument1.ToLower());
//                },
//                error =>
//                {
//                    Assert.Equal("[1].Code", error.Key);
//                    Assert.Equal("Error_TheCode0IsDuplicated", error.ErrorName);
//                    Assert.Equal("ly", error.Argument1.ToLower());
//                }
//            );

//            Assert.Empty(result.Ids);
//        }

//        [Fact(DisplayName = "Saving two Units with the same name fails")]
//        public async Task SavingDuplicateNamesFails()
//        {
//            var ly = LightYears();
//            var pl = PlanksLength();

//            pl.Name = "LY"; // Error!

//            // Arrange
//            var units = new List<UnitForSave> { ly, pl };

//            // Act
//            using var trx = TransactionFactory.ReadCommitted();
//            var result = await _repo.Units__Save(units, returnIds: true, validateOnly: false, top: int.MaxValue, userId: _userId);

//            // Assert
//            Assert.True(result.IsError);
//            Assert.Collection(result.Errors,
//                error =>
//                {
//                    Assert.Equal("[0].Name", error.Key);
//                    Assert.Equal("Error_TheName0IsDuplicated", error.ErrorName);
//                    Assert.Equal("ly", error.Argument1.ToLower());
//                },
//                error =>
//                {
//                    Assert.Equal("[1].Name", error.Key);
//                    Assert.Equal("Error_TheName0IsDuplicated", error.ErrorName);
//                    Assert.Equal("ly", error.Argument1.ToLower());
//                }
//            );

//            Assert.Empty(result.Ids);
//        }

//        [Fact(DisplayName = "Saving a valid Unit succeeds")]
//        public async Task SavingValidUnitSucceeds()
//        {
//            // Arrange
//            var unit = LightYears();
//            var units = new List<UnitForSave> { unit };

//            // Act
//            using var trx = TransactionFactory.ReadCommitted();
//            var result = await _repo.Units__Save(units, returnIds: true, validateOnly: false, top: int.MaxValue, userId: _userId);

//            // Assert
//            Assert.False(result.IsError);
//            var id = Assert.Single(result.Ids);

//            var dbUnit = await _repo.Units
//                .Filter($"Id eq {id}")
//                .FirstOrDefaultAsync(_ctx);

//            Assert.Equal(unit.Code, dbUnit.Code);
//            Assert.Equal(unit.Name, dbUnit.Name);
//            Assert.Equal(unit.Name2, dbUnit.Name2);
//            Assert.Equal(unit.Name3, dbUnit.Name3);
//            Assert.Equal(unit.Description, dbUnit.Description);
//            Assert.Equal(unit.Description2, dbUnit.Description2);
//            Assert.Equal(unit.Description3, dbUnit.Description3);
//            Assert.Equal(unit.BaseAmount, dbUnit.BaseAmount);
//            Assert.Equal(unit.UnitAmount, dbUnit.UnitAmount);
//            Assert.Equal(unit.UnitType, dbUnit.UnitType);
//        }

//        [Fact(DisplayName = "Deleting an existing unit succeeds")]
//        public async Task DeletingUnitSucceeds()
//        {
//            // Arrange
//            using var trx = TransactionFactory.ReadCommitted();
//            var unit = await _repo.Units.FirstOrDefaultAsync(_ctx);
//            var unitId = unit.Id;
//            var idsToDelete = new List<int> { unitId };

//            // Act
//            var deleteResult = await _repo.Units__Delete(idsToDelete, validateOnly: false, top: int.MaxValue, userId: _userId);
//            var afterUnit = await _repo.Units
//                .FilterByIds(idsToDelete)
//                .FirstOrDefaultAsync(_ctx);

//            // Assert
//            Assert.False(deleteResult.IsError);
//            Assert.Empty(deleteResult.Errors);
//            Assert.Null(afterUnit);
//        }

//        [Fact(DisplayName = "Deactivating an active unit succeeds")]
//        public async Task DeactivatingUnitSucceeds()
//        {
//            // Arrange
//            using var trx = TransactionFactory.ReadCommitted();
//            var unit = await _repo.Units.FirstOrDefaultAsync(_ctx);
//            var unitId = unit.Id;
//            var ids = new List<int> { unitId };

//            // Act
//            bool isActive = false;
//            var result = await _repo.Units__Activate(ids, isActive, validateOnly: false, top: int.MaxValue, userId: _userId);
//            var afterUnit = await _repo.Units.FilterByIds(ids).FirstOrDefaultAsync(_ctx);

//            // Assert
//            Assert.True(unit.IsActive);
//            Assert.False(afterUnit.IsActive);
//        }
//    }
//}
