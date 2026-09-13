// Copyright (c) Tellma Ltd. All rights reserved.

using System.Net;
using Microsoft.Extensions.Time.Testing;

namespace Tellma.Integration.MarminAe.Tests
{
    /// <summary>
    ///     Covers the request/response path of <see cref="MarminAeClient" /> against a scripted
    ///     transport: the happy path, how refusals surface, and the token cache.
    /// </summary>
    public class MarminAeClientTests
    {
        private const string ProfileId = "MBP-TEST";

        private const string CreatedDocument =
            """
            {"id":"doc-9001","document_number":"INV-1042",
             "meta_info":{"peppol_status":{"overall_status":"PENDING"}}}
            """;

        private static (MarminAeClient Client, ScriptedHandler Handler) Build(
            Func<HttpRequestMessage, int, HttpResponseMessage> respond, TimeProvider? clock = null)
        {
            var handler = new ScriptedHandler(respond);
            var options = new MarminAeClientOptions
            {
                BaseAddress = MarminAeClientOptions.SandboxBaseAddress,
                ClientId = "org_test",
                ClientSecret = "shhh",
            };

            return (new MarminAeClient(new HttpClient(handler), options, timeProvider: clock), handler);
        }

        [Fact]
        public async Task CreateSalesInvoice_ParsesACreatedDocument()
        {
            var (client, handler) = Build((request, _) => ScriptedHandler.IsToken(request)
                ? ScriptedHandler.Token()
                : ScriptedHandler.Json(HttpStatusCode.Created, CreatedDocument));

            var response = await client.CreateSalesInvoiceAsync(ProfileId, TestSamples.Invoice, CancellationToken.None);

            Assert.Equal(201, response.StatusCode);
            Assert.Equal("doc-9001", response.Value.Id);
            Assert.Equal("INV-1042", response.Value.DocumentNumber);

            // The outcome Tellma branches on is the Peppol status, not the HTTP status: a 201 only
            // means the vendor accepted the submission for onward transmission.
            Assert.Equal(
                MarminAePeppolStatus.Pending,
                response.Value.MetaInfo?.PeppolStatus?.OverallStatus);

            // The bearer goes on the request, never on the shared HttpClient default headers.
            HttpRequestMessage submission = handler.Requests.Last();
            Assert.Equal("Bearer", submission.Headers.Authorization?.Scheme);
            Assert.Equal("token-1", submission.Headers.Authorization?.Parameter);
            Assert.Equal(
                MarminAeClient.ApiVersion,
                submission.Headers.GetValues(MarminAeClient.VersionHeaderName).Single());
        }

        [Fact]
        public async Task ARefusal_SurfacesAsAnExceptionCarryingTheFieldErrors()
        {
            const string Refusal =
                """
                {"message":"Validation failed",
                 "errors":[{"field":"document_lines[0].unit_code","message":"Unknown unit code"}]}
                """;

            var (client, _) = Build((request, _) => ScriptedHandler.IsToken(request)
                ? ScriptedHandler.Token()
                : ScriptedHandler.Json(HttpStatusCode.UnprocessableEntity, Refusal));

            var exception = await Assert.ThrowsAsync<MarminAeRequestException>(
                () => client.CreateSalesInvoiceAsync(ProfileId, TestSamples.Invoice, CancellationToken.None));

            Assert.Equal(422, exception.StatusCode);
            Assert.False(exception.IsRateLimited);
            Assert.Equal("Validation failed", exception.Detail?.Message);

            // The field-level detail is what makes the tenant-admin alert actionable.
            Assert.Contains("unit_code", exception.Detail?.Describe());
        }

        [Fact]
        public async Task BeingRateLimited_IsDistinguishableAndCarriesTheQuota()
        {
            var (client, _) = Build((request, _) =>
            {
                if (ScriptedHandler.IsToken(request))
                {
                    return ScriptedHandler.Token();
                }

                var response = ScriptedHandler.Json(
                    HttpStatusCode.TooManyRequests, """{"message":"Slow down"}""");
                response.Headers.TryAddWithoutValidation(
                    MarminAeClient.RateLimitLimitPerMinuteHeaderName, "60");
                response.Headers.TryAddWithoutValidation(
                    MarminAeClient.RateLimitRemainingPerMinuteHeaderName, "0");
                return response;
            });

            var exception = await Assert.ThrowsAsync<MarminAeRequestException>(
                () => client.CreateSalesInvoiceAsync(ProfileId, TestSamples.Invoice, CancellationToken.None));

            Assert.True(exception.IsRateLimited);
            Assert.Equal(60, exception.RateLimit.Limit);
            Assert.Equal(0, exception.RateLimit.Remaining);
        }

        [Fact]
        public async Task TwoCalls_ShareOneToken()
        {
            // The token endpoint is rate-limited like everything else, so a token per call would
            // roughly halve the usable submission throughput.
            var (client, handler) = Build((request, _) => ScriptedHandler.IsToken(request)
                ? ScriptedHandler.Token()
                : ScriptedHandler.Json(HttpStatusCode.Created, CreatedDocument));

            await client.CreateSalesInvoiceAsync(ProfileId, TestSamples.Invoice, CancellationToken.None);
            await client.CreateSalesInvoiceAsync(ProfileId, TestSamples.Invoice, CancellationToken.None);

            Assert.Equal(1, handler.TokenRequests);
        }

        [Fact]
        public async Task AnExpiredToken_IsRefreshedRatherThanReused()
        {
            var clock = new FakeTimeProvider(DateTimeOffset.Parse("2026-09-03T00:00:00Z"));
            int issued = 0;

            var (client, handler) = Build(
                (request, _) => ScriptedHandler.IsToken(request)
                    ? ScriptedHandler.Token($"token-{++issued}")
                    : ScriptedHandler.Json(HttpStatusCode.Created, CreatedDocument),
                clock);

            await client.CreateSalesInvoiceAsync(ProfileId, TestSamples.Invoice, CancellationToken.None);

            // Past the hour the token said it was good for, so the cache must not serve it again.
            clock.Advance(TimeSpan.FromHours(2));
            await client.CreateSalesInvoiceAsync(ProfileId, TestSamples.Invoice, CancellationToken.None);

            Assert.Equal(2, handler.TokenRequests);
            Assert.Equal("token-2", handler.Requests.Last().Headers.Authorization?.Parameter);
        }

        [Fact]
        public async Task A401_IsRetriedExactlyOnceWithAFreshToken()
        {
            // The one retry the client performs: a token that expired between issue and use.
            int issued = 0;
            var (client, handler) = Build((request, _) =>
            {
                if (ScriptedHandler.IsToken(request))
                {
                    return ScriptedHandler.Token($"token-{++issued}");
                }

                return issued == 1
                    ? ScriptedHandler.Json(HttpStatusCode.Unauthorized, """{"message":"expired"}""")
                    : ScriptedHandler.Json(HttpStatusCode.Created, CreatedDocument);
            });

            var response = await client.CreateSalesInvoiceAsync(ProfileId, TestSamples.Invoice, CancellationToken.None);

            Assert.Equal(201, response.StatusCode);
            Assert.Equal(2, handler.TokenRequests);
        }
    }
}
