using Tellma.Controllers.Dto;
using Tellma.Entities;
using Tellma.Services.Utilities;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Tellma.IntegrationTests.Scenario_01
{
    public class Tests_09_IfrsConcepts : Scenario_01
    {
        public Tests_09_IfrsConcepts(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        public readonly string _baseAddress = "ifrs-concepts";

        public string Url => $"/api/{_baseAddress}";
        public string View => _baseAddress; // For permissions

        [Fact(DisplayName = "01 Getting all ifrs concepts before granting permissions returns a 403 Forbidden response")]
        public async Task Test01()
        {
            var response = await Client.GetAsync(Url);

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact(DisplayName = "02 Getting all ifrs concepts before creating any returns a 200 OK empty collection")]
        public async Task Test02()
        {
            await GrantPermissionToSecurityAdministrator(View, Constants.Update, "Id ne null");

            // Call the API
            var response = await Client.GetAsync(Url);
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<IfrsConcept>>();

            // Assert the result makes sense
            Assert.Equal("IfrsConcept", responseData.CollectionName);

            Assert.NotEqual(0, responseData.TotalCount);
            Assert.NotEmpty(responseData.Result);
        }

        [Fact(DisplayName = "03 Getting a non-existent ifrs concept id returns a 404 Not Found")]
        public async Task Test03()
        {
            int nonExistentId = 0;
            var response = await Client.GetAsync($"{Url}/{nonExistentId}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
