using BSharp.Controllers.DTO;
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
        public const string settings = "06 - Settings";
        public const string settingsURL = "/api/settings";


        [Trait(Testing, settings)]
        [Fact(DisplayName = "001 - Getting settings before granting permissions returns a 403 Forbidden response")]
        public async Task Test3300()
        {
            var response = await _client.GetAsync(settingsURL);

            // Call the API
            _output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Trait(Testing, settings)]
        [Fact(DisplayName = "002 - Getting settings returns a 200 OK settings object")]
        public async Task Test3301()
        {
            await GrantPermissionToSecurityAdministrator("settings", "Update", criteria: null);

            // Call the API
            var response = await _client.GetAsync(settingsURL);
            _output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetByIdResponse<Settings>>();

            // Assert the result makes sense
            Assert.Equal("Settings", responseData.CollectionName);
            var settings = responseData.Entity;

            Assert.Contains("Contoso", settings.ShortCompanyName);
            Assert.Equal("en", settings.PrimaryLanguageId);

            _shared.SetItem("Settings", responseData.Entity);
        }

        [Trait(Testing, settings)]
        [Fact(DisplayName = "003 - Saving a well formed settings returns a 200 OK response")]
        public async Task Test3302()
        {
            // Prepare a well formed entity
            var settings = _shared.GetItem<Settings>("Settings");

            settings.ShortCompanyName2 = "كونتوسو المحدودة";
            settings.PrimaryLanguageSymbol = "En";
            settings.SecondaryLanguageId = "ar";
            settings.SecondaryLanguageSymbol = "ع";
            settings.BrandColor = "#123456";

            var response = await _client.PostAsJsonAsync($"{settingsURL}?returnEntities=true", settings);

            // Assert that the response status code is a happy 200 OK
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetByIdResponse<Settings>>();
            var responsedDto = responseData.Entity;

            // Assert that the result matches the saved entity
            Assert.Equal("Settings", responseData.CollectionName);

            var responseDto = responseData.Entity;
            Assert.Equal(settings.ShortCompanyName, responseDto.ShortCompanyName);
            Assert.Equal(settings.ShortCompanyName2, responseDto.ShortCompanyName2);
            Assert.Equal(settings.PrimaryLanguageId, responseDto.PrimaryLanguageId);
            Assert.Equal(settings.PrimaryLanguageSymbol, responseDto.PrimaryLanguageSymbol);
            Assert.Equal(settings.SecondaryLanguageId, responseDto.SecondaryLanguageId);
            Assert.Equal(settings.SecondaryLanguageSymbol, responseDto.SecondaryLanguageSymbol);
            Assert.Equal(settings.BrandColor, responseDto.BrandColor);

            // Make sure the settings hash is updated
            Assert.NotEqual(settings.SettingsVersion, responseDto.SettingsVersion);
        }

        [Trait(Testing, settings)]
        [Fact(DisplayName = "004 - Getting settings for client returns a 200 OK settings object with a version")]
        public async Task Test3303()
        {
            // Call the API
            var response = await _client.GetAsync($"{settingsURL}/client");
            _output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<DataWithVersion<SettingsForClient>>();

            // Assert the result makes sense
            Assert.NotNull(responseData.Version);
            var settings = responseData.Data;

            Assert.Contains("Contoso", settings.ShortCompanyName);
            Assert.Equal("English", settings.PrimaryLanguageName);
        }
    }
}
