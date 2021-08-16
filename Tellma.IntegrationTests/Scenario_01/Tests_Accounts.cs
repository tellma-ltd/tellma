using IdentityModel.Client;
using IdentityServer4.Configuration;
using System.Threading.Tasks;
using System.Web;
using Tellma.Client;
using Xunit;
using Xunit.Abstractions;

namespace Tellma.IntegrationTests.Scenario_01
{
    public class Tests_Accounts : Scenario_01
    {
        private readonly ITestOutputHelper _output;

        public Tests_Accounts(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory)
        {
            _output = output;
        }

        [Fact(DisplayName = "Pinging general settings succeeds")]
        public async Task Scenario01()
        {
            //var tokenResponse = await Client.RequestPasswordTokenAsync(new PasswordTokenRequest
            //{
            //    Address = "/connect/token",
            //    ClientId = Services.Utilities.Constants.WebClientName,
            //    ClientSecret = "top-secret",
            //    Scope = Services.Utilities.Constants.ApiResourceName, // What about the others?
            //    UserName = "ahmad.akra@tellma.com",
            //    Password = "Banan@123"
            //});

            var tokenResponse = await Client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = "/connect/token",
                ClientId = "m2m-5d3f198c-287b-49e9-bf7c-5879b6f2a4d8",
                ClientSecret = "aced45ff3450ff81afb9c73492e069fbc8cd92faae113635ef1fae766b6e4591",
                Scope = Services.Utilities.Constants.ApiResourceName,
            });

            Assert.False(tokenResponse.IsError, $"Admin authentication failed, Error: {tokenResponse.Error}.");
            var accessToken = tokenResponse.AccessToken;
            Assert.NotNull(accessToken);

            // Call the protected API
            var client = new TellmaClient(Client, accessToken);

            var response = await client.GeneralSettings.PingResponse(new ApplicationRequest { TenantId = 201 });
            Assert.False(response.IsError, $"Failed with status code {response.StatusCode}.");

            //var settings = await response.Content.ReadAsAsync<Versioned<SettingsForClient>>();
            //Assert.Equal("Soreti Trading", settings.Data.ShortCompanyName);
        }

        //[Fact(DisplayName = "02 Getting accounts of a specific type before creating any returns a 200 OK empty collection")]
        //public async Task Test02()
        //{
        //    await GrantPermissionToSecurityAdministrator(View, Constants.Update, "Id gt -1");

        //    // Call the API
        //    var response = await AdminClient.GetAsync(Url);
        //    Output.WriteLine(await response.Content.ReadAsStringAsync());

        //    // Assert the result is 200 OK
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //    // Confirm the result is a well formed response
        //    var responseData = await response.Content.ReadAsAsync<GetResponse<Account>>();

        //    // Assert the result makes sense
        //    Assert.Equal("Account", responseData.CollectionName);

        //    Assert.Null(responseData.TotalCount);
        //    Assert.Empty(responseData.Result);
        //}
    }
}
