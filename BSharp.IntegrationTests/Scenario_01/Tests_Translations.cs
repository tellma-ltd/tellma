using BSharp.Controllers.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace BSharp.IntegrationTests.Scenario_01
{
    public partial class Scenario_01
    {
        public const string Translations = "99 - Translations";


        [Trait(Testing, Translations)]
        [Fact(DisplayName = "001 - Getting client translations returns a 200 OK result")]
        public async Task Translations001()
        {
            var lang = "en";
            var response = await _client.GetAsync($"/api/translations/client/{lang}");

            // Call the API
            _output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed dictionary
            var responseData = await response.Content.ReadAsAsync<DataWithVersion<Dictionary<string, string>>>();

            // Assert the result makes sense
            Assert.True(responseData.Data.Count > 0, "Result was empty");
            Assert.True(responseData.Data.ContainsKey("AppName"));
            Assert.True(responseData.Data.ContainsValue("BSharp"));
        }

    }
}
