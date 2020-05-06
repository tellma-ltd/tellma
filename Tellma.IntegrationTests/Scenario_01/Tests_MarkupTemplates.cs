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
    public class Tests_17_MarkupTemplates : Scenario_01
    {
        public Tests_17_MarkupTemplates(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        public readonly string _baseAddress = "markup-templates";

        public string Url => $"/api/{_baseAddress}";
        private string View => _baseAddress;

        [Fact(DisplayName = "01 Getting all markup templates before granting permissions returns a 403 Forbidden response")]
        public async Task Test01()
        {
            var response = await Client.GetAsync(Url);

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact(DisplayName = "02 Getting all markup templates before creating any returns a 200 OK empty collection")]
        public async Task Test02()
        {
            await GrantPermissionToSecurityAdministrator(View, Constants.Update, "Id lt 100000");

            // Call the API
            var response = await Client.GetAsync(Url);
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<MarkupTemplate>>();

            // Assert the result makes sense
            Assert.Equal("MarkupTemplate", responseData.CollectionName);

            Assert.Null(responseData.TotalCount);
            Assert.Empty(responseData.Result);
        }

        [Fact(DisplayName = "03 Getting a non-existent markup template id returns a 404 Not Found")]
        public async Task Test03()
        {
            int nonExistentId = 1;
            var response = await Client.GetAsync($"{Url}/{nonExistentId}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "04 Saving a single well-formed MarkupTemplateForSave returns a 200 OK result")]
        public async Task Test04()
        {
            // Prepare a well formed entity
            var dtoForSave = new MarkupTemplateForSave
            {
                Name = "Invoice",
                Name2 = "فاتورة",
                Code = "INV",
                Description = "This is a invoice",
                Description2 = "هذه فاتورة",
                MarkupLanguage = "text/html",
                Usage = "QueryByFilter",
                Collection = "Document",
                DefinitionId = "manual-journal-vouchers",
                Body = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>Document</title>
</head>
<body>
    Hello!
</body>
</html>",
                DownloadName = "My Invoice.html",
                SupportsPrimaryLanguage = true,
                SupportsSecondaryLanguage = false,
                SupportsTernaryLanguage = true
            };

            // Save it
            var dtosForSave = new List<MarkupTemplateForSave> { dtoForSave };
            var response = await Client.PostAsJsonAsync(Url, dtosForSave);

            // Assert that the response status code is a happy 200 OK
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert that the response is well-formed singleton of MarkupTemplate
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<MarkupTemplate>>();
            Assert.Single(responseData.Result);

            // Assert that the result matches the saved entity
            Assert.Equal("MarkupTemplate", responseData.CollectionName);

            // Retreve the entity from the entities
            var responseDto = responseData.Result.SingleOrDefault();

            Assert.NotNull(responseDto?.Id);
            Assert.Equal(dtoForSave.Name, responseDto.Name);
            Assert.Equal(dtoForSave.Name2, responseDto.Name2);
            Assert.Equal(dtoForSave.Code, responseDto.Code);
            Assert.Equal(dtoForSave.Description, responseDto.Description);
            Assert.Equal(dtoForSave.Description2, responseDto.Description2);
            Assert.Equal(dtoForSave.MarkupLanguage, responseDto.MarkupLanguage);
            Assert.Equal(dtoForSave.Usage, responseDto.Usage);
            Assert.Equal(dtoForSave.Collection, responseDto.Collection);
            Assert.Equal(dtoForSave.DefinitionId, responseDto.DefinitionId);
            Assert.Equal(dtoForSave.Body, responseDto.Body);
            Assert.Equal(dtoForSave.DownloadName, responseDto.DownloadName);
            Assert.Equal(dtoForSave.SupportsPrimaryLanguage, responseDto.SupportsPrimaryLanguage);
            Assert.Equal(dtoForSave.SupportsSecondaryLanguage, responseDto.SupportsSecondaryLanguage);
            Assert.False(responseDto.SupportsTernaryLanguage);

            Shared.Set("MarkupTemplate_Invoice", responseDto);
        }

        [Fact(DisplayName = "05 Getting the Id of the MarkupTemplateForSave just saved returns a 200 OK result")]
        public async Task Test05()
        {
            // Query the API for the Id that was just returned from the Save
            var entity = Shared.Get<MarkupTemplate>("MarkupTemplate_Invoice");
            var id = entity.Id;
            var response = await Client.GetAsync($"{Url}/{id}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of markup template
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<MarkupTemplate>>();
            Assert.Equal("MarkupTemplate", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Equal(entity.Name2, responseDto.Name2);
            Assert.Equal(entity.Code, responseDto.Code);
            Assert.Equal(entity.Description, responseDto.Description);
            Assert.Equal(entity.Description2, responseDto.Description2);
            Assert.Equal(entity.MarkupLanguage, responseDto.MarkupLanguage);
            Assert.Equal(entity.Usage, responseDto.Usage);
            Assert.Equal(entity.Collection, responseDto.Collection);
            Assert.Equal(entity.DefinitionId, responseDto.DefinitionId);
            Assert.Equal(entity.Body, responseDto.Body);
            Assert.Equal(entity.DownloadName, responseDto.DownloadName);
            Assert.Equal(entity.SupportsPrimaryLanguage, responseDto.SupportsPrimaryLanguage);
            Assert.Equal(entity.SupportsSecondaryLanguage, responseDto.SupportsSecondaryLanguage);
            Assert.Equal(entity.SupportsSecondaryLanguage, responseDto.SupportsTernaryLanguage);
        }

        [Fact(DisplayName = "06 Saving a MarkupTemplateForSave with an existing currency and date returns a 422 Unprocessable Entity")]
        public async Task Test06()
        {
            // Prepare a markup template with the same code 'kg' as one that has been saved already
            var list = new List<MarkupTemplateForSave> {
                new MarkupTemplateForSave
                {
                    Name = "Invoice 2",
                    Name2 = "فاتورة 2",
                    Code = "INV", // Duplicated
                    Description = "This is a invoice 2",
                    Description2 = "هذه فاتورة 2",
                    MarkupLanguage = "text/html",
                    Usage = "QueryByFilter",
                    Collection = "Document",
                    DefinitionId = "manual-journal-vouchers",
                    Body = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>Document</title>
</head>
<body>
    Hello!
</body>
</html>",
                    DownloadName = "My Invoice 2.html",
                    SupportsPrimaryLanguage = true,
                    SupportsSecondaryLanguage = false,
                    SupportsTernaryLanguage = true
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

        [Fact(DisplayName = "07 Deleting an existing markup template Id returns a 200 OK")]
        public async Task Test08()
        {
            await GrantPermissionToSecurityAdministrator(View, Constants.Delete, null);

            // Get the Id
            var entity = Shared.Get<MarkupTemplate>("MarkupTemplate_Invoice");
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
            var entity = Shared.Get<MarkupTemplate>("MarkupTemplate_Invoice");
            var id = entity.Id;

            // Verify that the id was deleted by calling get        
            var getResponse = await Client.GetAsync($"{Url}/{id}");

            // Assert that the response is correct
            Output.WriteLine(await getResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
