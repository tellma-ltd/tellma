using BSharp.Controllers.Dto;
using BSharp.Entities;
using BSharp.IntegrationTests.Utilities;
using BSharp.Services.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace BSharp.IntegrationTests.Scenario_01
{
    public class Tests_MeasurementUnits : Scenario_01
    {
        public Tests_MeasurementUnits(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        public const string measurementUnitsUrl = "/api/measurement-units";

        [Fact(DisplayName = "01 Getting all measurement units before granting permissions returns a 403 Forbidden response")]
        public async Task Test01()
        {
            var response = await Client.GetAsync(measurementUnitsUrl);

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact(DisplayName = "02 Getting all measurement units before creating any returns a 200 OK empty collection")]
        public async Task Test02()
        {
            await GrantPermissionToSecurityAdministrator("measurement-units", Constants.Update, "Id lt 100000");

            // Call the API
            var response = await Client.GetAsync(measurementUnitsUrl);
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<MeasurementUnit>>();

            // Assert the result makes sense
            Assert.Equal("MeasurementUnit", responseData.CollectionName);

            Assert.Equal(0, responseData.TotalCount);
            Assert.Empty(responseData.Result);
        }

        [Fact(DisplayName = "03 Getting a non-existent measurement unit id returns a 404 Not Found")]
        public async Task Test03()
        {
            int nonExistentId = 1;
            var response = await Client.GetAsync($"{measurementUnitsUrl}/{nonExistentId}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "04 Saving a single well-formed MeasurementUnitForSave returns a 200 OK result")]
        public async Task Test04()
        {
            // Prepare a well formed entity
            var dtoForSave = new MeasurementUnitForSave
            {
                Name = "KG",
                Name2 = "كج",
                Description = "Kilogram",
                Description2 = "كيلوجرام",
                Code = "kg",
                UnitType = "Mass",
                BaseAmount = 1,
                UnitAmount = 1
            };

            // Save it
            var dtosForSave = new List<MeasurementUnitForSave> { dtoForSave };
            var response = await Client.PostAsJsonAsync(measurementUnitsUrl, dtosForSave);

            // Assert that the response status code is a happy 200 OK
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert that the response is well-formed singleton of MeasurementUnit
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<MeasurementUnit>>();
            Assert.Single(responseData.Result);

            // Assert that the result matches the saved entity
            Assert.Equal("MeasurementUnit", responseData.CollectionName);

            // Retreve the entity from the entities
            var responseDto = responseData.Result.SingleOrDefault();

            Assert.NotNull(responseDto?.Id);
            Assert.Equal(dtoForSave.Name, responseDto.Name);
            Assert.Equal(dtoForSave.Name2, responseDto.Name2);
            Assert.Equal(dtoForSave.Code, responseDto.Code);
            Assert.Equal(dtoForSave.UnitType, responseDto.UnitType);
            Assert.Equal(dtoForSave.BaseAmount, responseDto.BaseAmount);
            Assert.Equal(dtoForSave.UnitAmount, responseDto.UnitAmount);


            Shared.Set("MeasurementUnit_kg", responseDto);
        }

        [Fact(DisplayName = "05 Getting the Id of the MeasurementUnitForSave just saved returns a 200 OK result")]
        public async Task Test05()
        {
            // Query the API for the Id that was just returned from the Save
            var entity = Shared.Get<MeasurementUnit>("MeasurementUnit_kg");
            var id = entity.Id;
            var response = await Client.GetAsync($"{measurementUnitsUrl}/{id}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of measurement unit
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<MeasurementUnit>>();
            Assert.Equal("MeasurementUnit", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Equal(entity.Name2, responseDto.Name2);
            Assert.Equal(entity.Code, responseDto.Code);
            Assert.Equal(entity.UnitType, responseDto.UnitType);
            Assert.Equal(entity.BaseAmount, responseDto.BaseAmount);
            Assert.Equal(entity.UnitAmount, responseDto.UnitAmount);
        }

        [Fact(DisplayName = "06 Saving a MeasurementUnitForSave with an existing code returns a 422 Unprocessable Entity")]
        public async Task Test06()
        {
            // Prepare a unit with the same code 'kg' as one that has been saved already
            var list = new List<MeasurementUnitForSave> {
                new MeasurementUnitForSave
                {
                    Name = "Another Name",
                    Name2 = "Another Name",
                    Code = "kg",
                    Description = "Another Description",
                    UnitType = "Mass",
                    BaseAmount = 1,
                    UnitAmount = 1
                }
            };

            // Call the API
            var response = await Client.PostAsJsonAsync(measurementUnitsUrl, list);

            // Assert that the response status code is 422 unprocessable entity (validation errors)
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            // Confirm that the result is a well-formed validation errors structure
            var errors = await response.Content.ReadAsAsync<ValidationErrors>();

            // Assert that it contains a validation key pointing to the Code property
            string expectedKey = "[0].Code";
            Assert.True(errors.ContainsKey(expectedKey), $"Expected error key '{expectedKey}' was not found");

            // Assert that it contains a useful error message in English
            var message = errors["[0].Code"].Single();
            Assert.Contains("already used", message.ToLower());
        }

        [Fact(DisplayName = "07 Saving a MeasurementUnitForSave trims string fields with trailing or leading spaces")]
        public async Task Test07()
        {
            // Prepare a DTO for save, that contains leading and 
            // trailing spaces in some string properties
            var dtoForSave = new MeasurementUnitForSave
            {
                Name = "  KM", // Leading space
                Name2 = "كم",
                Code = "km  ", // Trailing space
                Description = "كيلومتر",
                Description2 = "Kilometer",
                UnitType = "Mass",
                BaseAmount = 1,
                UnitAmount = 1
            };

            // Call the API
            var response = await Client.PostAsJsonAsync(measurementUnitsUrl, new List<MeasurementUnitForSave> { dtoForSave });
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Confirm that the response is well-formed
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<MeasurementUnit>>();
            var responseDto = responseData.Result.FirstOrDefault();

            // Confirm the entity was saved
            Assert.NotEqual(0, responseDto.Id);

            // Confirm that the leading and trailing spaces have been trimmed
            Assert.Equal(dtoForSave.Name?.Trim(), responseDto.Name);
            Assert.Equal(dtoForSave.Code?.Trim(), responseDto.Code);

            // share the entity, for the subsequent delete test
            Shared.Set("MeasurementUnit_km", responseDto);
        }

        [Fact(DisplayName = "08 Deleting an existing measurement unit Id returns a 200 OK")]
        public async Task Test08()
        {
            await GrantPermissionToSecurityAdministrator("measurement-units", Constants.Delete, null);

            // Get the Id
            var entity = Shared.Get<MeasurementUnit>("MeasurementUnit_km");
            var id = entity.Id;

            // Query the delete API
            var msg = new HttpRequestMessage(HttpMethod.Delete, measurementUnitsUrl);
            msg.Content = new ObjectContent<List<int>>(new List<int> { id }, new JsonMediaTypeFormatter());
            var deleteResponse = await Client.SendAsync(msg);

            Output.WriteLine(await deleteResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact(DisplayName = "09 Getting an Id that was just deleted returns a 404 Not Found")]
        public async Task Test09()
        {
            // Get the Id
            var entity = Shared.Get<MeasurementUnit>("MeasurementUnit_km");
            var id = entity.Id;

            // Verify that the id was deleted by calling get        
            var getResponse = await Client.GetAsync($"{measurementUnitsUrl}/{id}");

            // Assert that the response is correct
            Output.WriteLine(await getResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact(DisplayName = "10 Deactivating an active measurement unit returns a 200 OK inactive entity")]
        public async Task Test10()
        {
            await GrantPermissionToSecurityAdministrator("measurement-units", "IsActive", null);

            // Get the Id
            var entity = Shared.Get<MeasurementUnit>("MeasurementUnit_kg");
            var id = entity.Id;

            // Call the API
            var response = await Client.PutAsJsonAsync($"{measurementUnitsUrl}/deactivate", new List<int>() { id });

            // Assert that the response status code is correct
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<MeasurementUnit>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was deactivated
            Assert.False(responseDto.IsActive, "The Measurement Unit was not deactivated");
        }

        [Fact(DisplayName = "11 Activating an inactive measurement unit returns a 200 OK active entity")]
        public async Task Test11()
        {
            // Get the Id
            var entity = Shared.Get<MeasurementUnit>("MeasurementUnit_kg");
            var id = entity.Id;

            // Call the API
            var response = await Client.PutAsJsonAsync($"{measurementUnitsUrl}/activate", new List<int>() { id });

            // Assert that the response status code is correct
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<MeasurementUnit>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was activated
            Assert.True(responseDto.IsActive, "The Measurement Unit was not activated");
        }

        [Fact(DisplayName = "12 Using Select argument works as expected")]
        public async Task Test12()
        {
            // Get the Id
            var entity = Shared.Get<MeasurementUnit>("MeasurementUnit_kg");
            var id = entity.Id;

            var response = await Client.GetAsync($"{measurementUnitsUrl}/{id}?select=Name");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of measurement unit
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<MeasurementUnit>>();
            Assert.Equal("MeasurementUnit", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Null(responseDto.Name2);
            Assert.Null(responseDto.Code);
            Assert.Null(responseDto.UnitType);
            Assert.Null(responseDto.BaseAmount);
            Assert.Null(responseDto.UnitAmount);
        }
    }
}
