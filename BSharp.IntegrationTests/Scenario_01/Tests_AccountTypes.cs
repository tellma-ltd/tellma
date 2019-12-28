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
    public class Tests_06_AccountTypes : Scenario_01
    {
        public Tests_06_AccountTypes(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        public readonly string _baseAddress = "account-types";

        public string Url => $"/api/{_baseAddress}"; // For querying and updating specific account definition
        public string View => $"{_baseAddress}"; // For permissions

        [Fact(DisplayName = "01 Getting all account types returns a 200 OK collection")]
        public async Task Test01()
        {
            await GrantPermissionToSecurityAdministrator(View, Constants.Read, "Id ne 'Bla'");

            // Call the API
            var response = await Client.GetAsync(Url);
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<AccountType>>();

            // Assert the result makes sense
            Assert.Equal("AccountType", responseData.CollectionName);

            Assert.NotEmpty(responseData.Result);

            Shared.Set("AccountType", responseData.Result.FirstOrDefault());
        }

        [Fact(DisplayName = "02 Getting a non-existent account type id returns a 404 Not Found")]
        public async Task Test02()
        {
            int nonExistentId = -1;
            var response = await Client.GetAsync($"{Url}/{nonExistentId}");

            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "03 Getting an existing Id returns a 200 OK result")]
        public async Task Test03()
        {
            // Query the API for the Id that was just returned from the Save
            var entity = Shared.Get<AccountType>("AccountType");
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
            Assert.Equal(entity.Name3, responseDto.Name3);
            Assert.Equal(entity.Description, responseDto.Description);
            Assert.Equal(entity.Description2, responseDto.Description2);
            Assert.Equal(entity.Description3, responseDto.Description3);
        }
    }
}
