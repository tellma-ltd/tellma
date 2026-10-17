// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Tellma.Connector.MarminAe
{
    /// <summary>A typed client over the slice of the Marmin UAE API the platform needs.</summary>
    /// <remarks>
    ///     <para>
    ///         <strong>Retry-free</strong> beyond one re-authentication. Durable retry policy
    ///         belongs to the caller, which owns the durability and can be idempotent by design; a
    ///         retry here would silently duplicate legal documents the caller believes failed.
    ///         Throttling is surfaced, never absorbed: the quota reading rides on every result and
    ///         on every refusal, and what to do about it is the caller's decision.
    ///     </para>
    ///     <para>
    ///         Acceptance is not the end of the story. A submission answered with a success status
    ///         has passed the vendor's own checks only; validation and transmission run afterwards
    ///         on two independent legs and can fail a document that was accepted. Those outcomes
    ///         arrive as data, on the document and on the status routes, never as exceptions.
    ///     </para>
    ///     <para>
    ///         Three exceptions leave this type and no others: a
    ///         <see cref="MarminAeRequestException" /> when the vendor refused the request or this
    ///         client refused to send it, a <see cref="TimeoutException" /> when a request outlived
    ///         the configured timeout, and an <see cref="HttpRequestException" /> when it never
    ///         reached the vendor at all. Cancellation propagates as
    ///         <see cref="OperationCanceledException" />, which is the caller's own doing rather
    ///         than a failure.
    ///     </para>
    ///     <para>
    ///         Thread-safe, and meant to live as long as the credentials it was built with.
    ///     </para>
    /// </remarks>
    public sealed class MarminAeClient
    {
        /// <summary>
        ///     The dated wire contract this client was written against, pinned rather than
        ///     configured: a version change alters payload shapes, so it is a deliberate release
        ///     with the live suite re-run, not a setting somebody flips.
        /// </summary>
        public const string ApiVersion = "20260507";

        /// <summary>The header naming the wire contract a request expects.</summary>
        public const string VersionHeaderName = "X-MARMIN-VERSION";

        /// <summary>The header carrying how many calls the current window allows.</summary>
        /// <remarks>
        ///     The vendor's reference names this one; its deployment sends
        ///     <see cref="StandardRateLimitLimitHeaderName" /> and
        ///     <see cref="RateLimitLimitPerMinuteHeaderName" /> instead. All three are read.
        /// </remarks>
        public const string RateLimitLimitHeaderName = "X-RateLimit-Limit";

        /// <summary>The header carrying how many of them are left.</summary>
        public const string RateLimitRemainingHeaderName = "X-RateLimit-Remaining";

        /// <summary>The header carrying when the window starts over.</summary>
        public const string RateLimitResetHeaderName = "X-RateLimit-Reset";

        /// <summary>The standards-track spelling of the window's allowance.</summary>
        public const string StandardRateLimitLimitHeaderName = "RateLimit-Limit";

        /// <summary>The standards-track spelling of what is left of it.</summary>
        public const string StandardRateLimitRemainingHeaderName = "RateLimit-Remaining";

        /// <summary>The standards-track spelling of when the window starts over.</summary>
        public const string StandardRateLimitResetHeaderName = "RateLimit-Reset";

        /// <summary>The vendor's per-minute spelling of the window's allowance.</summary>
        public const string RateLimitLimitPerMinuteHeaderName = "X-RateLimit-Limit-Minute";

        /// <summary>The vendor's per-minute spelling of what is left of it.</summary>
        public const string RateLimitRemainingPerMinuteHeaderName = "X-RateLimit-Remaining-Minute";

        /// <summary>The header a download names its file in.</summary>
        public const string SuggestedFileNameHeaderName = "X-Suggested-Filename";

        /// <summary>
        ///     The largest submission the vendor accepts, counted over the whole serialized body.
        /// </summary>
        /// <remarks>
        ///     The vendor states its limit as "8MB", which is ambiguous between eight million bytes
        ///     and eight binary megabytes. It is the latter: a submission of 8,099,998 bytes is
        ///     accepted and reaches validation, and one of 8,499,998 is refused.
        /// </remarks>
        public const int MaxRequestPayloadBytes = 8 * 1024 * 1024;

        private const string JsonMediaType = "application/json";

        private readonly HttpClient _httpClient;
        private readonly MarminAeClientOptions _options;
        private readonly MarminAeTokenProvider _tokenProvider;
        private readonly TimeProvider _timeProvider;

        /// <summary>Creates a client over an <see cref="HttpClient" /> the caller owns.</summary>
        /// <param name="httpClient">The transport; hosts wire it through a factory, tests through
        ///     a scripted handler.</param>
        /// <param name="options">The credentials, the host, and the timings.</param>
        /// <param name="tokenProvider">The token provider to share, when a caller has one already.</param>
        /// <param name="timeProvider">The clock, for request timeouts and quota readings.</param>
        public MarminAeClient(
            HttpClient httpClient,
            MarminAeClientOptions options,
            MarminAeTokenProvider? tokenProvider = null,
            TimeProvider? timeProvider = null)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentException.ThrowIfNullOrWhiteSpace(options.ClientId);
            ArgumentException.ThrowIfNullOrWhiteSpace(options.ClientSecret);
            MarminAeClientOptions.Validate(options);

            _httpClient = httpClient;
            _options = options;
            _timeProvider = timeProvider ?? TimeProvider.System;
            _tokenProvider = tokenProvider ?? new MarminAeTokenProvider(httpClient, options, _timeProvider);
        }

        /// <summary>Submits a sales invoice.</summary>
        /// <param name="businessProfileId">The business profile issuing it.</param>
        /// <param name="request">The invoice.</param>
        /// <param name="cancellationToken">Abandons the request.</param>
        /// <returns>The invoice as the vendor stored it, with its identifiers and totals.</returns>
        /// <exception cref="MarminAeRequestException">The vendor refused the submission, or it was
        ///     too large to send.</exception>
        /// <exception cref="TimeoutException">The request, or the token request behind it, did not
        ///     finish inside the configured timeout.</exception>
        /// <exception cref="HttpRequestException">The request never reached the vendor.</exception>
        public async Task<MarminAeResponse<MarminAeDocument>> CreateSalesInvoiceAsync(
            string businessProfileId,
            MarminAeSalesInvoiceRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(businessProfileId);
            ArgumentNullException.ThrowIfNull(request);

            return await SubmitAsync(
                MarminAeDocumentKind.SalesInvoice,
                HttpMethod.Post,
                MarminAeRoutes.Create(MarminAeDocumentKind.SalesInvoice, businessProfileId),
                Serialize(request, MarminAeJsonContext.Default.MarminAeSalesInvoiceRequest),
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Replaces a sales invoice and resubmits it.</summary>
        /// <remarks>
        ///     A whole-document operation: the vendor accepts no partial change and clears what the
        ///     payload omits, which is why this takes the same type a submission does. It is
        ///     permitted only while the document is in a state that allows correction.
        /// </remarks>
        /// <param name="businessProfileId">The business profile issuing it.</param>
        /// <param name="documentId">The invoice being replaced.</param>
        /// <param name="request">The invoice in full, corrections included.</param>
        /// <param name="cancellationToken">Abandons the request.</param>
        /// <returns>The invoice as the vendor stored it.</returns>
        /// <exception cref="MarminAeRequestException">The vendor refused the submission, or it was
        ///     too large to send.</exception>
        /// <exception cref="TimeoutException">The request, or the token request behind it, did not
        ///     finish inside the configured timeout.</exception>
        /// <exception cref="HttpRequestException">The request never reached the vendor.</exception>
        public async Task<MarminAeResponse<MarminAeDocument>> ResubmitSalesInvoiceAsync(
            string businessProfileId,
            string documentId,
            MarminAeSalesInvoiceRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(businessProfileId);
            ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
            ArgumentNullException.ThrowIfNull(request);

            return await SubmitAsync(
                MarminAeDocumentKind.SalesInvoice,
                HttpMethod.Put,
                MarminAeRoutes.Resubmit(MarminAeDocumentKind.SalesInvoice, businessProfileId, documentId),
                Serialize(request, MarminAeJsonContext.Default.MarminAeSalesInvoiceRequest),
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Submits a sales credit note.</summary>
        /// <param name="businessProfileId">The business profile issuing it.</param>
        /// <param name="request">The credit note.</param>
        /// <param name="cancellationToken">Abandons the request.</param>
        /// <returns>The credit note as the vendor stored it.</returns>
        /// <exception cref="MarminAeRequestException">The vendor refused the submission, or it was
        ///     too large to send.</exception>
        /// <exception cref="TimeoutException">The request, or the token request behind it, did not
        ///     finish inside the configured timeout.</exception>
        /// <exception cref="HttpRequestException">The request never reached the vendor.</exception>
        public async Task<MarminAeResponse<MarminAeDocument>> CreateSalesCreditNoteAsync(
            string businessProfileId,
            MarminAeSalesCreditNoteRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(businessProfileId);
            ArgumentNullException.ThrowIfNull(request);

            return await SubmitAsync(
                MarminAeDocumentKind.SalesCreditNote,
                HttpMethod.Post,
                MarminAeRoutes.Create(MarminAeDocumentKind.SalesCreditNote, businessProfileId),
                Serialize(request, MarminAeJsonContext.Default.MarminAeSalesCreditNoteRequest),
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Replaces a sales credit note and resubmits it.</summary>
        /// <param name="businessProfileId">The business profile issuing it.</param>
        /// <param name="documentId">The credit note being replaced.</param>
        /// <param name="request">The credit note in full, corrections included.</param>
        /// <param name="cancellationToken">Abandons the request.</param>
        /// <returns>The credit note as the vendor stored it.</returns>
        /// <exception cref="MarminAeRequestException">The vendor refused the submission, or it was
        ///     too large to send.</exception>
        /// <exception cref="TimeoutException">The request, or the token request behind it, did not
        ///     finish inside the configured timeout.</exception>
        /// <exception cref="HttpRequestException">The request never reached the vendor.</exception>
        public async Task<MarminAeResponse<MarminAeDocument>> ResubmitSalesCreditNoteAsync(
            string businessProfileId,
            string documentId,
            MarminAeSalesCreditNoteRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(businessProfileId);
            ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
            ArgumentNullException.ThrowIfNull(request);

            return await SubmitAsync(
                MarminAeDocumentKind.SalesCreditNote,
                HttpMethod.Put,
                MarminAeRoutes.Resubmit(MarminAeDocumentKind.SalesCreditNote, businessProfileId, documentId),
                Serialize(request, MarminAeJsonContext.Default.MarminAeSalesCreditNoteRequest),
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Reads one document.</summary>
        /// <param name="kind">Which family it belongs to.</param>
        /// <param name="documentId">The document.</param>
        /// <param name="cancellationToken">Abandons the request.</param>
        /// <returns>The document.</returns>
        /// <exception cref="MarminAeRequestException">The vendor refused the request.</exception>
        /// <exception cref="TimeoutException">The request, or the token request behind it, did not
        ///     finish inside the configured timeout.</exception>
        /// <exception cref="HttpRequestException">The request never reached the vendor.</exception>
        public async Task<MarminAeResponse<MarminAeDocument>> GetDocumentAsync(
            MarminAeDocumentKind kind, string documentId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

            MarminAeResponse<MarminAeDocument> response = await ReadJsonAsync(
                () => Request(HttpMethod.Get, MarminAeRoutes.Document(kind, documentId)),
                MarminAeJsonContext.Default.MarminAeDocument,
                cancellationToken).ConfigureAwait(false);

            return response with { Value = response.Value with { Kind = kind } };
        }

        /// <summary>Lists a family, filtered and paged.</summary>
        /// <param name="kind">Which family to list.</param>
        /// <param name="query">The filters and paging.</param>
        /// <param name="cancellationToken">Abandons the request.</param>
        /// <returns>One page of documents.</returns>
        /// <exception cref="MarminAeRequestException">The vendor refused the request.</exception>
        /// <exception cref="TimeoutException">The request, or the token request behind it, did not
        ///     finish inside the configured timeout.</exception>
        /// <exception cref="HttpRequestException">The request never reached the vendor.</exception>
        public async Task<MarminAeResponse<MarminAePage<MarminAeDocument>>> ListDocumentsAsync(
            MarminAeDocumentKind kind, MarminAeDocumentQuery query, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(query);

            MarminAeResponse<MarminAePage<MarminAeDocument>> response = await ReadJsonAsync(
                () => Request(HttpMethod.Get, MarminAeRoutes.List(kind, query)),
                MarminAeJsonContext.Default.MarminAePageMarminAeDocument,
                cancellationToken).ConfigureAwait(false);

            IReadOnlyList<MarminAeDocument>? content = response.Value.Content;
            if (content is null || content.Count == 0)
            {
                return response;
            }

            MarminAePage<MarminAeDocument> stamped = response.Value with
            {
                Content = [.. content.Select(document => document with { Kind = kind })],
            };

            return response with { Value = stamped };
        }

        /// <summary>Lists the identifiers of a family's documents created on one day.</summary>
        /// <remarks>
        ///     A per-day listing: the vendor requires the date, so covering a range means asking
        ///     day by day.
        /// </remarks>
        /// <param name="kind">Which family to list.</param>
        /// <param name="createdOn">The day to list.</param>
        /// <param name="page">The zero-indexed page, when paging explicitly.</param>
        /// <param name="size">The page size, when setting one.</param>
        /// <param name="cancellationToken">Abandons the request.</param>
        /// <returns>One page of identifiers.</returns>
        /// <exception cref="MarminAeRequestException">The vendor refused the request.</exception>
        /// <exception cref="TimeoutException">The request, or the token request behind it, did not
        ///     finish inside the configured timeout.</exception>
        /// <exception cref="HttpRequestException">The request never reached the vendor.</exception>
        public Task<MarminAeResponse<MarminAePage<string>>> ListDocumentIdsAsync(
            MarminAeDocumentKind kind,
            DateOnly createdOn,
            int? page,
            int? size,
            CancellationToken cancellationToken)
        {
            return ReadJsonAsync(
                () => Request(HttpMethod.Get, MarminAeRoutes.DocumentIds(kind, createdOn, page, size)),
                MarminAeJsonContext.Default.MarminAePageString,
                cancellationToken);
        }

        /// <summary>Reads where a document currently stands on each transmission leg.</summary>
        /// <param name="kind">Which family it belongs to.</param>
        /// <param name="documentId">The document.</param>
        /// <param name="cancellationToken">Abandons the request.</param>
        /// <returns>The snapshot.</returns>
        /// <exception cref="MarminAeRequestException">The vendor refused the request.</exception>
        /// <exception cref="TimeoutException">The request, or the token request behind it, did not
        ///     finish inside the configured timeout.</exception>
        /// <exception cref="HttpRequestException">The request never reached the vendor.</exception>
        public Task<MarminAeResponse<MarminAePeppolStatusSnapshot>> GetPeppolStatusAsync(
            MarminAeDocumentKind kind, string documentId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

            return ReadJsonAsync(
                () => Request(HttpMethod.Get, MarminAeRoutes.PeppolStatus(kind, documentId)),
                MarminAeJsonContext.Default.MarminAePeppolStatusSnapshot,
                cancellationToken);
        }

        /// <summary>Reads a document's transmission history.</summary>
        /// <param name="kind">Which family it belongs to.</param>
        /// <param name="documentId">The document.</param>
        /// <param name="cancellationToken">Abandons the request.</param>
        /// <returns>The log, oldest step first.</returns>
        /// <exception cref="MarminAeRequestException">The vendor refused the request.</exception>
        /// <exception cref="TimeoutException">The request, or the token request behind it, did not
        ///     finish inside the configured timeout.</exception>
        /// <exception cref="HttpRequestException">The request never reached the vendor.</exception>
        public async Task<MarminAeResponse<IReadOnlyList<MarminAePeppolStatusLogEntry>>> GetPeppolStatusLogsAsync(
            MarminAeDocumentKind kind, string documentId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

            MarminAeResponse<List<MarminAePeppolStatusLogEntry>> response = await ReadJsonAsync(
                () => Request(HttpMethod.Get, MarminAeRoutes.PeppolStatusLogs(kind, documentId)),
                MarminAeJsonContext.Default.ListMarminAePeppolStatusLogEntry,
                cancellationToken).ConfigureAwait(false);

            return new MarminAeResponse<IReadOnlyList<MarminAePeppolStatusLogEntry>>(
                response.Value, response.StatusCode, response.RateLimit);
        }

        /// <summary>Reads one business profile.</summary>
        /// <param name="businessProfileId">The profile.</param>
        /// <param name="cancellationToken">Abandons the request.</param>
        /// <returns>The profile.</returns>
        /// <exception cref="MarminAeRequestException">The vendor refused the request.</exception>
        /// <exception cref="TimeoutException">The request, or the token request behind it, did not
        ///     finish inside the configured timeout.</exception>
        /// <exception cref="HttpRequestException">The request never reached the vendor.</exception>
        public Task<MarminAeResponse<MarminAeBusinessProfile>> GetBusinessProfileAsync(
            string businessProfileId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(businessProfileId);

            return ReadJsonAsync(
                () => Request(HttpMethod.Get, MarminAeRoutes.BusinessProfile(businessProfileId)),
                MarminAeJsonContext.Default.MarminAeBusinessProfile,
                cancellationToken);
        }

        /// <summary>Downloads a document's UBL rendering.</summary>
        /// <param name="kind">Which family it belongs to.</param>
        /// <param name="documentId">The document.</param>
        /// <param name="cancellationToken">Abandons the request.</param>
        /// <returns>The file, which the caller disposes.</returns>
        /// <exception cref="MarminAeRequestException">The vendor refused the request.</exception>
        /// <exception cref="TimeoutException">The request, or the token request behind it, did not
        ///     finish inside the configured timeout.</exception>
        /// <exception cref="HttpRequestException">The request never reached the vendor.</exception>
        public Task<MarminAeDownload> DownloadXmlAsync(
            MarminAeDocumentKind kind, string documentId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

            return DownloadAsync(
                () => Request(HttpMethod.Get, MarminAeRoutes.Xml(kind, documentId), acceptsJson: false),
                cancellationToken);
        }

        /// <summary>Downloads a document's PDF rendering.</summary>
        /// <param name="kind">Which family it belongs to.</param>
        /// <param name="documentId">The document.</param>
        /// <param name="cancellationToken">Abandons the request.</param>
        /// <returns>The file, which the caller disposes.</returns>
        /// <exception cref="MarminAeRequestException">The vendor refused the request.</exception>
        /// <exception cref="TimeoutException">The request, or the token request behind it, did not
        ///     finish inside the configured timeout.</exception>
        /// <exception cref="HttpRequestException">The request never reached the vendor.</exception>
        public Task<MarminAeDownload> DownloadPdfAsync(
            MarminAeDocumentKind kind, string documentId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

            return DownloadAsync(
                () => Request(HttpMethod.Get, MarminAeRoutes.Pdf(kind, documentId), acceptsJson: false),
                cancellationToken);
        }

        /// <summary>Downloads one of the files held against a document.</summary>
        /// <param name="kind">Which family it belongs to.</param>
        /// <param name="documentId">The document.</param>
        /// <param name="attachmentId">The file, as identified on the document.</param>
        /// <param name="cancellationToken">Abandons the request.</param>
        /// <returns>The file, which the caller disposes.</returns>
        /// <exception cref="MarminAeRequestException">The vendor refused the request.</exception>
        /// <exception cref="TimeoutException">The request, or the token request behind it, did not
        ///     finish inside the configured timeout.</exception>
        /// <exception cref="HttpRequestException">The request never reached the vendor.</exception>
        public Task<MarminAeDownload> DownloadAttachmentAsync(
            MarminAeDocumentKind kind,
            string documentId,
            string attachmentId,
            CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
            ArgumentException.ThrowIfNullOrWhiteSpace(attachmentId);

            return DownloadAsync(
                () => Request(
                    HttpMethod.Get,
                    MarminAeRoutes.Attachment(kind, documentId, attachmentId),
                    acceptsJson: false),
                cancellationToken);
        }

        private static ReadOnlyMemory<byte> Serialize<T>(T request, JsonTypeInfo<T> typeInfo)
        {
            byte[] payload = JsonSerializer.SerializeToUtf8Bytes(request, typeInfo);
            ThrowIfOversized(payload.Length);

            return payload;
        }

        /// <summary>Refuses a payload the vendor would refuse for its size.</summary>
        /// <param name="byteCount">The serialized payload size.</param>
        /// <exception cref="MarminAeRequestException">The payload is over the cap.</exception>
        internal static void ThrowIfOversized(int byteCount)
        {
            // Checked before a token is obtained and before anything is sent: an oversized document
            // would otherwise spend a token request and a unit of the caller's quota only to be
            // refused, and the refusal would arrive without saying by how much.
            if (byteCount > MaxRequestPayloadBytes)
            {
                throw new MarminAeRequestException(
                    string.Create(
                        CultureInfo.InvariantCulture,
                        $"The document serializes to {byteCount} bytes, over the Marmin UAE limit of {MaxRequestPayloadBytes} bytes for one request. Attachments are counted after encoding, which adds about a third to their size."));
            }
        }

        private HttpRequestMessage Request(HttpMethod method, string relativeRoute, bool acceptsJson = true)
        {
            HttpRequestMessage request = new(method, new Uri(_options.NormalizedBaseAddress, relativeRoute));

            if (acceptsJson)
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));
            }

            return request;
        }

        private HttpRequestMessage Request(HttpMethod method, string relativeRoute, ReadOnlyMemory<byte> payload)
        {
            HttpRequestMessage request = Request(method, relativeRoute);

            // Built afresh for every attempt from the same bytes: a request message cannot be sent
            // twice, and the re-authentication retry has to send the identical body.
            request.Content = new ReadOnlyMemoryContent(payload);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(JsonMediaType);

            return request;
        }

        private async Task<MarminAeResponse<MarminAeDocument>> SubmitAsync(
            MarminAeDocumentKind kind,
            HttpMethod method,
            string relativeRoute,
            ReadOnlyMemory<byte> payload,
            CancellationToken cancellationToken)
        {
            MarminAeResponse<MarminAeDocument> response = await ReadJsonAsync(
                () => Request(method, relativeRoute, payload),
                MarminAeJsonContext.Default.MarminAeDocument,
                cancellationToken).ConfigureAwait(false);

            return response with { Value = response.Value with { Kind = kind } };
        }

        private async Task<MarminAeResponse<T>> ReadJsonAsync<T>(
            Func<HttpRequestMessage> requestFactory,
            JsonTypeInfo<T> typeInfo,
            CancellationToken cancellationToken)
        {
            using HttpResponseMessage response = await SendAsync(
                requestFactory, HttpCompletionOption.ResponseContentRead, cancellationToken)
                .ConfigureAwait(false);

            var rateLimit = MarminAeRateLimit.From(response.Headers, _timeProvider.GetUtcNow());
            string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            MarminAeRequestException.ThrowIfRefused(response, body, rateLimit);

            T? value;
            try
            {
                value = JsonSerializer.Deserialize(body, typeInfo);
            }
            catch (JsonException exception)
            {
                throw new MarminAeRequestException(
                    "The Marmin UAE API answered with a body that could not be read.", exception);
            }

            T content = value ?? throw new MarminAeRequestException(
                "The Marmin UAE API answered with an empty body where content was expected.");

            return new MarminAeResponse<T>(content, (int)response.StatusCode, rateLimit);
        }

        private async Task<MarminAeDownload> DownloadAsync(
            Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await SendAsync(
                requestFactory, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            MarminAeDownload? download = null;
            try
            {
                var rateLimit = MarminAeRateLimit.From(
                    response.Headers, _timeProvider.GetUtcNow());

                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync(cancellationToken)
                        .ConfigureAwait(false);

                    throw new MarminAeRequestException(
                        (int)response.StatusCode, MarminAeErrorParser.Parse(body), rateLimit);
                }

                Stream content = await response.Content.ReadAsStreamAsync(cancellationToken)
                    .ConfigureAwait(false);

                download = new MarminAeDownload(
                    response,
                    content,
                    response.Content.Headers.ContentType?.MediaType,
                    ReadFileName(response),
                    response.Content.Headers.ContentLength,
                    rateLimit);

                return download;
            }
            finally
            {
                // Ownership moves to the download only once one exists; on any other path the
                // response — and the connection behind it — has to be let go here.
                if (download is null)
                {
                    response.Dispose();
                }
            }
        }

        private static string? ReadFileName(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues(SuggestedFileNameHeaderName, out IEnumerable<string>? suggested))
            {
                string? value = suggested.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            ContentDispositionHeaderValue? disposition = response.Content.Headers.ContentDisposition;
            string? fromDisposition = disposition?.FileNameStar ?? disposition?.FileName;

            return string.IsNullOrWhiteSpace(fromDisposition)
                ? null
                : fromDisposition.Trim('"');
        }

        private async Task<HttpResponseMessage> SendAsync(
            Func<HttpRequestMessage> requestFactory,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            (HttpResponseMessage response, string token) = await SendOnceAsync(
                requestFactory, completionOption, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }

            // The token-expiry race, resolved where it happens. This is not a durable retry: an
            // unauthenticated request did no work at the vendor whatever its verb, so repeating it
            // cannot duplicate a document. A second refusal is a real one and is surfaced.
            //
            // Naming the refused token matters when several calls carry the same one into expiry:
            // they come back refused one after another, and by then the first has already installed
            // a replacement that the others must not discard.
            response.Dispose();
            _tokenProvider.Invalidate(token);

            (HttpResponseMessage retried, _) = await SendOnceAsync(
                requestFactory, completionOption, cancellationToken).ConfigureAwait(false);

            return retried;
        }

        private async Task<(HttpResponseMessage Response, string Token)> SendOnceAsync(
            Func<HttpRequestMessage> requestFactory,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            // Checked before anything else: a caller that has already given up must not have a
            // request sent on its behalf, and not every transport notices a token that was cancelled
            // before the call began.
            cancellationToken.ThrowIfCancellationRequested();

            string token = await _tokenProvider.GetTokenAsync(cancellationToken).ConfigureAwait(false);

            using HttpRequestMessage request = requestFactory();

            // Per request rather than on the client's default headers: the transport is injected
            // and may be shared, and this is what lets a rotated credential take effect on the
            // next call rather than on the next restart.
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.TryAddWithoutValidation(VersionHeaderName, ApiVersion);

            // The client owns the timeout so that "this request timed out" can be told apart from
            // "the caller gave up", which are different failures with different remedies.
            using CancellationTokenSource timeout = new(_options.Timeout, _timeProvider);
            using var linked =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

            try
            {
                HttpResponseMessage response = await _httpClient
                    .SendAsync(request, completionOption, linked.Token).ConfigureAwait(false);

                return (response, token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException(
                    string.Create(
                        CultureInfo.InvariantCulture,
                        $"The Marmin UAE request did not complete within {_options.Timeout}."));
            }
        }
    }
}
