using BSharp.Controllers.Dto;
using BSharp.Entities;
using BSharp.IntegrationTests.Utilities;
using BSharp.Services.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace BSharp.IntegrationTests.Scenario_01
{
    public class Tests_08_AccountTypes : Scenario_01
    {
        public Tests_08_AccountTypes(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        public readonly string _baseAddress = "account-types";

        public string Url => $"/api/{_baseAddress}";
        public string View => _baseAddress; // For permissions


        [Fact(DisplayName = "01 Getting all account types before granting permissions returns a 403 Forbidden response")]
        public async Task Test01()
        {
            var response = await Client.GetAsync(Url);

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact(DisplayName = "02 Getting all account types before creating any returns a 200 OK empty collection")]
        public async Task Test02()
        {
            await GrantPermissionToSecurityAdministrator(View, Constants.Update, "Id lt 100000");

            // Call the API
            var response = await Client.GetAsync(Url);
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<AccountType>>();

            // Assert the result makes sense
            Assert.Equal("AccountType", responseData.CollectionName);

            Assert.Equal(2, responseData.TotalCount);
            Assert.NotEmpty(responseData.Result);
        }

        [Fact(DisplayName = "03 Getting a non-existent account type id returns a 404 Not Found")]
        public async Task Test03()
        {
            int nonExistentId = 0;
            var response = await Client.GetAsync($"{Url}/{nonExistentId}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "04 Saving a single well-formed AccountTypeForSave returns a 200 OK result")]
        public async Task Test04()
        {
            var parentId = (await (await Client.GetAsync($"{Url}?filter=Code eq 'AccountsPayable'")).Content.ReadAsAsync<GetResponse<AccountType>>()).Result.Single().Id;

            // Prepare a well formed entity
            var dtoForSave = new AccountTypeForSave
            {
                Name = "Sheet Metals",
                Name2 = "صفائح المعدن",
                Code = "SM",
                ParentId = parentId,
                IsAssignable = true,
                Description = "Sheet Metals",
                Description2 = "صفائح المعدن",
                IsCurrent = true,
                IsReal = true,
                IsPersonal = false,
                IsResourceClassification = true
            };

            // Save it
            var dtosForSave = new List<AccountTypeForSave> { dtoForSave };
            var response = await Client.PostAsJsonAsync(Url, dtosForSave);

            // Assert that the response status code is a happy 200 OK
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert that the response is well-formed singleton of AccountType
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<AccountType>>();
            Assert.Single(responseData.Result);

            // Assert that the result matches the saved entity
            Assert.Equal("AccountType", responseData.CollectionName);

            // Retreve the entity from the entities
            var responseDto = responseData.Result.SingleOrDefault();

            Assert.NotNull(responseDto?.Id);
            Assert.Equal(dtoForSave.Name, responseDto.Name);
            Assert.Equal(dtoForSave.Name2, responseDto.Name2);
            Assert.Equal(dtoForSave.Description, responseDto.Description);
            Assert.Equal(dtoForSave.Description2, responseDto.Description2);
            Assert.Equal(dtoForSave.Code, responseDto.Code);
            Assert.Equal(dtoForSave.ParentId, responseDto.ParentId);
            Assert.Equal(dtoForSave.IsAssignable, responseDto.IsAssignable);
            Assert.Equal(dtoForSave.IsCurrent, responseDto.IsCurrent);
            Assert.Equal(dtoForSave.IsReal, responseDto.IsReal);
            Assert.Equal(dtoForSave.IsPersonal, responseDto.IsPersonal);
            Assert.Equal(dtoForSave.IsResourceClassification, responseDto.IsResourceClassification);


            Shared.Set("AccountType_SM", responseDto);
        }

        [Fact(DisplayName = "05 Getting the Id of the AccountTypeForSave just saved returns a 200 OK result")]
        public async Task Test05()
        {
            // Query the API for the Id that was just returned from the Save
            var entity = Shared.Get<AccountType>("AccountType_SM");
            var id = entity.Id;
            var response = await Client.GetAsync($"{Url}/{id}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of account type
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<AccountType>>();
            Assert.Equal("AccountType", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Equal(entity.Name2, responseDto.Name2);
            Assert.Equal(entity.Code, responseDto.Code);
            Assert.Equal(entity.ParentId, responseDto.ParentId);
        }

        [Fact(DisplayName = "06 Saving a AccountTypeForSave with an existing code returns a 422 Unprocessable Entity")]
        public async Task Test06()
        {
            // Prepare a record with the same code 'kg' as one that has been saved already
            var list = new List<AccountTypeForSave> {
                new AccountTypeForSave
                {
                    Name = "Another Name",
                    Name2 = "Another Name",
                    Code = "SM",
                    IsAssignable = true
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

        [Fact(DisplayName = "07 Saving a AccountTypeForSave trims string fields with trailing or leading spaces")]
        public async Task Test07()
        {
            // Prepare a DTO for save, that contains leading and 
            // trailing spaces in some string properties
            var dtoForSave = new AccountTypeForSave
            {
                Name = "  Hollow Section", // Leading space
                Name2 = "مقطع أجوف",
                Code = "HS  ", // Trailing space
                ParentId = null,
                IsAssignable = false
            };

            // Call the API
            var response = await Client.PostAsJsonAsync(Url, new List<AccountTypeForSave> { dtoForSave });
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Confirm that the response is well-formed
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<AccountType>>();
            var responseDto = responseData.Result.FirstOrDefault();

            // Confirm the entity was saved
            Assert.NotEqual(0, responseDto.Id);

            // Confirm that the leading and trailing spaces have been trimmed
            Assert.Equal(dtoForSave.Name?.Trim(), responseDto.Name);
            Assert.Equal(dtoForSave.Code?.Trim(), responseDto.Code);

            // share the entity, for the subsequent delete test
            Shared.Set("AccountType_HS", responseDto);
        }

        [Fact(DisplayName = "08 Deleting an existing account type Id returns a 200 OK")]
        public async Task Test08()
        {
            await GrantPermissionToSecurityAdministrator(View, Constants.Delete, null);

            // Get the Id
            var entity = Shared.Get<AccountType>("AccountType_HS");
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
            var entity = Shared.Get<AccountType>("AccountType_HS");
            var id = entity.Id;

            // Verify that the id was deleted by calling get        
            var getResponse = await Client.GetAsync($"{Url}/{id}");

            // Assert that the response is correct
            Output.WriteLine(await getResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact(DisplayName = "10 Deactivating an active account type returns a 200 OK inactive entity")]
        public async Task Test10()
        {
            await GrantPermissionToSecurityAdministrator(View, "IsActive", null);

            // Get the Id
            var entity = Shared.Get<AccountType>("AccountType_SM");
            var id = entity.Id;

            // Call the API
            var response = await Client.PutAsJsonAsync($"{Url}/deactivate", new List<int>() { id });

            // Assert that the response status code is correct
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<AccountType>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was deactivated
            Assert.False(responseDto.IsActive, "The account type was not deactivated");
        }

        [Fact(DisplayName = "11 Activating an inactive account type returns a 200 OK active entity")]
        public async Task Test11()
        {
            // Get the Id
            var entity = Shared.Get<AccountType>("AccountType_SM");
            var id = entity.Id;

            // Call the API
            var response = await Client.PutAsJsonAsync($"{Url}/activate", new List<int>() { id });

            // Assert that the response status code is correct
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<AccountType>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was activated
            Assert.True(responseDto.IsActive, "The account type was not activated");
        }

        [Fact(DisplayName = "12 Using Select argument works as expected")]
        public async Task Test12()
        {
            // Get the Id
            var entity = Shared.Get<AccountType>("AccountType_SM");
            var id = entity.Id;

            var response = await Client.GetAsync($"{Url}/{id}?select=Name");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of account type
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<AccountType>>();
            Assert.Equal("AccountType", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Null(responseDto.Name2);
            Assert.Null(responseDto.Code);
            Assert.Null(responseDto.ParentId);
        }
    }
}
