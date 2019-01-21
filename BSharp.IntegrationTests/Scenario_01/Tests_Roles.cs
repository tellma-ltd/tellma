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
        public const string roles = "04 - Roles";
        public const string rolesURL = "/api/roles";

        [Trait(Testing, roles)]
        [Fact(DisplayName = "001 - Getting all roles before creating any returns a 200 OK empty collection")]
        public async Task Test3000()
        {
            var response = await _client.GetAsync(rolesURL);

            // Call the API
            _output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<Agent>>();

            // Assert the result makes sense
            Assert.Equal("Roles", responseData.CollectionName);

            Assert.Equal(0, responseData.TotalCount);
            Assert.Empty(responseData.Data);
        }

        [Trait(Testing, roles)]
        [Fact(DisplayName = "002 - Getting a non-existent role id returns a 404 Not Found")]
        public async Task Test3001()
        {
            int nonExistentId = 1;
            var response = await _client.GetAsync($"{rolesURL}/{nonExistentId}");

            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Trait(Testing, roles)]
        [Fact(DisplayName = "003 - Saving a single well-formed role for save returns a 200 OK result")]
        public async Task Test3002()
        {
            // Prepare a well formed entity
            var dtoForSave = new RoleForSave
            {
                EntityState = "Inserted",
                Name = "Sales Manager",
                Name2 = "مدير المبيعات",
                Code = "SM",
                IsPublic = false,
                Permissions = new List<PermissionForSave>
                {
                    new PermissionForSave
                    {
                        EntityState = "Inserted",
                        ViewId = "Individual",
                        Level = "Read"
                    },
                    new PermissionForSave
                    {
                        EntityState = "Inserted",
                        ViewId = "Organization",
                        Level = "Update"
                    }
                }
            };

            // Save it
            var dtosForSave = new List<RoleForSave> { dtoForSave };
            var response = await _client.PostAsJsonAsync($"{rolesURL}?expand=Permissions", dtosForSave);

            // Assert that the response status code is a happy 200 OK
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert that the response is well-formed singleton of Agent
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Role>>();
            Assert.Single(responseData.Data);

            // Assert that the result matches the saved entity
            Assert.Equal("Roles", responseData.CollectionName);

            var responseDto = responseData.Data.FirstOrDefault();
            Assert.NotNull(responseDto?.Id);
            Assert.Equal(dtoForSave.Name, responseDto.Name);
            Assert.Equal(dtoForSave.Name2, responseDto.Name2);
            Assert.Equal(dtoForSave.Code, responseDto.Code);
            Assert.Equal(dtoForSave.IsPublic, responseDto.IsPublic);
            Assert.Collection(responseDto.Permissions,
                    p => {
                        Assert.Equal(dtoForSave.Permissions[0].Level, p.Level);
                        Assert.Equal(dtoForSave.Permissions[0].ViewId, p.ViewId);
                        Assert.NotNull(p.Id);
                    },
                    p => {
                        Assert.Equal(dtoForSave.Permissions[1].Level, p.Level);
                        Assert.Equal(dtoForSave.Permissions[1].ViewId, p.ViewId);
                        Assert.NotNull(p.Id);
                    }
                );

            _shared.SetItem("Role_SalesManager", responseDto);
        }

        [Trait(Testing, roles)]
        [Fact(DisplayName = "004 - Getting the Id of the role just saved returns a 200 OK result")]
        public async Task Test3003()
        {
            // Query the API for the Id that was just returned from the Save
            var entity = _shared.GetItem<Role>("Role_SalesManager");
            var id = entity.Id;
            var response = await _client.GetAsync($"{rolesURL}/{id}?expand=Permissions");

            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse of agent
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<Role>>();
            Assert.Equal("Roles", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Entity;
            Assert.NotNull(responseDto?.Id);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Equal(entity.Name2, responseDto.Name2);
            Assert.Equal(entity.Code, responseDto.Code);
            Assert.Equal(entity.IsPublic, responseDto.IsPublic);
            Assert.Collection(responseDto.Permissions,
                    p => {
                        Assert.Equal(entity.Permissions[0].Level, p.Level);
                        Assert.Equal(entity.Permissions[0].ViewId, p.ViewId);
                        Assert.NotNull(p.Id);
                    },
                    p => {
                        Assert.Equal(entity.Permissions[1].Level, p.Level);
                        Assert.Equal(entity.Permissions[1].ViewId, p.ViewId);
                        Assert.NotNull(p.Id);
                    }
                );
        }

        [Trait(Testing, roles)]
        [Fact(DisplayName = "005 - Saving a role with the wrong code returns a 422 Unprocessable Entity")]
        public async Task Test3004()
        {
            // Prepare a unit with the same code as one that has been saved already
            var dtoForSave = new RoleForSave
            {
                EntityState = "Inserted",
                Name = "HR Manager",
                Name2 = "مدير الموارد البشرية",
                Code = "HR",
                IsPublic = false,
                Permissions = new List<PermissionForSave>
                {
                    new PermissionForSave
                    {
                        EntityState = "Inserted",
                        ViewId = "DoesntExist", // Doesn't exist
                        Level = "Read"
                    }
                }
            };

            // Call the API
            var dtosForSave = new List<RoleForSave> { dtoForSave };
            var response = await _client.PostAsJsonAsync(rolesURL, dtosForSave);

            // Assert that the response status code is 422 unprocessable entity (validation errors)
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            // Confirm that the result is a well-formed validation errors structure
            var errors = await response.Content.ReadAsAsync<ValidationErrors>();

            // Assert that it contains a validation key pointing to the Code property
            string expectedKey = "[0].Permissions[0].ViewId";
            Assert.True(errors.ContainsKey(expectedKey), $"Expected error key '{expectedKey}' was not found");

            // Assert that it contains a useful error message in English
            var message = errors[expectedKey].Single();
            Assert.Contains("is not activated", message.ToLower());
        }

        [Trait(Testing, roles)]
        [Fact(DisplayName = "006 - Updating a role works as expected")]
        public async Task Test3005()
        {
            // Get the entity we just saved
            var id = _shared.GetItem<Role>("Role_SalesManager").Id;
            var response1 = await _client.GetAsync($"{rolesURL}/{id}?expand=Permissions");
            var dto = (await response1.Content.ReadAsAsync<GetByIdResponse<Role>>()).Entity;
            
            // Modify it slightly
            dto.EntityState = "Updated";
            dto.Permissions[0].Level = "Create";
            dto.Permissions[0].EntityState = "Updated";
            dto.Permissions[1].EntityState = "Deleted";

            // Save it and get the result back
            var dtosForSave = new List<Role> { dto };
            var response2 = await _client.PostAsJsonAsync($"{rolesURL}?expand=Permissions", dtosForSave);
            _output.WriteLine(await response2.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
            var dto2 = (await response2.Content.ReadAsAsync<EntitiesResponse<Role>>()).Data.FirstOrDefault();

            // Confirm it has been changed
            Assert.Equal(dto.Name, dto2.Name);
            Assert.Equal(dto.Name2, dto2.Name2);
            Assert.Equal(dto.Code, dto2.Code);
            Assert.Equal(dto.IsPublic, dto2.IsPublic);
            Assert.Collection(dto2.Permissions,
                    p => {
                        Assert.Equal(dto.Permissions[0].Level, p.Level);
                        Assert.Equal(dto.Permissions[0].ViewId, p.ViewId);
                    }
                );
        }

        [Trait(Testing, roles)]
        [Fact(DisplayName = "007 - Deactivating an active organization returns a 200 OK inactive entity")]
        public async Task Test3006()
        {
            // Get the Id
            var id = _shared.GetItem<Role>("Role_SalesManager").Id.Value;

            // Call the API
            var response = await _client.PutAsJsonAsync($"{rolesURL}/deactivate", new List<int>() { id });

            // Assert that the response status code is correct
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Agent>>();
            Assert.Single(responseData.Data);
            var responseDto = responseData.Data.Single();

            // Confirm that the entity was deactivated
            Assert.False(responseDto.IsActive, "The organization was not deactivated");
        }

        [Trait(Testing, roles)]
        [Fact(DisplayName = "008 - Activating an inactive organization returns a 200 OK active entity")]
        public async Task Test3007()
        {
            // Get the Id
            var id = _shared.GetItem<Role>("Role_SalesManager").Id.Value;

            // Call the API
            var response = await _client.PutAsJsonAsync($"{rolesURL}/activate", new List<int>() { id });

            // Assert that the response status code is correct
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Agent>>();
            Assert.Single(responseData.Data);
            var responseDto = responseData.Data.Single();

            // Confirm that the entity was activated
            Assert.True(responseDto.IsActive, "The Organization was not activated");
        }


        [Trait(Testing, roles)]
        [Fact(DisplayName = "009 - Deleting an existing organization Id returns a 200 OK")]
        public async Task Test3008()
        {
            // Get the Id
            var id = _shared.GetItem<Role>("Role_SalesManager").Id.Value;

            // Query the delete API
            var msg = new HttpRequestMessage(HttpMethod.Delete, rolesURL);
            msg.Content = new ObjectContent<List<int>>(new List<int> { id }, new JsonMediaTypeFormatter());
            var deleteResponse = await _client.SendAsync(msg);

            _output.WriteLine(await deleteResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Trait(Testing, roles)]
        [Fact(DisplayName = "010 - Getting an Id that was just deleted returns a 404 Not Found")]
        public async Task Test3009()
        {
            // Get the Id
            var id = _shared.GetItem<Role>("Role_SalesManager").Id.Value;

            // Verify that the id was deleted by calling get        
            var getResponse = await _client.GetAsync($"{rolesURL}/{id}");

            // Assert that the response is correct
            _output.WriteLine(await getResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        // TODO add Import/Export tests
    }
}
