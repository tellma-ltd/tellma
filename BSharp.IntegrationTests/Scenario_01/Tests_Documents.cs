using BSharp.Controllers.Dto;
using BSharp.Entities;
using BSharp.IntegrationTests.Utilities;
using BSharp.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace BSharp.IntegrationTests.Scenario_01
{
    public class Tests_14_Documents : Scenario_01
    {
        public Tests_14_Documents(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        public readonly string _baseAddress = "documents";
        public readonly string _definitionId = "manual-journal-vouchers";

        public string View => $"{_baseAddress}/{_definitionId}"; // For permissions
        public string GenericlUrl => $"/api/{_baseAddress}"; // For querying generic documents
        public string Url => $"/api/{_baseAddress}/{_definitionId}"; // For querying and updating specific document definition


        [Fact(DisplayName = "01 Getting all documents before granting permissions returns a 403 Forbidden response")]
        public async Task Test01()
        {
            var response = await Client.GetAsync(Url);

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact(DisplayName = "02 Getting all JV documents before creating any returns a 200 OK singleton collection")]
        public async Task Test02()
        {
            await GrantPermissionToSecurityAdministrator(View, Constants.Update, "Id lt 100000");

            // Call the API
            var response = await Client.GetAsync(Url);
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<Document>>();

            // Assert the result makes sense
            Assert.Equal("Document", responseData.CollectionName);

            Assert.Equal(0, responseData.TotalCount);
            Assert.Empty(responseData.Result);
        }

        [Fact(DisplayName = "03 Getting all generic documents before creating any returns a 200 OK singleton collection")]
        public async Task Test03()
        {
            // Call the API
            var response = await Client.GetAsync(GenericlUrl);
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<Document>>();

            // Assert the result makes sense
            Assert.Equal("Document", responseData.CollectionName);

            Assert.Equal(0, responseData.TotalCount);
            Assert.Empty(responseData.Result);
        }

        [Fact(DisplayName = "04 Getting a non-existent document id returns a 404 Not Found")]
        public async Task Test04()
        {
            int nonExistentId = 500;
            var response = await Client.GetAsync($"{Url}/{nonExistentId}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "05 Saving a well-formed DocumentForSave returns a 200 OK result")]
        public async Task Test05()
        {
            // var etbId = "ETB";
            var accountId = Shared.Get<Account>("Account_Payables").Id;
            var entryClassificationId = Shared.Get<EntryType>("EntryType_SM").Id;
            var resourceId = Shared.Get<Resource>("Resource_HR1000x0.8").Id;

            // Prepare a well formed entity
            var dtoForSave = new DocumentForSave
            {
                DocumentDate = DateTime.Today,
                Memo = "Capital investment",
                MemoIsCommon = true,
                Lines = new List<LineForSave>
                {
                    new LineForSave
                    {
                         DefinitionId = "ManualLine",
                         Entries = new List<EntryForSave>
                         {
                             new EntryForSave
                             {
                                 EntryNumber = 1,
                                 Direction = 1,
                                 AccountId = accountId,
                                 EntryTypeId = entryClassificationId,
                                 ResourceId = resourceId,
                                 MonetaryValue = 25000,
                                 Value = 25000
                             }
                         }
                    },
                    new LineForSave
                    {
                         DefinitionId = "ManualLine",
                         Entries = new List<EntryForSave>
                         {
                             new EntryForSave
                             {
                                 EntryNumber = 1,
                                 Direction = -1,
                                 AccountId = accountId,
                                 EntryTypeId = entryClassificationId,
                                 ResourceId = resourceId,
                                 MonetaryValue = 25000,
                                 Value = 25000
                             }
                         }
                    }
                }
            };

            // Save it
            var dtosForSave = new List<DocumentForSave> { dtoForSave };
            var response = await Client.PostAsJsonAsync($"{Url}?expand=Lines/Entries", dtosForSave);

            // Assert that the response status code is a happy 200 OK
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert that the response is well-formed singleton of Document
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Document>>();
            Assert.NotEmpty(responseData.Result);

            // Assert that the result matches the saved entity
            Assert.Equal("Document", responseData.CollectionName);

            // Retreve the entity from the entities
            var responseDto = responseData.Result.FirstOrDefault();

            Assert.NotEqual(0, responseDto.Id);
            Assert.NotNull(responseDto.SerialNumber);
            Assert.NotEqual(0, responseDto.SerialNumber);
            Assert.Equal((short)0, responseDto.State);
            Assert.Equal(dtoForSave.DocumentDate, responseDto.DocumentDate);
            Assert.Equal(dtoForSave.Memo, responseDto.Memo);
            Assert.Equal(dtoForSave.MemoIsCommon, responseDto.MemoIsCommon);
            Assert.Collection(responseDto.Lines,
                    line =>
                    {
                        Assert.Equal(dtoForSave.Lines[0].DefinitionId, line.DefinitionId);
                        Assert.Collection(line.Entries,
                            entry =>
                            {
                                var entryForSave = dtoForSave.Lines[0].Entries[0];

                                Assert.Equal(entryForSave.EntryNumber, entry.EntryNumber);
                                Assert.Equal(entryForSave.Direction, entry.Direction);
                                Assert.Equal(entryForSave.AccountId, entry.AccountId);
                                Assert.Null(entry.EntryTypeId);
                                Assert.Null(entry.ResourceId);
                                Assert.Equal(entryForSave.MonetaryValue, entry.MonetaryValue);
                                Assert.Equal(entryForSave.Value, entry.Value);
                            }
                        );
                    },
                    line =>
                    {
                        Assert.Equal(dtoForSave.Lines[1].DefinitionId, line.DefinitionId);
                        Assert.Collection(line.Entries,
                            entry =>
                            {
                                var entryForSave = dtoForSave.Lines[1].Entries[0];

                                Assert.Equal(entryForSave.EntryNumber, entry.EntryNumber);
                                Assert.Equal(entryForSave.Direction, entry.Direction);
                                Assert.Equal(entryForSave.AccountId, entry.AccountId);
                                Assert.Null(entry.EntryTypeId);
                                Assert.Null(entry.ResourceId);
                                Assert.Equal(entryForSave.MonetaryValue, entry.MonetaryValue);
                                Assert.Equal(entryForSave.Value, entry.Value);
                            }
                        );
                    }
                );

            Shared.Set("Document_CapitalInvestment", responseDto);
        }

        [Fact(DisplayName = "06 Getting the Id of the DocumentForSave just saved returns a 200 OK result")]
        public async Task Test06()
        {
            // Query the API for the Id that was just returned from the Save
            var entity = Shared.Get<Document>("Document_CapitalInvestment");
            var id = entity.Id;
            var response = await Client.GetAsync($"{Url}/{id}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of document
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<Document>>();
            Assert.Equal("Document", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(entity.SerialNumber, responseDto.SerialNumber);
        }

        //[Fact(DisplayName = "07 Saving an DocumentForSave with an existing code returns a 422 Unprocessable Entity")]
        //public async Task Test07()
        //{
        //    // Prepare a unit with the same code 'kg' as one that has been saved already
        //    var list = new List<DocumentForSave> {
        //        new DocumentForSave
        //        {
        //            Name = "Another Name",
        //            Name2 = "Another Name",
        //            Code = "JW",
        //            IsRelated = false
        //        }
        //    };

        //    // Call the API
        //    var response = await Client.PostAsJsonAsync(Url, list);

        //    // Assert that the response status code is 422 unprocessable entity (validation errors)
        //    Output.WriteLine(await response.Content.ReadAsStringAsync());
        //    Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        //    // Confirm that the result is a well-formed validation errors structure
        //    var errors = await response.Content.ReadAsAsync<ValidationErrors>();

        //    // Assert that it contains a validation key pointing to the Code property
        //    string expectedKey = "[0].Code";
        //    Assert.True(errors.ContainsKey(expectedKey), $"Expected error key '{expectedKey}' was not found");

        //    // Assert that it contains a useful error message in English
        //    var message = errors["[0].Code"].Single();
        //    Assert.Contains("already used", message.ToLower());
        //}

        //[Fact(DisplayName = "08 Saving an DocumentForSave trims string fields with trailing or leading spaces")]
        //public async Task Test08()
        //{
        //    // Prepare a DTO for save, that contains leading and 
        //    // trailing spaces in some string properties
        //    var dtoForSave = new DocumentForSave
        //    {
        //        Name = "  Matilda", // Leading space
        //        Name2 = "ماتيلدا",
        //        Code = "MA  ", // Trailing space
        //        IsRelated = false,
        //    };

        //    // Call the API
        //    var response = await Client.PostAsJsonAsync(Url, new List<DocumentForSave> { dtoForSave });
        //    Output.WriteLine(await response.Content.ReadAsStringAsync());

        //    // Confirm that the response is well-formed
        //    var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Document>>();
        //    var responseDto = responseData.Result.FirstOrDefault();

        //    // Confirm the entity was saved
        //    Assert.NotEqual(0, responseDto.Id);

        //    // Confirm that the leading and trailing spaces have been trimmed
        //    Assert.Equal(dtoForSave.Name?.Trim(), responseDto.Name);
        //    Assert.Equal(dtoForSave.Code?.Trim(), responseDto.Code);

        //    // share the entity, for the subsequent delete test
        //    Shared.Set("Document_Matilda", responseDto);
        //}

        //[Fact(DisplayName = "09 Deleting an existing document Id returns a 200 OK")]
        //public async Task Test09()
        //{
        //    await GrantPermissionToSecurityAdministrator(View, Constants.Delete, null);

        //    // Get the Id
        //    var entity = Shared.Get<Document>("Document_Matilda");
        //    var id = entity.Id;

        //    // Query the delete API
        //    var msg = new HttpRequestMessage(HttpMethod.Delete, Url)
        //    {
        //        Content = new ObjectContent<List<int>>(new List<int> { id }, new JsonMediaTypeFormatter())
        //    };

        //    var deleteResponse = await Client.SendAsync(msg);

        //    Output.WriteLine(await deleteResponse.Content.ReadAsStringAsync());
        //    Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        //}

        //[Fact(DisplayName = "10 Getting an Id that was just deleted returns a 404 Not Found")]
        //public async Task Test10()
        //{
        //    // Get the Id
        //    var entity = Shared.Get<Document>("Document_Matilda");
        //    var id = entity.Id;

        //    // Verify that the id was deleted by calling get        
        //    var getResponse = await Client.GetAsync($"{Url}/{id}");

        //    // Assert that the response is correct
        //    Output.WriteLine(await getResponse.Content.ReadAsStringAsync());
        //    Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        //}

        //[Fact(DisplayName = "11 Deactivating an active document returns a 200 OK inactive entity")]
        //public async Task Test11()
        //{
        //    await GrantPermissionToSecurityAdministrator(View, "IsActive", null);

        //    // Get the Id
        //    var entity = Shared.Get<Document>("Document_JohnWick");
        //    var id = entity.Id;

        //    // Call the API
        //    var response = await Client.PutAsJsonAsync($"{Url}/deactivate", new List<int>() { id });

        //    // Assert that the response status code is correct
        //    Output.WriteLine(await response.Content.ReadAsStringAsync());
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //    // Confirm that the response content is well formed singleton
        //    var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Document>>();
        //    Assert.Single(responseData.Result);
        //    var responseDto = responseData.Result.Single();

        //    // Confirm that the entity was deactivated
        //    Assert.False(responseDto.IsActive, "The Document was not deactivated");
        //}

        //[Fact(DisplayName = "12 Activating an inactive document returns a 200 OK active entity")]
        //public async Task Test12()
        //{
        //    // Get the Id
        //    var entity = Shared.Get<Document>("Document_JohnWick");
        //    var id = entity.Id;

        //    // Call the API
        //    var response = await Client.PutAsJsonAsync($"{Url}/activate", new List<int>() { id });

        //    // Assert that the response status code is correct
        //    Output.WriteLine(await response.Content.ReadAsStringAsync());
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //    // Confirm that the response content is well formed singleton
        //    var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Document>>();
        //    Assert.Single(responseData.Result);
        //    var responseDto = responseData.Result.Single();

        //    // Confirm that the entity was activated
        //    Assert.True(responseDto.IsActive, "The Document was not activated");
        //}

        //[Fact(DisplayName = "13 Using Select argument works as expected")]
        //public async Task Test13()
        //{
        //    // Get the Id
        //    var entity = Shared.Get<Document>("Document_JohnWick");
        //    var id = entity.Id;

        //    var response = await Client.GetAsync($"{Url}/{id}?select=Name");

        //    Output.WriteLine(await response.Content.ReadAsStringAsync());
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //    // Confirm that the response is a well formed GetByIdResponse of Document
        //    var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<Document>>();
        //    Assert.Equal("Document", getByIdResponse.CollectionName);

        //    var responseDto = getByIdResponse.Result;
        //    Assert.Equal(id, responseDto.Id);
        //    Assert.Equal(entity.Name, responseDto.Name);
        //    Assert.Null(responseDto.Name2);
        //    Assert.Null(responseDto.Code);
        //}
    }
}
