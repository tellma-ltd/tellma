// Copyright (c) Tellma Ltd. All rights reserved.

using System.Net;
using System.Text;

namespace Tellma.Integration.MarminAe.Tests
{
    /// <summary>
    ///     An <see cref="HttpMessageHandler" /> that answers from a script instead of the network.
    /// </summary>
    /// <remarks>
    ///     <see cref="MarminAeClient" /> takes the <see cref="HttpClient" /> from its caller, which
    ///     is what makes the whole request/response path testable without the vendor.
    /// </remarks>
    internal sealed class ScriptedHandler(
        Func<HttpRequestMessage, int, HttpResponseMessage> respond) : HttpMessageHandler
    {
        private int _count;

        /// <summary>Every request that reached the handler, in order.</summary>
        internal List<HttpRequestMessage> Requests { get; } = [];

        /// <summary>How many requests went to the token endpoint.</summary>
        internal int TokenRequests => Requests.Count(IsToken);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(respond(request, _count++));
        }

        /// <summary>A token response the provider will accept, valid for an hour.</summary>
        internal static HttpResponseMessage Token(string value = "token-1") =>
            Json(HttpStatusCode.OK, "{\"token\":\"" + value + "\",\"expires_in\":3600}");

        internal static HttpResponseMessage Json(HttpStatusCode status, string body) =>
            new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

        internal static bool IsToken(HttpRequestMessage request) =>
            request.RequestUri!.AbsolutePath.EndsWith("auth/token", StringComparison.Ordinal);
    }
}
