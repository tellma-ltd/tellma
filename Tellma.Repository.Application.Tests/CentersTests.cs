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
//    public class CentersTests : IClassFixture<ApplicationRepositoryFixture>
//    {
//        #region Lifecycle

//        private readonly ApplicationRepository _repo;
//        private readonly int _userId;
//        private readonly QueryContext _ctx;

//        public CentersTests(ApplicationRepositoryFixture fixture)
//        {
//            _repo = fixture.Repo;
//            _userId = fixture.UserId;
//            _ctx = new QueryContext(_userId);
//        }

//        public static CenterForSave Export()
//        {
//            return new CenterForSave
//            {
//                Id = 0,
//                Code = "Export",
//                Name = "Export",
//                Name2 = "تصدير",
//                CenterType = CenterTypes.BusinessUnit
//            };
//        }

//        public static CenterForSave Import()
//        {
//            return new CenterForSave
//            {
//                Id = 0,
//                Code = "Import",
//                Name = "Import",
//                Name2 = "استيراد",
//                CenterType = CenterTypes.Operation
//            };
//        }

//        #endregion

//        [Fact(DisplayName = "Saving two Centers with the same code fails")]
//        public async Task SavingDuplicateCodesFails()
//        {
//            var ex = Export();
//            var im = Import();

//            im.Code = "Export"; // Error!

//            // Arrange
//            var entity = new List<CenterForSave> { ex, im };

//            // Act
//            using var trx = TransactionFactory.ReadCommitted();
//            var result = await _repo.Centers__Save(entity, returnIds: true, validateOnly: false, top: int.MaxValue, _userId);

//            // Assert
//            Assert.True(result.IsError);
//            Assert.Collection(result.Errors,
//                error =>
//                {
//                    Assert.Equal("[0].Code", error.Key);
//                    Assert.Equal("Error_TheCode0IsDuplicated", error.ErrorName);
//                    Assert.Equal("export", error.Argument1.ToLower());
//                },
//                error =>
//                {
//                    Assert.Equal("[1].Code", error.Key);
//                    Assert.Equal("Error_TheCode0IsDuplicated", error.ErrorName);
//                    Assert.Equal("export", error.Argument1.ToLower());
//                }
//            );

//            Assert.Empty(result.Ids);
//        }

//        [Fact(DisplayName = "Saving a valid Center succeeds")]
//        public async Task SavingValidCenterSucceeds()
//        {
//            // Arrange
//            var ex = Export();
//            var im = Import();
//            im.ParentIndex = 0;

//            var entities = new List<CenterForSave> { ex, im };

//            // Act
//            using var trx = TransactionFactory.ReadCommitted();
//            var result = await _repo.Centers__Save(entities, returnIds: true, validateOnly: false, top: int.MaxValue, _userId);

//            // Assert
//            Assert.False(result.IsError);

//            var dbCenters = await _repo.Centers
//                .FilterByIds(result.Ids)
//                .ToListAsync(_ctx);

//            var dbEx = dbCenters.FirstOrDefault(e => e.Code == ex.Code);
//            Assert.NotNull(dbEx);
//            Assert.Equal(ex.Name, dbEx.Name);
//            Assert.Equal(ex.Name2, dbEx.Name2);
//            Assert.Equal(ex.Name3, dbEx.Name3);

//            var dbIm = dbCenters.FirstOrDefault(e => e.Code == im.Code);
//            Assert.NotNull(dbIm);
//            Assert.Equal(im.Name, dbIm.Name);
//            Assert.Equal(im.Name2, dbIm.Name2);
//            Assert.Equal(im.Name3, dbIm.Name3);

//            // Parent Id was set correctly
//            Assert.Equal(dbEx.Id, dbIm.ParentId);
//        }
//    }
//}
