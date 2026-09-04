// Copyright (c) Tellma Ltd. All rights reserved.
//
// This source code is licensed under the Apache-2.0 license found in the
// LICENSE file in the root directory of this source tree.

namespace Tellma.Connector.MarminAe
{
    /// <summary>Connection settings for one Marmin UAE organization.</summary>
    /// <remarks>
    ///     One instance describes one set of API credentials, which the vendor issues per
    ///     organization. A deployment serving several organizations holds one client per set.
    /// </remarks>
    public sealed record MarminAeClientOptions
    {
        /// <summary>The public sandbox API root.</summary>
        public static Uri SandboxBaseAddress { get; } =
            new Uri("https://api-sandbox.ae.marmin.ai", UriKind.Absolute);

        /// <summary>The API host, which is per country and per environment.</summary>
        public required Uri BaseAddress { get; init; }

        /// <summary>The organization client id, which the vendor issues in the form "org_...".</summary>
        /// <remarks>Not a secret: it travels as a query parameter on the token request.</remarks>
        public required string ClientId { get; init; }

        /// <summary>The client secret, from the host's secret store.</summary>
        /// <remarks>
        ///     Never sent. It is only ever used as the key of the HMAC the token request proves
        ///     possession with, and it is read afresh on every token request so that rotating it
        ///     takes effect on the next refresh rather than on the next restart.
        /// </remarks>
        public required string ClientSecret { get; init; }

        /// <summary>
        ///     The per-request timeout. The client owns it rather than the
        ///     <see cref="HttpClient" /> so that a request timing out can be told apart from the
        ///     caller abandoning the operation.
        /// </summary>
        public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);

        /// <summary>
        ///     How long before its stated expiry a cached token counts as expired.
        /// </summary>
        /// <remarks>
        ///     The margin is what keeps a data call from departing with a token that dies in
        ///     flight; the single re-authentication retry covers whatever the margin misses.
        /// </remarks>
        public TimeSpan TokenExpirySkew { get; init; } = TimeSpan.FromSeconds(60);

        /// <summary>
        ///     The base address with a trailing slash, which is what relative route composition
        ///     needs to preserve a host's path prefix.
        /// </summary>
        /// <remarks>
        ///     <see cref="Uri" /> resolution drops the last path segment of a base that does not
        ///     end in a slash, so a deployment behind a gateway path would otherwise lose it.
        /// </remarks>
        public Uri NormalizedBaseAddress => Normalize(BaseAddress);

        /// <summary>Refuses settings a client could be built with but never used.</summary>
        /// <remarks>
        ///     Called from every constructor that takes these, so a misconfiguration fails where it
        ///     was made rather than on the first transmission — which, for an e-invoicing client, is
        ///     the worst possible moment to discover it.
        /// </remarks>
        /// <param name="options">The settings.</param>
        /// <exception cref="ArgumentException">The settings could not produce a working client.</exception>
        internal static void Validate(MarminAeClientOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            // Touched rather than merely read: composing the base address is where an unusable one
            // announces itself.
            _ = options.NormalizedBaseAddress;

            if (options.Timeout <= TimeSpan.Zero)
            {
                throw new ArgumentException(
                    "The Marmin UAE request timeout must be positive.", nameof(options));
            }

            if (options.TokenExpirySkew < TimeSpan.Zero)
            {
                throw new ArgumentException(
                    "The Marmin UAE token expiry margin must not be negative.", nameof(options));
            }
        }

        private static Uri Normalize(Uri baseAddress)
        {
            ArgumentNullException.ThrowIfNull(baseAddress);

            if (!baseAddress.IsAbsoluteUri)
            {
                throw new ArgumentException(
                    "The Marmin UAE base address must be an absolute URI.", nameof(baseAddress));
            }

            // A query or a fragment on an API root cannot survive route composition — the route
            // would land after it, or replace it — so it is refused rather than silently dropped.
            if (!string.IsNullOrEmpty(baseAddress.Query) || !string.IsNullOrEmpty(baseAddress.Fragment))
            {
                throw new ArgumentException(
                    "The Marmin UAE base address must carry no query string and no fragment.",
                    nameof(baseAddress));
            }

            if (baseAddress.AbsolutePath.EndsWith('/'))
            {
                return baseAddress;
            }

            // The slash goes on the path, not on the whole address: relative route composition
            // otherwise drops the last path segment, which is the gateway prefix it exists to keep.
            UriBuilder builder = new(baseAddress) { Path = baseAddress.AbsolutePath + "/" };

            return builder.Uri;
        }
    }
}
