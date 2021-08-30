using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Client;
using Tellma.Model.Application;
using Tellma.Model.Common;
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
            try
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

                const int totalCount = 34;
                // Get Entities
                {
                    var response = await client
                       .Application(tenantId: 201)
                       .Units
                       .GetEntities(new GetArguments
                       {
                           Select = $"{nameof(Unit.Name)},{nameof(Unit.CreatedBy)}.{nameof(User.Name)}",
                           OrderBy = nameof(Unit.Id),
                           Top = 5,
                           CountEntities = true
                       });

                    Assert.Equal(totalCount, response.Count);
                    Assert.NotNull(response.Data);
                    Assert.Equal(5, response.Data.Count);

                    var unit = response.Data[0];
                    Assert.NotNull(unit.CreatedBy);
                    Assert.Equal("Mohamad Akra", unit.CreatedBy.Name);
                }

                // Get Fact
                {
                    var response = await client
                       .Application(tenantId: 201)
                       .Units
                       .GetFact(new FactArguments
                       {
                           Select = $"{nameof(Unit.Name)},{nameof(Unit.CreatedBy)}.{nameof(User.Name)}",
                           OrderBy = nameof(Unit.Id),
                           Top = 5,
                           CountEntities = true
                       });

                    Assert.Equal(totalCount, response.Count);
                    Assert.NotNull(response.Data);
                    Assert.Equal(5, response.Data.Count);

                    var row = response.Data[0];
                    Assert.Equal(2, row.Count);
                    Assert.Equal("pure", row[0]);
                    Assert.Equal("Mohamad Akra", row[1]);
                }

                // Get Aggregate
                {
                    var response = await client
                       .Application(tenantId: 201)
                       .Units
                       .GetAggregate(new GetAggregateArguments
                       {
                           Select = $"Count({nameof(Unit.Id)})",
                       });

                    var rows = response.Data;
                    var row = Assert.Single(rows);
                    var datum = Assert.Single(row);

                    Assert.Equal(totalCount, Convert.ToInt32(datum));
                }

                // Get By Id
                {
                    var response = await client.Application(tenantId: 201)
                        .Units
                        .GetById(1, new GetByIdArguments
                        {
                            Select = $"{nameof(Unit.Name)},{nameof(Unit.CreatedBy)}.{nameof(User.Name)}",
                        });

                    var unit = response.Entity;
                    Assert.Equal("pure", unit.Name);
                    Assert.NotNull(unit.CreatedBy);
                    Assert.Equal("Mohamad Akra", unit.CreatedBy.Name);
                }

                // 
                {
                    var unitForSave = new UnitForSave
                    {
                        Name = "ly",
                        Description = "Lightyear",
                        UnitType = UnitTypes.Distance,
                        Code = "ly",
                        BaseAmount = 1,
                        UnitAmount = 1000
                    };

                    var response = await client.Application(tenantId: 201)
                        .Units
                        .Save(new List<UnitForSave> { unitForSave },
                        new SaveArguments
                        {
                            Expand = nameof(Unit.CreatedBy),
                            ReturnEntities = true
                        });

                    var unit = Assert.Single(response.Data);

                    Assert.Equal(unitForSave.Name, unit.Name);
                    Assert.Equal(unitForSave.Description, unit.Description);
                    Assert.Equal(unitForSave.UnitType, unit.UnitType);
                    Assert.Equal(unitForSave.Code, unit.Code);
                    Assert.Equal(unitForSave.BaseAmount, unit.BaseAmount);
                    Assert.Equal(unitForSave.UnitAmount, unit.UnitAmount);

                    Assert.NotNull(unit.CreatedBy);
                    Assert.Equal("Integration Tests", unit.CreatedBy.Name);
                }

                //var settings = await response.Content.ReadAsAsync<Versioned<SettingsForClient>>();
                //Assert.Equal("Soreti Trading", settings.Data.ShortCompanyName);
            }
            catch (TellmaException ex)
            {
                _output.WriteLine(ex.ToString());
                throw;
            }
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

        //[Fact(DisplayName = "Temp")]
        //public void PrintAllTypes()
        //{
        //    var allTypes =
        //        typeof(User).Assembly.GetTypes()
        //        .Concat(typeof(Model.Admin.AdminUser).Assembly.GetTypes())
        //        .Where(t => !t.IsGenericType && t.IsSubclassOf(typeof(Entity)))
        //        .Select(t => TypeDescriptor.Get(t));

        //    var navTypes = allTypes
        //        .SelectMany(t => t.NavigationProperties)
        //        .Select(p => p.TypeDescriptor.Name)
        //        .Distinct()
        //        .OrderBy(t => t);

        //    foreach (var t in navTypes)
        //    {
        //        _output.WriteLine($"public List<{t}> {t} {{ get; set; }}");
        //    }
        //}


        //[Fact(DisplayName = "Temp")]
        //public void PrintAllTypes2()
        //{
        //    var allTypes =
        //        typeof(User).Assembly.GetTypes()
        //        .Concat(typeof(Model.Admin.AdminUser).Assembly.GetTypes())
        //        .Where(t => !t.IsGenericType && t.IsSubclassOf(typeof(Entity)))
        //        .Select(t => TypeDescriptor.Get(t));

        //    var navTypes = allTypes
        //        .SelectMany(t => t.NavigationProperties.Select(p => $"{p.TypeDescriptor.Name} ({t.Name})"))
        //        .Distinct()
        //        .OrderBy(t => t);

        //    foreach (var t in navTypes)
        //    {
        //        _output.WriteLine(t);
        //    }
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
