using BSharp.Controllers.Dto;
using BSharp.Entities;
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
        /// <summary>
        /// The <see cref="HttpClient"/> used by all test methods
        /// </summary>
        private HttpClient Client { set; get; }

        /// <summary>
        /// A dictionary-like collection shared across test methods
        /// </summary>
        private SharedCollection Shared { set; get; }

        /// <summary>
        /// Output for the test methods to do some logging
        /// </summary>
        private ITestOutputHelper Output { set; get; }

        /// <summary>
        /// The default trait used to group the results in the Test Explorer window
        /// </summary>
        public const string Testing = TestOrderer.Testing;

        public Scenario_01(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output)
        {
            Client = factory.GetClient();
            Shared = factory.GetSharedCollection();
            Output = output;
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
            var response = await Client.GetAsync($"/api/roles/{1}?expand=Permissions/View,Members/User");
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<Role>>();
            var role = getByIdResponse.Result;

            role.Permissions.Add(new Permission
            {
                ViewId = viewId,
                Action = level,
                Criteria = criteria
            });


            var dtosForSave = new List<Role> { role };
            var postResponse = await Client.PostAsJsonAsync($"/api/roles?expand=Permissions/View,Members/User", dtosForSave);
            Output.WriteLine(await postResponse.Content.ReadAsStringAsync());
        }
    }
}
