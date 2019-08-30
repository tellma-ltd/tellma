//using BSharp.Controllers.Dto;
//using BSharp.IntegrationTests.Utilities;
//using BSharp.Services.Utilities;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Formatting;
//using System.Threading.Tasks;
//using Xunit;

//namespace BSharp.IntegrationTests.Scenario_01
//{
//    public partial class Scenario_01
//    {
//        public const string individualAgents = "02 - Individual Agents";
//        public const string individualAgentsURL = "/api/agents/individuals";


//        [Trait(Testing, individualAgents)]
//        [Fact(DisplayName = "000 - Getting all individuals before granting permissions returns a 403 Forbidden response")]
//        public async Task Test10000()
//        {
//            var response = await _client.GetAsync(individualAgentsURL);

//            // Call the API
//            _output.WriteLine(await response.Content.ReadAsStringAsync());

//            // Assert the result is 403 OK
//            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
//        }

//        [Trait(Testing, individualAgents)]
//        [Fact(DisplayName = "001 - Getting all individuals before creating any returns a 200 OK empty collection")]
//        public async Task Test1000()
//        {
//            // Grant permission
//            await GrantPermissionToSecurityAdministrator("individuals", Constants.Update, "Id lt 100000");

//            var response = await _client.GetAsync(individualAgentsURL);

//            // Call the API
//            _output.WriteLine(await response.Content.ReadAsStringAsync());

//            // Assert the result is 200 OK
//            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//            // Confirm the result is a well formed response
//            var responseData = await response.Content.ReadAsAsync<GetResponse<Agent>>();

//            // Assert the result makes sense
//            Assert.Equal("Custodies", responseData.CollectionName);

//            Assert.Equal(0, responseData.TotalCount);
//            Assert.Empty(responseData.Result);
//        }

//        [Trait(Testing, individualAgents)]
//        [Fact(DisplayName = "002 - Getting a non-existent individual id returns a 404 Not Found")]
//        public async Task Test1001()
//        {
//            int nonExistentId = 1;
//            var response = await _client.GetAsync($"{individualAgentsURL}/{nonExistentId}");

//            _output.WriteLine(await response.Content.ReadAsStringAsync());
//            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
//        }

//        [Trait(Testing, individualAgents)]
//        [Fact(DisplayName = "003 - Saving a single well-formed individual for save returns a 200 OK result")]
//        public async Task Test1002()
//        {
//            // Prepare a well formed entity
//            var dtoForSave = new AgentForSave
//            {
//                EntityState = "Inserted",
//                Name = "Ahmad",
//                Name2 = "أحمد",
//                Code = "I0001",
//                Address = "13 Huntington Rd.",
//                BirthDateTime = new System.DateTime(1990, 9, 21),
//                Gender = 'M',
//                IsRelated = false,
//                TaxIdentificationNumber = "12340643",
//                Title = "Mr.",
//                Title2 = "السيد",
//            };

//            // Save it
//            var dtosForSave = new List<AgentForSave> { dtoForSave };
//            var response = await _client.PostAsJsonAsync(individualAgentsURL, dtosForSave);

//            // Assert that the response status code is a happy 200 OK
//            _output.WriteLine(await response.Content.ReadAsStringAsync());
//            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//            // Assert that the response is well-formed singleton of Agent
//            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Agent>>();
//            Assert.Single(responseData.Result);

//            // Assert that the result matches the saved entity
//            Assert.Equal("Custodies", responseData.CollectionName);

//            var responseDto = responseData.Result.FirstOrDefault();
//            Assert.NotNull(responseDto?.Id);
//            Assert.Equal(dtoForSave.Name, responseDto.Name);
//            Assert.Equal(dtoForSave.Name2, responseDto.Name2);
//            Assert.Equal(dtoForSave.Code, responseDto.Code);
//            Assert.Equal(dtoForSave.Address, responseDto.Address);
//            Assert.Equal(dtoForSave.BirthDateTime, responseDto.BirthDateTime);
//            Assert.Equal(dtoForSave.Gender, responseDto.Gender);
//            Assert.Equal(dtoForSave.IsRelated, responseDto.IsRelated);
//            Assert.Equal(dtoForSave.TaxIdentificationNumber, responseDto.TaxIdentificationNumber);
//            Assert.Equal(dtoForSave.Title, responseDto.Title);
//            Assert.Equal(dtoForSave.Title2, responseDto.Title2);

