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
    public class Tests_07_AccountClassifications : Scenario_01
    {
        public Tests_07_AccountClassifications(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        public readonly string _baseAddress = "account-classifications";

        public string Url => $"/api/{_baseAddress}"; // For querying and updating specific account definition
        public string ViewId => $"{_baseAddress}"; // For permissions


        [Fact(DisplayName = "01 Getting all account classifications before granting permissions returns a 403 Forbidden response")]
        public async Task Test01()
        {
            var response = await Client.GetAsync(Url);

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact(DisplayName = "02 Getting all account classifications before creating any returns a 200 OK empty collection")]
        public async Task Test02()
        {
            await GrantPermissionToSecurityAdministrator(ViewId, Constants.Update, "Id ge 0");

            // Call the API
            var response = await Client.GetAsync(Url);
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<AccountClassification>>();

            // Assert the result makes sense
            Assert.Equal("AccountClassification", responseData.CollectionName);

            Assert.Equal(0, responseData.TotalCount);
            Assert.Empty(responseData.Result);
        }

        [Fact(DisplayName = "03 Getting a non-existent account classification id returns a 404 Not Found")]
        public async Task Test03()
        {
            int nonExistentId = 1;
            var response = await Client.GetAsync($"{Url}/{nonExistentId}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "04 Saving a single well-formed AccountClassificationForSave returns a 200 OK result")]
        public async Task Test04()
        {
            // Prepare a well formed entity
            var dtoForSaveParent = new AccountClassificationForSave
            {
                Name = "Parent Account",
                Name2 = "الحساب الأصل",
                Code = "110"
            };

            // Prepare a well formed entity
            var dtoForSaveChild = new AccountClassificationForSave
            {
                Name = "Child Account",
                Name2 = "حساب فرعي",
                Code = "1101"
            };

            // Save it
            var dtosForSave = new List<AccountClassificationForSave> { dtoForSaveParent, dtoForSaveChild };
            var response = await Client.PostAsJsonAsync(Url, dtosForSave);

            // Assert that the response status code is a happy 200 OK
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert that the response is well-formed singleton of AccountClassification
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<AccountClassification>>();
            Assert.Equal(2, responseData.Result.Count());

            // Assert that the result matches the saved entity
            Assert.Equal("AccountClassification", responseData.CollectionName);

            // Retreve the entity from the entities
            var responseDtoParent = responseData.Result.FirstOrDefault();

            Assert.NotNull(responseDtoParent?.Id);
            Assert.Equal(dtoForSaveParent.Name, responseDtoParent.Name);
            Assert.Equal(dtoForSaveParent.Name2, responseDtoParent.Name2);
            Assert.Equal(dtoForSaveParent.Code, responseDtoParent.Code);


            var responseDtoChild = responseData.Result.LastOrDefault();

            Assert.NotNull(responseDtoParent?.Id);
            Assert.Equal(dtoForSaveChild.Name, responseDtoChild.Name);
            Assert.Equal(dtoForSaveChild.Name2, responseDtoChild.Name2);
            Assert.Equal(dtoForSaveChild.Code, responseDtoChild.Code);


            Shared.Set("AccountClassification_Parent", responseDtoParent);
            Shared.Set("AccountClassification_Child", responseDtoChild);
        }

        [Fact(DisplayName = "05 Getting the Id of the AccountClassificationForSave just saved returns a 200 OK result")]
        public async Task Test05()
        {
            // Query the API for the Id that was just returned from the Save
            var entity = Shared.Get<AccountClassification>("AccountClassification_Child");
            var id = entity.Id;
            var response = await Client.GetAsync($"{Url}/{id}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of account classification
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<AccountClassification>>();
            Assert.Equal("AccountClassification", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Equal(entity.Name2, responseDto.Name2);
            Assert.Equal(entity.Code, responseDto.Code);
            Assert.Equal(entity.ParentId, responseDto.ParentId);
        }

        [Fact(DisplayName = "06 Saving a AccountClassificationForSave with an existing code returns a 422 Unprocessable Entity")]
        public async Task Test06()
        {
            // Prepare a unit with the same code 'kg' as one that has been saved already
            var list = new List<AccountClassificationForSave> {
                new AccountClassificationForSave
                {
                    Name = "Another Name",
                    Name2 = "Another Name",
                    Code = "110"
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

        [Fact(DisplayName = "07 Saving a AccountClassificationForSave trims string fields with trailing or leading spaces")]
        public async Task Test07()
        {
            // Prepare a DTO for save, that contains leading and 
            // trailing spaces in some string properties
            var dtoForSave = new AccountClassificationForSave
            {
                Name = "  Child Account 2", // Leading space
                Name2 = "حساب فرعي 2",
                Code = "1102  ", // Trailing space
            };

            // Call the API
            var response = await Client.PostAsJsonAsync(Url, new List<AccountClassificationForSave> { dtoForSave });
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Confirm that the response is well-formed
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<AccountClassification>>();
            var responseDto = responseData.Result.FirstOrDefault();

            // Confirm the entity was saved
            Assert.NotEqual(0, responseDto.Id);

            // Confirm that the leading and trailing spaces have been trimmed
            Assert.Equal(dtoForSave.Name?.Trim(), responseDto.Name);
            Assert.Equal(dtoForSave.Code?.Trim(), responseDto.Code);

            // share the entity, for the subsequent delete test
            Shared.Set("AccountClassification_Child2", responseDto);
        }

        [Fact(DisplayName = "08 Deleting an existing account classification Id returns a 200 OK")]
        public async Task Test08()
        {
            await GrantPermissionToSecurityAdministrator(ViewId, Constants.Delete, null);

            // Get the Id
            var entity = Shared.Get<AccountClassification>("AccountClassification_Child2");
            var id = entity.Id;

            // Query the delete API
            var msg = new HttpRequestMessage(HttpMethod.Delete, Url);
            msg.Content = new ObjectContent<List<int>>(new List<int> { id }, new JsonMediaTypeFormatter());
            var deleteResponse = await Client.SendAsync(msg);

            Output.WriteLine(await deleteResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact(DisplayName = "09 Getting an Id that was just deleted returns a 404 Not Found")]
        public async Task Test09()
        {
            // Get the Id
            var entity = Shared.Get<AccountClassification>("AccountClassification_Child2");
            var id = entity.Id;

            // Verify that the id was deleted by calling get        
            var getResponse = await Client.GetAsync($"{Url}/{id}");

            // Assert that the response is correct
            Output.WriteLine(await getResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact(DisplayName = "10 Deactivating an active account classification returns a 200 OK inactive entity")]
        public async Task Test10()
        {
            await GrantPermissionToSecurityAdministrator(ViewId, "IsDeprecated", null);

            // Get the Id
            var entity = Shared.Get<AccountClassification>("AccountClassification_Child");
            var id = entity.Id;

            // Call the API
            var response = await Client.PutAsJsonAsync($"{Url}/deactivate", new List<int>() { id });

            // Assert that the response status code is correct
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<AccountClassification>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was deactivated
            Assert.True(responseDto.IsDeprecated, "The account classification was not deprecated");
        }

        [Fact(DisplayName = "11 Activating an inactive account classification returns a 200 OK active entity")]
        public async Task Test11()
        {
            // Get the Id
            var entity = Shared.Get<AccountClassification>("AccountClassification_Child");
            var id = entity.Id;

            // Call the API
            var response = await Client.PutAsJsonAsync($"{Url}/activate", new List<int>() { id });

            // Assert that the response status code is correct
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<AccountClassification>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was activated
            Assert.False(responseDto.IsDeprecated, "The account classification was not activated");
        }

        [Fact(DisplayName = "12 Using Select argument works as expected")]
        public async Task Test12()
        {
            // Get the Id
            var entity = Shared.Get<AccountClassification>("AccountClassification_Child");
            var id = entity.Id;

            var response = await Client.GetAsync($"{Url}/{id}?select=Name");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of account classification
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<AccountClassification>>();
            Assert.Equal("AccountClassification", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Null(responseDto.Name2);
            Assert.Null(responseDto.Code);
            Assert.Null(responseDto.ParentId);
        }
    }
}
