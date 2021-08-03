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
    public class AgentsTests : IClassFixture<ApplicationRepositoryFixture>
    {
        #region Lifecycle

        private readonly ApplicationRepository _repo;
        private readonly int _userId;
        private readonly QueryContext _ctx;

        public AgentsTests(ApplicationRepositoryFixture fixture)
        {
            _repo = fixture.Repo;
            _userId = fixture.UserId;
            _ctx = new QueryContext(_userId);
        }

        public static AgentForSave Abbas()
        {
            return new AgentForSave
            {
                Id = 0,
                Name = "Abbas",
                Name2 = "عباس", 
                IsRelated = false,
            };
        }

        public static AgentForSave Thomas()
        {
            return new AgentForSave
            {
                Id = 0,
                Name = "Thomas",
                Name2 = "توماس",
                IsRelated = true,
            };
        }

        #endregion

        [Fact(DisplayName = "Saving two Agents with the same name fails")]
        public async Task SavingDuplicateNamesFails()
        {
            var a = Abbas();
            var t = Thomas();

            t.Name = "abbas"; // Error!

            // Arrange
            var agents = new List<AgentForSave> { a, t };

            // Act
            using var trx = TransactionFactory.ReadCommitted();
            var result = await _repo.Agents__Save(agents, returnIds: true, validateOnly: false, top: int.MaxValue, _userId);

            // Assert
            Assert.True(result.IsError);
            Assert.Collection(result.Errors,
                error =>
                {
                    Assert.Equal("[0].Name", error.Key);
                    Assert.Equal("Error_TheName0IsDuplicated", error.ErrorName);
                    Assert.Equal("abbas", error.Argument1.ToLower());
                },
                error =>
                {
                    Assert.Equal("[1].Name", error.Key);
                    Assert.Equal("Error_TheName0IsDuplicated", error.ErrorName);
                    Assert.Equal("abbas", error.Argument1.ToLower());
                }
            );

            Assert.Empty(result.Ids);
        }

        [Fact(DisplayName = "Saving a valid Agent succeeds")]
        public async Task SavingValidAgentSucceeds()
        {
            // Arrange
            var agent = Abbas();
            var agents = new List<AgentForSave> { agent };

            // Act
            using var trx = TransactionFactory.ReadCommitted();
            var result = await _repo.Agents__Save(agents, returnIds: true, validateOnly: false, top: int.MaxValue, _userId);

            // Assert
            Assert.False(result.IsError);
            var id = Assert.Single(result.Ids);

            var dbAgent = await _repo.Agents
                .Filter($"Id eq {id}")
                .FirstOrDefaultAsync(_ctx);

            Assert.Equal(agent.Name, dbAgent.Name);
            Assert.Equal(agent.Name2, dbAgent.Name2);
            Assert.Equal(agent.Name3, dbAgent.Name3);
            Assert.Equal(agent.IsRelated, dbAgent.IsRelated);
        }
    }
}
