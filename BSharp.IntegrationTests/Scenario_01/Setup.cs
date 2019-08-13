using BSharp.Controllers.Dto;
using BSharp.IntegrationTests.Utilities;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace BSharp.IntegrationTests.Scenario_01
{
    [TestCaseOrderer(TestOrderer.TypeName, TestOrderer.AssemblyName)]
    public partial class Scenario_01 : IClassFixture<Scenario_01_WebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly SharedCollection _shared;
        private readonly ITestOutputHelper _output;

        public const string Testing = TestOrderer.Testing;

        public Scenario_01(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output)
        {
            _client = factory.GetClient();
            _shared = factory.GetSharedCollection();
            _output = output;
        }

        [Trait(Testing, "00 - Setup")]
        [Fact(DisplayName = "000 - Setting up the testing database and web host")]
        public void Setup()
        {
            // This empty test takes the blame for the dozen seconds or so that are needed in the web app
            // factory to provision the database and instantiate the web host at the beginning of the test
            // run, with this empty test in place, the actual subsequent tests report reasonable durations
        }


        protected async Task GrantPermissionToSecurityAdministrator(string viewId, string level, string criteria)
        {
            // Query the API for the Id that was just returned from the Save
            var response = await _client.GetAsync($"/api/roles/{1}?expand=Permissions/View,Members/User");
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<Role>>();
            var role = getByIdResponse.Result;

            role.EntityState = "Updated";
            role.Permissions.Add(new Permission
            {
                ViewId = viewId,
                Action = level,
                Criteria = criteria,
                EntityState = "Inserted"
            });


            var dtosForSave = new List<Role> { role };
            var postResponse = await _client.PostAsJsonAsync($"/api/roles?expand=Permissions/View,Members/User", dtosForSave);
            var postResponseText = await response.Content.ReadAsStringAsync();
        }
    }
}
