// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Tellma.Connector.MarminAe
{
    /// <summary>
    ///     Obtains and caches the short-lived access token: computes the signature, calls the token
    ///     endpoint, and serves what it got until the token nears expiry.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Thread-safe and meant to live as long as the client does. Concurrent callers that
    ///         all need a refresh await one token request rather than issuing one each: the token
    ///         endpoint is rate-limited like everything else, and a stampede there takes the whole
    ///         integration down rather than one call.
    ///     </para>
    ///     <para>
    ///         The signature is recomputed for every token request rather than derived once, so
    ///         nothing made from the secret outlives a single call, and a host that rebuilds this
    ///         from rotated credentials needs no restart: the transport it shares carries no
    ///         authentication state of its own.
    ///     </para>
    /// </remarks>
    public sealed class MarminAeTokenProvider
    {
        /// <summary>The header the token request proves possession of the secret in.</summary>
        public const string SignatureHeaderName = "x-marmin-signature";

        // What to assume when the response says nothing readable about expiry and the token itself
        // does not either. Short, because guessing long risks sending a dead token, and the cost of
        // guessing short is one extra request.
        private static readonly TimeSpan UnknownLifetime = TimeSpan.FromMinutes(5);

        // Nothing the vendor issues lives anywhere near this long, so a value beyond it is a
        // misread rather than a very long-lived token, and treating it as one would wedge the cache.
        private static readonly TimeSpan MaximumLifetime = TimeSpan.FromHours(24);

        private readonly HttpClient _httpClient;
        private readonly MarminAeClientOptions _options;
        private readonly TimeProvider _timeProvider;
        // Upstream used System.Threading.Lock, which is .NET 9+. This project targets net8.0
        // to match Tellma.Api, and `lock (object)` has identical semantics -- the Lock type
        // only adds a codegen optimisation, not a behavioural difference.
        private readonly object _gate = new();

        private CachedToken? _cached;
        private Task<CachedToken>? _inFlight;

        /// <summary>Creates a provider over an <see cref="HttpClient" /> the caller owns.</summary>
        /// <param name="httpClient">The transport.</param>
        /// <param name="options">The credentials, the host, and the timings.</param>
        /// <param name="timeProvider">The clock, for expiry and for the request timeout.</param>
        public MarminAeTokenProvider(
            HttpClient httpClient, MarminAeClientOptions options, TimeProvider? timeProvider = null)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentException.ThrowIfNullOrWhiteSpace(options.ClientId);
            ArgumentException.ThrowIfNullOrWhiteSpace(options.ClientSecret);
            MarminAeClientOptions.Validate(options);

            _httpClient = httpClient;
            _options = options;
            _timeProvider = timeProvider ?? TimeProvider.System;
        }

        /// <summary>The valid token, from the cache or freshly obtained.</summary>
        /// <param name="cancellationToken">Abandons the wait.</param>
        /// <returns>A token that will still be valid when a request carrying it departs.</returns>
        /// <exception cref="MarminAeRequestException">The token could not be obtained.</exception>
        /// <exception cref="TimeoutException">The token request did not finish inside the
        ///     configured timeout.</exception>
        /// <exception cref="HttpRequestException">The token request never reached the vendor.</exception>
        public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DateTimeOffset now = _timeProvider.GetUtcNow();

            CachedToken? cached = Volatile.Read(ref _cached);
            if (IsUsable(cached, now))
            {
                return cached.Token;
            }

            Task<CachedToken> inFlight;
            lock (_gate)
            {
                // Re-checked under the lock: another caller may have installed a token between the
                // fast path above and here.
                CachedToken? current = _cached;
                if (IsUsable(current, now))
                {
                    return current.Token;
                }

                inFlight = _inFlight ?? StartFetch();
            }

            // Awaited with the caller's own token rather than passing it into the request: one
            // caller giving up must not cancel a fetch every other caller is waiting on.
            CachedToken fetched = await inFlight.WaitAsync(cancellationToken).ConfigureAwait(false);

            return fetched.Token;
        }

        /// <summary>Drops the cached token so the next call obtains a fresh one.</summary>
        /// <remarks>
        ///     Unconditional, and therefore the blunt instrument: it discards whatever is cached,
        ///     including a token some other caller obtained a moment ago. Prefer
        ///     <see cref="Invalidate(string)" /> when the caller knows which token was refused.
        /// </remarks>
        public void Invalidate()
        {
            lock (_gate)
            {
                _cached = null;
            }
        }

        /// <summary>Drops the cached token only if it is the one that was refused.</summary>
        /// <remarks>
        ///     <para>
        ///         What keeps a burst of refusals from becoming a burst of token requests. Several
        ///         calls can be in flight carrying the same token when it expires; they come back
        ///         refused one after another, and by the time the second one does, the first has
        ///         already installed a replacement. Discarding unconditionally would throw that
        ///         replacement away and start another fetch, once per straggler — against an
        ///         endpoint that allows far fewer requests a minute than the document routes do.
        ///     </para>
        ///     <para>
        ///         A fetch already in flight is left alone either way: it was started after the
        ///         token being discarded was issued, so its result is the fresher of the two.
        ///     </para>
        /// </remarks>
        /// <param name="refusedToken">The token the vendor would not accept.</param>
        public void Invalidate(string refusedToken)
        {
            ArgumentNullException.ThrowIfNull(refusedToken);

            lock (_gate)
            {
                if (_cached is not null && string.Equals(_cached.Token, refusedToken, StringComparison.Ordinal))
                {
                    _cached = null;
                }
            }
        }

        private bool IsUsable([NotNullWhen(true)] CachedToken? cached, DateTimeOffset now)
        {
            return cached is not null && cached.ExpiresAt > now + _options.TokenExpirySkew;
        }

        /// <summary>Starts the one token request every waiting caller will share.</summary>
        /// <remarks>
        ///     The shared task is published before any work begins, rather than being the task the
        ///     fetch method returns. A fetch that happened to complete synchronously would
        ///     otherwise clear the field before the caller had assigned it, leaving a finished task
        ///     parked there forever and every later refresh joining it to get the stale token back.
        /// </remarks>
        private Task<CachedToken> StartFetch()
        {
            TaskCompletionSource<CachedToken> completion =
                new(TaskCreationOptions.RunContinuationsAsynchronously);

            _inFlight = completion.Task;
            _ = FetchAsync(completion);

            return completion.Task;
        }

        private async Task FetchAsync(TaskCompletionSource<CachedToken> completion)
        {
            try
            {
                CachedToken token = await RequestTokenAsync().ConfigureAwait(false);
                Settle(completion.Task, token);
                completion.SetResult(token);
            }
            catch (Exception exception)
            {
                // Nothing is swallowed: the failure is handed to every caller waiting on the shared
                // task. What must not happen is a failed request staying parked in the field, which
                // would make one bad refresh permanent.
                Settle(completion.Task, null);
                completion.SetException(exception);

                // Every caller may have given up before this landed — each awaits the shared task
                // through its own cancellation, so a cancelled wait leaves nobody to receive the
                // fault. Reading it here marks it observed, which keeps a failed refresh off the
                // unobserved-exception path without hiding it from anyone still waiting.
                _ = completion.Task.Exception;
            }
        }

        private void Settle(Task<CachedToken> completed, CachedToken? token)
        {
            lock (_gate)
            {
                if (token is not null)
                {
                    _cached = token;
                }

                if (ReferenceEquals(_inFlight, completed))
                {
                    _inFlight = null;
                }
            }
        }

        private async Task<CachedToken> RequestTokenAsync()
        {
            DateTimeOffset now = _timeProvider.GetUtcNow();

            // Computed here rather than once at construction: this is what makes a rotated secret
            // take effect on the next refresh. Note that the message is the raw client id, while
            // the query string carries the escaped one.
            string signature = MarminAeSignature.Compute(_options.ClientSecret, _options.ClientId);

            using HttpRequestMessage request = new(
                HttpMethod.Get,
                new Uri(_options.NormalizedBaseAddress, MarminAeRoutes.Token(_options.ClientId)));
            request.Headers.TryAddWithoutValidation(SignatureHeaderName, signature);
            request.Headers.TryAddWithoutValidation(MarminAeClient.VersionHeaderName, MarminAeClient.ApiVersion);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using CancellationTokenSource timeout = new(_options.Timeout, _timeProvider);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request, timeout.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // The same exception a data request raises when it runs out of time. Reporting one
                // as a timeout and the other as a refusal would make the failure depend on which
                // leg of the same call happened to be in flight.
                throw new TimeoutException(
                    string.Create(
                        CultureInfo.InvariantCulture,
                        $"The Marmin UAE token request did not complete within {_options.Timeout}."));
            }

            using (response)
            {
                string body = await response.Content.ReadAsStringAsync(timeout.Token).ConfigureAwait(false);
                var rateLimit = MarminAeRateLimit.From(response.Headers, now);
                MarminAeRequestException.ThrowIfRefused(response, body, rateLimit);

                return ReadToken(body, now);
            }
        }

        private CachedToken ReadToken(string body, DateTimeOffset now)
        {
            string? token = null;
            DateTimeOffset? expiresAt = null;

            try
            {
                using var document = JsonDocument.Parse(body);
                JsonElement root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    token = ReadText(root, "token") ?? ReadText(root, "access_token");

                    if (root.TryGetProperty("expires_at", out JsonElement expiry))
                    {
                        expiresAt = MarminAeTimestamps.Interpret(expiry, now);
                    }

                    if (expiresAt is null && root.TryGetProperty("expires_in", out JsonElement lifetime))
                    {
                        expiresAt = MarminAeTimestamps.Interpret(lifetime, now);
                    }
                }
            }
            catch (JsonException exception)
            {
                throw new MarminAeRequestException(
                    "The Marmin UAE token response was not readable as JSON.", exception);
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new MarminAeRequestException(
                    "The Marmin UAE token response carried no access token.");
            }

            // The token states its own lifetime, which settles the question when the envelope
            // around it does not.
            expiresAt ??= MarminAeTimestamps.FromJwtExpiry(token);
            expiresAt ??= now + UnknownLifetime;

            return new CachedToken(token, Clamp(expiresAt.Value, now));
        }

        private DateTimeOffset Clamp(DateTimeOffset expiresAt, DateTimeOffset now)
        {
            // A token that is already inside the refresh margin the moment it arrives would be
            // discarded and re-requested forever, so the floor buys it a usable instant.
            DateTimeOffset floor = now + _options.TokenExpirySkew + TimeSpan.FromSeconds(1);
            DateTimeOffset ceiling = now + MaximumLifetime;

            return expiresAt < floor ? floor : expiresAt > ceiling ? ceiling : expiresAt;
        }

        private static string? ReadText(JsonElement root, string name)
        {
            return root.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        private sealed record CachedToken(string Token, DateTimeOffset ExpiresAt);
    }
}
