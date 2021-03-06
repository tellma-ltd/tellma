﻿using Tellma.Controllers.Dto;
using Tellma.Entities;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Tellma.IntegrationTests.Scenario_01
{
    public class Tests_01_Settings : Scenario_01
    {
        public Tests_01_Settings(Scenario_01_WebApplicationFactory factory, ITestOutputHelper output) : base(factory, output)
        {
        }

        public const string settingsURL = "/api/settings";

        [Fact(DisplayName = "01 Getting settings before granting permissions returns a 403 Forbidden response")]
        public async Task Test01()
        {
            var response = await Client.GetAsync(settingsURL);

            // Call the API
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 403 OK
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact(DisplayName = "02 Getting settings returns a 200 OK settings object")]
        public async Task Test02()
        {
            await GrantPermissionToSecurityAdministrator("settings", "Update", criteria: null);

            // Call the API
            var response = await Client.GetAsync(settingsURL);
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<GetEntityResponse<Settings>>();

            // Assert the result makes sense
            var settings = responseData.Result;

            Assert.Contains("ACME", settings.ShortCompanyName);
            Assert.Equal("en", settings.PrimaryLanguageId);

            Shared.Set("Settings", responseData.Result);
        }
        
        [Fact(DisplayName = "03 Saving a well formed settings returns a 200 OK response")]
        public async Task Test03()
        {
            // Prepare a well formed entity
            var settings = Shared.Get<Settings>("Settings");

            settings.ShortCompanyName2 = "كونتوسو المحدودة";
            settings.PrimaryLanguageSymbol = "En";
            settings.SecondaryLanguageId = "ar";
            settings.SecondaryLanguageSymbol = "ع";
            settings.BrandColor = "#123456";

            var response = await Client.PostAsJsonAsync($"{settingsURL}?returnEntities=true", settings);

            // Assert that the response status code is a happy 200 OK
            Output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<SaveSettingsResponse>();

            var responseDto = responseData.Result;
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
        
        [Fact(DisplayName = "04 Getting settings for client returns a 200 OK settings object with a version")]
        public async Task Test04()
        {
            // Call the API
            var response = await Client.GetAsync($"{settingsURL}/client");
            Output.WriteLine(await response.Content.ReadAsStringAsync());

            // Assert the result is 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Confirm the result is a well formed response
            var responseData = await response.Content.ReadAsAsync<Versioned<SettingsForClient>>();

            // Assert the result makes sense
            Assert.NotNull(responseData.Version);
            var settings = responseData.Data;

            Assert.Contains("ACME", settings.ShortCompanyName);
            Assert.Equal("English", settings.PrimaryLanguageName);
        }
    }
}
