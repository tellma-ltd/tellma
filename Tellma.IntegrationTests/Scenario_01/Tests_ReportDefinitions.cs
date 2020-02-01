using Tellma.Controllers.Dto;
using Tellma.Entities;
using Tellma.IntegrationTests.Utilities;
using Tellma.Services.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Tellma.IntegrationTests.Scenario_01
{
    public class Tests_04_ReportDefinitions : Scenario_01
    {
        public Tests_04_ReportDefinitions(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        public readonly string _baseAddress = "report-definitions";

        public string Url => $"/api/{_baseAddress}";
        private string View => _baseAddress;

        [Fact(DisplayName = "01 Getting all report definitions before granting permissions returns a 403 Forbidden response")]
        public async Task Test01()
        {
            var response = await Client.GetAsync(Url);

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact(DisplayName = "02 Getting all report definitions before creating any returns a 200 OK empty collection")]
        public async Task Test02()
        {
            await GrantPermissionToSecurityAdministrator(View, Constants.Update, "Id ne 'Bla'");

            // Call the API
            var response = await Client.GetAsync(Url);
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<ReportDefinition>>();

            // Assert the result makes sense
            Assert.Equal("ReportDefinition", responseData.CollectionName);

            Assert.Equal(2, responseData.TotalCount);
        }

        [Fact(DisplayName = "03 Getting a non-existent report definition id returns a 404 Not Found")]
        public async Task Test03()
        {
            int nonExistentId = 1;
            var response = await Client.GetAsync($"{Url}/{nonExistentId}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "04 Saving a single well-formed ReportDefinitionForSave returns a 200 OK result")]
        public async Task Test04()
        {
            // Prepare a well formed entity
            var dtoForSave = new ReportDefinitionForSave
            {
                // Basics
                Collection = "MeasurementUnit",

                // Data
                Type = "Summary",
                Rows = new List<ReportRowDefinitionForSave>
                {
                    new ReportRowDefinitionForSave
                    {
                        Path = "UnitType",
                        Label = "Unit Type",
                        Label2 = "نوع الوحدة",
                        Label3 = "单位类型",
                        AutoExpand = true,
                        OrderDirection = "desc"
                    }
                },
                Columns = new List<ReportColumnDefinitionForSave>
                {
                    new ReportColumnDefinitionForSave
                    {
                        Path = "CreatedBy",
                        Label = "Created By",
                        Label2 = "المنشئ",
                        Label3 = "由...制作",
                        AutoExpand = false,
                        OrderDirection = "asc"
                    }
                },
                Measures = new List<ReportMeasureDefinitionForSave>
                {
                    new ReportMeasureDefinitionForSave
                    {
                        Path = "Id",
                        Label = "Count",
                        Label2 = "العدد",
                        Label3 = "计数",
                        Aggregation = "count",
                        OrderDirection = "asc"
                    }
                },
                ShowColumnsTotal = true,
                ShowRowsTotal = false,

                // Filter
                Filter = "BaseAmount lt @Amount",
                Parameters = new List<ReportParameterDefinitionForSave>
                {
                    new ReportParameterDefinitionForSave
                    {
                         Key = "Amount",
                         Label = "Amount",
                         Label2 = "القيمة",
                         Label3 = "量",
                         Value = "",
                         Visibility = "Required"
                    }
                },


                // Chart
                Chart = "BarsVerticalGrouped",
                DefaultsToChart = false,

                // Title
                Id = "report-1",
                Title = "Report 1",
                Title2 = "التقرير 2",
                Title3 = "报告1",
                Description = "The first report",
                Description2 = "التقرير الأول",
                Description3 = "第一份报告",

                // Main Menu
                ShowInMainMenu = true,
                MainMenuIcon = "chart-pie",
                MainMenuSection = "Financials",
                MainMenuSortKey = 50
            };

            // Save it
            var dtosForSave = new List<ReportDefinitionForSave> { dtoForSave };
            var response = await Client.PostAsJsonAsync(Url, dtosForSave);

            // Assert that the response status code is a happy 200 OK
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert that the response is well-formed singleton of ReportDefinition
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<ReportDefinition>>();
            Assert.Single(responseData.Result);

            // Assert that the result matches the saved entity
            Assert.Equal("ReportDefinition", responseData.CollectionName);

            // Retreve the entity from the entities
            var responseDto = responseData.Result.SingleOrDefault();

            ////////// Assert that save worked

            // Basics
            Assert.Equal(dtoForSave.Collection, responseDto.Collection);

            // Data 
            Assert.Equal(dtoForSave.Type, responseDto.Type);
            Assert.Collection(responseDto.Rows, (responseRow) =>
            {
                var rowForSave = dtoForSave.Rows[0];
                Assert.NotEqual(0, rowForSave.Id);
                Assert.Equal(dtoForSave.Id, responseRow.ReportDefinitionId);

                Assert.Equal(rowForSave.Path, responseRow.Path);
                Assert.Equal(rowForSave.Label, responseRow.Label);
                Assert.Equal(rowForSave.Label2, responseRow.Label2);
                Assert.Equal(rowForSave.Label3, responseRow.Label3);
                Assert.Equal(rowForSave.Modifier, responseRow.Modifier);
                Assert.Equal(rowForSave.OrderDirection, responseRow.OrderDirection);
                Assert.Equal(rowForSave.AutoExpand, responseRow.AutoExpand);
            });
            Assert.Collection(responseDto.Columns, (responseCol) =>
            {
                var colForSave = dtoForSave.Columns[0];
                Assert.NotEqual(0, colForSave.Id);
                Assert.Equal(dtoForSave.Id, responseCol.ReportDefinitionId);

                Assert.Equal(colForSave.Path, responseCol.Path);
                Assert.Equal(colForSave.Label, responseCol.Label);
                Assert.Equal(colForSave.Label2, responseCol.Label2);
                Assert.Equal(colForSave.Label3, responseCol.Label3);
                Assert.Equal(colForSave.Modifier, responseCol.Modifier);
                Assert.Equal(colForSave.OrderDirection, responseCol.OrderDirection);
                Assert.Equal(colForSave.AutoExpand, responseCol.AutoExpand);
            });
            Assert.Collection(responseDto.Measures, (responseMeasure) =>
            {
                var measureForSave = dtoForSave.Measures[0];
                Assert.NotEqual(0, measureForSave.Id);
                Assert.Equal(dtoForSave.Id, responseMeasure.ReportDefinitionId);

                Assert.Equal(measureForSave.Path, responseMeasure.Path);
                Assert.Equal(measureForSave.Label, responseMeasure.Label);
                Assert.Equal(measureForSave.Label2, responseMeasure.Label2);
                Assert.Equal(measureForSave.Label3, responseMeasure.Label3);
                Assert.Equal(measureForSave.OrderDirection, responseMeasure.OrderDirection);
                Assert.Equal(measureForSave.Aggregation, responseMeasure.Aggregation);
            });
            Assert.Equal(dtoForSave.ShowRowsTotal, responseDto.ShowRowsTotal);
            Assert.Equal(dtoForSave.ShowColumnsTotal, responseDto.ShowColumnsTotal);

            // Filter
            Assert.Equal(dtoForSave.Filter, responseDto.Filter);
            Assert.Collection(responseDto.Parameters, (responseParam) =>
            {
                var paramForSave = dtoForSave.Parameters[0];
                Assert.NotEqual(0, paramForSave.Id);
                Assert.Equal(dtoForSave.Id, responseParam.ReportDefinitionId);

                Assert.Equal(paramForSave.Key, responseParam.Key);
                Assert.Equal(paramForSave.Label, responseParam.Label);
                Assert.Equal(paramForSave.Label2, responseParam.Label2);
                Assert.Equal(paramForSave.Label3, responseParam.Label3);
                Assert.Equal(paramForSave.Visibility, responseParam.Visibility);
                Assert.Equal(paramForSave.Value, responseParam.Value);
            });

            // Chart
            Assert.Equal(dtoForSave.Chart, responseDto.Chart);
            Assert.Equal(dtoForSave.DefaultsToChart, responseDto.DefaultsToChart);

            // Title
            Assert.Equal(dtoForSave.Id, responseDto.Id);
            Assert.Equal(dtoForSave.Title, responseDto.Title);
            Assert.Equal(dtoForSave.Title2, responseDto.Title2);
            Assert.Equal(dtoForSave.Title3, responseDto.Title3);
            Assert.Equal(dtoForSave.Description, responseDto.Description);
            Assert.Equal(dtoForSave.Description2, responseDto.Description2);
            Assert.Equal(dtoForSave.Description3, responseDto.Description3);

            // Main Menu
            Assert.Equal(dtoForSave.ShowInMainMenu, responseDto.ShowInMainMenu);
            Assert.Equal(dtoForSave.MainMenuSection, responseDto.MainMenuSection);
            Assert.Equal(dtoForSave.MainMenuIcon, responseDto.MainMenuIcon);
            Assert.Equal(dtoForSave.MainMenuSortKey, responseDto.MainMenuSortKey);

            // share the entity, for the subsequent delete test
            Shared.Set("Report1", responseDto);
        }

        [Fact(DisplayName = "05 Getting the Id of the ReportDefinitionForSave just saved returns a 200 OK result")]
        public async Task Test05()
        {
            // Query the API for the Id that was just returned from the Save
            var dtoForSave = Shared.Get<ReportDefinition>("Report1");
            var id = dtoForSave.Id;
            var response = await Client.GetAsync($"{Url}/{id}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of ReportDefinition
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<ReportDefinition>>();
            Assert.Equal("ReportDefinition", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;

            ////////// Assert that save worked

            // Basics
            Assert.Equal(dtoForSave.Collection, responseDto.Collection);

            // Data 
            Assert.Equal(dtoForSave.Type, responseDto.Type);
            Assert.Collection(responseDto.Rows, (responseRow) =>
            {
                var rowForSave = dtoForSave.Rows[0];
                Assert.NotEqual(0, rowForSave.Id);
                Assert.Equal(dtoForSave.Id, responseRow.ReportDefinitionId);

                Assert.Equal(rowForSave.Path, responseRow.Path);
                Assert.Equal(rowForSave.Label, responseRow.Label);
                Assert.Equal(rowForSave.Label2, responseRow.Label2);
                Assert.Equal(rowForSave.Label3, responseRow.Label3);
                Assert.Equal(rowForSave.Modifier, responseRow.Modifier);
                Assert.Equal(rowForSave.OrderDirection, responseRow.OrderDirection);
                Assert.Equal(rowForSave.AutoExpand, responseRow.AutoExpand);
            });
            Assert.Collection(responseDto.Columns, (responseCol) =>
            {
                var colForSave = dtoForSave.Columns[0];
                Assert.NotEqual(0, colForSave.Id);
                Assert.Equal(dtoForSave.Id, responseCol.ReportDefinitionId);

                Assert.Equal(colForSave.Path, responseCol.Path);
                Assert.Equal(colForSave.Label, responseCol.Label);
                Assert.Equal(colForSave.Label2, responseCol.Label2);
                Assert.Equal(colForSave.Label3, responseCol.Label3);
                Assert.Equal(colForSave.Modifier, responseCol.Modifier);
                Assert.Equal(colForSave.OrderDirection, responseCol.OrderDirection);
                Assert.Equal(colForSave.AutoExpand, responseCol.AutoExpand);
            });
            Assert.Collection(responseDto.Measures, (responseMeasure) =>
            {
                var measureForSave = dtoForSave.Measures[0];
                Assert.NotEqual(0, measureForSave.Id);
                Assert.Equal(dtoForSave.Id, responseMeasure.ReportDefinitionId);

                Assert.Equal(measureForSave.Path, responseMeasure.Path);
                Assert.Equal(measureForSave.Label, responseMeasure.Label);
                Assert.Equal(measureForSave.Label2, responseMeasure.Label2);
                Assert.Equal(measureForSave.Label3, responseMeasure.Label3);
                Assert.Equal(measureForSave.OrderDirection, responseMeasure.OrderDirection);
                Assert.Equal(measureForSave.Aggregation, responseMeasure.Aggregation);
            });
            Assert.Equal(dtoForSave.ShowRowsTotal, responseDto.ShowRowsTotal);
            Assert.Equal(dtoForSave.ShowColumnsTotal, responseDto.ShowColumnsTotal);

            // Filter
            Assert.Equal(dtoForSave.Filter, responseDto.Filter);
            Assert.Collection(responseDto.Parameters, (responseParam) =>
            {
                var paramForSave = dtoForSave.Parameters[0];
                Assert.NotEqual(0, paramForSave.Id);
                Assert.Equal(dtoForSave.Id, responseParam.ReportDefinitionId);

                Assert.Equal(paramForSave.Key, responseParam.Key);
                Assert.Equal(paramForSave.Label, responseParam.Label);
                Assert.Equal(paramForSave.Label2, responseParam.Label2);
                Assert.Equal(paramForSave.Label3, responseParam.Label3);
                Assert.Equal(paramForSave.Visibility, responseParam.Visibility);
                Assert.Equal(paramForSave.Value, responseParam.Value);
            });

            // Chart
            Assert.Equal(dtoForSave.Chart, responseDto.Chart);
            Assert.Equal(dtoForSave.DefaultsToChart, responseDto.DefaultsToChart);

            // Title
            Assert.Equal(dtoForSave.Id, responseDto.Id);
            Assert.Equal(dtoForSave.Title, responseDto.Title);
            Assert.Equal(dtoForSave.Title2, responseDto.Title2);
            Assert.Equal(dtoForSave.Title3, responseDto.Title3);
            Assert.Equal(dtoForSave.Description, responseDto.Description);
            Assert.Equal(dtoForSave.Description2, responseDto.Description2);
            Assert.Equal(dtoForSave.Description3, responseDto.Description3);

            // Main Menu
            Assert.Equal(dtoForSave.ShowInMainMenu, responseDto.ShowInMainMenu);
            Assert.Equal(dtoForSave.MainMenuSection, responseDto.MainMenuSection);
            Assert.Equal(dtoForSave.MainMenuIcon, responseDto.MainMenuIcon);
            Assert.Equal(dtoForSave.MainMenuSortKey, responseDto.MainMenuSortKey);
        }

        [Fact(DisplayName = "07 Saving a ReportDefinitionForSave trims string fields with trailing or leading spaces")]
        public async Task Test07()
        {
            // Prepare a DTO for save, that contains leading and 
            // trailing spaces in some string properties
            var dtoForSave = new ReportDefinitionForSave
            {
                Collection = "MeasurementUnit",
                Type = "Details",
                Id = "report-2",
                Title = "  Report 2", // Leading space
                Title2 = "التقرير 2",
                Description = "Second Report  ", // Trailing space
                Description2 = "التقرير الثاني",
            };

            // Call the API
            var response = await Client.PostAsJsonAsync(Url, new List<ReportDefinitionForSave> { dtoForSave });
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Confirm that the response is well-formed
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<ReportDefinition>>();
            var responseDto = responseData.Result.FirstOrDefault();

            // Confirm that the leading and trailing spaces have been trimmed
            Assert.Equal(dtoForSave.Id?.Trim(), responseDto.Id);
            Assert.Equal(dtoForSave.Title?.Trim(), responseDto.Title);
            Assert.Equal(dtoForSave.Description?.Trim(), responseDto.Description);

            // share the entity, for the subsequent delete test
            Shared.Set("Report2", responseDto);
        }

        [Fact(DisplayName = "08 Deleting an existing report definition Id returns a 200 OK")]
        public async Task Test08()
        {
            await GrantPermissionToSecurityAdministrator(View, Constants.Delete, null);

            // Get the Id
            var entity = Shared.Get<ReportDefinition>("Report2");
            var id = entity.Id;

            // Query the delete API
            var deleteResponse = await Client.DeleteAsync($"{Url}/{id}");

            Output.WriteLine(await deleteResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact(DisplayName = "09 Getting an Id that was just deleted returns a 404 Not Found")]
        public async Task Test09()
        {
            // Get the Id
            var entity = Shared.Get<ReportDefinition>("Report2");
            var id = entity.Id;

            // Verify that the id was deleted by calling get        
            var getResponse = await Client.GetAsync($"{Url}/{id}");

            // Assert that the response is correct
            Output.WriteLine(await getResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