//            _shared.SetItem("Agent_Ahmad", responseDto);
//        }

//        [Trait(Testing, individualAgents)]
//        [Fact(DisplayName = "004 - Getting the Id of the AgentForSave just saved returns a 200 OK result")]
//        public async Task Test1003()
//        {
//            // Query the API for the Id that was just returned from the Save
//            var entity = _shared.GetItem<Agent>("Agent_Ahmad");
//            var id = entity.Id;
//            var response = await _client.GetAsync($"{individualAgentsURL}/{id}");

//            _output.WriteLine(await response.Content.ReadAsStringAsync());
//            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//            // Confirm that the response is a well formed GetByIdResponse of agent
//            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<Agent>>();
//            Assert.Equal("Custodies", getByIdResponse.CollectionName);

//            var responseDto = getByIdResponse.Result;
//            Assert.Equal(id, responseDto.Id);
//            Assert.Equal(entity.Name, responseDto.Name);
//            Assert.Equal(entity.Name2, responseDto.Name2);
//            Assert.Equal(entity.Code, responseDto.Code);
//            Assert.Equal(entity.Address, responseDto.Address);
//            Assert.Equal(entity.BirthDateTime, responseDto.BirthDateTime);
//            Assert.Equal(entity.Gender, responseDto.Gender);
//            Assert.Equal(entity.IsRelated, responseDto.IsRelated);
//            Assert.Equal(entity.TaxIdentificationNumber, responseDto.TaxIdentificationNumber);
//            Assert.Equal(entity.Title, responseDto.Title);
//            Assert.Equal(entity.Title2, responseDto.Title2);
//        }

//        [Trait(Testing, individualAgents)]
//        [Fact(DisplayName = "005 - Saving a AgentForSave with an existing code returns a 422 Unprocessable Entity")]
//        public async Task Test1004()
//        {
//            // Prepare a unit with the same code as one that has been saved already
//            var list = new List<AgentForSave> {
//                new AgentForSave
//                {
//                    EntityState = "Inserted",
//                    Name = "Claire",
//                    Name2 = "كلير",
//                    Code = "I0001",
//                    Address = "14 Huntington Rd.",
//                    BirthDateTime = new System.DateTime(1981, 6, 11),
//                    Gender = 'F',
//                    IsRelated = false,
//                    TaxIdentificationNumber = "9527272",
//                    Title = "Mrs.",
//                    Title2 = "السيدة",
//                }
//            };

//            // Call the API
//            var response = await _client.PostAsJsonAsync(individualAgentsURL, list);

//            // Assert that the response status code is 422 unprocessable entity (validation errors)
//            _output.WriteLine(await response.Content.ReadAsStringAsync());
//            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

//            // Confirm that the result is a well-formed validation errors structure
//            var errors = await response.Content.ReadAsAsync<ValidationErrors>();

//            // Assert that it contains a validation key pointing to the Code property
//            string expectedKey = "[0].Code";
//            Assert.True(errors.ContainsKey(expectedKey), $"Expected error key '{expectedKey}' was not found");

//            // Assert that it contains a useful error message in English
//            var message = errors["[0].Code"].Single();
//            Assert.Contains("already used", message.ToLower());
//        }

//        [Trait(Testing, individualAgents)]
//        [Fact(DisplayName = "006 - Saving a AgentForSave trims string fields with trailing or leading spaces")]
//        public async Task Test1005()
//        {
//            // Prepare a DTO for save, that contains leading and 
//            // trailing spaces in some string properties
//            var dtoForSave = new AgentForSave
//            {
//                EntityState = "Inserted",
//                Name = "Claire  ",
//                Name2 = "كلير",
//                Code = "  I0002",
//                Address = "14 Huntington Rd.",
//                BirthDateTime = new System.DateTime(1981, 6, 11),
//                Gender = 'F',
//                IsRelated = false,
//                TaxIdentificationNumber = "9527272",
//                Title = "Mrs.",
//                Title2 = "السيدة",

