using Tellma.Controllers.Dto;
using Tellma.Entities;
using Tellma.IntegrationTests.Utilities;
using Tellma.Services.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using System;

namespace Tellma.IntegrationTests.Scenario_01
{
    public class Tests_05_ExchangeRates : Scenario_01
    {
        public Tests_05_ExchangeRates(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        public readonly string _baseAddress = "exchange-rates";

        public string Url => $"/api/{_baseAddress}";
        private string View => _baseAddress;

        [Fact(DisplayName = "01 Getting all exchange rates before granting permissions returns a 403 Forbidden response")]
        public async Task Test01()
        {
            var response = await Client.GetAsync(Url);

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact(DisplayName = "02 Getting all exchange rates before creating any returns a 200 OK empty collection")]
        public async Task Test02()
        {
            await GrantPermissionToSecurityAdministrator(View, Constants.Update, "Id lt 100000");

            // Call the API
            var response = await Client.GetAsync(Url);
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<ExchangeRate>>();

            // Assert the result makes sense
            Assert.Equal("ExchangeRate", responseData.CollectionName);

            Assert.Equal(0, responseData.TotalCount);
            Assert.Empty(responseData.Result);
        }

        [Fact(DisplayName = "03 Getting a non-existent exchange rate id returns a 404 Not Found")]
        public async Task Test03()
        {
            int nonExistentId = 1;
            var response = await Client.GetAsync($"{Url}/{nonExistentId}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "04 Saving a single well-formed ExchangeRateForSave returns a 200 OK result")]
        public async Task Test04()
        {
            // Prepare a well formed entity
            var dtoForSave = new ExchangeRateForSave
            {
                CurrencyId = "ETB",
                ValidAsOf = DateTime.Today,
                AmountInCurrency = 29m,
                AmountInFunctional = 1m
            };

            // Save it
            var dtosForSave = new List<ExchangeRateForSave> { dtoForSave };
            var response = await Client.PostAsJsonAsync(Url, dtosForSave);

            // Assert that the response status code is a happy 200 OK
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert that the response is well-formed singleton of ExchangeRate
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<ExchangeRate>>();
            Assert.Single(responseData.Result);

            // Assert that the result matches the saved entity
            Assert.Equal("ExchangeRate", responseData.CollectionName);

            // Retreve the entity from the entities
            var responseDto = responseData.Result.SingleOrDefault();

            Assert.NotNull(responseDto?.Id);
            Assert.Equal(dtoForSave.CurrencyId, responseDto.CurrencyId);
            Assert.Equal(dtoForSave.ValidAsOf, responseDto.ValidAsOf);
            Assert.Equal(dtoForSave.AmountInCurrency, responseDto.AmountInCurrency);
            Assert.Equal(dtoForSave.AmountInFunctional, responseDto.AmountInFunctional);


            Shared.Set("ExchangeRate_ETB_Today", responseDto);
        }

        [Fact(DisplayName = "05 Getting the Id of the ExchangeRateForSave just saved returns a 200 OK result")]
        public async Task Test05()
        {
            // Query the API for the Id that was just returned from the Save
            var entity = Shared.Get<ExchangeRate>("ExchangeRate_ETB_Today");
            var id = entity.Id;
            var response = await Client.GetAsync($"{Url}/{id}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of exchange rate
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<ExchangeRate>>();
            Assert.Equal("ExchangeRate", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(entity.CurrencyId, responseDto.CurrencyId);
            Assert.Equal(entity.ValidAsOf, responseDto.ValidAsOf);
            Assert.Equal(entity.AmountInCurrency, responseDto.AmountInCurrency);
            Assert.Equal(entity.AmountInFunctional, responseDto.AmountInFunctional);
        }

        [Fact(DisplayName = "06 Saving a ExchangeRateForSave with an existing currency and date returns a 422 Unprocessable Entity")]
        public async Task Test06()
        {
            // Prepare a exchange rate with the same code 'kg' as one that has been saved already
            var list = new List<ExchangeRateForSave> {
                new ExchangeRateForSave
                {
                    CurrencyId = "ETB",
                    ValidAsOf = DateTime.Today,
                    AmountInCurrency = 29m,
                    AmountInFunctional = 1m
                }
            };

            // Call the API
            var response = await Client.PostAsJsonAsync(Url, list);

            // Assert that the response status code is 422 unprocessable entity (validation errors)
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            // Confirm that the result is a well-formed validation errors structure
            var errors = await response.Content.ReadAsAsync<ValidationErrors>();

            // Assert that it contains a validation key pointing to the Code property
            string expectedKey = "[0]";
            Assert.True(errors.ContainsKey(expectedKey), $"Expected error key '{expectedKey}' was not found");

            // Assert that it contains a useful error message in English
            var message = errors["[0]"].Single();
            Assert.Contains("duplicated", message.ToLower());
        }

        [Fact(DisplayName = "07 Deleting an existing exchange rate Id returns a 200 OK")]
        public async Task Test08()
        {
            await GrantPermissionToSecurityAdministrator(View, Constants.Delete, null);

            // Get the Id
            var entity = Shared.Get<ExchangeRate>("ExchangeRate_ETB_Today");
            var id = entity.Id;

            // Query the delete API
            var deleteResponse = await Client.DeleteAsync($"{Url}/{id}");

            Output.WriteLine(await deleteResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact(DisplayName = "08 Getting an Id that was just deleted returns a 404 Not Found")]
        public async Task Test09()
        {
            // Get the Id
            var entity = Shared.Get<ExchangeRate>("ExchangeRate_ETB_Today");
            var id = entity.Id;

            // Verify that the id was deleted by calling get        
            var getResponse = await Client.GetAsync($"{Url}/{id}");

            // Assert that the response is correct
            Output.WriteLine(await getResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
