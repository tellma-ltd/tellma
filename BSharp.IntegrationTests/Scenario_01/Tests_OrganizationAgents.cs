using BSharp.Controllers.Dto;
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
        public const string organizationAgents = "03 - Organization Agents";
        public const string organizationAgentsURL = "/api/agents/organizations";

        [Trait(Testing, organizationAgents)]
        [Fact(DisplayName = "000 - Getting all organizations before granting permissions returns a 403 Forbidden response")]
        public async Task Test20000()
        {
            var response = await _client.GetAsync(organizationAgentsURL);

            // Call the API
            _output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Trait(Testing, organizationAgents)]
        [Fact(DisplayName = "001 - Getting all organizations before creating any returns a 200 OK empty collection")]
        public async Task Test2000()
        {
            // Grant permission
            await GrantPermissionToSecurityAdministrator("organizations", Constants.Update, "Id lt 100000");

            var response = await _client.GetAsync(organizationAgentsURL);

            // Call the API
            _output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<Agent>>();

            // Assert the result makes sense
            Assert.Equal("Custodies", responseData.CollectionName);

            Assert.Equal(0, responseData.TotalCount);
            Assert.Empty(responseData.Result);
        }

        [Trait(Testing, organizationAgents)]
        [Fact(DisplayName = "002 - Getting a non-existent organization id returns a 404 Not Found")]
        public async Task Test2001()
        {
            int nonExistentId = 1;
            var response = await _client.GetAsync($"{organizationAgentsURL}/{nonExistentId}");

            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait(Testing, organizationAgents)]
        [Fact(DisplayName = "003 - Saving a single well-formed organization for save returns a 200 OK result")]
        public async Task Test2002()
        {
            // Prepare a well formed entity
            var dtoForSave = new AgentForSave
            {
                EntityState = "Inserted",
                Name = "World Health Organization",
                Name2 = "منظمة الصحة العالمية",
                Code = "O0001",
                Address = "13 Huntington Rd.",
                BirthDateTime = new System.DateTime(1948, 7, 4),
                Gender = 'M',
                IsRelated = false,
                TaxIdentificationNumber = "12340643",
                Title = "Mr.",
                Title2 = "السيد",
            };

            // Save it
            var dtosForSave = new List<AgentForSave> { dtoForSave };
            var response = await _client.PostAsJsonAsync(organizationAgentsURL, dtosForSave);

            // Assert that the response status code is a happy 200 OK
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert that the response is well-formed singleton of Agent
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Agent>>();
            Assert.Single(responseData.Result);

            // Assert that the result matches the saved entity
            Assert.Equal("Custodies", responseData.CollectionName);

            var responseDto = responseData.Result.FirstOrDefault();
            Assert.NotNull(responseDto?.Id);
            Assert.Equal(dtoForSave.Name, responseDto.Name);
            Assert.Equal(dtoForSave.Name2, responseDto.Name2);
            Assert.Equal(dtoForSave.Code, responseDto.Code);
            Assert.Equal(dtoForSave.Address, responseDto.Address);
            Assert.Equal(dtoForSave.BirthDateTime, responseDto.BirthDateTime);
            Assert.Equal(dtoForSave.IsRelated, responseDto.IsRelated);
            Assert.Equal(dtoForSave.TaxIdentificationNumber, responseDto.TaxIdentificationNumber);

            // These should always be null for organizations
            Assert.Null(responseDto.Gender);
            Assert.Null(responseDto.Title);
            Assert.Null(responseDto.Title2);


            _shared.SetItem("Agent_WHO", responseDto);
        }

        [Trait(Testing, organizationAgents)]
        [Fact(DisplayName = "004 - Getting the Id of the AgentForSave just saved returns a 200 OK result")]
        public async Task Test2003()
        {
            // Query the API for the Id that was just returned from the Save
            var entity = _shared.GetItem<Agent>("Agent_WHO");
            var id = entity.Id;
            var response = await _client.GetAsync($"{organizationAgentsURL}/{id}");

            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of agent
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<Agent>>();
            Assert.Equal("Custodies", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.Equal(id, responseDto.Id);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Equal(entity.Name2, responseDto.Name2);
            Assert.Equal(entity.Code, responseDto.Code);
            Assert.Equal(entity.Address, responseDto.Address);
            Assert.Equal(entity.BirthDateTime, responseDto.BirthDateTime);
            Assert.Equal(entity.IsRelated, responseDto.IsRelated);
            Assert.Equal(entity.TaxIdentificationNumber, responseDto.TaxIdentificationNumber);
        }

        [Trait(Testing, organizationAgents)]
        [Fact(DisplayName = "005 - Saving a AgentForSave with an existing code returns a 422 Unprocessable Entity")]
        public async Task Test2004()
        {
            // Prepare a unit with the same code as one that has been saved already
            var list = new List<AgentForSave> {
                new AgentForSave
                {
                    EntityState = "Inserted",
                    Name = "Walia Steel",
                    Name2 = "واليا الحديد",
                    Code = "O0001",
                    Address = "14 Huntington Rd.",
                    BirthDateTime = new System.DateTime(1981, 6, 11),
                    IsRelated = false,
                    TaxIdentificationNumber = "9527272",
                }
            };

            // Call the API
            var response = await _client.PostAsJsonAsync(organizationAgentsURL, list);

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

        [Trait(Testing, organizationAgents)]
        [Fact(DisplayName = "006 - Saving a AgentForSave trims string fields with trailing or leading spaces")]
        public async Task Test2005()
        {
            // Prepare a DTO for save, that contains leading and 
            // trailing spaces in some string properties
            var dtoForSave = new AgentForSave
            {
                EntityState = "Inserted",
                Name = "Walia Steel   ",
                Name2 = "واليا الحديد",
                Code = "   O0002",
                Address = "14 Huntington Rd.",
                BirthDateTime = new System.DateTime(1981, 6, 11),
                IsRelated = false,
                TaxIdentificationNumber = "9527272",
            };

            // Call the API
            var response = await _client.PostAsJsonAsync(organizationAgentsURL, new List<AgentForSave> { dtoForSave });

            // Confirm that the response is well-formed
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Agent>>();
            var responseDto = responseData.Result.FirstOrDefault();

            // Confirm the entity was saved
            Assert.NotNull(responseDto.Id);

            // Confirm that the leading and trailing spaces have been trimmed
            Assert.Equal(dtoForSave.Name?.Trim(), responseDto.Name);
            Assert.Equal(dtoForSave.Code?.Trim(), responseDto.Code);

            // share the entity, for the subsequent delete test
            _shared.SetItem("Agent_Walia", responseDto);
        }

        [Trait(Testing, organizationAgents)]
        [Fact(DisplayName = "007 - Deleting an existing organization Id returns a 200 OK")]
        public async Task Test2006()
        {
            // Get the Id
            var entity = _shared.GetItem<Agent>("Agent_Walia");
            var id = entity.Id.Value;

            // Query the delete API
            var msg = new HttpRequestMessage(HttpMethod.Delete, organizationAgentsURL);
            msg.Content = new ObjectContent<List<int>>(new List<int> { id }, new JsonMediaTypeFormatter());
            var deleteResponse = await _client.SendAsync(msg);

            _output.WriteLine(await deleteResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Trait(Testing, organizationAgents)]
        [Fact(DisplayName = "008 - Getting an Id that was just deleted returns a 404 Not Found")]
        public async Task Test2007()
        {
            // Get the Id
            var entity = _shared.GetItem<Agent>("Agent_Walia");
            var id = entity.Id.Value;

            // Verify that the id was deleted by calling get        
            var getResponse = await _client.GetAsync($"{organizationAgentsURL}/{id}");

            // Assert that the response is correct
            _output.WriteLine(await getResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Trait(Testing, organizationAgents)]
        [Fact(DisplayName = "009 - Deactivating an active organization returns a 200 OK inactive entity")]
        public async Task Test2008()
        {
            // Get the Id
            var entity = _shared.GetItem<Agent>("Agent_WHO");
            var id = entity.Id.Value;

            // Call the API
            var response = await _client.PutAsJsonAsync($"{organizationAgentsURL}/deactivate", new List<int>() { id });

            // Assert that the response status code is correct
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Agent>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was deactivated
            Assert.False(responseDto.IsActive, "The organization was not deactivated");
        }

        [Trait(Testing, organizationAgents)]
        [Fact(DisplayName = "010 - Activating an inactive organization returns a 200 OK active entity")]
        public async Task Test2009()
        {
            // Get the Id
            var entity = _shared.GetItem<Agent>("Agent_WHO");
            var id = entity.Id.Value;

            // Call the API
            var response = await _client.PutAsJsonAsync($"{organizationAgentsURL}/activate", new List<int>() { id });

            // Assert that the response status code is correct
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Agent>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was activated
            Assert.True(responseDto.IsActive, "The Organization was not activated");
        }

        // TODO add Import/Export tests
    }
}
