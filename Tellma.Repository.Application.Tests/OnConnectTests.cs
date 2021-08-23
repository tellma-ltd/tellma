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
//    public class OnConnectTests : IClassFixture<ApplicationRepositoryFixture>
//    {
//        #region Lifecycle

//        private readonly ApplicationRepository _repo;
//        private readonly string _externalId;
//        private readonly string _email;

//        public OnConnectTests(ApplicationRepositoryFixture fixture)
//        {
//            _repo = fixture.Repo;
//            _externalId = fixture.ExternalUserId;
//            _email = fixture.UserEmail;
//        }

//        #endregion

//        [Fact(DisplayName = "Simultaneous OnConnect calls execute without deadlocks")]
//        public async Task OnConnectDoesNotCauseDeadLock()
//        {
//            for (int i = 0; i < 200; i++)
//            {
//                var task1 = _repo.OnConnect(_externalId, _email, setLastActive: true, cancellation: default);
//                var task2 = _repo.OnConnect(_externalId, _email, setLastActive: true, cancellation: default);

//                await Task.WhenAll(task1, task2);
//            }
//        }
//    }
//}
