using BSharp.Controllers.DTO;
using BSharp.IntegrationTests.Utilities;
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
        public const string localUsers = "05 - Local Users";
        public const string localUsersURL = "/api/local-users";

        [Trait(Testing, localUsers)]
        [Fact(DisplayName = "001 - Getting all localUsers before creating any returns a 200 OK singleton collection")]
        public async Task Test3100()
        {
            var response = await _client.GetAsync(localUsersURL);

            // Call the API
            _output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is well formed
            var responseData = await response.Content.ReadAsAsync<GetResponse<LocalUser>>();

            // Assert the result makes sense
            Assert.Equal("LocalUser", responseData.CollectionName);
            Assert.Single(responseData.Result); // First user
        }

        [Trait(Testing, localUsers)]
        [Fact(DisplayName = "002 - Getting a non-existent local user id returns a 404 Not Found")]
        public async Task Test3101()
        {
            int nonExistentId = 9999999;
            var response = await _client.GetAsync($"{localUsersURL}/{nonExistentId}");

            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait(Testing, localUsers)]
        [Fact(DisplayName = "003 - Saving a single well-formed local user for save returns a 200 OK result")]
        public async Task Test3102()
        {
            await CreateNewRole(); // To create a new role
            int salesManagerId = _shared.GetItem<Role>("Role_SalesManager").Id.Value;

            // Prepare a well formed entity
            var dtoForSave = new LocalUserForSave
            {
                EntityState = "Inserted",
                Name = "Ahmad Akra",
                Name2 = "أحمد عكره",
                Email = "ahmad.akra@banan-it.com",
                AgentId = null,
                Roles = new List<RoleMembershipForSave>
                {
                    new RoleMembershipForSave
                    {
                        EntityState = "Inserted",
                        RoleId = salesManagerId,
                        Memo = "Nice"
                    }
                }
            };

            // Save it
            var dtosForSave = new List<LocalUserForSave> { dtoForSave };
            var response = await _client.PostAsJsonAsync($"{localUsersURL}?expand=Roles/Role", dtosForSave);

            // Assert that the response status code is a happy 200 OK
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert that the response is well-formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<LocalUser>>();
            Assert.Single(responseData.Result);

            // Assert that the result matches the saved entity
            Assert.Equal("LocalUser", responseData.CollectionName);

            var responseDto = responseData.Result.FirstOrDefault();
            Assert.NotNull(responseDto?.Id);
            Assert.Equal(dtoForSave.Name, responseDto.Name);
            Assert.Equal(dtoForSave.Name2, responseDto.Name2);
            Assert.Equal(dtoForSave.Email, responseDto.Email);
            Assert.Equal(dtoForSave.AgentId, responseDto.AgentId);
            Assert.Collection(responseDto.Roles,
                    p =>
                    {
                        Assert.Equal(dtoForSave.Roles[0].RoleId, p.RoleId);
                        Assert.Equal(dtoForSave.Roles[0].Memo, p.Memo);
                        Assert.NotNull(p.Id);
                    }
                );

            _shared.SetItem("LocalUsers_AhmadAkra", responseDto);
        }

        [Trait(Testing, localUsers)]
        [Fact(DisplayName = "004 - Getting the Id of the local user just saved returns a 200 OK result")]
        public async Task Test3103()
        {
            // Query the API for the Id that was just returned from the Save
            var entity = _shared.GetItem<LocalUser>("LocalUsers_AhmadAkra");
            var id = entity.Id;
            var response = await _client.GetAsync($"{localUsersURL}/{id}?expand=Roles/Role");

            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<LocalUser>>();
            Assert.Equal("LocalUser", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.NotNull(responseDto?.Id);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Equal(entity.Name2, responseDto.Name2);
            Assert.Equal(entity.Email, responseDto.Email);
            Assert.Equal(entity.AgentId, responseDto.AgentId);
            Assert.Collection(responseDto.Roles,
                    p =>
                    {
                        Assert.Equal(entity.Roles[0].RoleId, p.RoleId);
                        Assert.Equal(entity.Roles[0].Memo, p.Memo);
                        Assert.NotNull(p.Id);
                    }
                );
        }

        [Trait(Testing, localUsers)]
        [Fact(DisplayName = "005 - Saving a local user with a non existent role Id returns a 422 Unprocessable Entity")]
        public async Task Test3104()
        {
            int salesManagerId = _shared.GetItem<Role>("Role_SalesManager").Id.Value;

            // Prepare a unit with the same code as one that has been saved already
            var dtoForSave = new LocalUserForSave
            {
                EntityState = "Inserted",
                Name = "Abdullah Ulber",
                Name2 = "عبد الله ألبر",
                Email = "abdullah-ulber", // Wrong email
                AgentId = null,
                Roles = new List<RoleMembershipForSave>
                {
                    new RoleMembershipForSave
                    {
                        EntityState = "Inserted",
                        RoleId = 9999, // non existent Id
                        Memo = "Nice"
                    }
                }
            };

            // Call the API
            var response = await _client.PostAsJsonAsync(localUsersURL, new List<LocalUserForSave> { dtoForSave });

            // Assert that the response status code is 422 unprocessable entity (validation errors)
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            // Confirm that the result is a well-formed validation errors structure
            var errors = await response.Content.ReadAsAsync<ValidationErrors>();

            // Assert that it contains a validation key pointing to the Email property
            {
                string expectedKey = "[0].Email";
                Assert.True(errors.ContainsKey(expectedKey), $"Expected error key '{expectedKey}' was not found");

                // Assert that it contains a useful error message in English
                var message = errors[expectedKey].Single();
                Assert.Contains("not a valid e-mail address", message.ToLower());
            }

            // Fix the email
            dtoForSave.Email = "abdullah.ulber@ulber.com";
            response = await _client.PostAsJsonAsync(localUsersURL, new List<LocalUserForSave> { dtoForSave });

            // Assert that the response status code is 422 unprocessable entity (validation errors)
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            // Confirm that the result is a well-formed validation errors structure
            errors = await response.Content.ReadAsAsync<ValidationErrors>();

            // Assert that it contains a validation key pointing to the RoleId property of the Role line
            {
                string expectedKey = "[0].Roles[0].RoleId";
                Assert.True(errors.ContainsKey(expectedKey), $"Expected error key '{expectedKey}' was not found");

                // Assert that it contains a useful error message in English
                var message = errors[expectedKey].Single();
                Assert.Contains("is not activated", message.ToLower());
            }
        }

        [Trait(Testing, localUsers)]
        [Fact(DisplayName = "006 - Updating a local user works as expected")]
        public async Task Test3105()
        {
            // Get the entity we just saved
            var id = _shared.GetItem<LocalUser>("LocalUsers_AhmadAkra").Id;
            var response1 = await _client.GetAsync($"{localUsersURL}/{id}?expand=Roles/Role");
            var dto = (await response1.Content.ReadAsAsync<GetByIdResponse<LocalUser>>()).Result;

            // Modify it slightly
            dto.EntityState = "Updated";
            dto.Name = "Ahmed Akra"; // Changed
            dto.Roles[0].EntityState = "Updated";
            dto.Roles[0].Memo = "Nice 2"; // Changed

            // Save it and get the result back
            var dtosForSave = new List<LocalUser> { dto };
            var response2 = await _client.PostAsJsonAsync($"{localUsersURL}?expand=Roles/Role", dtosForSave);
            _output.WriteLine(await response2.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
            var dto2 = (await response2.Content.ReadAsAsync<EntitiesResponse<LocalUser>>()).Result.FirstOrDefault();

            // Confirm it has been changed
            Assert.Equal(dto.Name, dto2.Name);
            Assert.Equal(dto.Name2, dto2.Name2);
            Assert.Equal(dto.Email, dto2.Email);
            Assert.Equal(dto.AgentId, dto2.AgentId);
            Assert.Collection(dto2.Roles,
                    p =>
                    {
                        Assert.Equal(dto.Roles[0].RoleId, p.RoleId);
                        Assert.Equal(dto.Roles[0].Memo, p.Memo);
                    }
                );
        }

        [Trait(Testing, localUsers)]
        [Fact(DisplayName = "007 - Deactivating an active local user returns a 200 OK inactive entity")]
        public async Task Test3106()
        {
            // Get the Id
            var id = _shared.GetItem<LocalUser>("LocalUsers_AhmadAkra").Id.Value;

            // Call the API
            var response = await _client.PutAsJsonAsync($"{localUsersURL}/deactivate", new List<int>() { id });

            // Assert that the response status code is correct
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<LocalUser>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was deactivated
            Assert.False(responseDto.IsActive, "The organization was not deactivated");
        }

        [Trait(Testing, localUsers)]
        [Fact(DisplayName = "008 - Activating an inactive local user returns a 200 OK active entity")]
        public async Task Test3107()
        {
            // Get the Id
            var id = _shared.GetItem<LocalUser>("LocalUsers_AhmadAkra").Id.Value;

            // Call the API
            var response = await _client.PutAsJsonAsync($"{localUsersURL}/activate", new List<int>() { id });

            // Assert that the response status code is correct
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<LocalUser>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was activated
            Assert.True(responseDto.IsActive, "The Organization was not activated");
        }


        [Trait(Testing, localUsers)]
        [Fact(DisplayName = "009 - Deleting an existing local user Id returns a 200 OK")]
        public async Task Test3108()
        {
            // Get the Id
            var id = _shared.GetItem<LocalUser>("LocalUsers_AhmadAkra").Id.Value;

            // Query the delete API
            var msg = new HttpRequestMessage(HttpMethod.Delete, localUsersURL);
            msg.Content = new ObjectContent<List<int>>(new List<int> { id }, new JsonMediaTypeFormatter());
            var deleteResponse = await _client.SendAsync(msg);

            _output.WriteLine(await deleteResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Trait(Testing, localUsers)]
        [Fact(DisplayName = "010 - Getting an Id that was just deleted returns a 404 Not Found")]
        public async Task Test3109()
        {
            // Get the Id
            var id = _shared.GetItem<LocalUser>("LocalUsers_AhmadAkra").Id.Value;

            // Verify that the id was deleted by calling get        
            var getResponse = await _client.GetAsync($"{localUsersURL}/{id}");

            // Assert that the response is correct
            _output.WriteLine(await getResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        // TODO add Import/Export tests
    }
}
