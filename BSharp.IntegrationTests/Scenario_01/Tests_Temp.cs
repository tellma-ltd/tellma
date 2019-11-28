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

        [Fact(DisplayName = "01 Voucher Booklets")]
        public async Task Test01()
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

        [Fact(DisplayName = "02 IFRS Entry Classifications")]
        public async Task Test02()
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
            Assert.Empty(responseData.Result); // First 
        }
    }
}
