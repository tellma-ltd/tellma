using BSharp.Controllers.DTO;
using BSharp.IntegrationTests.Utilities;
using BSharp.Services.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Xunit;

namespace BSharp.IntegrationTests.Scenario_01
{
    public partial class Scenario_01
    {
        public const string MeasurementUnits = "01 - Measurement Units";

        [Trait(Testing, MeasurementUnits)]
        [Fact(DisplayName = "000 - Getting all measurement units before granting permissions returns a 403 Forbidden response")]
        public async Task Test00000()
        {
            var response = await _client.GetAsync($"/api/measurement-units");

            // Call the API
            _output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Trait(Testing, MeasurementUnits)]
        [Fact(DisplayName = "001 - Getting all measurement units before creating any returns a 200 OK empty collection")]
        public async Task Test0000()
        {
            await GrantPermissionToSecurityAdministrator("measurement-units", Constants.Update, "Id lt 100000");

            // Call the API
            var response = await _client.GetAsync($"/api/measurement-units");
            _output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<MeasurementUnit>>();

            // Assert the result makes sense
            Assert.Equal("MeasurementUnits", responseData.CollectionName);

            Assert.Equal(0, responseData.TotalCount);
            Assert.Empty(responseData.Data);
        }

        [Trait(Testing, MeasurementUnits)]
        [Fact(DisplayName = "002 - Getting a non-existent measurement unit id returns a 404 Not Found")]
        public async Task Test0001()
        {
            int nonExistentId = 1;
            var response = await _client.GetAsync($"/api/measurement-units/{nonExistentId}");

            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait(Testing, MeasurementUnits)]
        [Fact(DisplayName = "003 - Saving a single well-formed MeasurementUnitForSave returns a 200 OK result")]
        public async Task Test0002()
        {
            // Prepare a well formed entity
            var dtoForSave = new MeasurementUnitForSave
            {
                EntityState = "Inserted",
                Name = "KG",
                Name2 = "كج",
                Code = "kg",
                UnitType = "Mass",
                BaseAmount = 1,
                UnitAmount = 1
            };

            // Save it
            var dtosForSave = new List<MeasurementUnitForSave> { dtoForSave };
            var response = await _client.PostAsJsonAsync($"/api/measurement-units", dtosForSave);

            // Assert that the response status code is a happy 200 OK
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert that the response is well-formed singleton of MeasurementUnit
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<MeasurementUnit>>();
            Assert.Single(responseData.Data);

            // Assert that the result matches the saved entity
            Assert.Equal("MeasurementUnits", responseData.CollectionName);

            var responseDto = responseData.Data.FirstOrDefault();
            Assert.NotNull(responseDto?.Id);
            Assert.Equal(dtoForSave.Name, responseDto.Name);
            Assert.Equal(dtoForSave.Name2, responseDto.Name2);
            Assert.Equal(dtoForSave.Code, responseDto.Code);
            Assert.Equal(dtoForSave.UnitType, responseDto.UnitType);
            Assert.Equal(dtoForSave.BaseAmount, responseDto.BaseAmount);
            Assert.Equal(dtoForSave.UnitAmount, responseDto.UnitAmount);
            

            _shared.SetItem("MeasurementUnit_kg", responseDto);
        }

