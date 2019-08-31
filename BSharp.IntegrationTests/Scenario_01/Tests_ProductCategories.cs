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

namespace BSharp.IntegrationTests.Scenario_01
{
    public partial class Scenario_01
    {
        public const string ProductCategories = "08 - Product Categories";

        [Trait(Testing, ProductCategories)]
        [Fact(DisplayName = "000 - Getting all product categories before granting permissions returns a 403 Forbidden response")]
        public async Task Test35000()
        {
            var response = await Client.GetAsync($"/api/product-categories");

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Trait(Testing, ProductCategories)]
        [Fact(DisplayName = "001 - Getting all product categories before creating any returns a 200 OK empty collection")]
        public async Task Test3500()
        {
            await GrantPermissionToSecurityAdministrator("product-categories", Constants.Update, "Id lt 100000");

            // Call the API
            var response = await Client.GetAsync($"/api/product-categories");
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<ProductCategory>>();

            // Assert the result makes sense
            Assert.Equal("ProductCategory", responseData.CollectionName);

            Assert.Equal(0, responseData.TotalCount);
            Assert.Empty(responseData.Result);
        }

        [Trait(Testing, ProductCategories)]
        [Fact(DisplayName = "002 - Getting a non-existent product category id returns a 404 Not Found")]
        public async Task Test3501()
        {
            int nonExistentId = 1;
            var response = await Client.GetAsync($"/api/product-categories/{nonExistentId}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait(Testing, ProductCategories)]
        [Fact(DisplayName = "003 - Saving a single well-formed ProductCategoryForSave returns a 200 OK result")]
        public async Task Test3502()
        {
            // Prepare a well formed entity
            var dtoForSave = new ProductCategoryForSave
            {
                Name = "All",
                Name2 = "الجميع",
                Name3 = "tout",
                Code = "all"
            };

            // Save it
            var dtosForSave = new List<ProductCategoryForSave> { dtoForSave };
            var response = await Client.PostAsJsonAsync($"/api/product-categories", dtosForSave);

            // Assert that the response status code is a happy 200 OK
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert that the response is well-formed singleton of ProductCategory
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<ProductCategory>>();
            Assert.Single(responseData.Result);

            // Assert that the result matches the saved entity
            Assert.Equal("ProductCategory", responseData.CollectionName);

            var responseDto = responseData.Result.FirstOrDefault();
            Assert.NotEqual(0, responseDto.Id);
            Assert.Equal(dtoForSave.Name, responseDto.Name);
            Assert.Equal(dtoForSave.Name2, responseDto.Name2);
            Assert.Equal(dtoForSave.Name3, responseDto.Name3);
            Assert.Equal(dtoForSave.Code, responseDto.Code);
            Assert.Equal(dtoForSave.ParentId, responseDto.ParentId);

            Shared.Set("ProductCategory_all", responseDto);
        }

        [Trait(Testing, ProductCategories)]
        [Fact(DisplayName = "004 - Getting the Id of the ProductCategoryForSave just saved returns a 200 OK result")]
        public async Task Test3503()
        {
            // Query the API for the Id that was just returned from the Save
            var entity = Shared.Get<ProductCategory>("ProductCategory_all");
            var id = entity.Id;
            var response = await Client.GetAsync($"/api/product-categories/{id}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of product category
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<ProductCategory>>();
            Assert.Equal("ProductCategory", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Equal(entity.Name2, responseDto.Name2);
            Assert.Equal(entity.Name3, responseDto.Name3);
            Assert.Equal(entity.Code, responseDto.Code);
            Assert.Equal(entity.ParentId, responseDto.ParentId);
        }

        [Trait(Testing, ProductCategories)]
        [Fact(DisplayName = "005 - Saving a ProductCategoryForSave with an existing code returns a 422 Unprocessable Entity")]
        public async Task Test3504()
        {
            // Prepare a product category with the same code 'all' as one that has been saved already
            var list = new List<ProductCategoryForSave> {
                new ProductCategoryForSave
                {
                    Name = "Another Name",
                    Name2 = "Another Name",
                    Name3 = "Another Name",
                    Code = "all",
                }
            };

            // Call the API
            var response = await Client.PostAsJsonAsync($"/api/product-categories", list);

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

        [Trait(Testing, ProductCategories)]
        [Fact(DisplayName = "006 - Saving a ProductCategoryForSave trims string fields with trailing or leading spaces")]
        public async Task Test3505()
        {
            // Prepare a DTO for save, that contains leading and 
            // trailing spaces in some string properties
            var dtoForSave = new ProductCategoryForSave
            {
                Name = "  Personal Care", // Leading space
                Name2 = "العناية الشخصية",
                Name3 = "soins personnels",
                Code = "pc  ", // Trailing space
            };

            // Call the API
            var response = await Client.PostAsJsonAsync($"/api/product-categories", new List<ProductCategoryForSave> { dtoForSave });

            // Confirm that the response is well-formed
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<ProductCategory>>();
            var responseDto = responseData.Result.FirstOrDefault();

            // Confirm the entity was saved
            Assert.NotEqual(0, responseDto.Id);

            // Confirm that the leading and trailing spaces have been trimmed
            Assert.Equal(dtoForSave.Name?.Trim(), responseDto.Name);
            Assert.Equal(dtoForSave.Code?.Trim(), responseDto.Code);

            // share the entity, for the subsequent delete test
            Shared.Set("ProductCategory_pc", responseDto);
        }

        [Trait(Testing, ProductCategories)]
        [Fact(DisplayName = "007 - Deleting an existing product category Id returns a 200 OK")]
        public async Task Test3506()
        {
            // Get the Id
            var entity = Shared.Get<ProductCategory>("ProductCategory_pc");
            var id = entity.Id;

            // Query the delete API
            var msg = new HttpRequestMessage(HttpMethod.Delete, $"/api/product-categories");
            msg.Content = new ObjectContent<List<int>>(new List<int> { id }, new JsonMediaTypeFormatter());
            var deleteResponse = await Client.SendAsync(msg);

            Output.WriteLine(await deleteResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Trait(Testing, ProductCategories)]
        [Fact(DisplayName = "008 - Getting an Id that was just deleted returns a 404 Not Found")]
        public async Task Test3507()
        {
            // Get the Id
            var entity = Shared.Get<ProductCategory>("ProductCategory_pc");
            var id = entity.Id;

            // Verify that the id was deleted by calling get        
            var getResponse = await Client.GetAsync($"/api/product-categories/{id}");

            // Assert that the response is correct
            Output.WriteLine(await getResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Trait(Testing, ProductCategories)]
        [Fact(DisplayName = "009 - Deactivating an active product category returns a 200 OK inactive entity")]
        public async Task Test3508()
        {
            // Get the Id
            var entity = Shared.Get<ProductCategory>("ProductCategory_all");
            var id = entity.Id;

            // Call the API
            var response = await Client.PutAsJsonAsync($"/api/product-categories/deactivate", new List<int>() { id });

            // Assert that the response status code is correct
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<ProductCategory>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was deactivated
            Assert.False(responseDto.IsActive, "The product category was not deactivated");
        }

        [Trait(Testing, ProductCategories)]
        [Fact(DisplayName = "010 - Activating an inactive product category returns a 200 OK active entity")]
        public async Task Test3509()
        {
            // Get the Id
            var entity = Shared.Get<ProductCategory>("ProductCategory_all");
            var id = entity.Id;

            // Call the API
            var response = await Client.PutAsJsonAsync($"/api/product-categories/activate", new List<int>() { id });

            // Assert that the response status code is correct
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<ProductCategory>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was activated
            Assert.True(responseDto.IsActive, "The product category was not activated");
        }

        [Trait(Testing, ProductCategories)]
        [Fact(DisplayName = "011 - Using Select argument works as expected")]
        public async Task Test3510()
        {
            // Get the Id
            var entity = Shared.Get<ProductCategory>("ProductCategory_all");
            var id = entity.Id;

            var response = await Client.GetAsync($"/api/product-categories/{id}?select=Name");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of product category
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<ProductCategory>>();
            Assert.Equal("ProductCategory", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Null(responseDto.Name2);
            Assert.Null(responseDto.Name3);
            Assert.Null(responseDto.Code);
            Assert.Null(responseDto.ParentId);
        }


        // TODO add Import/Export tests
    }
}
