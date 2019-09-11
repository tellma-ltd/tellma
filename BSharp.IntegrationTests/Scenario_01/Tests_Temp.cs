using BSharp.Controllers.Dto;
using BSharp.Entities;
using BSharp.Services.Utilities;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace BSharp.IntegrationTests.Scenario_01
{
    // Here I test all the temporary read-only controllers we need for the JV
    public class Tests_Temp : Scenario_01
    {
        public Tests_Temp(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        [Fact(DisplayName = "01 Responsibility Centers")]
        public async Task Test01()
        {
            await GrantPermissionToSecurityAdministrator("responsibility-centers", Constants.Update, "Id gt 0");

            var response = await Client.GetAsync("/api/responsibility-centers?search=Bla");

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is well formed
            var responseData = await response.Content.ReadAsAsync<GetResponse<ResponsibilityCenter>>();

            // Assert the result makes sense
            Assert.Equal("ResponsibilityCenter", responseData.CollectionName);
            Assert.Empty(responseData.Result); // First 
        }

        [Fact(DisplayName = "02 Resources")]
        public async Task Test02()
        {
            await GrantPermissionToSecurityAdministrator("resources", Constants.Update, "Id gt 0");

            var response = await Client.GetAsync("/api/resources?search=Bla");

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is well formed
            var responseData = await response.Content.ReadAsAsync<GetResponse<Resource>>();

            // Assert the result makes sense
            Assert.Equal(nameof(Resource), responseData.CollectionName);
            Assert.Empty(responseData.Result); // First 
        }


        [Fact(DisplayName = "03 Resource Picks")]
        public async Task Test03()
        {
            await GrantPermissionToSecurityAdministrator("resource-picks", Constants.Update, "Id gt 0");

            var response = await Client.GetAsync("/api/resource-picks?search=Bla");

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is well formed
            var responseData = await response.Content.ReadAsAsync<GetResponse<ResourcePick>>();

            // Assert the result makes sense
            Assert.Equal(nameof(ResourcePick), responseData.CollectionName);
            Assert.Empty(responseData.Result); // First 
        }
    }
}
