using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Tellma.Api.MarminAe;
using Tellma.Connector.MarminAe;

// Same namespace the SendGrid and Twilio callback extensions use, so that Startup picks the
// extension method up without an extra using.
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Maps the anonymous endpoint Marmin posts status changes to.
    /// </summary>
    /// <remarks>
    /// Middleware rather than an MVC controller, following the only precedent this repo has for an
    /// unauthenticated inbound webhook (<c>UseTwilioCallback</c> and <c>UseSendGridCallback</c>).
    /// Being mapped before <c>UseRouting</c>/<c>UseAuthentication</c> keeps it entirely outside the
    /// auth pipeline, rather than relying on an <c>[AllowAnonymous]</c> that has to opt out of it.
    /// </remarks>
    public static class MarminAeCallbackExtensions
    {
        /// <summary>The route prefix; the tenant id follows as the next path segment.</summary>
        public const string RoutePrefix = "/api/marmin-ae-callback";

        /// <summary>
        /// A delivery is a small JSON envelope naming a document. Anything larger is not one, and
        /// buffering it would be doing an anonymous caller's memory allocation for them.
        /// </summary>
        private const int MaxBodyBytes = 256 * 1024;

        /// <summary>
        /// Maps <c>/api/marmin-ae-callback/{tenantId}</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The tenant id travels in the URL because nothing in the vendor's payload can be
        /// resolved to a Tellma tenant: it carries the vendor's own org and profile ids, and this
        /// repo has no lookup from either. Each of the two customers therefore registers its own
        /// callback URL in the Marmin portal. The URL is not a secret and does not need to be: the
        /// body is HMAC'd with that tenant's signing secret, so naming the wrong tenant simply
        /// fails verification.
        /// </para>
        /// </remarks>
        public static IApplicationBuilder UseMarminAeCallback(this IApplicationBuilder builder)
        {
            return builder.Map(RoutePrefix, app => app.Run(async ctx =>
            {
                var req = ctx.Request;
                var res = ctx.Response;
                var cancellation = ctx.RequestAborted;

                var logger = ctx.RequestServices.GetService<ILogger<IMarminAeCallbackHandler>>();
                var handler = ctx.RequestServices.GetService<IMarminAeCallbackHandler>();

                if (handler == null)
                {
                    // Helps during configuration for making sure the endpoint is working
                    res.StatusCode = StatusCodes.Status200OK;
                    await res.WriteAsync($"No implementation of {nameof(IMarminAeCallbackHandler)} was registered.", cancellation);
                    return;
                }

                if (req.Method == "GET")
                {
                    // Helps during configuration for making sure the endpoint is accessible
                    res.StatusCode = StatusCodes.Status200OK;
                    await res.WriteAsync("Welcome to the Marmin UAE e-invoicing callback endpoint!", cancellation);
                    return;
                }

                if (req.Method != "POST")
                {
                    res.StatusCode = StatusCodes.Status405MethodNotAllowed;
                    await res.WriteAsync($"{req.Method} method is not supported.", cancellation);
                    return;
                }

                // The remaining path is the tenant id, e.g. /api/marmin-ae-callback/101
                if (!int.TryParse(req.Path.Value?.Trim('/'), out int tenantId))
                {
                    res.StatusCode = StatusCodes.Status404NotFound;
                    await res.WriteAsync("The callback URL must end with the tenant Id.", cancellation);
                    return;
                }

                // Read the RAW bytes. The signature is computed over the body exactly as it was
                // sent, so decoding it to a string and re-encoding could change it and would
                // silently fail every verification.
                byte[] body;
                try
                {
                    body = await ReadBodyAsync(req, cancellation);
                }
                catch (InvalidOperationException)
                {
                    res.StatusCode = StatusCodes.Status413PayloadTooLarge;
                    await res.WriteAsync("The request body is too large.", cancellation);
                    return;
                }

                MarminAeCallbackContext context;
                try
                {
                    context = await handler.GetContextAsync(tenantId, cancellation);
                }
                catch (Exception ex)
                {
                    // An unknown tenant id, or one with no settings. Not a 500: an anonymous caller
                    // should not be able to tell which tenant ids exist.
                    logger?.LogWarning(ex, "Marmin callback: could not load settings for tenant {TenantId}.", tenantId);
                    res.StatusCode = StatusCodes.Status401Unauthorized;
                    await res.WriteAsync("Invalid signature.", cancellation);
                    return;
                }

                string signature = req.Headers[MarminAeWebhookVerifier.SignatureHeaderName];
                if (!MarminAeWebhookVerifier.Verify(body, signature, context.Secrets))
                {
                    res.StatusCode = StatusCodes.Status401Unauthorized;
                    await res.WriteAsync("Invalid signature.", cancellation);
                    return;
                }

                if (!MarminAeWebhookEventParser.TryParse(body, out var webhookEvent) || webhookEvent is null)
                {
                    res.StatusCode = StatusCodes.Status422UnprocessableEntity;
                    await res.WriteAsync("Failed to parse the body contents.", cancellation);
                    return;
                }

                // Defence in depth. The signature already proves the sender holds this tenant's
                // secret, but this catches the realistic two-tenant mistake of pasting one
                // tenant's callback URL into the other's portal.
                if (!string.IsNullOrWhiteSpace(context.ExpectedOrgId)
                    && !string.Equals(context.ExpectedOrgId, webhookEvent.OrgId.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    logger?.LogWarning(
                        "Marmin callback for tenant {TenantId} carried org id {OrgId}, which is not the one configured.",
                        tenantId, webhookEvent.OrgId);

                    res.StatusCode = StatusCodes.Status401Unauthorized;
                    await res.WriteAsync("Invalid signature.", cancellation);
                    return;
                }

                try
                {
                    var outcome = await handler.HandleAsync(tenantId, webhookEvent, cancellation);

                    // 200 even when nothing changed. A duplicate delivery, a stale one, and a
                    // document we do not have are all normal, and answering any of them with a
                    // 4xx/5xx would make the vendor retry them indefinitely.
                    res.StatusCode = StatusCodes.Status200OK;
                    await res.WriteAsync(outcome.ToString(), cancellation);
                }
                catch (Exception ex)
                {
                    // A genuine failure on our side. 500 IS right here: the vendor should retry.
                    logger?.LogError(ex, "Marmin callback failed for tenant {TenantId}.", tenantId);
                    res.StatusCode = StatusCodes.Status500InternalServerError;
                    await res.WriteAsync("Failed to handle the event.", cancellation);
                }
            }));
        }

        /// <summary>Buffers the request body, refusing anything over the cap.</summary>
        private static async Task<byte[]> ReadBodyAsync(HttpRequest req, System.Threading.CancellationToken cancellation)
        {
            if (req.ContentLength > MaxBodyBytes)
            {
                throw new InvalidOperationException("Body too large.");
            }

            using var buffer = new MemoryStream();
            await req.Body.CopyToAsync(buffer, cancellation);

            if (buffer.Length > MaxBodyBytes)
            {
                // Covers a chunked request, which arrives with no Content-Length to check.
                throw new InvalidOperationException("Body too large.");
            }

            return buffer.ToArray();
        }
    }
}
