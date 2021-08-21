using IdentityModel.Client;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Client;
using Tellma.Model.Application;
using Xunit;
using Xunit.Abstractions;

namespace Tellma.IntegrationTests.Scenario_01
{
    public class Scenario_01_Tests : Scenario_01
    {
        private readonly ITestOutputHelper _output;

        public Scenario_01_Tests(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory)
        {
            _output = output;
        }

        [Fact(DisplayName = "Pinging general settings succeeds")]
        public async Task Scenario01()
        {
            #region Deployment Experiment

            //// https://stackoverflow.com/questions/10438258/using-microsoft-build-evaluation-to-publish-a-database-project-sqlproj

            //const string projectPath = "";
            //const string connString = "";

            ////This Snapshot should be created by our build process using MSDeploy
            //const string snapshotPath = "";

            //var project = ProjectCollection.GlobalProjectCollection.LoadProject(projectPath);
            //project.Build();

            //DacServices dbServices = new DacServices(connString);
            //DacPackage dbPackage = DacPackage.Load(snapshotPath);




            //DacDeployOptions dbDeployOptions = new DacDeployOptions();

            ////Cut out a lot of options here for configuring deployment, but are all part of DacDeployOptions
            //dbDeployOptions.SqlCommandVariableValues.Add("debug", "false");

            //string dbName = "Tellma.101";
            //dbServices.Deploy(dbPackage, dbName, true, dbDeployOptions);

            #endregion

            #region Access Token

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

            #endregion

            // Call the protected API
            var accessTokenFactory = new StaticAccessTokenFactory(accessToken);
            var client = new TellmaClient(Client, accessTokenFactory);

            EntitiesResult<Agent> response = await client
                .Application(tenantId: 201)
                .Agents
                .GetEntities(new GetArguments
                {
                    Select = $"{nameof(Agent.Name)}",
                    Top = 5,
                    CountEntities = true
                });

            Assert.Equal(7, response.Count);
            Assert.NotNull(response.Data);
            Assert.Equal(5, response.Data.Count);

            //var settings = await response.Content.ReadAsAsync<Versioned<SettingsForClient>>();
            //Assert.Equal("Soreti Trading", settings.Data.ShortCompanyName);
        }

        //[Fact(DisplayName = "02 Getting accounts of a specific type before creating any returns a 200 OK empty collection")]
        //public async Task Test02()
        //{
        //    await GrantPermissionToSecurityAdministrator(View, Constants.Update, "Id gt -1");

        //    // Call the API
        //    var response = await Client.GetAsync("");
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

        private class StaticAccessTokenFactory : IAccessTokenFactory
        {
            private readonly string _accessToken;

            public StaticAccessTokenFactory(string accessToken)
            {
                _accessToken = accessToken;
            }

            public Task<string> GetValidAccessToken(CancellationToken cancellation = default)
            {
                return Task.FromResult(_accessToken);
            }
        }
    }
}
