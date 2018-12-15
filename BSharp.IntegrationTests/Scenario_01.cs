using BSharp.Controllers.DTO;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using BSharp.IntegrationTests.Utilities;
using System.Linq;

namespace BSharp.IntegrationTests
{
    [TestCaseOrderer(TestOrderer.TypeName, TestOrderer.AssemblyName)]
    public class Scenario_01 : IClassFixture<Scenario_01_WebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly SharedCollection _shared;
        private readonly ITestOutputHelper _output;

        public Scenario_01(Scenario_01_WebApplicationFactory sharedContext, ITestOutputHelper output)
        {
            _client = sharedContext.GetClient();
            _shared = sharedContext.GetSharedCollection();
            _output = output;
        }

        [Fact(DisplayName = "A0001 - Getting a non-existent measurement unit id returns a 404 Not Found")]
        public async Task Test0001()
        {
            int nonExistentId = 1;
            var response = await _client.GetAsync($"/api/measurement-units/{nonExistentId}");

            _output.WriteLine(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "A0002 - Saving a single well-formed MeasurementUnitForSave returns a 200 OK")]
        public async Task Test0002()
        {
            var dtoForSave = new MeasurementUnitForSave
            {
                EntityState = "Inserted",
                Name = "KG",
                Name2 = "كج",
                Code = "kg",
                UnitType = "Mass",
                BaseAmount = 1,
                UnitAmount = 1
            };

            var dtosForSave = new List<MeasurementUnitForSave> { dtoForSave };

            var response = await _client.PostAsJsonAsync($"/api/measurement-units", dtosForSave);

            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            _shared.SetItem("DtoForSave", dtoForSave);
            _shared.SetItem("Response", response);
        }

        [Fact(DisplayName = "A0003 - The response is a singleton MeasurementUnit with the same values")]
        public async Task Test0003()
        {
            var dtoForSave = _shared.GetItem<MeasurementUnitForSave>("DtoForSave");
            var response = _shared.GetItem<HttpResponseMessage>("Response");
            var responseData = await response.Content.ReadAsAsync<List<MeasurementUnit>>();
            Assert.Single(responseData);

            var responseDto = responseData[0];

            Assert.NotNull(responseDto.Id);
            Assert.Equal(dtoForSave.Name, responseDto.Name);
            Assert.Equal(dtoForSave.Name2, responseDto.Name2);
            Assert.Equal(dtoForSave.Code, responseDto.Code);
            Assert.Equal(dtoForSave.UnitType, responseDto.UnitType);
            Assert.Equal(dtoForSave.BaseAmount, responseDto.BaseAmount);
            Assert.Equal(dtoForSave.UnitAmount, responseDto.UnitAmount);
        }

        [Fact(DisplayName = "A0004 - Saving a MeasurementUnitForSave with an existing code returns a 422 Unprocessable Entity")]
        public async Task Test0004()
        {
            var list = new List<MeasurementUnitForSave> {
                new MeasurementUnitForSave
                {
                    EntityState = "Inserted",
                    Name = "Another Name",
                    Name2 = "Another Name",
                    Code = "kg",
                    UnitType = "Mass",
                    BaseAmount = 1,
                    UnitAmount = 1
                }
            };

            var response = await _client.PostAsJsonAsync($"/api/measurement-units", list);

            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            _shared.SetItem("Response", response);
        }

        [Fact(DisplayName = "A0005 - The response contains a validation error with a key referring to the Code property")]
        public async Task Test0005()
        {
            var response = _shared.GetItem<HttpResponseMessage>("Response");
            var errors = await response.Content.ReadAsAsync<ValidationErrors>();
            string expectedKey = "[0].Code";
            Assert.True(errors.ContainsKey(expectedKey), $"Expected key '{expectedKey}' was not found");

            _shared.SetItem("Errors", errors);
        }

        [Fact(DisplayName = "A0006 - The validation error contains an informative error message")]
        public void Test0006()
        {
            var errors = _shared.GetItem<ValidationErrors>("Errors");
            var message = errors["[0].Code"].Single();
            Assert.Contains("already used", message.ToLower());
        }
    }
}
