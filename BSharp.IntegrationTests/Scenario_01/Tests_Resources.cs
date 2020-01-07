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
    public class Tests_09_Resources : Scenario_01
    {
        public Tests_09_Resources(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        public readonly string _baseAddress = "resources";
        public readonly string _definitionId = "currencies";

        public string View => $"{_baseAddress}/{_definitionId}"; // For permissions
        public string GenericlUrl => $"/api/{_baseAddress}"; // For querying generic resources
        public string Url => $"/api/{_baseAddress}/{_definitionId}"; // For querying and updating specific resource definition

        [Fact(DisplayName = "01 Getting a specific type of resource before granting permissions returns a 403 Forbidden response")]
        public async Task Test01()
        {
            var response = await Client.GetAsync(Url);

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }


        [Fact(DisplayName = "02 Getting all resources before granting permissions returns a 403 Forbidden response")]
        public async Task Test02()
        {
            var response = await Client.GetAsync(GenericlUrl);

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact(DisplayName = "03 Getting resources of a specific type before creating any returns a 200 OK empty collection")]
        public async Task Test03()
        {
            await GrantPermissionToSecurityAdministrator(View, Constants.Update, "Id gt -1");

            // Call the API
            var response = await Client.GetAsync(Url);
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<Resource>>();

            // Assert the result makes sense
            Assert.Equal("Resource", responseData.CollectionName);
        }

        [Fact(DisplayName = "04 Getting all resources before creating any returns a 200 OK empty collection")]
        public async Task Test04()
        {
            // Call the API
            var response = await Client.GetAsync(GenericlUrl);
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<Resource>>();

            // Assert the result makes sense
            Assert.Equal("Resource", responseData.CollectionName);
        }

        [Fact(DisplayName = "05 Getting a non-existent specific resource id returns a 404 Not Found")]
        public async Task Test05()
        {
            int nonExistentId = 5000;
            var response = await Client.GetAsync($"{Url}/{nonExistentId}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "06 Saving a single well-formed ResourceForSave returns a 200 OK result")]
        public async Task Test06()
        {
            // Prepare a well formed entity
            var dtoForSave = new ResourceForSave
            {
                ResourceClassificationId = Shared.Get<ResourceClassification>("ResourceClassification_SM").Id,
                Name = "HR 1000x0.8",
                Name2 = "HR 1000x0.8",
                Code = "HR 1000x0.8",
                MassUnitId = Shared.Get<MeasurementUnit>("MeasurementUnit_kg").Id,
            };

            // Save it
            var dtosForSave = new List<ResourceForSave> { dtoForSave };
            var response = await Client.PostAsJsonAsync(Url, dtosForSave);

            // Assert that the response status code is a happy 200 OK
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert that the response is well-formed singleton of Resource
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Resource>>();
            Assert.Single(responseData.Result);

            // Assert that the result matches the saved entity
            Assert.Equal("Resource", responseData.CollectionName);

            // Retreve the entity from the entities
            var responseDto = responseData.Result.SingleOrDefault();

            Assert.NotNull(responseDto?.Id);
            Assert.Equal(_definitionId, responseDto.DefinitionId);
            Assert.Equal(dtoForSave.Name, responseDto.Name);
            Assert.Equal(dtoForSave.Name2, responseDto.Name2);
            Assert.Equal(dtoForSave.Code, responseDto.Code);
            Assert.Equal(dtoForSave.MassUnitId, responseDto.MassUnitId);

            Shared.Set("Resource_HR1000x0.8", responseDto);
        }

        [Fact(DisplayName = "07 Getting the Id of the specific ResourceForSave just saved returns a 200 OK result")]
        public async Task Test07()
        {
            // Query the API for the Id that was just returned from the Save
            var entity = Shared.Get<Resource>("Resource_HR1000x0.8");
            var id = entity.Id;
            var response = await Client.GetAsync($"{Url}/{id}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of resource
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<Resource>>();
            Assert.Equal("Resource", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(_definitionId, responseDto.DefinitionId);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Equal(entity.Name2, responseDto.Name2);
            Assert.Equal(entity.Code, responseDto.Code);
            Assert.Equal(entity.MassUnitId, responseDto.MassUnitId);
        }

        [Fact(DisplayName = "08 Saving a ResourceForSave with an existing code returns a 422 Unprocessable Entity")]
        public async Task Test08()
        {
            // Prepare a record with the same code 'kg' as one that has been saved already
            var list = new List<ResourceForSave> {
                new ResourceForSave
                {
                ResourceClassificationId = Shared.Get<ResourceClassification>("ResourceClassification_SM").Id,
                    Name = "Another Name",
                    Name2 = "Another Name",
                    Code = "HR 1000x0.8",
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
            string expectedKey = "[0].Code";
            Assert.True(errors.ContainsKey(expectedKey), $"Expected error key '{expectedKey}' was not found");

            // Assert that it contains a useful error message in English
            var message = errors["[0].Code"].Single();
            Assert.Contains("already used", message.ToLower());
        }

        [Fact(DisplayName = "09 Saving a ResourceForSave trims string fields with trailing or leading spaces")]
        public async Task Test09()
        {
            // Prepare a DTO for save, that contains leading and 
            // trailing spaces in some string properties
            var dtoForSave = new ResourceForSave
            {
                ResourceClassificationId = Shared.Get<ResourceClassification>("ResourceClassification_SM").Id,
                Name = "  HR 1000x0.9", // Leading space
                Name2 = "HR 1000x0.9",
                Code = "0HR 1000x0.9  ", // Trailing space
            };

            // Call the API
            var response = await Client.PostAsJsonAsync(Url, new List<ResourceForSave> { dtoForSave });
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Confirm that the response is well-formed
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Resource>>();
            var responseDto = responseData.Result.FirstOrDefault();

            // Confirm the entity was saved
            Assert.NotEqual(0, responseDto.Id);

            // Confirm that the leading and trailing spaces have been trimmed
            Assert.Equal(dtoForSave.Name?.Trim(), responseDto.Name);
            Assert.Equal(dtoForSave.Code?.Trim(), responseDto.Code);

            // share the entity, for the subsequent delete test
            Shared.Set("Resource_HR1000x0.9", responseDto);
        }

        [Fact(DisplayName = "10 Deleting an existing resource Id returns a 200 OK")]
        public async Task Test10()
        {
            await GrantPermissionToSecurityAdministrator(View, Constants.Delete, null);

            // Get the Id
            var entity = Shared.Get<Resource>("Resource_HR1000x0.9");
            var id = entity.Id;

            // Query the delete API
            var deleteResponse = await Client.DeleteAsync($"{Url}/{id}");

            Output.WriteLine(await deleteResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact(DisplayName = "11 Getting an Id that was just deleted returns a 404 Not Found")]
        public async Task Test11()
        {
            // Get the Id
            var entity = Shared.Get<Resource>("Resource_HR1000x0.9");
            var id = entity.Id;

            // Verify that the id was deleted by calling get        
            var getResponse = await Client.GetAsync($"{Url}/{id}");

            // Assert that the response is correct
            Output.WriteLine(await getResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact(DisplayName = "12 Deactivating an active resource returns a 200 OK inactive entity")]
        public async Task Test12()
        {
            await GrantPermissionToSecurityAdministrator(View, "IsActive", null);

            // Get the Id
            var entity = Shared.Get<Resource>("Resource_HR1000x0.8");
            var id = entity.Id;

            // Call the API
            var response = await Client.PutAsJsonAsync($"{Url}/deactivate", new List<int>() { id });

            // Assert that the response status code is correct
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Resource>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was deactivated
            Assert.False(responseDto.IsActive, "The resource was not deactivated");
        }

        [Fact(DisplayName = "13 Activating an inactive resource returns a 200 OK active entity")]
        public async Task Test13()
        {
            // Get the Id
            var entity = Shared.Get<Resource>("Resource_HR1000x0.8");
            var id = entity.Id;

            // Call the API
            var response = await Client.PutAsJsonAsync($"{Url}/activate", new List<int>() { id });

            // Assert that the response status code is correct
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Resource>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was activated
            Assert.True(responseDto.IsActive, "The resource was not activated");
        }

        [Fact(DisplayName = "14 Using Select argument works as expected")]
        public async Task Test14()
        {
            // Get the Id
            var entity = Shared.Get<Resource>("Resource_HR1000x0.8");
            var id = entity.Id;

            var response = await Client.GetAsync($"{Url}/{id}?select=Name");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of resource
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<Resource>>();
            Assert.Equal("Resource", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Null(responseDto.Name2);
            Assert.Null(responseDto.Code);
        }
    }
}