        [Trait(Testing, MeasurementUnits)]
        [Fact(DisplayName = "004 - Getting the Id of the MeasurementUnitForSave just saved returns a 200 OK result")]
        public async Task Test0003()
        {
            // Query the API for the Id that was just returned from the Save
            var entity = _shared.GetItem<MeasurementUnit>("MeasurementUnit_kg");
            var id = entity.Id;
            var response = await _client.GetAsync($"/api/measurement-units/{id}");

            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of measurement unit
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<MeasurementUnit>>();
            Assert.Equal("MeasurementUnits", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Entity;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Equal(entity.Name2, responseDto.Name2);
            Assert.Equal(entity.Code, responseDto.Code);
            Assert.Equal(entity.UnitType, responseDto.UnitType);
            Assert.Equal(entity.BaseAmount, responseDto.BaseAmount);
            Assert.Equal(entity.UnitAmount, responseDto.UnitAmount);
        }

        [Trait(Testing, MeasurementUnits)]
        [Fact(DisplayName = "005 - Saving a MeasurementUnitForSave with an existing code returns a 422 Unprocessable Entity")]
        public async Task Test0004()
        {
            // Prepare a unit with the same code 'kg' as one that has been saved already
            var list = new List<MeasurementUnitForSave> {
                new MeasurementUnitForSave
                {
                    EntityState = "Inserted",
                    Name = "Another Name",
                    Name2 = "Another Name",
                    Code = "kg",
                    UnitType = "Mass",
                    BaseAmount = 1,
                    UnitAmount = 1
                }
            };

            // Call the API
            var response = await _client.PostAsJsonAsync($"/api/measurement-units", list);

            // Assert that the response status code is 422 unprocessable entity (validation errors)
            _output.WriteLine(await response.Content.ReadAsStringAsync());
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

        [Trait(Testing, MeasurementUnits)]
        [Fact(DisplayName = "006 - Saving a MeasurementUnitForSave trims string fields with trailing or leading spaces")]
        public async Task Test0005()
        {
            // Prepare a DTO for save, that contains leading and 
            // trailing spaces in some string properties
            var dtoForSave = new MeasurementUnitForSave
            {
                EntityState = "Inserted",
                Name = "  KM", // Leading space
                Name2 = "كم",
                Code = "km  ", // Trailing space
                UnitType = "Mass",
                BaseAmount = 1,
                UnitAmount = 1
            };

            // Call the API
            var response = await _client.PostAsJsonAsync($"/api/measurement-units", new List<MeasurementUnitForSave> { dtoForSave });

            // Confirm that the response is well-formed
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<MeasurementUnit>>();
            var responseDto = responseData.Data.FirstOrDefault();

            // Confirm the entity was saved
            Assert.NotNull(responseDto.Id);

            // Confirm that the leading and trailing spaces have been trimmed
            Assert.Equal(dtoForSave.Name?.Trim(), responseDto.Name);
            Assert.Equal(dtoForSave.Code?.Trim(), responseDto.Code);

            // share the entity, for the subsequent delete test
            _shared.SetItem("MeasurementUnit_km", responseDto);
        }

        [Trait(Testing, MeasurementUnits)]
        [Fact(DisplayName = "007 - Deleting an existing measurement unit Id returns a 200 OK")]
        public async Task Test0006()
        {
            // Get the Id
            var entity = _shared.GetItem<MeasurementUnit>("MeasurementUnit_km");
            var id = entity.Id.Value;

            // Query the delete API
            var msg = new HttpRequestMessage(HttpMethod.Delete, $"/api/measurement-units");
            msg.Content = new ObjectContent<List<int>>(new List<int> { id }, new JsonMediaTypeFormatter());
            var deleteResponse = await _client.SendAsync(msg);

            _output.WriteLine(await deleteResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Trait(Testing, MeasurementUnits)]
        [Fact(DisplayName = "008 - Getting an Id that was just deleted returns a 404 Not Found")]
        public async Task Test0007()
        {
            // Get the Id
            var entity = _shared.GetItem<MeasurementUnit>("MeasurementUnit_km");
            var id = entity.Id.Value;

            // Verify that the id was deleted by calling get        
            var getResponse = await _client.GetAsync($"/api/measurement-units/{id}");

            // Assert that the response is correct
            _output.WriteLine(await getResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Trait(Testing, MeasurementUnits)]
        [Fact(DisplayName = "009 - Deactivating an active measurement unit returns a 200 OK inactive entity")]
        public async Task Test0008()
        {
            // Get the Id
            var entity = _shared.GetItem<MeasurementUnit>("MeasurementUnit_kg");
            var id = entity.Id.Value;

            // Call the API
            var response = await _client.PutAsJsonAsync($"/api/measurement-units/deactivate", new List<int>() { id });
            
            // Assert that the response status code is correct
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<MeasurementUnit>>();
            Assert.Single(responseData.Data);
            var responseDto = responseData.Data.Single();

            // Confirm that the entity was deactivated
            Assert.False(responseDto.IsActive, "The Measurement Unit was not deactivated");
        }

        [Trait(Testing, MeasurementUnits)]
        [Fact(DisplayName = "010 - Activating an inactive measurement unit returns a 200 OK active entity")]
        public async Task Test0009()
        {
            // Get the Id
            var entity = _shared.GetItem<MeasurementUnit>("MeasurementUnit_kg");
            var id = entity.Id.Value;

            // Call the API
            var response = await _client.PutAsJsonAsync($"/api/measurement-units/activate", new List<int>() { id });

            // Assert that the response status code is correct
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<MeasurementUnit>>();
            Assert.Single(responseData.Data);
            var responseDto = responseData.Data.Single();

            // Confirm that the entity was activated
            Assert.True(responseDto.IsActive, "The Measurement Unit was not activated");
        }

        // TODO add Import/Export tests
    }
}
