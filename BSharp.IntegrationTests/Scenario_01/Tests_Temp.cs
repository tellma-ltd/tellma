using BSharp.Controllers.Dto;
using BSharp.Entities;
using BSharp.Services.Utilities;
using System.Linq;
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

        [Fact(DisplayName = "02 Resource Picks")]
        public async Task Test02()
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

        [Fact(DisplayName = "03 Voucher Booklets")]
        public async Task Test03()
        {
            await GrantPermissionToSecurityAdministrator("voucher-booklets", Constants.Update, "Id gt 0");

            var response = await Client.GetAsync("/api/voucher-booklets?search=Bla");

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is well formed
            var responseData = await response.Content.ReadAsAsync<GetResponse<VoucherBooklet>>();

            // Assert the result makes sense
            Assert.Equal(nameof(VoucherBooklet), responseData.CollectionName);
            Assert.Empty(responseData.Result); // First 
        }

        [Fact(DisplayName = "04 IFRS Account Classifications")]
        public async Task Test04()
        {
            await GrantPermissionToSecurityAdministrator("ifrs-account-classifications", Constants.Update, "Id ne 'bla'");

            var response = await Client.GetAsync("/api/ifrs-account-classifications?search=e&top=10");

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is well formed
            var responseData = await response.Content.ReadAsAsync<GetResponse<IfrsAccountClassification>>();

            // Assert the result makes sense
            Assert.Equal(nameof(IfrsAccountClassification), responseData.CollectionName);
            Assert.Equal(10, responseData.Result.Count()); // First 
        }

        [Fact(DisplayName = "05 IFRS Entry Classifications")]
        public async Task Test05()
        {
            await GrantPermissionToSecurityAdministrator("ifrs-entry-classifications", Constants.Update, "Id ne 'bla'");

            var response = await Client.GetAsync("/api/ifrs-entry-classifications?search=e&top=10");

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is well formed
            var responseData = await response.Content.ReadAsAsync<GetResponse<IfrsEntryClassification>>();

            // Assert the result makes sense
            Assert.Equal(nameof(IfrsEntryClassification), responseData.CollectionName);
            Assert.Equal(10, responseData.Result.Count()); // First 
        }

        [Fact(DisplayName = "06 Accounts")]
        public async Task Test06()
        {
            await GrantPermissionToSecurityAdministrator("accounts", Constants.Update, "Id gt -1");

            var response = await Client.GetAsync("/api/accounts?search=Bla");

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is well formed
            var responseData = await response.Content.ReadAsAsync<GetResponse<Account>>();

            // Assert the result makes sense
            Assert.Equal(nameof(Account), responseData.CollectionName);
            Assert.Empty(responseData.Result); 
        }
    }
}