//                // TODO: Add User Id
//            };

//            // Call the API
//            var response = await _client.PostAsJsonAsync(individualAgentsURL, new List<AgentForSave> { dtoForSave });

//            // Confirm that the response is well-formed
//            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Agent>>();
//            var responseDto = responseData.Result.FirstOrDefault();

//            // Confirm the entity was saved
//            Assert.NotNull(responseDto.Id);

//            // Confirm that the leading and trailing spaces have been trimmed
//            Assert.Equal(dtoForSave.Name?.Trim(), responseDto.Name);
//            Assert.Equal(dtoForSave.Code?.Trim(), responseDto.Code);

//            // share the entity, for the subsequent delete test
//            _shared.SetItem("Agent_Claire", responseDto);
//        }

//        [Trait(Testing, individualAgents)]
//        [Fact(DisplayName = "007 - Deleting an existing individual Id returns a 200 OK")]
//        public async Task Test1006()
//        {
//            // Get the Id
//            var entity = _shared.GetItem<Agent>("Agent_Claire");
//            var id = entity.Id.Value;

//            // Query the delete API
//            var msg = new HttpRequestMessage(HttpMethod.Delete, individualAgentsURL);
//            msg.Content = new ObjectContent<List<int>>(new List<int> { id }, new JsonMediaTypeFormatter());
//            var deleteResponse = await _client.SendAsync(msg);

//            _output.WriteLine(await deleteResponse.Content.ReadAsStringAsync());
//            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
//        }

//        [Trait(Testing, individualAgents)]
//        [Fact(DisplayName = "008 - Getting an Id that was just deleted returns a 404 Not Found")]
//        public async Task Test1007()
//        {
//            // Get the Id
//            var entity = _shared.GetItem<Agent>("Agent_Claire");
//            var id = entity.Id.Value;

//            // Verify that the id was deleted by calling get        
//            var getResponse = await _client.GetAsync($"{individualAgentsURL}/{id}");

//            // Assert that the response is correct
//            _output.WriteLine(await getResponse.Content.ReadAsStringAsync());
//            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
//        }

//        [Trait(Testing, individualAgents)]
//        [Fact(DisplayName = "009 - Deactivating an active individual returns a 200 OK inactive entity")]
//        public async Task Test1008()
//        {
//            // Get the Id
//            var entity = _shared.GetItem<Agent>("Agent_Ahmad");
//            var id = entity.Id.Value;

//            // Call the API
//            var response = await _client.PutAsJsonAsync($"{individualAgentsURL}/deactivate", new List<int>() { id });

//            // Assert that the response status code is correct
//            _output.WriteLine(await response.Content.ReadAsStringAsync());
//            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//            // Confirm that the response content is well formed singleton
//            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Agent>>();
//            Assert.Single(responseData.Result);
//            var responseDto = responseData.Result.Single();

//            // Confirm that the entity was deactivated
//            Assert.False(responseDto.IsActive, "The Individual was not deactivated");
//        }

//        [Trait(Testing, individualAgents)]
//        [Fact(DisplayName = "010 - Activating an inactive individual returns a 200 OK active entity")]
//        public async Task Test1009()
//        {
//            // Get the Id
//            var entity = _shared.GetItem<Agent>("Agent_Ahmad");
//            var id = entity.Id.Value;

//            // Call the API
//            var response = await _client.PutAsJsonAsync($"{individualAgentsURL}/activate", new List<int>() { id });

//            // Assert that the response status code is correct
//            _output.WriteLine(await response.Content.ReadAsStringAsync());
//            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

//            // Confirm that the response content is well formed singleton
//            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Agent>>();
//            Assert.Single(responseData.Result);
//            var responseDto = responseData.Result.Single();

//            // Confirm that the entity was activated
//            Assert.True(responseDto.IsActive, "The Individual was not activated");
//        }

//        // TODO add Import/Export tests
//    }
//}
