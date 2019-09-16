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
    public class Tests_ResourceLookups : Scenario_01
    {
        public Tests_ResourceLookups(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        public readonly string _definitionId = "colors";
        public string ResourceLookupsUrl => $"/api/resource-lookups/{_definitionId}";

        [Fact(DisplayName = "01 Getting all resource lookups before granting permissions returns a 403 Forbidden response")]
        public async Task Test01()
        {
            var response = await Client.GetAsync(ResourceLookupsUrl);

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact(DisplayName = "02 Getting all resource lookups before creating any returns a 200 OK empty collection")]
        public async Task Test02()
        {
            await GrantPermissionToSecurityAdministrator(_definitionId, Constants.Update, "Id lt 100000");

            // Call the API
            var response = await Client.GetAsync(ResourceLookupsUrl);
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<ResourceLookup>>();

            // Assert the result makes sense
            Assert.Equal("ResourceLookup", responseData.CollectionName);

            Assert.Equal(0, responseData.TotalCount);
            Assert.Empty(responseData.Result);
        }

        [Fact(DisplayName = "03 Getting a non-existent resource lookup id returns a 404 Not Found")]
        public async Task Test03()
        {
            int nonExistentId = 1;
            var response = await Client.GetAsync($"{ResourceLookupsUrl}/{nonExistentId}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "04 Saving a single well-formed ResourceLookupForSave returns a 200 OK result")]
        public async Task Test04()
        {
            // Prepare a well formed entity
            var dtoForSave = new ResourceLookupForSave
            {
                Name = "Red",
                Name2 = "أحمر",
                Code = "01",
            };

            // Save it
            var dtosForSave = new List<ResourceLookupForSave> { dtoForSave };
            var response = await Client.PostAsJsonAsync(ResourceLookupsUrl, dtosForSave);

            // Assert that the response status code is a happy 200 OK
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert that the response is well-formed singleton of ResourceLookup
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<ResourceLookup>>();
            Assert.Single(responseData.Result);

            // Assert that the result matches the saved entity
            Assert.Equal("ResourceLookup", responseData.CollectionName);

            // Retreve the entity from the entities
            var responseDto = responseData.Result.SingleOrDefault();

            Assert.NotNull(responseDto?.Id);
            Assert.Equal(_definitionId, responseDto.ResourceLookupDefinitionId);
            Assert.Equal(dtoForSave.Name, responseDto.Name);
            Assert.Equal(dtoForSave.Name2, responseDto.Name2);
            Assert.Equal(dtoForSave.Code, responseDto.Code);


            Shared.Set("ResourceLookup_Red", responseDto);
        }

        [Fact(DisplayName = "05 Getting the Id of the ResourceLookupForSave just saved returns a 200 OK result")]
        public async Task Test05()
        {
            // Query the API for the Id that was just returned from the Save
            var entity = Shared.Get<ResourceLookup>("ResourceLookup_Red");
            var id = entity.Id;
            var response = await Client.GetAsync($"{ResourceLookupsUrl}/{id}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of resource lookup
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<ResourceLookup>>();
            Assert.Equal("ResourceLookup", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(_definitionId, responseDto.ResourceLookupDefinitionId);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Equal(entity.Name2, responseDto.Name2);
            Assert.Equal(entity.Code, responseDto.Code);
        }

        [Fact(DisplayName = "06 Saving a ResourceLookupForSave with an existing code returns a 422 Unprocessable Entity")]
        public async Task Test06()
        {
            // Prepare a unit with the same code 'kg' as one that has been saved already
            var list = new List<ResourceLookupForSave> {
                new ResourceLookupForSave
                {
                    Name = "Another Name",
                    Name2 = "Another Name",
                    Code = "01"
                }
            };

            // Call the API
            var response = await Client.PostAsJsonAsync(ResourceLookupsUrl, list);

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

        [Fact(DisplayName = "07 Saving a ResourceLookupForSave trims string fields with trailing or leading spaces")]
        public async Task Test07()
        {
            // Prepare a DTO for save, that contains leading and 
            // trailing spaces in some string properties
            var dtoForSave = new ResourceLookupForSave
            {
                Name = "  Blue", // Leading space
                Name2 = "أزرق",
                Code = "02  ", // Trailing space
            };

            // Call the API
            var response = await Client.PostAsJsonAsync(ResourceLookupsUrl, new List<ResourceLookupForSave> { dtoForSave });
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Confirm that the response is well-formed
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<ResourceLookup>>();
            var responseDto = responseData.Result.FirstOrDefault();

            // Confirm the entity was saved
            Assert.NotEqual(0, responseDto.Id);

            // Confirm that the leading and trailing spaces have been trimmed
            Assert.Equal(dtoForSave.Name?.Trim(), responseDto.Name);
            Assert.Equal(dtoForSave.Code?.Trim(), responseDto.Code);

            // share the entity, for the subsequent delete test
            Shared.Set("ResourceLookup_Blue", responseDto);
        }

        [Fact(DisplayName = "08 Deleting an existing resource lookup Id returns a 200 OK")]
        public async Task Test08()
        {
            await GrantPermissionToSecurityAdministrator(_definitionId, Constants.Delete, null);

            // Get the Id
            var entity = Shared.Get<ResourceLookup>("ResourceLookup_Blue");
            var id = entity.Id;

            // Query the delete API
            var deleteResponse = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, ResourceLookupsUrl)
            {
                Content = new ObjectContent<List<int>>(new List<int> { id }, new JsonMediaTypeFormatter())
            });

            Output.WriteLine(await deleteResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact(DisplayName = "09 Getting an Id that was just deleted returns a 404 Not Found")]
        public async Task Test09()
        {
            // Get the Id
            var entity = Shared.Get<ResourceLookup>("ResourceLookup_Blue");
            var id = entity.Id;

            // Verify that the id was deleted by calling get        
            var getResponse = await Client.GetAsync($"{ResourceLookupsUrl}/{id}");

            // Assert that the response is correct
            Output.WriteLine(await getResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact(DisplayName = "10 Deactivating an active resource lookup returns a 200 OK inactive entity")]
        public async Task Test10()
        {
            await GrantPermissionToSecurityAdministrator(_definitionId, "IsActive", null);

            // Get the Id
            var entity = Shared.Get<ResourceLookup>("ResourceLookup_Red");
            var id = entity.Id;

            // Call the API
            var response = await Client.PutAsJsonAsync($"{ResourceLookupsUrl}/deactivate", new List<int>() { id });

            // Assert that the response status code is correct
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<ResourceLookup>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was deactivated
            Assert.False(responseDto.IsActive, "The resource lookup was not deactivated");
        }

        [Fact(DisplayName = "11 Activating an inactive resource lookup returns a 200 OK active entity")]
        public async Task Test11()
        {
            // Get the Id
            var entity = Shared.Get<ResourceLookup>("ResourceLookup_Red");
            var id = entity.Id;

            // Call the API
            var response = await Client.PutAsJsonAsync($"{ResourceLookupsUrl}/activate", new List<int>() { id });

            // Assert that the response status code is correct
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<ResourceLookup>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was activated
            Assert.True(responseDto.IsActive, "The resource lookup was not activated");
        }

        [Fact(DisplayName = "12 Using Select argument works as expected")]
        public async Task Test12()
        {
            // Get the Id
            var entity = Shared.Get<ResourceLookup>("ResourceLookup_Red");
            var id = entity.Id;

            var response = await Client.GetAsync($"{ResourceLookupsUrl}/{id}?select=Name");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of resource lookup
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<ResourceLookup>>();
            Assert.Equal("ResourceLookup", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Null(responseDto.Name2);
            Assert.Null(responseDto.Code);
        }
    }
}
