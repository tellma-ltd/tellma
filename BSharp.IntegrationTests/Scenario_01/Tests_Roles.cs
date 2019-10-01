using BSharp.Controllers.Dto;
using BSharp.Entities;
using BSharp.IntegrationTests.Utilities;
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
    public class Tests_10_Roles : Scenario_01
    {
        public Tests_10_Roles(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        public readonly string _baseAddress = "roles";

        public string Url => $"/api/{_baseAddress}";

        [Fact(DisplayName = "01 Getting all roles before creating any returns a 200 OK singleton collection")]
        public async Task Test01()
        {
            var response = await Client.GetAsync(Url);

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<Role>>();

            // Assert the result makes sense
            Assert.Equal("Role", responseData.CollectionName);

            Assert.Equal(1, responseData.TotalCount);
            Assert.Single(responseData.Result); // Security Administrator role
        }

        [Fact(DisplayName = "02 Getting a non-existent role id returns a 404 Not Found")]
        public async Task Test02()
        {
            int nonExistentId = 999;
            var response = await Client.GetAsync($"{Url}/{nonExistentId}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "03 Saving 2 well-formed role for save returns a 200 OK result")]
        public async Task Test03()
        {
            int johnWickId = Shared.Get<Agent>("Agent_JohnWick").Id;

            // Prepare a well formed entity
            var dtoForSave = new RoleForSave
            {
                Name = "Sales Manager",
                Name2 = "مدير المبيعات",
                Code = "SM",
                IsPublic = false,
                Permissions = new List<PermissionForSave>
                {
                    new PermissionForSave
                    {
                        ViewId = "users",
                        Action = "Read"
                    },
                    new PermissionForSave
                    {
                        ViewId = "measurement-units",
                        Action = "Update"
                    }
                },
                Members = new List<RoleMembershipForSave>
                {
                    new RoleMembershipForSave
                    {
                        AgentId = johnWickId,
                        Memo = "So Good"
                    }
                }
            };

            var dtoForSave2 = new RoleForSave
            {
                Name = "Chief of Staff",
                Name2 = "مدير المكتب",
                Code = "CS",
                IsPublic = false,
                Permissions = new List<PermissionForSave>
                {
                    new PermissionForSave
                    {
                        ViewId = "agents",
                        Action = "Update"
                    }
                },
                Members = new List<RoleMembershipForSave>
                {
                }
            };

            // Save it
            var dtosForSave = new List<RoleForSave> { dtoForSave, dtoForSave2 };
            var response = await Client.PostAsJsonAsync($"{Url}?expand=Permissions,Members/Agent", dtosForSave);

            // Assert that the response status code is a happy 200 OK
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert that the response is well-formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Role>>();
            Assert.Collection(responseData.Result, 
                e => Assert.NotEqual(0, e.Id),
                e => Assert.NotEqual(0, e.Id));

            // Assert that the result matches the saved entity
            Assert.Equal("Role", responseData.CollectionName);

            var responseDto = responseData.Result.FirstOrDefault();
            Assert.Equal(dtoForSave.Name, responseDto.Name);
            Assert.Equal(dtoForSave.Name2, responseDto.Name2);
            Assert.Equal(dtoForSave.Code, responseDto.Code);
            Assert.Equal(dtoForSave.IsPublic, responseDto.IsPublic);
            Assert.Collection(responseDto.Permissions,
                    p =>
                    {
                        Assert.Equal(dtoForSave.Permissions[0].Action, p.Action);
                        Assert.Equal(dtoForSave.Permissions[0].ViewId, p.ViewId);
                        Assert.NotEqual(0, p.Id);
                    },
                    p =>
                    {
                        Assert.Equal(dtoForSave.Permissions[1].Action, p.Action);
                        Assert.Equal(dtoForSave.Permissions[1].ViewId, p.ViewId);
                        Assert.NotEqual(0, p.Id);
                    }
                );

            Assert.Collection(responseDto.Members,
                    m =>
                    {
                        Assert.Equal(dtoForSave.Members[0].AgentId, m.AgentId);
                        Assert.Equal(dtoForSave.Members[0].Memo, m.Memo);
                        Assert.NotEqual(0, m.Id);
                    }
                );

            // Get the second result
            var responseDto2 = responseData.Result.LastOrDefault();
            Assert.Equal(dtoForSave2.Name, responseDto2.Name);
            Assert.Equal(dtoForSave2.Name2, responseDto2.Name2);
            Assert.Equal(dtoForSave2.Code, responseDto2.Code);
            Assert.Equal(dtoForSave2.IsPublic, responseDto2.IsPublic);
            Assert.Collection(responseDto2.Permissions,
                    p =>
                    {
                        Assert.Equal(dtoForSave2.Permissions[0].Action, p.Action);
                        Assert.Equal(dtoForSave2.Permissions[0].ViewId, p.ViewId);
                        Assert.NotEqual(0, p.Id);
                    }
                );

            Assert.Empty(responseDto2.Members);

            Shared.Set("Role_SalesManager", responseDto);
            Shared.Set("Role_ChiefOfStaff", responseDto2);
        }

        [Fact(DisplayName = "04 Getting the Id of the role just saved returns a 200 OK result")]
        public async Task Test04()
        {
            // Query the API for the Id that was just returned from the Save
            var entity = Shared.Get<Role>("Role_SalesManager");
            var id = entity.Id;
            var response = await Client.GetAsync($"{Url}/{id}?expand=Permissions,Members/Agent");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response is a well formed GetByIdResponse
            var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<Role>>();
            Assert.Equal("Role", getByIdResponse.CollectionName);

            var responseDto = getByIdResponse.Result;
            Assert.NotNull(responseDto?.Id);
            Assert.Equal(entity.Name, responseDto.Name);
            Assert.Equal(entity.Name2, responseDto.Name2);
            Assert.Equal(entity.Code, responseDto.Code);
            Assert.Equal(entity.IsPublic, responseDto.IsPublic);
            Assert.Collection(responseDto.Permissions,
                    p =>
                    {
                        Assert.Equal(entity.Permissions[0].Action, p.Action);
                        Assert.Equal(entity.Permissions[0].ViewId, p.ViewId);
                        Assert.NotEqual(0, p.Id);
                    },
                    p =>
                    {
                        Assert.Equal(entity.Permissions[1].Action, p.Action);
                        Assert.Equal(entity.Permissions[1].ViewId, p.ViewId);
                        Assert.NotEqual(0, p.Id);
                    }
                );


            Assert.Collection(responseDto.Members,
                    m =>
                    {
                        Assert.Equal(entity.Members[0].AgentId, m.AgentId);
                        Assert.Equal(entity.Members[0].Memo, m.Memo);
                        Assert.NotEqual(0, m.Id);
                    }
                );
        }

        //[Fact(DisplayName = "Saving a role with an inactive view Id returns a 422 Unprocessable Entity")]
        //public async Task Test05()
        //{
        //    // Prepare a unit with the same code as one that has been saved already
        //    var dtoForSave = new RoleForSave
        //    {
        //        Name = "HR Manager",
        //        Name2 = "مدير الموارد البشرية",
        //        Code = "HR",
        //        IsPublic = false,
        //        Permissions = new List<PermissionForSave>
        //        {
        //            new PermissionForSave
        //            {
        //                ViewId = "DoesntExist", // Doesn't exist
        //                Action = "Read"
        //            }
        //        }
        //    };

        //    // Call the API
        //    var dtosForSave = new List<RoleForSave> { dtoForSave };
        //    var response = await Client.PostAsJsonAsync(rolesURL, dtosForSave);

        //    // Assert that the response status code is 422 unprocessable entity (validation errors)
        //    Output.WriteLine(await response.Content.ReadAsStringAsync());
        //    Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        //    // Confirm that the result is a well-formed validation errors structure
        //    var errors = await response.Content.ReadAsAsync<ValidationErrors>();

        //    // Assert that it contains a validation key pointing to the ViewId property of the permission line
        //    string expectedKey = "[0].Permissions[0].ViewId";
        //    Assert.True(errors.ContainsKey(expectedKey), $"Expected error key '{expectedKey}' was not found");

        //    // Assert that it contains a useful error message in English
        //    var message = errors[expectedKey].Single();
        //    Assert.Contains("is not activated", message.ToLower());
        //}

        [Fact(DisplayName = "06 Updating a role works as expected")]
        public async Task Test06()
        {
            // Get the entity we just saved
            var id = Shared.Get<Role>("Role_SalesManager").Id;
            var response1 = await Client.GetAsync($"{Url}/{id}?expand=Permissions,Members/Agent");
            var dto = (await response1.Content.ReadAsAsync<GetByIdResponse<Role>>()).Result;

            // Modify it slightly
            dto.Permissions[0].Action = "Update";
            dto.Permissions.RemoveAt(1);

            dto.Members.RemoveAt(0);

            // Save it and get the result back
            var dtosForSave = new List<Role> { dto };
            var response2 = await Client.PostAsJsonAsync($"{Url}?expand=Permissions,Members/Agent", dtosForSave);
            Output.WriteLine(await response2.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
            var dto2 = (await response2.Content.ReadAsAsync<EntitiesResponse<Role>>()).Result.FirstOrDefault();

            // Confirm it has been changed
            Assert.Equal(dto.Name, dto2.Name);
            Assert.Equal(dto.Name2, dto2.Name2);
            Assert.Equal(dto.Code, dto2.Code);
            Assert.Equal(dto.IsPublic, dto2.IsPublic);
            Assert.Collection(dto2.Permissions,
                    p =>
                    {
                        Assert.Equal(dto.Permissions[0].Action, p.Action);
                        Assert.Equal(dto.Permissions[0].ViewId, p.ViewId);
                    }
                );

            Assert.Empty(dto2.Members);
        }

        [Fact(DisplayName = "07 Deactivating an active role returns a 200 OK inactive entity")]
        public async Task Test07()
        {
            // Get the Id
            var id = Shared.Get<Role>("Role_SalesManager").Id;

            // Call the API
            var response = await Client.PutAsJsonAsync($"{Url}/deactivate", new List<int>() { id });

            // Assert that the response status code is correct
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Role>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was deactivated
            Assert.False(responseDto.IsActive, "The role was not deactivated");
        }

        [Fact(DisplayName = "08 Activating an inactive role returns a 200 OK active entity")]
        public async Task Test08()
        {
            // Get the Id
            var id = Shared.Get<Role>("Role_SalesManager").Id;

            // Call the API
            var response = await Client.PutAsJsonAsync($"{Url}/activate", new List<int>() { id });

            // Assert that the response status code is correct
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm that the response content is well formed singleton
            var responseData = await response.Content.ReadAsAsync<EntitiesResponse<Role>>();
            Assert.Single(responseData.Result);
            var responseDto = responseData.Result.Single();

            // Confirm that the entity was activated
            Assert.True(responseDto.IsActive, "The role was not activated");
        }

        [Fact(DisplayName = "09 Deleting an existing role Id returns a 200 OK")]
        public async Task Test09()
        {
            // Get the Id
            var id = Shared.Get<Role>("Role_SalesManager").Id;

            // Query the delete API
            var msg = new HttpRequestMessage(HttpMethod.Delete, Url);
            msg.Content = new ObjectContent<List<int>>(new List<int> { id }, new JsonMediaTypeFormatter());
            var deleteResponse = await Client.SendAsync(msg);

            Output.WriteLine(await deleteResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact(DisplayName = "10 Getting an Id that was just deleted returns a 404 Not Found")]
        public async Task Test10()
        {
            // Get the Id
            var id = Shared.Get<Role>("Role_SalesManager").Id;

            // Verify that the id was deleted by calling get        
            var getResponse = await Client.GetAsync($"{Url}/{id}");

            // Assert that the response is correct
            Output.WriteLine(await getResponse.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }
    }
}
