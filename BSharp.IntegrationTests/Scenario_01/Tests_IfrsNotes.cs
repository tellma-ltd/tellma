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
        public const string IfrsNotes = "07 - IFRS notes";

        [Trait(Testing, IfrsNotes)]
        [Fact(DisplayName = "000 - Getting all IFRS notes before granting permissions returns a 403 Forbidden response")]
        public async Task Test3400()
        {
            var response = await _client.GetAsync($"/api/ifrs-notes");

            // Call the API
            _output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Trait(Testing, IfrsNotes)]
        [Fact(DisplayName = "001 - Getting all IFRS notes before creating any returns a 200 OK empty collection")]
        public async Task Test3401()
        {
            await GrantPermissionToSecurityAdministrator("ifrs-notes", Constants.Read, null);

            // Call the API
            var response = await _client.GetAsync($"/api/ifrs-notes");
            _output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetResponse<IfrsNote>>();

            // Assert the result makes sense
            Assert.Equal("IfrsNote", responseData.CollectionName);

            Assert.Equal(0, responseData.TotalCount);
            Assert.Empty(responseData.Result);
        }

        [Trait(Testing, IfrsNotes)]
        [Fact(DisplayName = "002 - Getting a non-existent IFRS note id returns a 404 Not Found")]
        public async Task Test3402()
        {
            string nonExistentId = "doesnt_exist";
            var response = await _client.GetAsync($"/api/ifrs-notes/{nonExistentId}");

            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        //[Trait(Testing, IfrsNotes)]
        //[Fact(DisplayName = "010 - Activating an inactive IFRS note returns a 200 OK active entity")]
        //public async Task Test3409()
        //{
        //    // Get the Id
        //    var entity = _shared.GetItem<IfrsNote>("IfrsNote_kg");
        //    var id = entity.Id.Value;

        //    // Call the API
        //    var response = await _client.PutAsJsonAsync($"/api/ifrs-notes/activate", new List<int>() { id });

        //    // Assert that the response status code is correct
        //    _output.WriteLine(await response.Content.ReadAsStringAsync());
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //    // Confirm that the response content is well formed singleton
        //    var responseData = await response.Content.ReadAsAsync<EntitiesResponse<IfrsNote>>();
        //    Assert.Single(responseData.Data);
        //    var responseDto = responseData.Data.Single();

        //    // Confirm that the entity was activated
        //    Assert.True(responseDto.IsActive, "The IFRS note was not activated");
        //}

        //[Trait(Testing, IfrsNotes)]
        //[Fact(DisplayName = "011 - Using Select argument works as expected")]
        //public async Task Test3410()
        //{
        //    // Get the Id
        //    var entity = _shared.GetItem<IfrsNote>("IfrsNote_kg");
        //    var id = entity.Id.Value;

        //    var response = await _client.GetAsync($"/api/ifrs-notes/{id}?select=Name");

        //    _output.WriteLine(await response.Content.ReadAsStringAsync());
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //    // Confirm that the response is a well formed GetByIdResponse of IFRS note
        //    var getByIdResponse = await response.Content.ReadAsAsync<GetByIdResponse<IfrsNote>>();
        //    Assert.Equal("IfrsNotes", getByIdResponse.CollectionName);

        //    var responseDto = getByIdResponse.Entity;
        //    Assert.Equal(id, responseDto.Id);
        //    Assert.Equal(entity.Name, responseDto.Name);
        //    Assert.Null(responseDto.Name2);
        //    Assert.Null(responseDto.Code);
        //    Assert.Null(responseDto.UnitType);
        //    Assert.Null(responseDto.BaseAmount);
        //    Assert.Null(responseDto.UnitAmount);
        //}


        // TODO add Import/Export tests
    }
}
