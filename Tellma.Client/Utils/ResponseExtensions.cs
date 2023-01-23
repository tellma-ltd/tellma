using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Client
{
    internal static class ResponseExtensions
    {
        internal static async Task EnsureSuccess(this HttpResponseMessage msg, CancellationToken cancellation)
        {
            // Handle all known status codes that tellma may return
            switch (msg.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new AuthenticationException();

                case HttpStatusCode.Forbidden:
                    throw new AuthorizationException();

                case HttpStatusCode.NotFound:
                    var ids = await msg.Content.ReadAsAsync<List<object>>(cancellation);
                    throw new NotFoundException(ids);

                case HttpStatusCode.UnprocessableEntity:
                    var errors = await msg.Content.ReadAsAsync<ValidationErrors>(cancellation);
                    var readonlyErros = new ReadonlyValidationErrors(errors);
                    throw new ValidationException(readonlyErros);

                case HttpStatusCode.BadRequest:
                    var errorMsg = await msg.Content.ReadAsStringAsync();
                    throw new TellmaException(errorMsg);

                case HttpStatusCode.InternalServerError:
                    var traceIdSpy = await msg.Content.ReadAsAsync<TraceIdentifierSpy>(cancellation);
                    throw new InternalServerException(traceIdSpy?.TraceIdentifier);

                case 0:
                    throw new TellmaOfflineException();
            }

            if (!msg.IsSuccessStatusCode)
            {
                // Future proofing
                throw new TellmaException($"Unhandled status code {msg.StatusCode}.");
            }
        }

        /// <summary>
        /// Extracts the server time value from the <see cref="HttpResponseMessage"/> headers.
        /// </summary>
        private static DateTimeOffset ServerTime(this HttpResponseMessage msg)
        {
            if (!(msg.Headers.TryGetValues(ResponseHeaders.ServerTime, out IEnumerable<string> values) &&
                values.Any() && DateTimeOffset.TryParse(values.First(), out DateTimeOffset serverTime)))
            {
                serverTime = DateTimeOffset.UtcNow;
            }

            return serverTime;
        }

        private class TraceIdentifierSpy
        {
            public string TraceIdentifier { get; set; }
        }
    }
}
