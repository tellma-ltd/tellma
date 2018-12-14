using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace BSharp.IntegrationTests
{
    public class Scenario01 : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public Scenario01(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Add("Tenant-Id", "101");
        }

        [Fact(DisplayName = "GETing a non-existent measurement Unit returns a 404 Not Found")]
        public async Task Run()
        {
            int nonExistentId = 9999999;
            var response = await _client.GetAsync($"/api/measurement-units/{nonExistentId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
