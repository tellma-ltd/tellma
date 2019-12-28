using BSharp.Controllers.Dto;
using BSharp.Entities;
using BSharp.IntegrationTests.Utilities;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

// To order the tests, as per https://bit.ly/2lFONcE
[assembly: TestCollectionOrderer(TestCollectionOrderer.TypeName, TestCollectionOrderer.AssemblyName)]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace BSharp.IntegrationTests.Scenario_01
{
    [Collection(nameof(Scenario_01))]
    [TestCaseOrderer(TestCaseOrderer.TypeName, TestCaseOrderer.AssemblyName)]
    public abstract class Scenario_01
    {
        /// <summary>
        /// The <see cref="HttpClient"/> used by all test methods
        /// </summary>
        protected HttpClient Client { set; get; }

        /// <summary>
        /// A dictionary-like collection shared across test methods
        /// </summary>
        protected SharedCollection Shared { set; get; }

        /// <summary>
        /// Output for the test methods to do some logging
        /// </summary>
        protected ITestOutputHelper Output { set; get; }

        public Scenario_01(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output)
        {
            Client = factory.GetClient();
            Shared = factory.GetSharedCollection();
            Output = output;
        }

        protected async Task GrantPermissionToSecurityAdministrator(string view, string level, string criteria)
        {
            // Query the API for the Id that was just returned from the Save
            var getResponse = await Client.GetAsync($"/api/roles/{1}?expand=Permissions,Members/User");
            if (getResponse.StatusCode != HttpStatusCode.OK)
            {
                Output.WriteLine(await getResponse.Content.ReadAsStringAsync());
            }
            var getByIdResponse = await getResponse.Content.ReadAsAsync<GetByIdResponse<Role>>();
            var role = getByIdResponse.Result;

            role.Permissions.Add(new Permission
            {
                View = view,
                Action = level,
                Criteria = criteria
            });

            var dtosForSave = new List<Role> { role };
            var postResponse = await Client.PostAsJsonAsync($"/api/roles?expand=Permissions,Members/User", dtosForSave);
            if (postResponse.StatusCode != HttpStatusCode.OK)
            {
                Output.WriteLine(await postResponse.Content.ReadAsStringAsync());
            }
        }
    }
}
