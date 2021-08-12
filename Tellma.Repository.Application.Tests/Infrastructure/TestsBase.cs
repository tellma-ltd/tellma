using Tellma.Repository.Common;

namespace Tellma.Repository.Application.Tests
{
    /// <summary>
    /// Base class containing convenience methods
    /// </summary>
    public class TestsBase
    {
        private readonly ApplicationRepository _repo;
        private readonly int _userId;
        private readonly QueryContext _ctx;

        public TestsBase(ApplicationRepositoryFixture fixture)
        {
            _repo = fixture.Repo;
            _userId = Declarations.AdminId;
            _ctx = new QueryContext(_userId);
        }

        protected ApplicationRepository Repo => _repo;
        protected int UserId => _userId;
        protected QueryContext Context => _ctx;
        protected static int Top => 200;
    }
}
