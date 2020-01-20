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
    public class Tests_08_LegacyTypes : Scenario_01
    {
        public Tests_08_LegacyTypes(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        public readonly string _baseAddress = "legacy-types";

        public string Url => $"/api/{_baseAddress}";
        public string View => _baseAddress; // For permissions

        [Fact(DisplayName = "01 Getting all legacy types before granting permissions returns a 403 Forbidden response")]
        public async Task Test01()
        {
            var response = await Client.GetAsync(Url);

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact(DisplayName = "02 Getting all legacy types before creating any returns a 200 OK empty collection")]
        public async Task Test02()
        {
            await GrantPermissionToSecurityAdministrator(View, Constants.Update, "Id lt 100000");

            // Call the API
            var response = await Client.GetAsync(Url);
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<LegacyType>>();

            // Assert the result makes sense
            Assert.Equal("LegacyType", responseData.CollectionName);

            Assert.Equal(3, responseData.TotalCount);
            Assert.NotEmpty(responseData.Result);
        }

        [Fact(DisplayName = "03 Getting a non-existent legacy type id returns a 404 Not Found")]
        public async Task Test03()
        {
            int nonExistentId = 0;
            var response = await Client.GetAsync($"{Url}/{nonExistentId}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
